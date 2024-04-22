using System;
using System.Collections.Generic;
using System.DirectoryServices.ActiveDirectory;

namespace Mihelcic.Net.Visio.Data
{
    internal class DomainTrust
    {
        public string FlatName { get; set; }
        public string Target { get; set; }
        public TrustDirection Direction { get; set; }
        public TrustType Type { get; set; }
        public IEnumerable<String> Attributes { get; set; } = new List<String>();

        public DomainTrust(string flatName,string target, uint direction, uint type, int attributes)
        {
            FlatName = flatName;
            Target = target;
            Direction = (TrustDirection)direction;
            Type = (TrustType)type;
            Attributes= DecodeAttributes(attributes);
        }

        public DomainTrust(SearchResult ldapRealm)
        {
            FlatName = ldapRealm.GetPropertyString(LdapStrings.FlatName);
            Target = ldapRealm.GetPropertyString(LdapStrings.TrustPartner);
            Direction = (TrustDirection)(ldapRealm.GetPropertyValue(LdapStrings.TrustDirection));
            Type = (TrustType)(ldapRealm.GetPropertyValue(LdapStrings.TrustType));
            Attributes = DecodeAttributes((int)ldapRealm.Properties[LdapStrings.TrustAttributes]);
        }

        private List<String> DecodeAttributes(int properties)
        {
            var attributes = new List<String>();
            if ((properties & 0x00000001) == 0x00000001)
                attributes.Add(Strings.TrustNonTransitive);
            if ((properties & 0x00000002) == 0x00000002)
                attributes.Add(Strings.TrustUplevelOnly);
            if ((properties & 0x00000004) == 0x00000004)
                attributes.Add(Strings.TrustQuarantined_Domain);
            if ((properties & 0x00000008) == 0x00000008)
                attributes.Add(Strings.TrustForestTransitive);
            if ((properties & 0x00000010) == 0x00000010)
                attributes.Add(Strings.TrustCrossOrganization);
            if ((properties & 0x00000020) == 0x00000020)
                attributes.Add(Strings.TrustWithinForest);
            if ((properties & 0x00000040) == 0x00000040)
                attributes.Add(Strings.TrustAsExternal);
            if ((properties & 0x00000080) == 0x00000080)
                attributes.Add(Strings.TrustRC4Encryption);
            if ((properties & 0x00000200) == 0x00000200)
                attributes.Add(Strings.TrustPreventTGTDelegation);
            if ((properties & 0x00000400) == 0x00000400)
                attributes.Add(Strings.TrustPIM);
            return attributes;
        }
    }
}
