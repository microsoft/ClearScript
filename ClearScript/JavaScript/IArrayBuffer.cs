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

namespace Microsoft.ClearScript.JavaScript
{
    /// <summary>
    /// Represents a JavaScript
    /// <see href="https://msdn.microsoft.com/en-us/library/br212474(v=vs.94).aspx">ArrayBuffer</see>.
    /// </summary>
    public interface IArrayBuffer
    {
        /// <summary>
        /// Gets the size of the <c>ArrayBuffer</c> in bytes.
        /// </summary>
        ulong Size { get; }

        /// <summary>
        /// Creates a byte array containing a copy of the <c>ArrayBuffer</c>'s contents.
        /// </summary>
        /// <returns>A new byte array containing a copy of the <c>ArrayBuffer</c>'s contents.</returns>
        byte[] GetBytes();

        /// <summary>
        /// Copies bytes from the <c>ArrayBuffer</c> into the specified byte array.
        /// </summary>
        /// <param name="offset">The offset within the <c>ArrayBuffer</c> of the first byte to copy.</param>
        /// <param name="count">The maximum number of bytes to copy.</param>
        /// <param name="destination">The byte array into which to copy the bytes.</param>
        /// <param name="destinationIndex">The index within <paramref name="destination"/> at which to store the first copied byte.</param>
        /// <returns>The number of bytes copied.</returns>
        ulong ReadBytes(ulong offset, ulong count, byte[] destination, ulong destinationIndex);

        /// <summary>
        /// Copies bytes from the specified byte array into the <c>ArrayBuffer</c>.
        /// </summary>
        /// <param name="source">The byte array from which to copy the bytes.</param>
        /// <param name="sourceIndex">The index within <paramref name="source"/> of the first byte to copy.</param>
        /// <param name="count">The maximum number of bytes to copy.</param>
        /// <param name="offset">The offset within the <c>ArrayBuffer</c> at which to store the first copied byte.</param>
        /// <returns>The number of bytes copied.</returns>
        ulong WriteBytes(byte[] source, ulong sourceIndex, ulong count, ulong offset);
    }
}
