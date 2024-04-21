using System.Collections.ObjectModel;
using System.ComponentModel;

namespace Mihelcic.Net.Visio.Diagrammer
{
    public class Statuses : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        
        public ObservableCollection<string> List { get; set; } = new ObservableCollection<string>();
        
        public string Last
        {
            get
            {
                return List.Count > 0 ? List[0] : null; // Also, handle empty list scenario
            }
        }

        public Statuses()
        {
            AddStatus(Strings.InitialStatus);
        }

        public void AddStatus(string status)
        {
            List.Insert(0, status);
            OnPropertyChanged(nameof(List));
            OnPropertyChanged(nameof(Last)); // Notify whenever a new status is added
        }

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
