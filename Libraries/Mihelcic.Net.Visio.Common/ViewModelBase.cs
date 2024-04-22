using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Mihelcic.Net.Visio.Common
{
    public abstract class ViewModelBase : INotifyPropertyChanged
    {
        public virtual event PropertyChangedEventHandler PropertyChanged;
        private readonly Dictionary<string, object> _properties = new Dictionary<string, object>();

        protected bool SetValue(object value, [CallerMemberName] string propertyName = null)
        {
            if (_properties.TryGetValue(propertyName, out var item) && value is decimal ? Decimal.Equals(item, value) : item == value)
            {
                return false;
            }

            _properties[propertyName] = value; 
            OnPropertyChanged(propertyName);
            return true;
        }

        protected T GetValue<T>([CallerMemberName] string propertyName = null)
        {
            if (_properties.TryGetValue(propertyName, out var value))
            {
                return (T)value;
            }

            return default;
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
