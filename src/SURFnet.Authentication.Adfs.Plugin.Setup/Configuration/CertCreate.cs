using Microsoft.Win32.SafeHandles;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace SURFnet.Authentication.Adfs.Plugin.Setup.Configuration
{
    public class CertCreate
    {
        public static void Create()
        {
            X509Certificate2 cert = null;
            X509Store store = new X509Store("MY", StoreLocation.LocalMachine);
            store.Open(OpenFlags.ReadWrite);

            int fiveYears = 5 * 365 * 24 * 60; // 5 * days/y * hour/day * min/hour (no leap days.... :-0)

            try
            {
                string hostname = "NoWhere.com";
                string subject = $"CN=SFO MFA extension {hostname}";
                cert = CreateSelfSignedCertificate(subject, fiveYears, null);

                store.Add(cert);

                cert.Reset();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
            finally
            {
                if (cert != null)
                    cert.Reset();
            }

            store.Close();
        }

        public static unsafe X509Certificate2 CreateSelfSignedCertificate(string subjectName, int durationInMinutes, OidCollection ekuExtensions)
        {
            int keySize = 3 * 1024;


            X509Certificate2 certificate;
            NativeMethods.CERT_NAME_BLOB pSubjectIssuerBlob = new NativeMethods.CERT_NAME_BLOB(0, null);
            string str = Guid.NewGuid().ToString();
            IntPtr zero = IntPtr.Zero;
            IntPtr ptr = IntPtr.Zero;
            try
            {
                uint pcbEncoded = 0;
                byte[] pbEncoded = null;
                if (!NativeMethods.CertStrToName(NativeMethods.X509_ASN_ENCODING, subjectName, NativeMethods.CERT_X500_NAME_STR, IntPtr.Zero, null, ref pcbEncoded, IntPtr.Zero) && (pcbEncoded == 0))
                {
                    throw new Win32Exception(Marshal.GetLastWin32Error());
                }
                pbEncoded = new byte[pcbEncoded];
                if (!NativeMethods.CertStrToName(NativeMethods.X509_ASN_ENCODING, subjectName, NativeMethods.CERT_X500_NAME_STR, IntPtr.Zero, pbEncoded, ref pcbEncoded, IntPtr.Zero))
                {
                    throw new Win32Exception(Marshal.GetLastWin32Error());
                }
                CspParameters parameters = new CspParameters(NativeMethods.XCN_PROV_RSA_AES)
                {
                    KeyContainerName = str,
                    KeyNumber = (int)KeyNumber.Signature,
                    Flags = CspProviderFlags.UseMachineKeyStore,
                };
                RSACryptoServiceProvider provider1 = new RSACryptoServiceProvider(keySize, parameters);
                pSubjectIssuerBlob.CopyData(pbEncoded);  // set to same as Subject
                NativeMethods.CRYPT_KEY_PROV_INFO pKeyProvInfo = new NativeMethods.CRYPT_KEY_PROV_INFO
                {
                    pwszContainerName = str,
                    dwProvType = NativeMethods.XCN_PROV_RSA_AES,
                    dwFlags = NativeMethods.CRYPT_MACHINE_KEYSET, // == 0x20,
                    dwKeySpec = NativeMethods.XCN_AT_SINATURE // was 1
                };
                NativeMethods.CRYPT_ALGORITHM_IDENTIFIER pSignatureAlgorithm = new NativeMethods.CRYPT_ALGORITHM_IDENTIFIER
                {
                    pszObjId = NativeMethods.szOID_RSA_SHA256RSA,
                    parameters = {
                        cbData = 0,
                        pbData = IntPtr.Zero
                    }
                };
                NativeMethods.SYSTEM_TIME pStartTime = new NativeMethods.SYSTEM_TIME(DateTime.UtcNow);
                NativeMethods.SYSTEM_TIME pEndTime = new NativeMethods.SYSTEM_TIME(DateTime.UtcNow.AddMinutes((double)durationInMinutes));
                if ((ekuExtensions != null) && (ekuExtensions.Count > 0))
                {
                    byte[] buffer2;
                    X509EnhancedKeyUsageExtension extension = new X509EnhancedKeyUsageExtension(ekuExtensions, false);

                    //fixed (byte* numRef = ((((buffer2 = extension.RawData) == null) || (buffer2.Length == 0)) ? null : ((byte*)buffer2[0])))
                    fixed (byte* numRef = (((buffer2 = extension.RawData) == null) || (buffer2.Length == 0)) ? null : buffer2)
                    {
                        NativeMethods.CERT_EXTENSION structure = new NativeMethods.CERT_EXTENSION
                        {
                            fCritical = extension.Critical,
                            pszObjId = NativeMethods.szOID_ENHANCED_KEY_USAGE,
                            Value = {
                                cbData = (uint) extension.RawData.Length,
                                pbData = new IntPtr((void*) numRef)
                            }
                        };
                        NativeMethods.CERT_EXTENSIONS cert_extensions = new NativeMethods.CERT_EXTENSIONS
                        {
                            cExtension = 1
                        };
                        zero = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(NativeMethods.CERT_EXTENSION)));
                        Marshal.StructureToPtr<NativeMethods.CERT_EXTENSION>(structure, zero, false);
                        cert_extensions.rgExtension = zero;
                        ptr = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(NativeMethods.CERT_EXTENSIONS)));
                        Marshal.StructureToPtr<NativeMethods.CERT_EXTENSIONS>(cert_extensions, ptr, false);
                    }
                }
                IntPtr ptr1 = NativeMethods.CertCreateSelfSignCertificate(IntPtr.Zero, ref pSubjectIssuerBlob, 0,
                                                ref pKeyProvInfo, ref pSignatureAlgorithm, ref pStartTime, ref pEndTime, ptr);
                if (NativeMethods.CertCreateSelfSignCertificate(IntPtr.Zero, ref pSubjectIssuerBlob, 0, ref pKeyProvInfo, ref pSignatureAlgorithm, ref pStartTime, ref pEndTime, ptr) == IntPtr.Zero)
                {
                    throw new Win32Exception(Marshal.GetLastWin32Error());
                }
                certificate = new X509Certificate2(NativeMethods.CertCreateSelfSignCertificate(IntPtr.Zero, ref pSubjectIssuerBlob, 0, ref pKeyProvInfo, ref pSignatureAlgorithm, ref pStartTime, ref pEndTime, ptr));
            }
            finally
            {
                if (IntPtr.Zero != zero)
                {
                    Marshal.FreeHGlobal(zero);
                    zero = IntPtr.Zero;
                }
                if (IntPtr.Zero != ptr)
                {
                    Marshal.FreeHGlobal(ptr);
                    ptr = IntPtr.Zero;
                }
                pSubjectIssuerBlob.Dispose();
            }
            return certificate;
        }
    }

    public class NativeMethods
    {
        /// mainly from certenrol.h
        public const uint XCN_AT_KEYEXCHANGE = 1;
        public const uint XCN_AT_SINATURE = 2;

        public const int XCN_PROV_RSA_AES = 24;
        public const uint X509_ASN_ENCODING = 1;
        public const uint CERT_X500_NAME_STR = 3;

        public const uint CRYPT_MACHINE_KEYSET = 0x20;

        public const string szOID_RSA_SHA256RSA = "1.2.840.113549.1.1.11";
        public const string szOID_ENHANCED_KEY_USAGE = "2.5.29.37";

        [DllImport("crypt32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        public static extern bool CertStrToName(uint dwCertEncodingType, string pszX500, uint dwStrType, IntPtr pvReserved, [In, Out] byte[] pbEncoded, ref uint pcbEncoded, IntPtr other);

        [StructLayout(LayoutKind.Sequential)]
        public struct CERT_NAME_BLOB : IDisposable
        {
            public int _cbData;
            public SafeGlobalMemoryHandle _pbData;
            public CERT_NAME_BLOB(int cb, SafeGlobalMemoryHandle handle)
            {
                this._cbData = cb;
                this._pbData = handle;
            }

            public void CopyData(byte[] encodedName)
            {
                this._pbData = new SafeGlobalMemoryHandle(encodedName);
                this._cbData = encodedName.Length;
            }

            public void Dispose()
            {
                if (this._pbData != null)
                {
                    this._pbData.Dispose();
                    this._pbData = null;
                }
            }

        }

        // wincrypt.h
        [StructLayout(LayoutKind.Sequential)]
        public struct CRYPT_KEY_PROV_INFO
        {
            [MarshalAs(UnmanagedType.LPWStr)]
            public string pwszContainerName;
            [MarshalAs(UnmanagedType.LPWStr)]
            public string pwszProvName;
            public uint dwProvType;
            public uint dwFlags;
            public uint cProvParam;
            public IntPtr rgProvParam;
            public uint dwKeySpec;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct CRYPT_OBJID_BLOB
        {
            public uint cbData;
            public IntPtr pbData;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct CRYPT_ALGORITHM_IDENTIFIER
        {
            [MarshalAs(UnmanagedType.LPStr)]
            public string pszObjId;
            public CRYPT_OBJID_BLOB parameters;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        public struct CRYPTOAPI_BLOB
        {
            public uint cbData;
            public IntPtr pbData;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        public struct CERT_EXTENSION
        {
            [MarshalAs(UnmanagedType.LPStr)]
            public string pszObjId;
            public bool fCritical;
            public CRYPTOAPI_BLOB Value;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct CERT_EXTENSIONS
        {
            public uint cExtension;
            public IntPtr rgExtension;
        }
        // wincrypt.h
        [DllImport("crypt32.dll", SetLastError = true)]
        public static extern IntPtr CertCreateSelfSignCertificate(IntPtr hProv, ref CERT_NAME_BLOB pSubjectIssuerBlob, uint dwFlagsm, ref CRYPT_KEY_PROV_INFO pKeyProvInfo, ref CRYPT_ALGORITHM_IDENTIFIER pSignatureAlgorithm, ref SYSTEM_TIME pStartTime, ref SYSTEM_TIME pEndTime, IntPtr other);

        [StructLayout(LayoutKind.Sequential)]
        public struct SYSTEM_TIME
        {
            public ushort wYear;
            public ushort wMonth;
            public ushort wDayOfWeek;
            public ushort wDay;
            public ushort wHour;
            public ushort wMinute;
            public ushort wSecond;
            public ushort wMilliseconds;
            public SYSTEM_TIME(DateTime dt)
            {
                this.wYear = (ushort)dt.Year;
                this.wMonth = (ushort)dt.Month;
                this.wDay = (ushort)dt.Day;
                this.wDayOfWeek = (ushort)dt.DayOfWeek;
                this.wHour = (ushort)dt.Hour;
                this.wMinute = (ushort)dt.Minute;
                this.wSecond = (ushort)dt.Second;
                this.wMilliseconds = (ushort)dt.Millisecond;
            }
        }
    }
    public class SafeGlobalMemoryHandle : SafeHandleZeroOrMinusOneIsInvalid
    {
        private SafeGlobalMemoryHandle() : base(true)
        {
        }

        public SafeGlobalMemoryHandle(byte[] data) : base(true)
        {
            base.handle = Marshal.AllocHGlobal(data.Length);
            Marshal.Copy(data, 0, base.handle, data.Length);
        }

        protected override bool ReleaseHandle()
        {
            Marshal.FreeHGlobal(base.handle);
            return true;
        }
    }
}
