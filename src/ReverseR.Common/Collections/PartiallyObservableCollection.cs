using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReverseR.Common.Collections
{
    public class PartiallyObservableCollection<T>:ObservableCollection<T>
    {
        bool _supressNotify = false;
        public void SetSupressNotification(bool value)
        {
            _supressNotify = value;
        }
        protected override void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            if (!_supressNotify)
                base.OnPropertyChanged(e);
        }
    }
}
