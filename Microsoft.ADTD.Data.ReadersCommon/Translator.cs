using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Microsoft.ADTD.Data
{
    public static class Translator
    {
        public static string osTranslate(string value)
        {
            switch (value.ToLower())
            {
                case "windows server® 2008 standard service pack 2":
                    return "W2K8 Stand. SP2";
                case "windows server® 2008 standard service pack 1":
                    return "W2K8 Stand. SP1";
                case "windows server® 2008 enterprise service pack 2":
                    return "W2K8 Ent. SP2";
                case "windows server® 2008 enterprise service pack 1":
                    return "W2K8 Ent. SP1";
                case "windows server 2003 service pack 2":
                    return "W2K3 Stand. SP2";
                case "windows server 2003 service pack 1":
                    return "W2K3 Stand. SP1";
                case "windows server 2008 r2 enterprise service pack 1": 
                    return "W2K8R2 Ent. SP1";
                case "windows server 2008 r2 enterprise service pack 2":
                    return "W2K8R2 Ent. SP2";
                case "5.2.3790":
                    return "W2K3";
                case "6.0.6002":
                    return "W2K8 SP2";
                case "6.1.7601":
                    return "W2K8R2 Ent. SP1";

                    

                default:
                    return value;
            }
        }
    }
}
