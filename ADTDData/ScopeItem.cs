using System.Collections.Generic;
using System.DirectoryServices;

namespace Mihelcic.Net.Visio.Data
{
    public class ScopeItem
    {
        #region Public Properties

        public string Name { get; set; }
        public string DisplayName { get; set; }
        public bool Included { get; set; }
        public Dictionary<string, object> Properties {get; set;}
        public DirectoryEntry ItemDE { get; set; }

        #endregion

        #region Constructors

        public ScopeItem(string name)
        {
            Properties = new Dictionary<string, object>();
            this.Name = name;
            this.DisplayName = name;
            this.Included = false;
        }

        public ScopeItem(string name, bool incl)
        {
            Properties = new Dictionary<string, object>();
            this.Name = name;
            this.DisplayName = name;
            this.Included = incl;
        }

        #endregion
    }
}
