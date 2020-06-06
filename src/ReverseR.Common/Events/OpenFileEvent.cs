using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Prism.Events;
using ReverseR.Common.ViewUtilities;
using ReverseR.Common.DecompUtilities;

namespace ReverseR.Common.Events
{
    public class OpenFileEvent:PubSubEvent<Tuple<string,FileTypes,Guid>>
    {
    }
}
