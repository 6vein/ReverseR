using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Prism.Events;
using ReverseR.Common.ViewUtilities;
using System.Collections.ObjectModel;

namespace ReverseR.Common.Events
{
    /// <summary>
    /// Event that indicates that menu should be updated.
    /// The application shell only creates and handles the "File" and "Help" menu
    /// </summary>
    public class MenuUpdatedEvent:PubSubEvent<Tuple<IEnumerable<IMenuViewModel>, Guid>>
    {
    }
}
