using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.ClearScript.Util;

namespace Microsoft.ClearScript.V8
{
    /// <summary>
    /// Represents a V8 CPU profile.
    /// </summary>
    public class V8CpuProfile
    {
        internal V8CpuProfile()
        {
            // the help file builder (SHFB) insists on an empty constructor here
        }

        /// <summary>
        /// Gets the profile's name.
        /// </summary>
        public string Name { get; internal set; }

        /// <summary>
        /// Gets the profile's starting timestamp in microseconds.
        /// </summary>
        /// <remarks>
        /// The timestamp specifies an offset relative to an unspecified moment in the past. All
        /// timestamps within the profile are relative to the same moment.
        /// </remarks>
        public ulong StartTimestamp { get; internal set; }

        /// <summary>
        /// Gets the profile's ending timestamp in microseconds.
        /// </summary>
        /// <remarks>
        /// The timestamp specifies an offset relative to an unspecified moment in the past. All
        /// timestamps within the profile are relative to the same moment.
        /// </remarks>
        public ulong EndTimestamp { get; internal set; }

        /// <summary>
        /// Gets the root node of the profile's call tree.
        /// </summary>
        public Node RootNode { get; internal set; }

        /// <summary>
        /// Gets the profile's sample collection.
        /// </summary>
        /// <remarks>
        /// This property returns <c>null</c> if the profile contains no samples.
        /// </remarks>
        public IReadOnlyList<Sample> Samples { get; internal set; }

        /// <summary>
        /// Returns a JSON representation of the profile.
        /// </summary>
        /// <remarks>
        /// See the
        /// <see href="https://github.com/v8/v8/blob/master/src/inspector/js_protocol-1.3.json">V8 Inspector JSON Protocol</see>
        /// for schema details.
        /// </remarks>
        /// <returns>A JSON representation of the profile in V8 Inspector format.</returns>
        public string ToJson()
        {
            using (var writer = new StringWriter())
            {
                WriteJson(writer);
                return writer.ToString();
            }
        }

        /// <summary>
        /// Writes a JSON representation of the profile to the given text writer.
        /// </summary>
        /// <param name="writer">The text writer to which to write the profile.</param>
        /// <remarks>
        /// See the
        /// <see href="https://github.com/v8/v8/blob/master/src/inspector/js_protocol-1.3.json">V8 Inspector JSON Protocol</see>
        /// for schema details.
        /// </remarks>
        public void WriteJson(TextWriter writer)
        {
            // V8 Inspector JSON Protocol 1.3: https://github.com/v8/v8/blob/master/src/inspector/js_protocol-1.3.json

            writer.Write('{');
            {
                WriteNodesJson(writer);
                writer.Write(",\"startTime\":" + StartTimestamp);
                writer.Write(",\"endTime\":" + EndTimestamp);
                WriteSamplesJson(writer);
                WriteTimeDeltasJson(writer);
            }
            writer.Write('}');
        }

        internal Node FindNode(ulong nodeId)
        {
            return (RootNode != null) ? RootNode.FindNode(nodeId) : null;
        }

        private void WriteNodesJson(TextWriter writer)
        {
            // V8 Inspector JSON Protocol 1.3: https://github.com/v8/v8/blob/master/src/inspector/js_protocol-1.3.json

            writer.Write("\"nodes\":[");
            {
                if (RootNode != null)
                {
                    var queue = new Queue<Node>();
                    RootNode.WriteJson(writer, queue);
                    while (queue.Count > 0)
                    {
                        writer.Write(',');
                        queue.Dequeue().WriteJson(writer, queue);
                    }
                }
            }
            writer.Write(']');
        }

        private void WriteSamplesJson(TextWriter writer)
        {
            // V8 Inspector JSON Protocol 1.3: https://github.com/v8/v8/blob/master/src/inspector/js_protocol-1.3.json

            if ((Samples != null) && (Samples.Count > 0))
            {
                writer.Write(",\"samples\":[{0}]", string.Join(",", Samples.Select(sample => sample.Node.NodeId)));
            }
        }

