﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Security;
using static Microsoft.Common.Core.NativeMethods;

namespace Microsoft.Common.Core.Security {
    public static class SecurityUtilities {
        public static IntPtr CreatePasswordBuffer() {
            return Marshal.AllocCoTaskMem(CREDUI_MAX_PASSWORD_LENGTH);
        }

        public static SecureString ToSecureString(this string s) {
            if (s == null) {
                return null;
            }
            var sec = new SecureString();
            foreach (var ch in s) {
                sec.AppendChar(ch);
            }
            return sec;
        }

        public static IntPtr CreateSecureStringBuffer(int length) {
            var sec = new SecureString();
            for (int i = 0; i <= length; i++) {
                sec.AppendChar('\0');
            }
            return Marshal.SecureStringToGlobalAllocUnicode(sec);
        }

        public static SecureString SecureStringFromNativeBuffer(IntPtr nativeBuffer) {
            var ss = new SecureString();
            unsafe
            {
                for (char* p = (char*)nativeBuffer; *p != '\0'; p++) {
                    ss.AppendChar(*p);
                }
            }
            return ss;
        }

        public static string ToUnsecureString(this SecureString ss) {
            if (ss == null) {
                return null;
            }

            IntPtr ptr = IntPtr.Zero;
            try {
                ptr = Marshal.SecureStringToGlobalAllocUnicode(ss);
                return Marshal.PtrToStringUni(ptr);
            } finally {
                Marshal.ZeroFreeGlobalAllocUnicode(ptr);
            }
        }

        public static void DeleteCredentials(string authority) {
            if(!CredDelete(authority, CRED_TYPE.GENERIC, 0)) {
                int err = Marshal.GetLastWin32Error();
                if(err != ERROR_NOT_FOUND) {
                    throw new Win32Exception(err);
                }
            }
        }

        public static string GetUserName(string authority) {
            using (CredentialHandle ch = CredentialHandle.ReadFromCredentialManager(authority)) {
                if (ch != null) {
                    CredentialData credData = ch.GetCredentialData();
                    return credData.UserName;
                }
                return string.Empty;
            }
        }
    }
}
