using System;
using System.Runtime.InteropServices;
using System.Text;
using static Mihelcic.Net.Visio.Diagrammer.Unsafe.NETAPI32;

namespace Mihelcic.Net.Visio.Diagrammer.Unsafe
{
    public class Join_Info
    {
        public int JoinType { get; set; }
        public string DeviceId { get; set; } = string.Empty;
        public string TenantId { get; set; } = string.Empty;
        public string JoinUserEmail { get; set; } = string.Empty;
        public string TenantDisplayName { get; set; } = string.Empty;
        
        public Join_Info(IntPtr ppJoinInfo)
        {
            DSREG_JOIN_INFO joinInfo = Marshal.PtrToStructure<DSREG_JOIN_INFO>(ppJoinInfo);
            JoinType = (int)joinInfo.joinType;
            DeviceId = joinInfo.DeviceId;
            TenantId = joinInfo.TenantId;
            JoinUserEmail = joinInfo.JoinUserEmail;
            TenantDisplayName = joinInfo.TenantDisplayName;
            NetApiBufferFree(joinInfo.pUserInfo);
            NetApiBufferFree(ppJoinInfo);
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine(String.Format("JoinType:\t{0}", JoinType));
            sb.AppendLine(String.Format("DeviceId:\t{0}", DeviceId));
            sb.AppendLine(String.Format("TenantId:\t{0}", TenantId));
            sb.AppendLine(String.Format("JoinUser:\t{0}", JoinUserEmail));
            sb.AppendLine(String.Format("TenantName:\t{0}", TenantDisplayName));
            return sb.ToString();
        }
    }
}
