// 
// Copyright (c) Microsoft Corporation. All rights reserved.
// 
// Microsoft Public License (MS-PL)
// 
// This license governs use of the accompanying software. If you use the
// software, you accept this license. If you do not accept the license, do not
// use the software.
// 
// 1. Definitions
// 
//   The terms "reproduce," "reproduction," "derivative works," and
//   "distribution" have the same meaning here as under U.S. copyright law. A
//   "contribution" is the original software, or any additions or changes to
//   the software. A "contributor" is any person that distributes its
//   contribution under this license. "Licensed patents" are a contributor's
//   patent claims that read directly on its contribution.
// 
// 2. Grant of Rights
// 
//   (A) Copyright Grant- Subject to the terms of this license, including the
//       license conditions and limitations in section 3, each contributor
//       grants you a non-exclusive, worldwide, royalty-free copyright license
//       to reproduce its contribution, prepare derivative works of its
//       contribution, and distribute its contribution or any derivative works
//       that you create.
// 
//   (B) Patent Grant- Subject to the terms of this license, including the
//       license conditions and limitations in section 3, each contributor
//       grants you a non-exclusive, worldwide, royalty-free license under its
//       licensed patents to make, have made, use, sell, offer for sale,
//       import, and/or otherwise dispose of its contribution in the software
//       or derivative works of the contribution in the software.
// 
// 3. Conditions and Limitations
// 
//   (A) No Trademark License- This license does not grant you rights to use
//       any contributors' name, logo, or trademarks.
// 
//   (B) If you bring a patent claim against any contributor over patents that
//       you claim are infringed by the software, your patent license from such
//       contributor to the software ends automatically.
// 
//   (C) If you distribute any portion of the software, you must retain all
//       copyright, patent, trademark, and attribution notices that are present
//       in the software.
// 
//   (D) If you distribute any portion of the software in source code form, you
//       may do so only under this license by including a complete copy of this
//       license with your distribution. If you distribute any portion of the
//       software in compiled or object code form, you may only do so under a
//       license that complies with this license.
// 
//   (E) The software is licensed "as-is." You bear the risk of using it. The
//       contributors give no express warranties, guarantees or conditions. You
//       may have additional consumer rights under your local laws which this
//       license cannot change. To the extent permitted under your local laws,
//       the contributors exclude the implied warranties of merchantability,
//       fitness for a particular purpose and non-infringement.
//       

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using Microsoft.ClearScript.Util;

namespace Microsoft.ClearScript.Windows
{
    public abstract partial class WindowsScriptEngine
    {
        #region Nested type: DebugDocument

        private class DebugDocument : IDebugDocumentInfo, IDebugDocumentProvider, IDebugDocument, IDebugDocumentText
        {
            private readonly WindowsScriptEngine engine;
            private readonly UIntPtr sourceContext;
            private readonly string name;
            private readonly string code;

            private string[] codeLines;
            private IDebugApplicationNode node;

            public DebugDocument(WindowsScriptEngine engine, UIntPtr sourceContext, string name, string code, bool transient)
            {
                this.engine = engine;
                this.sourceContext = sourceContext;
                this.name = name + (transient ? " [temp]" : "");
                this.code = code;
                Initialize();
            }

            private void Initialize()
            {
                codeLines = code.Split('\n').Select(line => line + '\n').ToArray();

                engine.debugApplication.CreateApplicationNode(out node);
                node.SetDocumentProvider(this);

                IDebugApplicationNode rootNode;
                engine.debugApplication.GetRootNode(out rootNode);
                node.Attach(rootNode);
            }

            public string Code
            {
                get { return code; }
            }

            public void Close()
            {
                node.Detach();
                node.Close();
                node = null;
            }

            #region IDebugDocumentInfo implementation

            public void GetName(DocumentNameType type, out string documentName)
            {
                switch (type)
                {
                    case DocumentNameType.URL:
                        var fullName = Path.HasExtension(name) ? name : Path.ChangeExtension(name, engine.FileNameExtension);
                        documentName = MiscHelpers.FormatInvariant("{0}/{1}", engine.Name, fullName);
                        break;

                    default:
                        documentName = name;
                        break;
                }
            }

