using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Prism.Events;
using System.Windows.Input;

namespace ReverseR.Common.Events
{
    public class MenuExecuteEvent:PubSubEvent<(Guid, ExecutedRoutedEventArgs)>
    {
    }
}
