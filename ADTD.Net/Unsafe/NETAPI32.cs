using System;
using System.ComponentModel;
using System.Runtime.InteropServices;

namespace Mihelcic.Net.Visio.Diagrammer.Unsafe
{
    public class NETAPI32
    {
        public const int NERR_Success = 0;
        public const int ERROR_NO_SUCH_DOMAIN = 0x54B;

        [DllImport("Netapi32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        public static extern int NetGetJoinInformation(string server, out IntPtr domain, out PNETSETUP_JOIN_STATUS status);

        [DllImport("Netapi32.dll")]
        public static extern int NetApiBufferFree(IntPtr Buffer);

        [DllImport("Netapi32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        public static extern void NetFreeAadJoinInformation(IntPtr pJoinInfo);

        [DllImport("Netapi32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        public static extern int NetGetAadJoinInformation(string pcszTenantId, out IntPtr ppJoinInfo);

        [DllImport("Netapi32.dll", CharSet = CharSet.Unicode)]
        public static extern Int32 DsGetDcNameW(
            string ComputerName,
            string DomainName,
            IntPtr DomainGuid,
            string SiteName,
            GetDcFlags Flags,
            out IntPtr DomainControllerInfo);

        public enum PNETSETUP_JOIN_STATUS
        {
            NetSetupUnknownStatus = 0,
            NetSetupUnjoined,
            NetSetupWorkgroupName,
            NetSetupDomainName
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        public struct DSREG_JOIN_INFO
        {
            public int joinType;
            public IntPtr pJoinCertificate;
            [MarshalAs(UnmanagedType.LPWStr)] public string DeviceId;
            [MarshalAs(UnmanagedType.LPWStr)] public string IdpDomain;
            [MarshalAs(UnmanagedType.LPWStr)] public string TenantId;
            [MarshalAs(UnmanagedType.LPWStr)] public string JoinUserEmail;
            [MarshalAs(UnmanagedType.LPWStr)] public string TenantDisplayName;
            [MarshalAs(UnmanagedType.LPWStr)] public string MdmEnrollmentUrl;
            [MarshalAs(UnmanagedType.LPWStr)] public string MdmTermsOfUseUrl;
            [MarshalAs(UnmanagedType.LPWStr)] public string MdmComplianceUrl;
            [MarshalAs(UnmanagedType.LPWStr)] public string UserSettingSyncUrl;
            public IntPtr pUserInfo;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        public struct DOMAIN_CONTROLLER_INFOW
        {
            public string DomainControllerName;
            public string DomainControllerAddress;
            public DCAddressType DomainControllerAddressType;
            public Guid DomainGuid;
            public string DomainName;
            public string DnsForestName;
            public DCFlags Flags;
            public string DcSiteName;
            public string ClientSiteName;
        }

        public enum DSREG_JOIN_TYPE
        {
            DSREG_UNKNOWN_JOIN = 0,
            DSREG_DEVICE_JOIN,
            DSREG_WORKPLACE_JOIN
        }

        [Flags]
        public enum GetDcFlags : uint
        {
            None = 0x00000000,
            ForceRediscovery = 0x00000001,
            DirectoryServiceRequired = 0x00000010,
            DirectoryServicePreferred = 0x00000020,
            GcServerRequired = 0x00000040,
            PdcRequired = 0x00000080,
            BackgroundOnly = 0x00000100,
            IpRequired = 0x00000200,
            KdcRequired = 0x00000400,
            TimeservRequired = 0x00000800,
            WritableRequired = 0x00001000,
            GoodTimeservPreferred = 0x00002000,
            AvoidSelf = 0x00004000,
            OnlyLdapNeeded = 0x00008000,
            IsFlatName = 0x00010000,
            IsDnsName = 0x00020000,
            TryNextClosestSite = 0x00040000,
            DirectoryService6Required = 0x00080000,
            WebServiceRequired = 0x00100000,
            DirectoryService8Required = 0x00200000,
            DirectoryService9Required = 0x00400000,
            DirectoryService10Required = 0x00800000,
            ReturnDnsName = 0x40000000,
            ReturnFlatName = 0x80000000,
        }

        // Wrapper Functions
        public static bool IsInADDomain()
        {
            PNETSETUP_JOIN_STATUS status = PNETSETUP_JOIN_STATUS.NetSetupUnknownStatus;
            IntPtr pDomain = IntPtr.Zero;
            int result = NetGetJoinInformation(null, out pDomain, out status);
            if (pDomain != IntPtr.Zero)
            {
                NetApiBufferFree(pDomain);
            }
            if (result == NERR_Success)
            {
                return status == PNETSETUP_JOIN_STATUS.NetSetupDomainName;
            }
            else
            {
                throw new Exception("Domain Info Get Failed", new Win32Exception());
            }
        }

        public static Join_Info GetAadJoinInformation()
        {
            Join_Info joinInfo = null;
            IntPtr ppJoinInfo = IntPtr.Zero;
            //NetFreeAadJoinInformation(IntPtr.Zero);
            int result = NetGetAadJoinInformation(null, out ppJoinInfo);
            if (result == NERR_Success && ppJoinInfo != IntPtr.Zero)
            {
                joinInfo = new Join_Info(ppJoinInfo);
                //NetApiBufferFree(ppJoinInfo);
            }
            return joinInfo;
        }

        public static DCInfo DsGetDcName(string domainName, string computerName = null, string siteName = null,
            GetDcFlags flags = GetDcFlags.None, Guid? domainGuid = null)
        {
            bool found = true;
            IntPtr rawInfo;
            IntPtr domainGuidPtr = IntPtr.Zero;
            try
            {
                if (domainGuid != null)
                {
                    byte[] guidBytes = ((Guid)domainGuid).ToByteArray();
                    domainGuidPtr = Marshal.AllocHGlobal(guidBytes.Length);
                    Marshal.Copy(guidBytes, 0, domainGuidPtr, guidBytes.Length);
                }
                Int32 res = DsGetDcNameW(computerName, domainName, domainGuidPtr, siteName, flags, out rawInfo);
                if(res == ERROR_NO_SUCH_DOMAIN)
                {
                    found = false;
                }
                else if (res != 0)
                    throw new Win32Exception(res);
            }
            finally
            {
                if (domainGuidPtr != IntPtr.Zero)
                    Marshal.FreeHGlobal(domainGuidPtr);
            }
            try
            {
                if (found)
                {
#pragma warning disable CS8605 // Unboxing a possibly null value.
                    DOMAIN_CONTROLLER_INFOW info = (DOMAIN_CONTROLLER_INFOW)Marshal.PtrToStructure(rawInfo, typeof(DOMAIN_CONTROLLER_INFOW));
#pragma warning restore CS8605 // Unboxing a possibly null value.
                    return new DCInfo(info);
                }
                else return null;
            }
            finally
            {
                NetApiBufferFree(rawInfo);
            }
        }
    }
}
