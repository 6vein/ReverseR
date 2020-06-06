using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ReverseR.Common.Services;

namespace ReverseR.Internal.Services
{
    internal class DefaultBackgroundTaskBuilder : IBackgroundTaskBuilder
    {
        public IBackgroundTask Create(Action<object> action)
        {
            IBackgroundTask task = new DefaultBackgroundTask();
            task.Task = new Task(action, task.TokenSource.Token);
            return task;
        }

        public IBackgroundTask<T> Create<T>(Func<object,T> action)
        {
            IBackgroundTask<T> task = new DefaultBackgroundTask<T>();
            task.Task = new Task<T>(action, task.TokenSource.Token);
            return task;
        }

        public IBackgroundTask CreateWithoutFunc()
        {
            return new DefaultBackgroundTask();
        }

        public IBackgroundTask<T> CreateWithoutFunc<T>()
        {
            return new DefaultBackgroundTask<T>();
        }
    }
}
