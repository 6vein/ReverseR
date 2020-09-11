using Prism.Events;
using ReverseR.Common.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReverseR.Internal.Events
{
    public class TaskStartedEvent : PubSubEvent<IBackgroundTask> { }
    public class TaskCompletedEvent : PubSubEvent<IBackgroundTask> { }
}
