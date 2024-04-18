using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.AccessControl;
using System.Text;
using System.Threading.Tasks;
using static Mihelcic.Net.Visio.Diagrammer.Unsafe.NETAPI32;

namespace Mihelcic.Net.Visio.Diagrammer.Unsafe
{
    public class DCInfo
    {
        public string Name { get; internal set; }
        public string Address { get; internal set; }
        public DCAddressType AddressType { get; internal set; }
        public Guid DomainGuid { get; internal set; }
        public string DomainName { get; internal set; }
        public string DnsForestName { get; internal set; }
        public DCFlags Flags { get; internal set; }
        public string DcSiteName { get; internal set; }
        public string ClientSiteName { get; internal set; }

        public DCInfo(DOMAIN_CONTROLLER_INFOW info)
        {
            Name = info.DomainControllerName;
            Address = info.DomainControllerAddress;
            AddressType = info.DomainControllerAddressType;
            DomainGuid = info.DomainGuid;
            DomainName = info.DomainName;
            DnsForestName = info.DnsForestName;
            Flags = info.Flags;
            DcSiteName = info.DcSiteName;
            ClientSiteName = info.ClientSiteName;
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendLine(String.Format("Name:\t\t{0}", Name));
            sb.AppendLine(String.Format("Address:\t{0}", Address));
            sb.AppendLine(String.Format("AddressType:\t{0}", AddressType));
            sb.AppendLine(String.Format("DomainGuid:\t{0}", DomainGuid));
            sb.AppendLine(String.Format("DomainName:\t{0}", DomainName));
            sb.AppendLine(String.Format("DnsForestName:\t{0}", DnsForestName));
            sb.AppendLine(String.Format("Flags:\t\t{0}", Flags));
            sb.AppendLine(String.Format("DcSiteName:\t{0}", DcSiteName));
            sb.AppendLine(String.Format("ClientSiteName:\t{0}", ClientSiteName));

            return sb.ToString();
        }
    }

    public enum DCAddressType : uint
    {
        INetAddress = 1,
        NetbiosAddress = 2,
    }
    [Flags]
    public enum DCFlags : uint
    {
        Pdc = 0x00000001,
        Gc = 0x00000004,
        Ldap = 0x00000008,
        Ds = 0x00000010,
        Kdc = 0x00000020,
        Timeserv = 0x00000040,
        Closest = 0x00000080,
        Writable = 0x00000100,
        GoodTimeserv = 0x00000200,
        Ndnc = 0x00000400,
        SelectSecretDomain6 = 0x00000800,
        FullSecretDomain6 = 0x00001000,
        Ws = 0x00002000,
        Ds8 = 0x00004000,
        Ds9 = 0x00008000,
        Ds10 = 0x00010000,
        DsKeyList = 0x00020000,
        Ping = 0x000FFFFF,
        DnsController = 0x20000000,
        DnsDomain = 0x40000000,
        DnsForest = 0x80000000,
    }
}
