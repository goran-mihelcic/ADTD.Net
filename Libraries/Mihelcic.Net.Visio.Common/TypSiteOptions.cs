using System;

namespace Mihelcic.Net.Visio.Data
{
    public class TypSiteOptions
    {
        public bool IntraSite_Topology_Generation = true;
        public bool Cleanup = true;
        public bool Optimizing_Edges = true;
        public bool Stale_Link_Detection = true;
        public bool InterSite_Topology_Generation = true;
        public bool Universal_Group_Membership_Caching = false;
        public string ISTG= String.Empty;
        public string UniversalGroupCachingSite= String.Empty;
    }
}
