using System;
using System.Collections.Concurrent;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Threading;

namespace Microsoft.ClearScript.Test
{
    /// <summary>
    /// An implementation of <see cref="SynchronizationContext"/> that dispatches
    /// the callbacks on a Windows message queue. It maintains thread affinity,
    /// and is suitable for use with COM objects like MSXML2.XMLHTTP XMLHttpRequest
    /// that implements async by posting to the message queue.
    /// </summary>
    public class MessageQueueSynchronizationContext : SynchronizationContext, IDisposable
    {
        private const uint WM_USER = 0x0400;
        private const uint WM_STOP = WM_USER + 0;
        private const uint WM_MESSAGE = WM_USER + 1;

        private readonly ConcurrentQueue<Message> messages = new ConcurrentQueue<Message>();
        private readonly InvisibleWindow invisibleWindow = new InvisibleWindow();
        private readonly AutoResetEvent runEvent = new AutoResetEvent(true);
        private bool disposed;

        public override SynchronizationContext CreateCopy() => this;

        public override void Post(SendOrPostCallback d, object state)
        {
            if (d == null)
            {
                throw new ArgumentNullException(nameof(d));
            }

            messages.Enqueue(new Message(d, state));
            PostMessage(WM_MESSAGE, IntPtr.Zero, IntPtr.Zero);
        }

        public override void Send(SendOrPostCallback d, object state)
        {
            if (d == null)
            {
                throw new ArgumentNullException(nameof(d));
            }

            var ev = new ManualResetEventSlim(false);
            var message = new Message(d, state, ev);

            try
            {
                messages.Enqueue(message);
                PostMessage(WM_MESSAGE, IntPtr.Zero, IntPtr.Zero);
                ev.Wait();
            }
            finally
            {
                ev.Dispose();
            }

            if (message.Exception != null)
            {
                throw new InvalidOperationException("An error occured during the callback invocation", message.Exception);
            }
        }

        public void Run()
        {
            runEvent.Reset();

            int ret;

            // retrieve messages for any window that belongs to the current thread,
            // and any messages on the current thread's message queue whose hwnd value is NULL
            while ((ret = NativeMethods.GetMessage(out NativeMethods.MSG msg, IntPtr.Zero, 0, 0)) != 0)
            {
                if (ret == -1)
                {
                    // an error occurred
                    continue;
                }

                switch (msg.message)
                {
                    case WM_STOP:
                        NativeMethods.PostQuitMessage(0);
                        break;
                    case WM_MESSAGE:
                        ProcessMessage();
                        break;
                    default:
                        NativeMethods.TranslateMessage(ref msg);
                        NativeMethods.DispatchMessage(ref msg);
                        break;
                }
            }

            runEvent.Set();
        }

        private void ProcessMessage()
        {
            if (!messages.TryDequeue(out var message))
            {
                throw new InvalidOperationException("Failed to dequeue message");
            }

            var previousContext = Current;
            SetSynchronizationContext(this);
            try
            {
                message.Run();
            }
            finally
            {
                SetSynchronizationContext(previousContext);
            }
        }

        public void Stop()
        {
            // tell the message pump to stop
            PostMessage(WM_STOP, IntPtr.Zero, IntPtr.Zero);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {

                    Stop();

                    // wait for the pump to stop
                    runEvent.WaitOne();

                    invisibleWindow.Dispose();
                }

                disposed = true;
            }
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        private void PostMessage(uint msg, IntPtr wParam, IntPtr lParam)
        {
            if (!NativeMethods.PostMessage(invisibleWindow.Hwnd, msg, wParam, lParam))
            {
                throw new Win32Exception(Marshal.GetLastWin32Error(), "Failed to post message");
            }
        }

        private class Message
        {
            private readonly SendOrPostCallback Callback;
            private readonly object State;
            private readonly ManualResetEventSlim FinishedEvent;

            public Message(
                SendOrPostCallback callback,
                object state,
                ManualResetEventSlim finishedEvent)
            {
                Callback = callback;
                State = state;
                FinishedEvent = finishedEvent;
            }

            public Message(SendOrPostCallback callback, object state)
                : this(callback, state, null)
            {
            }

            public Exception Exception { get; private set; }

            public void Run()
            {
                try
                {
                    Callback(State);
                }
#pragma warning disable CA1031 // Do not catch general exception types
                catch (Exception exception)
                {
                    Exception = exception;
                }
#pragma warning restore CA1031 // Do not catch general exception types
                finally
                {
                    FinishedEvent?.Set();
                }
            }
        }

