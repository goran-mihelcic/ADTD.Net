using Mihelcic.Net.Visio.Common;
using System;
using System.DirectoryServices;
using System.Text;

namespace Mihelcic.Net.Visio.Data
{
    public class RootDSE
    {
        #region Public Properties

        public string DomainDnsName { get; set; }
        //rootDomainNamingContext
        public String RootDomainNamingContext { get; set; }
        //serverName
        //schemaNamingContext
        public string SchemaNamingContext { get; set; }
        //dnsHostName
        public string ServerName { get; set; }
        //defaultNamingContext
        //configurationNamingContext 
        public string ConfigurationNamingContext { get; set; }
        public bool IsValid {  get; set; }

        #endregion

        #region Public Methods

        public RootDSE(string domainDnsName)
        {
            IsValid = false;
            try
            {
                DomainDnsName = domainDnsName;
                var entry = new DirectoryEntry($"LDAP://{domainDnsName}/RootDSE");
                ServerName = entry.Properties["dnsHostName"].Value.ToString();
                RootDomainNamingContext = entry.Properties["rootDomainNamingContext"].Value.ToString();
                SchemaNamingContext = entry.Properties["schemaNamingContext"].Value.ToString();
                ConfigurationNamingContext = entry.Properties["configurationNamingContext"].Value.ToString();
                IsValid = true;
                //DomainDnsName = entry.Properties[""].Value.ToString();
            }
            catch (Exception ex)
            {
                Logger.TraceException(ex.Message);
            }
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendLine(String.Format("DomainDnsName:\t\t\t{0}", DomainDnsName));
            sb.AppendLine(String.Format("RootDomainNamingContext:\t{0}", RootDomainNamingContext));
            sb.AppendLine(String.Format("SchemaNamingContext:\t\t{0}", SchemaNamingContext));
            sb.AppendLine(String.Format("ServerName:\t\t\t{0}", ServerName));
            sb.AppendLine(String.Format("ConfigurationNamingContext:\t{0}", ConfigurationNamingContext));

            return sb.ToString();
        }

        #endregion
    }
}
