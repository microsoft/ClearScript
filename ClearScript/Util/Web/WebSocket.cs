// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.ClearScript.Util.Web
{
    [SuppressMessage("Microsoft.Design", "CA1001:TypesThatOwnDisposableFieldsShouldBeDisposable", Justification = "This class uses a custom method for deterministic teardown.")]
    internal sealed class WebSocket
    {
        #region data

        private readonly Socket socket;
        private readonly bool isServerSocket;

        private readonly Random random = MiscHelpers.CreateSeededRandom();
        private readonly SemaphoreSlim receiveSemaphore = new(1);
        private readonly SemaphoreSlim sendSemaphore = new(1);

        private readonly InterlockedOneWayFlag closedFlag = new();

        #endregion

        #region constructors

        internal WebSocket(Socket socket, bool isServerSocket)
        {
            this.socket = socket;
            this.isServerSocket = isServerSocket;
        }

        #endregion

        #region message I/O

        public async Task<Message> ReceiveMessageAsync()
        {
            using (await receiveSemaphore.CreateLockScopeAsync().ConfigureAwait(false))
            {
                bool? isBinary = null;
                var payloads = new List<byte[]>();

                while (true)
                {
                    var frame = await ReceiveFrameAsync().ConfigureAwait(false);

                    if (frame.OpCode == OpCodes.Text)
                    {
                        if (isBinary.HasValue)
                        {
                            throw new Exception(ErrorCode.ProtocolError, "Received unexpected text frame from WebSocket");
                        }

                        isBinary = false;

                        payloads.Add(frame.Payload);
                        if (frame.Final)
                        {
                            break;
                        }
                    }
                    else if (frame.OpCode == OpCodes.Binary)
                    {
                        if (isBinary.HasValue)
                        {
                            throw new Exception(ErrorCode.ProtocolError, "Received unexpected binary frame from WebSocket");
                        }

                        isBinary = true;

                        payloads.Add(frame.Payload);
                        if (frame.Final)
                        {
                            break;
                        }
                    }
                    else if (frame.OpCode == OpCodes.Continuation)
                    {
                        if (!isBinary.HasValue)
                        {
                            throw new Exception(ErrorCode.ProtocolError, "Received unexpected continuation frame from WebSocket");
                        }

                        payloads.Add(frame.Payload);
                        if (frame.Final)
                        {
                            break;
                        }
                    }
                    else if (frame.OpCode == OpCodes.Ping)
                    {
                        var response = new Frame { Final = true, OpCode = OpCodes.Pong, Payload = frame.Payload };
                        await SendFrameAsync(response).ConfigureAwait(false);
                    }
                    else if (frame.OpCode == OpCodes.Pong)
                    {
                    }
                    else if (frame.OpCode == OpCodes.Close)
                    {
                        var errorCode = ErrorCode.NormalClosure;
                        var message = "The WebSocket was closed by the remote endpoint";

                        if (frame.Payload.Length >= 2)
                        {
                            errorCode = (ErrorCode)frame.Payload.ToHostUInt16();
                            if (frame.Payload.Length > 2)
                            {
                                message = Encoding.UTF8.GetString(frame.Payload, 2, frame.Payload.Length - 2);
                            }
                        }

                        throw new Exception(errorCode, message);
                    }
                    else
                    {
                        throw new Exception(ErrorCode.ProtocolError, "Received unrecognized frame from WebSocket");
                    }
                }

                var length = 0L;

                var overflow = false;
                try
                {
                    length = payloads.Aggregate(0L, (tempLength, segment) => checked(tempLength + segment.LongLength));
                }
                catch (OverflowException)
                {
                    overflow = true;
                }

                const long maxLength = 64 * 1024 * 1024;
                if (overflow || (length > maxLength))
                {
                    throw new Exception(ErrorCode.MessageTooBig, "Incoming WebSocket message payload is too large");
                }

                var fullPayload = new byte[length];

                var index = 0L;
                foreach (var payload in payloads)
                {
                    Array.Copy(payload, 0L, fullPayload, index, payload.LongLength);
                    index += payload.LongLength;
                }

                // ReSharper disable once ConditionIsAlwaysTrueOrFalse
                Debug.Assert(isBinary.HasValue);

                return new Message { IsBinary = isBinary.Value, Payload = fullPayload };
            }
        }

        public Task SendMessageAsync(byte[] payload, bool isBinary = false)
        {
            var frame = new Frame { Final = true, OpCode = isBinary ? OpCodes.Binary : OpCodes.Text, Payload = payload };
            return SendFrameAsync(frame);
        }

        #endregion

        #region frame I/O

        private async Task<Frame> ReceiveFrameAsync()
        {
            var header = await socket.ReceiveBytesAsync(2).ConfigureAwait(false);

            var final = header[0].Has(HeaderBits.Final);
            var opCode = header[0].And(HeaderBits.OpCodeMask);
            var masked = header[1].Has(HeaderBits.Masked);

            ulong length = header[1].And(HeaderBits.LengthMask);
            if (length == 126)
            {
                var lengthBytes = await socket.ReceiveBytesAsync(2).ConfigureAwait(false);
                length = lengthBytes.ToHostUInt16();
            }
            else if (length == 127)
            {
                var lengthBytes = await socket.ReceiveBytesAsync(8).ConfigureAwait(false);
                length = lengthBytes.ToHostUInt64() & 0x7FFFFFFFFFFFFFFF;
            }

            byte[] key = null;
            if (masked)
            {
                key = await socket.ReceiveBytesAsync(4).ConfigureAwait(false);
            }

            const ulong maxLength = 64 * 1024 * 1024;
            if (length > maxLength)
            {
                throw new Exception(ErrorCode.MessageTooBig, "Incoming WebSocket frame payload is too large");
            }

            // ReSharper disable once ConditionIsAlwaysTrueOrFalse
            Debug.Assert(length < long.MaxValue);

            var payload = new byte[length];

            // ReSharper disable once ConditionIsAlwaysTrueOrFalse
            if (length <= int.MaxValue)
            {
                await socket.ReceiveBytesAsync(payload, 0, Convert.ToInt32(length)).ConfigureAwait(false);
            }
            else
            {
                const int segmentLength = 1024 * 1024;
                var segment = new byte[segmentLength];

                var index = 0L;
                for (; length > segmentLength; index += segmentLength)
                {
                    await socket.ReceiveBytesAsync(segment, 0, segmentLength).ConfigureAwait(false);
                    Array.Copy(segment, 0L, payload, index, segmentLength);
                    length -= segmentLength;
                }

                // ReSharper disable once ConditionIsAlwaysTrueOrFalse
                Debug.Assert(length < int.MaxValue);

                var remainingLength = Convert.ToInt32(length);

                if (remainingLength > 0)
                {
                    await socket.ReceiveBytesAsync(segment, 0, remainingLength).ConfigureAwait(false);
                    Array.Copy(segment, 0L, payload, index, remainingLength);
                }
            }

            if (masked)
            {
                for (var index = 0L; index < payload.LongLength; index++)
                {
                    payload[index] ^= key[index % 4];
                }
            }

            return new Frame { Final = final, OpCode = opCode, Payload = payload };
        }

        private async Task SendFrameAsync(Frame frame)
        {
            using (await sendSemaphore.CreateLockScopeAsync().ConfigureAwait(false))
            {
                var header = new byte[2];
                var masked = !isServerSocket;

                header[0] = frame.OpCode.And(HeaderBits.OpCodeMask);
                if (masked)
                {
                    header[1] = HeaderBits.Masked;
                }

                if (frame.Final)
                {
                    header[0] = header[0].Or(HeaderBits.Final);
                }

                var length = frame.Payload.LongLength;
                byte[] lengthBytes = null;

                if (length <= 125)
                {
                    header[1] = header[1].Or(Convert.ToByte(length));
                }
                else if (length <= ushort.MaxValue)
                {
                    header[1] = header[1].Or(126);
                    lengthBytes = Convert.ToUInt16(length).ToNetworkBytes();
                }
                else
                {
                    header[1] = header[1].Or(127);
                    lengthBytes = Convert.ToUInt64(length).ToNetworkBytes();
                }

                await socket.SendBytesAsync(header).ConfigureAwait(false);
                if (lengthBytes is not null)
                {
                    await socket.SendBytesAsync(lengthBytes).ConfigureAwait(false);
                }

                byte[] key = null;
                if (masked)
                {
                    key = new byte[4];
                    random.NextBytes(key);

                    await socket.SendBytesAsync(key).ConfigureAwait(false);
                }

                const int segmentLength = 1024 * 1024; // must be a multiple of 4
                var segment = new byte[segmentLength];

                var index = 0L;
                for (; length > segmentLength; index += segmentLength)
                {
                    Array.Copy(frame.Payload, index, segment, 0L, segmentLength);

                    if (masked)
                    {
                        for (var segmentIndex = 0; segmentIndex < segmentLength; segmentIndex++)
                        {
                            segment[segmentIndex] ^= key[segmentIndex % 4];
                        }
                    }

                    await socket.SendBytesAsync(segment).ConfigureAwait(false);
                    length -= segmentLength;
                }

                // ReSharper disable once ConditionIsAlwaysTrueOrFalse
                Debug.Assert(length < int.MaxValue);

                var remainingLength = Convert.ToInt32(length);

                if (remainingLength > 0)
                {
                    Array.Copy(frame.Payload, index, segment, 0L, remainingLength);

                    if (masked)
                    {
                        for (var segmentIndex = 0; segmentIndex < remainingLength; segmentIndex++)
                        {
                            segment[segmentIndex] ^= key[segmentIndex % 4];
                        }
                    }

                    await socket.SendBytesAsync(segment, 0, remainingLength).ConfigureAwait(false);
                }
            }
        }

        private void SendFrameAsync(Frame frame, Action<bool> callback)
        {
            SendFrameAsync(frame).ContinueWith(task => callback(MiscHelpers.Try(static task => task.Wait(), task)));
        }

        #endregion

        #region teardown

        public void Close(ErrorCode errorCode, string message)
        {
            if (closedFlag.Set())
            {
                var payload = ((ushort)errorCode).ToNetworkBytes();
                if (!string.IsNullOrEmpty(message))
                {
                    payload = payload.Concat(Encoding.UTF8.GetBytes(message)).ToArray();
                }

                var frame = new Frame { Final = true, OpCode = OpCodes.Close, Payload = payload };
                SendFrameAsync(frame, _ =>
                {
                    socket.Close();
                    receiveSemaphore.Dispose();
                    sendSemaphore.Dispose();
                });
            }
        }

        #endregion

        #region Nested type: Message

        public sealed class Message
        {
            public bool IsBinary;
            public byte[] Payload;
        }

        #endregion

        #region Nested type: ErrorCode

        internal enum ErrorCode
        {
            NormalClosure = 1000,
            EndpointUnavailable = 1001,
            ProtocolError = 1002,
            InvalidMessageType = 1003,
            InvalidPayloadData = 1007,
            PolicyViolation = 1008,
            MessageTooBig = 1009
        }

        #endregion

        #region Nested type: Exception

        [Serializable]
        public sealed class Exception : System.Exception
        {
            public ErrorCode ErrorCode { get; private set; }

            public Exception(ErrorCode errorCode, string message)
                : base(message)
            {
                ErrorCode = errorCode;
            }
        }

        #endregion

        #region Nested type: HeaderBits

        private static class HeaderBits
        {
            public const byte Final = 0x80;
            public const byte OpCodeMask = 0x0F;
            public const byte Masked = 0x80;
            public const byte LengthMask = 0x7F;
        }

        #endregion

        #region Nested type: OpCodes

        private static class OpCodes
        {
            public const byte Continuation = 0x00;
            public const byte Text = 0x01;
            public const byte Binary = 0x02;
            public const byte Close = 0x08;
            public const byte Ping = 0x09;
            public const byte Pong = 0x0A;
        }

        #endregion

        #region Nested type: Frame

        private sealed class Frame
        {
            public bool Final;
            public byte OpCode;
            public byte[] Payload;
        }

        #endregion
    }
}