        private static class NativeMethods
        {
            [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
            public struct WNDCLASS
            {
                public uint style;
                public IntPtr lpfnWndProc;
                public int cbClsExtra;
                public int cbWndExtra;
                public IntPtr hInstance;
                public IntPtr hIcon;
                public IntPtr hCursor;
                public IntPtr hbrBackground;
                [MarshalAs(UnmanagedType.LPWStr)]
                public string lpszMenuName;
                [MarshalAs(UnmanagedType.LPWStr)]
                public string lpszClassName;
            }

            [StructLayout(LayoutKind.Sequential)]
            public struct MSG
            {
                public IntPtr hwnd;
                public uint message;
                public IntPtr wParam;
                public IntPtr lParam;
                public uint time;
            }

            [DllImport("user32.dll", SetLastError = true)]
            public static extern UInt16 RegisterClassW([In] ref WNDCLASS lpWndClass);

            [DllImport("user32.dll", SetLastError = true)]
            public static extern IntPtr CreateWindowExW(
               uint dwExStyle,
               [MarshalAs(UnmanagedType.LPWStr)] string lpClassName,
               [MarshalAs(UnmanagedType.LPWStr)] string lpWindowName,
               uint dwStyle,
               int x,
               int y,
               int nWidth,
               int nHeight,
               IntPtr hWndParent,
               IntPtr hMenu,
               IntPtr hInstance,
               IntPtr lpParam
            );

            [DllImport("user32.dll", SetLastError = true)]
            public static extern IntPtr DefWindowProcW(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);

            [DllImport("user32.dll", SetLastError = true)]
            public static extern bool DestroyWindow(IntPtr hWnd);

            [DllImport("user32.dll")]
            public static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

            [DllImport("user32.dll")]
            public static extern bool UpdateWindow(IntPtr hWnd);

            [return: MarshalAs(UnmanagedType.Bool)]
            [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
            public static extern bool PostMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);

            [DllImport("user32.dll")]
            public static extern int GetMessage(out MSG lpMsg, IntPtr hWnd, uint wMsgFilterMin, uint wMsgFilterMax);

            [DllImport("user32.dll")]
            public static extern bool TranslateMessage([In] ref MSG lpMsg);

            [DllImport("user32.dll")]
            public static extern IntPtr DispatchMessage([In] ref MSG lpmsg);

            [DllImport("user32.dll")]
            public static extern void PostQuitMessage(int nExitCode);
        }

        private sealed class InvisibleWindow : IDisposable
        {
            delegate IntPtr WndProc(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);

            private const int ERROR_CLASS_ALREADY_EXISTS = 1410;
            private const string className = "InvisibleWindow";

            private static readonly WndProc wndProcDelegate;

            private bool disposed;

            static InvisibleWindow()
            {
                wndProcDelegate = CustomWndProc;

                var windClass = new NativeMethods.WNDCLASS
                {
                    lpszClassName = className,
                    lpfnWndProc = Marshal.GetFunctionPointerForDelegate(wndProcDelegate)
                };

                ushort classAtom = NativeMethods.RegisterClassW(ref windClass);

                int lastError = Marshal.GetLastWin32Error();

                if (classAtom == 0 && lastError != ERROR_CLASS_ALREADY_EXISTS)
                {
                    throw new Win32Exception(lastError, "Could not register window class");
                }
            }

            public InvisibleWindow()
            {
                var windClass = new NativeMethods.WNDCLASS
                {
                    lpszClassName = className,
                    lpfnWndProc = Marshal.GetFunctionPointerForDelegate(wndProcDelegate)
                };

                ushort classAtom = NativeMethods.RegisterClassW(ref windClass);

                int lastError = Marshal.GetLastWin32Error();

                if (classAtom == 0 && lastError != ERROR_CLASS_ALREADY_EXISTS)
                {
                    throw new Win32Exception(lastError, "Could not register window class");
                }

                IntPtr HWND_MESSAGE = new IntPtr(-3);

                Hwnd = NativeMethods.CreateWindowExW(
                    0,
                    className,
                    className,
                    0, 0, 0, 0, 0,
                    HWND_MESSAGE, // message-only window
                    IntPtr.Zero,
                    IntPtr.Zero,
                    IntPtr.Zero
                );

                lastError = Marshal.GetLastWin32Error();
                if (lastError != 0)
                {
                    throw new Win32Exception(lastError, "Could not create the window");
                }

                const int ShowWindowCommandsNormal = 1;
                NativeMethods.ShowWindow(Hwnd, ShowWindowCommandsNormal);
                NativeMethods.UpdateWindow(Hwnd);
            }

            public IntPtr Hwnd { get; private set; }

            public void Dispose()
            {
                Dispose(true);
                GC.SuppressFinalize(this);
            }

            private void Dispose(bool disposing)
            {
                if (!disposed)
                {
                    disposed = true;

                    if (disposing)
                    {
                        // Dispose managed resources
                    }

                    // Dispose unmanaged resources
                    if (Hwnd != IntPtr.Zero)
                    {
                        //NativeMethods.DestroyWindow(Hwnd);
                        Hwnd = IntPtr.Zero;
                    }
                }
            }

            private static IntPtr CustomWndProc(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam)
            {
                return NativeMethods.DefWindowProcW(hWnd, msg, wParam, lParam);
            }
        }
    }
}
