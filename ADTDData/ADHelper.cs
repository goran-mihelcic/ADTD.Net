using Mihelcic.Net.Visio.Common;
using System;
using System.Diagnostics;
using System.DirectoryServices;
using System.DirectoryServices.Protocols;
using SearchScope = System.DirectoryServices.Protocols.SearchScope;

namespace Mihelcic.Net.Visio.Data
{
    public class ADHelper
    {
        public static DirectoryEntry CreateDirectoryEntry(string target, string path)
        {
            // create and return new LDAP connection with desired settings
            DirectoryEntry ldapConnection = new DirectoryEntry(target);
            if (!String.IsNullOrWhiteSpace(path))
                ldapConnection.Path = path;
            ldapConnection.AuthenticationType = AuthenticationTypes.Secure;
            return ldapConnection;
        }

        public static string GetDomainDnsName(string input)
        {
            try
            {
                using (var connection = new LdapConnection(input))
                {
                    connection.Bind();

                    // Fetch the configuration naming context
                    var request = new SearchRequest(null, "(objectClass=*)", SearchScope.Base, "configurationNamingContext");
                    var response = (SearchResponse)connection.SendRequest(request);
                    var configNamingContext = response.Entries[0].Attributes["configurationNamingContext"][0].ToString();

                    // Search for the crossRef object
                    var searchFilter = $"(&(objectClass=crossRef)(nCName={configNamingContext}))";
                    request = new SearchRequest(configNamingContext, searchFilter, SearchScope.Subtree, "dnsRoot");
                    response = (SearchResponse)connection.SendRequest(request);

                    foreach (SearchResultEntry entry in response.Entries)
                    {
                        return entry.Attributes["dnsRoot"][0].ToString();
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.TraceException(ex.ToString());
                return null;
            }
            return null;
        }
    }
}
