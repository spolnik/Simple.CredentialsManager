using System;
using System.Runtime.InteropServices;
using System.Text;
using Microsoft.Win32.SafeHandles;

namespace Simple.CredentialManager
{
    /// <summary>
    ///     Wrapper for advapi32.dll library.
    ///     Advanced Services
    ///     Provide access to functionality additional to the kernel.
    ///     Included are things like the Windows registry, shutdown/restart the system (or abort),
    ///     start/stop/create a Windows service, manage user accounts.
    ///     These functions reside in advapi32.dll on 32-bit Windows.
    /// </summary>
    public class NativeMethods
    {
        /// <summary>
        ///     The CredRead function reads a credential from the user's credential set.
        ///     The credential set used is the one associated with the logon session of the current token.
        ///     The token must not have the user's SID disabled.
        /// </summary>
        /// <remarks>
        ///     If the value of the Type member of the CREDENTIAL structure specified by the Credential parameter is
        ///     CRED_TYPE_DOMAIN_EXTENDED, a namespace must be specified in the target name. This function can return only one
        ///     credential of the specified type.
        /// </remarks>
        /// <param name="target">Pointer to a null-terminated string that contains the name of the credential to read.</param>
        /// <param name="type">Type of the credential to read. Type must be one of the CRED_TYPE_* defined types.</param>
        /// <param name="reservedFlag">Currently reserved and must be zero.</param>
        /// <param name="credentialPtr">
        ///     Pointer to a single allocated block buffer to return the credential.
        ///     Any pointers contained within the buffer are pointers to locations within this single allocated block.
        ///     The single returned buffer must be freed by calling CredFree.
        /// </param>
        /// <returns>The function returns TRUE on success and FALSE on failure.</returns>
        [DllImport("Advapi32.dll", EntryPoint = "CredReadW", CharSet = CharSet.Unicode, SetLastError = true)]
        internal static extern bool CredRead(string target, CredentialType type, int reservedFlag,
            out IntPtr credentialPtr);

        /// <summary>
        ///     The CredWrite function creates a new credential or modifies an existing credential in the user's credential set.
        ///     The new credential is associated with the logon session of the current token.
        ///     The token must not have the user's security identifier (SID) disabled.
        /// </summary>
        /// <remarks>
        ///     This function creates a credential if a credential with the specified TargetName and Type does not exist. If a
        ///     credential with the specified TargetName and Type exists, the new specified credential replaces the existing one.
        ///     When this function writes a CRED_TYPE_CERTIFICATE credential, the Credential->CredentialBlob member specifies the
        ///     PIN protecting the private key of the certificate specified by the Credential->UserName member. The credential
        ///     manager does not maintain the PIN. Rather, the PIN is passed to the cryptographic service provider (CSP) indicated
        ///     on the certificate for later use by the CSP and the authentication packages. The CSP defines the lifetime of the
        ///     PIN. Most CSPs flush the PIN when the smart card removal from the smart card reader.
        ///     If the value of the Type member of the CREDENTIAL structure specified by the Credential parameter is
        ///     CRED_TYPE_DOMAIN_EXTENDED, a namespace must be specified in the target name. This function does not support writing
        ///     to target names that contain wildcards.
        /// </remarks>
        /// <param name="userCredential">A pointer to the CREDENTIAL structure to be written.</param>
        /// <param name="flags">Flags that control the function's operation. The following flag is defined.</param>
        /// <returns>If the function succeeds, the function returns TRUE, if the function fails, it returns FALSE. </returns>
        [DllImport("Advapi32.dll", EntryPoint = "CredWriteW", CharSet = CharSet.Unicode, SetLastError = true)]
        internal static extern bool CredWrite([In] ref CREDENTIAL userCredential, [In] UInt32 flags);

        /// <summary>
        ///     The CredFree function frees a buffer returned by any of the credentials management functions.
        /// </summary>
        /// <param name="cred">Pointer to the buffer to be freed.</param>
        [DllImport("Advapi32.dll", EntryPoint = "CredFree", SetLastError = true)]
        internal static extern void CredFree([In] IntPtr cred);

        /// <summary>
        ///     The CredDelete function deletes a credential from the user's credential set.
        ///     The credential set used is the one associated with the logon session of the current token.
        ///     The token must not have the user's SID disabled.
        /// </summary>
        /// <param name="target">Pointer to a null-terminated string that contains the name of the credential to delete.</param>
        /// <param name="type">
        ///     Type of the credential to delete. Must be one of the CRED_TYPE_* defined types.
        ///     For a list of the defined types, see the Type member of the CREDENTIAL structure.
        ///     If the value of this parameter is CRED_TYPE_DOMAIN_EXTENDED,
        ///     this function can delete a credential that specifies a user name when there are multiple credentials for the same
        ///     target. The value of the TargetName parameter must specify the user name as Target|UserName.
        /// </param>
        /// <param name="flags">Reserved and must be zero.</param>
        /// <returns>The function returns TRUE on success and FALSE on failure.</returns>
        [DllImport("Advapi32.dll", EntryPoint = "CredDeleteW", CharSet = CharSet.Unicode)]
        internal static extern bool CredDelete(StringBuilder target, CredentialType type, int flags);

        /// <summary>
        /// The CREDENTIAL structure contains an individual credential.
        /// 
        /// See CREDENTIAL structure <see href="http://msdn.microsoft.com/en-us/library/windows/desktop/aa374788(v=vs.85).aspx">documentation.</see>
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        internal struct CREDENTIAL
        {
            public int Flags;
            public int Type;
            [MarshalAs(UnmanagedType.LPWStr)] public string TargetName;
            [MarshalAs(UnmanagedType.LPWStr)] public string Comment;
            public long LastWritten;
            public int CredentialBlobSize;
            public IntPtr CredentialBlob;
            public int Persist;
            public int AttributeCount;
            public IntPtr Attributes;
            [MarshalAs(UnmanagedType.LPWStr)] public string TargetAlias;
            [MarshalAs(UnmanagedType.LPWStr)] public string UserName;
        }

        internal sealed class CriticalCredentialHandle : CriticalHandleZeroOrMinusOneIsInvalid
        {
            // Set the handle.
            internal CriticalCredentialHandle(IntPtr preexistingHandle)
            {
                SetHandle(preexistingHandle);
            }

            internal CREDENTIAL GetCredential()
            {
                if (!IsInvalid)
                {
                    // Get the Credential from the mem location
                    return (CREDENTIAL) Marshal.PtrToStructure(handle, typeof (CREDENTIAL));
                }

                throw new InvalidOperationException("Invalid CriticalHandle!");
            }

            // Perform any specific actions to release the handle in the ReleaseHandle method.
            // Often, you need to use P/Invoke to make a call into the Win32 API to release the 
            // handle. In this case, however, we can use the Marshal class to release the unmanaged memory.
            protected override bool ReleaseHandle()
            {
                // If the handle was set, free it. Return success.
                if (!IsInvalid)
                {
                    // NOTE: We should also ZERO out the memory allocated to the handle, before free'ing it
                    // so there are no traces of the sensitive data left in memory.
                    CredFree(handle);
                    // Mark the handle as invalid for future users.
                    SetHandleAsInvalid();
                    return true;
                }
                // Return false. 
                return false;
            }
        }
    }
}