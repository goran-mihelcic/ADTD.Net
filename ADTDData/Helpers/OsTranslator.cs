namespace Mihelcic.Net.Visio.Data
{
    public static class OsTranslator
    {
        public static string OsTranslate(string value)
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
                    return Shorten(value);
            }
        }

        private static string Shorten(string os)
        {
            if (os.Contains("Windows Server®"))
                os = os.Replace("Windows Server®", "W");
            else if (os.Contains("Windows Server"))
                    os = os.Replace("Windows Server", "W");
            if (os.Contains(" 20"))
                os = os.Replace(" 20", "2K");
            if (os.Contains("Standard"))
                os = os.Replace("Standard", "STE");
            if (os.Contains("Enterprise"))
                os = os.Replace("Enterprise", "EE");
            if (os.Contains("Datacenter"))
                os = os.Replace("Datacenter", "DCE");
            if (os.Contains("Service Pack "))
                os = os.Replace("Service Pack ", "SP");
            return os;
        }
    }
}