        private void WriteTimeDeltasJson(TextWriter writer)
        {
            // V8 Inspector JSON Protocol 1.3: https://github.com/v8/v8/blob/master/src/inspector/js_protocol-1.3.json

            if ((Samples != null) && (Samples.Count > 0))
            {
                writer.Write(",\"timeDeltas\":[");
                {
                    writer.Write(Samples[0].Timestamp - StartTimestamp);
                    for (var index = 1; index < Samples.Count; ++index)
                    {
                        writer.Write(',');
                        writer.Write(Samples[index].Timestamp - Samples[index - 1].Timestamp);
                    }
                }
                writer.Write(']');
            }
        }

        #region Nested type: Node

        /// <summary>
        /// Represents a node in a V8 CPU profile's call tree.
        /// </summary>
        public sealed class Node
        {
            internal Node()
            {
                // the help file builder (SHFB) insists on an empty constructor here
            }

            /// <summary>
            /// Gets the node's numeric identifier.
            /// </summary>
            /// <remarks>
            /// This value is unique within the profile.
            /// </remarks>
            public ulong NodeId { get; set; }

            /// <summary>
            /// Gets the numeric identifier of the document containing the node's script function.
            /// </summary>
            public long ScriptId { get; set; }

            /// <summary>
            /// Gets the name or URL of the document containing the node's script function.
            /// </summary>
            public string ScriptName { get; internal set; }

            /// <summary>
            /// Gets the name of the node's script function.
            /// </summary>
            public string FunctionName { get; internal set; }

            /// <summary>
            /// Gets the 1-based line number of the start of the node's script function.
            /// </summary>
            /// <remarks>
            /// A value of zero indicates that no line number is available.
            /// </remarks>
            /// <seealso cref="V8CpuProfileFlags"/>
            public long LineNumber { get; internal set; }

            /// <summary>
            /// Gets the 1-based column number of the start of the node's script function.
            /// </summary>
            /// <remarks>
            /// A value of zero indicates that no column number is available.
            /// </remarks>
            /// <seealso cref="V8CpuProfileFlags"/>
            public long ColumnNumber { get; internal set; }

            /// <summary>
            /// Gets the node's hit count.
            /// </summary>
            /// <remarks>
            /// This value represents the number of times the CPU profiler observed the node's
            /// script function at the top of the call stack.
            /// </remarks>
            public ulong HitCount { get; internal set; }

            /// <summary>
            /// Gets an optional string describing the reason why the node's script function was not optimized.
            /// </summary>
            public string BailoutReason { get; internal set; }

            /// <summary>
            /// Gets the node's hit line collection.
            /// </summary>
            /// <remarks>
            /// This property returns <c>null</c> if the node contains no hit lines.
            /// </remarks>
            public IReadOnlyList<HitLine> HitLines { get; internal set; }

            /// <summary>
            /// Gets the node's child node collection.
            /// </summary>
            /// <remarks>
            /// This property returns <c>null</c> if the node has no child nodes.
            /// </remarks>
            public IReadOnlyList<Node> ChildNodes { get; internal set; }

            internal Node FindNode(ulong nodeId)
            {
                if (NodeId == nodeId)
                {
                    return this;
                }

                if (ChildNodes != null)
                {
                    foreach (var childNode in ChildNodes)
                    {
                        var node = childNode.FindNode(nodeId);
                        if (node != null)
                        {
                            return node;
                        }
                    }
                }

                return null;
            }

