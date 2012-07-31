// Copyright(c) Microsoft Corporation.
// This content is subject to the Microsoft Reference Source License,
// see http://www.microsoft.com/resources/sharedsource/licensingbasics/sharedsourcelicenses.mspx.
// All other rights reserved.


using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Security;
using System.Security.Cryptography;

namespace Microsoft.Health.Certificate
{
    /// <summary>
    /// 	Blob to pass to CAPI.
    /// </summary>
    /// <remarks>
    /// 	See http://msdn.microsoft.com/library/default.asp?url=/library/en-us/seccrypto/security/cryptoapi_blob.asp
    /// </remarks>
    [StructLayout(LayoutKind.Sequential)]
    internal sealed class CryptoApiBlob : IDisposable
    {
        #region Private variables
        private int cbData;
        private IntPtr pbData = IntPtr.Zero;
        #endregion 

        #region Contructors
        /// <summary>
        /// Create a null blob
        /// </summary>
        internal CryptoApiBlob()
        {
            pbData = IntPtr.Zero;
        }
    
        /// <summary>
        /// Create a new blob from the given data.
        /// </summary>
        /// <exception cref="ArgumentNullException">If data is null or of zero length.</exception>
        internal CryptoApiBlob(byte[] data)
        {
            Validator.ThrowIfArgumentNull(data, "data", "CryptoApiBlobNotNullData");
       
            AllocateBlob(data.Length);
            Marshal.Copy(data, 0, pbData, data.Length);
            return;
        }
        #endregion

        #region Public properties
        /// <summary>
        /// Gets the size of the contained blob
        /// </summary>
        internal int DataSize
        {
            get
            {
                Debug.Assert(cbData >= 0);		
                return cbData;
            }
        }
        #endregion

        #region Internal Methods

        /// <summary>
        /// Allocate space for the blob.
        /// </summary>
        /// <remarks>
        /// Will also free the blob if it was already allocated.
        /// </remarks>
        /// <param name="size">Size of the blob to allocate.</param>
        /// <exception cref="ArgumentOutOfRangeException">If size is less than zero.</exception>
        /// <exception cref="CryptographicException">If the blob could not be allocated.</exception>
        /// 
        internal void AllocateBlob(int size)
        {
            Debug.Assert(cbData >= 0);
            Debug.Assert(
                    (pbData == IntPtr.Zero && cbData == 0) ||
                    (pbData != IntPtr.Zero && cbData != 0));

            Validator.ThrowArgumentOutOfRangeIf(
                size < 0,
                "size",
                "CryptoApiBlobSizeGreaterThanZero");

            // allocate the new memory block
            IntPtr newMemory = IntPtr.Zero;
            if(size > 0)
            {
                newMemory = Marshal.AllocHGlobal(size);
                if (newMemory == IntPtr.Zero)
                {
                    throw new CryptographicException(
                        ResourceRetriever.GetResourceString(
                            "CryptoApiBlobUnableToAllocateBlob"));
                }
            }
        
            // if that succeeds then replace the old one
            IntPtr oldMemory = pbData;
            pbData = newMemory;
            cbData = size;

            // then release the old memory
            if(oldMemory != IntPtr.Zero)
                Marshal.FreeHGlobal(oldMemory);
        }
    
        /// <summary>
        /// Clear the blob, releasing held memory if necessary.
        /// </summary>
        internal void ClearBlob()
        {
            if (pbData != IntPtr.Zero)
            {
                Marshal.FreeHGlobal(pbData);
            }

            pbData = IntPtr.Zero;
            cbData = 0;
        }
        
        ///	<summary>
        ///	Create a byte array for this blob.
        ///	</summary>
        internal byte[] GetBytes()
        {
            Debug.Assert(cbData >= 0);
            Debug.Assert(
                    (pbData == IntPtr.Zero && cbData == 0) ||
                    (pbData != IntPtr.Zero && cbData != 0));

            if(pbData == IntPtr.Zero)
                return null;

            byte[] bytes = new byte[cbData];
            Marshal.Copy(pbData, bytes, 0, cbData);
            return bytes;
        }
        #endregion

        #region Dispose
        /// <summary>
        /// Clean up after the blob
        /// </summary>
        /// <param name="disposing">true if called from Dispose, false if from the finalizer</param>
        private void Dispose(bool disposing)
        {
            if(pbData != IntPtr.Zero)
            {
                Marshal.FreeHGlobal(pbData);
                pbData = IntPtr.Zero;
            }

            if(disposing)
                GC.SuppressFinalize(this);
        }
    
        /// <summary>
        /// Clean up the blob
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
        }

        /// <summary>
        /// 	Last resort blob cleanup
        /// </summary>
        ~CryptoApiBlob()
        {
            Dispose(false);
        }

        #endregion
    }
}
