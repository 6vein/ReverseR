using Prism.Events;
using ReverseR.Common.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReverseR.Internal.Events
{
    public class TaskStartedEvent : PubSubEvent<IAbstractBackgroundTask> { }
    public class TaskCompletedEvent : PubSubEvent<IAbstractBackgroundTask> { }
}
