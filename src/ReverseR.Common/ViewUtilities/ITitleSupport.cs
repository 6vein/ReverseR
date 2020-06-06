using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;

namespace ReverseR.Common.ViewUtilities
{
    public interface ITitleSupport:INotifyPropertyChanged
    {
        public string Title { get; set; }
    }
}