            public void GetDocumentClassId(out Guid clsid)
            {
                clsid = Guid.Empty;
            }

            #endregion

            #region IDebugDocumentProvider implementation

            public void GetDocument(out IDebugDocument document)
            {
                document = this;
            }

            #endregion

            #region IDebugDocument implementation

            #endregion

            #region IDebugDocumentText implementation

            public void GetDocumentAttributes(out TextDocAttrs attrs)
            {
                attrs = TextDocAttrs.ReadOnly;
            }

            public void GetSize(out uint numLines, out uint length)
            {
                numLines = (uint)codeLines.Length;
                length = (uint)code.Length;
            }

            public void GetPositionOfLine(uint lineNumber, out uint position)
            {
                if (lineNumber >= codeLines.Length)
                {
                    throw new ArgumentOutOfRangeException("lineNumber");
                }

                position = 0;
                for (var index = 0; index < lineNumber; index++)
                {
                    position += (uint)codeLines[index].Length;
                }
            }

            public void GetLineOfPosition(uint position, out uint lineNumber, out uint offsetInLine)
            {
                if (position >= code.Length)
                {
                    throw new ArgumentOutOfRangeException("position");
                }

                offsetInLine = position;
                for (lineNumber = 0; lineNumber < codeLines.Length; lineNumber++)
                {
                    var lineLength = (uint)codeLines[lineNumber].Length;
                    if (offsetInLine < lineLength)
                    {
                        break;
                    }

                    offsetInLine -= lineLength;
                }
            }

            public void GetText(uint position, IntPtr pChars, IntPtr pAttrs, ref uint length, uint maxChars)
            {
                var codeLength = (uint)code.Length;
                if (position < codeLength)
                {
                    length = Math.Min(codeLength - position, maxChars);

                    if (pChars != IntPtr.Zero)
                    {
                        Marshal.Copy(code.ToCharArray(), (int)position, pChars, (int)length);
                    }

                    if (pAttrs != IntPtr.Zero)
                    {
                        var attrs = Enumerable.Repeat((short)SourceTextAttrs.None, (int)length).ToArray();
                        Marshal.Copy(attrs, 0, pAttrs, (int)length);
                    }
                }
            }

            public void GetPositionOfContext(IDebugDocumentContext context, out uint position, out uint length)
            {
                var documentContext = (DebugDocumentContext)context;
                position = documentContext.Position;
                length = documentContext.Length;
            }

            public void GetContextOfPosition(uint position, uint length, out IDebugDocumentContext context)
            {
                IEnumDebugCodeContexts enumCodeContexts;
                engine.activeScript.EnumCodeContextsOfPosition(sourceContext, position, length, out enumCodeContexts);
                context = new DebugDocumentContext(this, position, length, enumCodeContexts);
            }

            #endregion
        }

        #endregion

        #region Nested type: DebugDocumentMap

        private class DebugDocumentMap : Dictionary<UIntPtr, DebugDocument>
        {
        }

        #endregion

        #region Nested type: DebugDocumentContext

        private class DebugDocumentContext : IDebugDocumentContext
        {
            private readonly DebugDocument document;
            private readonly uint position;
            private readonly uint length;
            private readonly IEnumDebugCodeContexts enumCodeContexts;

            public DebugDocumentContext(DebugDocument document, uint position, uint length, IEnumDebugCodeContexts enumCodeContexts)
            {
                this.document = document;
                this.position = position;
                this.length = length;
                this.enumCodeContexts = enumCodeContexts;
            }

            public uint Position
            {
                get { return position; }
            }

            public uint Length
            {
                get { return length; }
            }

            #region IDebugDocumentContext implementation

            public void GetDocument(out IDebugDocument debugDocument)
            {
                debugDocument = document;
            }

            public void EnumCodeContexts(out IEnumDebugCodeContexts enumContexts)
            {
                enumCodeContexts.Clone(out enumContexts);
            }

            #endregion
        }

        #endregion
    }
}
