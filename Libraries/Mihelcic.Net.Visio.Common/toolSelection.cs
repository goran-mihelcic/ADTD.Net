namespace Mihelcic.Net.Visio.Common
{
    public class ToolSelection
    {
        public bool Windows2000Trust { set; get; }
        public bool DownlevelTrusts { set; get; }
        public bool CrossForestTrust { set; get; }
        public bool DrawExternalDomainDetails { set; get; }
        public bool SiteIncludeServers { set; get; }

        public bool IPSiteLink { set; get; }
        public bool SMTPSiteLink { set; get; }
        public bool IntraReplCon { set; get; }
        public bool InterReplCon { set; get; }
        public bool DetailedReplCon { set; get; }
        public bool SuppressEmptySites { set; get; }
        public bool Subnets { set; get; }
        public bool ComplexSiteLinks { set; get; }
        public bool DomainIncludeServers { set; get; }

        public bool ServerVersion { set; get; }
        public bool DrawFQDN { set; get; }
        public bool ColorCodeServers { set; get; }
    }
}