            internal void WriteJson(TextWriter writer, Queue<Node> queue)
            {
                // V8 Inspector JSON Protocol 1.3: https://github.com/v8/v8/blob/master/src/inspector/js_protocol-1.3.json

                writer.Write('{');
                {
                    writer.Write("\"id\":" + NodeId);

                    WriteCallFrameJson(writer);
                    writer.Write(",\"hitCount\":" + HitCount);
                    WriteChildrenJson(writer, queue);

                    if (!string.IsNullOrEmpty(BailoutReason))
                    {
                        writer.Write(",\"deoptReason\":" + BailoutReason.ToQuotedJson());
                    }

                    WritePositionTicksJson(writer);
                }
                writer.Write('}');
            }

            private void WriteCallFrameJson(TextWriter writer)
            {
                // V8 Inspector JSON Protocol 1.3: https://github.com/v8/v8/blob/master/src/inspector/js_protocol-1.3.json

                writer.Write(",\"callFrame\":{");
                {
                    writer.Write("\"functionName\":" + (FunctionName ?? string.Empty).ToQuotedJson());
                    writer.Write(",\"scriptId\":" + ScriptId);
                    writer.Write(",\"url\":" + (ScriptName ?? string.Empty).ToQuotedJson());
                    writer.Write(",\"lineNumber\":" + (LineNumber - 1));
                    writer.Write(",\"columnNumber\":" + (ColumnNumber - 1));
                }
                writer.Write('}');
            }

            private void WriteChildrenJson(TextWriter writer, Queue<Node> queue)
            {
                // V8 Inspector JSON Protocol 1.3: https://github.com/v8/v8/blob/master/src/inspector/js_protocol-1.3.json

                if ((ChildNodes != null) && (ChildNodes.Count > 0))
                {
                    writer.Write(",\"children\":[{0}]", string.Join(",", ChildNodes.Select(node => node.NodeId)));
                    ChildNodes.ForEach(queue.Enqueue);
                }
            }

            private void WritePositionTicksJson(TextWriter writer)
            {
                // V8 Inspector JSON Protocol 1.3: https://github.com/v8/v8/blob/master/src/inspector/js_protocol-1.3.json

                if ((HitLines != null) && (HitLines.Count > 0))
                {
                    writer.Write(",\"positionTicks\":[");
                    {
                        HitLines[0].WriteJson(writer);
                        for (var index = 1; index < HitLines.Count; ++index)
                        {
                            writer.Write(',');
                            HitLines[index].WriteJson(writer);
                        }
                    }
                    writer.Write(']');
                }
            }

            #region Nested type: HitLine

            /// <summary>
            /// Represents a script line observed by the V8 CPU profiler.
            /// </summary>
            public struct HitLine
            {
                /// <summary>
                /// Gets the 1-based line number.
                /// </summary>
                public long LineNumber;

                /// <summary>
                /// Gets the hit count for the script line.
                /// </summary>
                /// <remarks>
                /// This value represents the number of times the CPU profiler observed the current
                /// script line at the top of the call stack.
                /// </remarks>
                public ulong HitCount;

                internal void WriteJson(TextWriter writer)
                {
                    // V8 Inspector JSON Protocol 1.3: https://github.com/v8/v8/blob/master/src/inspector/js_protocol-1.3.json

                    writer.Write("{{\"line\":{0},\"ticks\":{1}}}", LineNumber, HitCount);
                }
            }

            #endregion
        }

        #endregion

        #region Nested type: Sample

        /// <summary>
        /// Represents a V8 CPU profile sample.
        /// </summary>
        public sealed class Sample
        {
            internal Sample()
            {
                // the help file builder (SHFB) insists on an empty constructor here
            }

            /// <summary>
            /// Gets the sample's node within the profile's call tree.
            /// </summary>
            public Node Node { get; internal set; }

            /// <summary>
            /// Gets the sample's timestamp in microseconds.
            /// </summary>
            /// <remarks>
            /// The timestamp specifies an offset relative to an unspecified moment in the past. All
            /// timestamps within the profile are relative to the same moment.
            /// </remarks>
            public ulong Timestamp { get; internal set; }
        }

        #endregion
    }
}
