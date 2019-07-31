// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

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

        private sealed class DebugDocument : IDebugDocumentInfo, IDebugDocumentProvider, IDebugDocument, IDebugDocumentText
        {
            private readonly WindowsScriptEngine engine;
            private readonly UIntPtr sourceContext;
            private readonly UniqueDocumentInfo documentInfo;
            private readonly string code;

            private string[] codeLines;
            private IDebugApplicationNode node;

            public DebugDocument(WindowsScriptEngine engine, UIntPtr sourceContext, UniqueDocumentInfo documentInfo, string code)
            {
                this.engine = engine;
                this.sourceContext = sourceContext;
                this.documentInfo = documentInfo;
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

            private string GetFileName()
            {
                return Path.HasExtension(documentInfo.Name) ? documentInfo.Name : Path.ChangeExtension(documentInfo.Name, engine.FileNameExtension);
            }

            private string GetUrl()
            {
                if (documentInfo.Uri != null)
                {
                    return documentInfo.Uri.IsAbsoluteUri ? documentInfo.Uri.AbsoluteUri : documentInfo.Uri.ToString();
                }

                return MiscHelpers.FormatInvariant("{0}/{1}", engine.Name, GetFileName());
            }

            private bool TryGetSourceMapUrl(out string sourceMapUrl)
            {
                if (documentInfo.SourceMapUri != null)
                {
                    sourceMapUrl = documentInfo.SourceMapUri.IsAbsoluteUri ? documentInfo.SourceMapUri.AbsoluteUri : documentInfo.SourceMapUri.ToString();
                    return true;
                }

                sourceMapUrl = null;
                return false;
            }

            #region IDebugDocumentInfo implementation

            public uint GetName(DocumentNameType type, out string documentName)
            {
                switch (type)
                {
                    case DocumentNameType.Title:
                        documentName = documentInfo.Name;
                        return RawCOMHelpers.HResult.S_OK;

                    case DocumentNameType.FileTail:
                        documentName = GetFileName();
                        return RawCOMHelpers.HResult.S_OK;

                    case DocumentNameType.URL:
                        documentName = GetUrl();
                        return RawCOMHelpers.HResult.S_OK;

                    case DocumentNameType.AppNode:
                    case DocumentNameType.UniqueTitle:
                        documentName = documentInfo.UniqueName;
                        return RawCOMHelpers.HResult.S_OK;

                    case DocumentNameType.SourceMapURL:
                        return TryGetSourceMapUrl(out documentName) ? RawCOMHelpers.HResult.S_OK : RawCOMHelpers.HResult.E_FAIL.ToUnsigned();
                }

                documentName = null;
                return RawCOMHelpers.HResult.E_FAIL.ToUnsigned();
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

        private sealed class DebugDocumentMap : Dictionary<UIntPtr, DebugDocument>
        {
        }

        #endregion

        #region Nested type: DebugDocumentContext

        private sealed class DebugDocumentContext : IDebugDocumentContext
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
