using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Microsoft.ADTD.Data
{
    public static class TrustTypes
    {
        public static string Downlevel = "Downlevel";
        public static string Windows2000 = "Windows2000";
        public static string MIT = "MIT";
        public static string DCS = "DCS";
        public static string CrossForest = "CrossForest";
        public static string Unknown = "Unknown";
    }

    public static class TrustDirections
    {
        public static string Inbound = "Inbound";
        public static string Outbound = "Outbound";
        public static string Bidirectional = "Bidirectional";
        public static string Unknown = "Unknown";
    }

    public static class TrustAttributes
    {
        public static string NonTransitive = "NonTransitive";
        public static string UplevelOnly = "UplevelOnly";
        public static string FilterSIDS = "FilterSIDS";
        public static string ForestTransitive = "ForestTransitive";
        public static string CrossOrganization = "CrossOrganization";
        public static string WithinForest = "WithinForest";
        public static string TreatAsExternal = "TreatAsExternal";
        public static string Unknown = "Unknown";
    }
}
