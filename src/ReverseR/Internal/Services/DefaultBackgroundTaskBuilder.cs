using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ReverseR.Common.Services;

namespace ReverseR.Internal.Services
{
    internal class DefaultBackgroundTaskBuilder : IBackgroundTaskBuilder
    {
        TaskScheduler _scheduler;
        Action<Task> _callback;
        Action<object> _action;
        CancellationToken? _token;
        TaskCreationOptions _options;
        public IBackgroundTask Build()
        {
            var task = new DefaultBackgroundTask();
            if (_scheduler != null)
                task.OnCompletedCallbackScheduler = _scheduler;
            task.Token = _token;
            task.SetCompleteCallback(_callback);
            task.SetTask(new Task(_action,_token));
            return task;
        }

        public IBackgroundTaskBuilder WithCallbackTaskScheduler(TaskScheduler scheduler)
        {
            _scheduler = scheduler;
            return this;
        }

        public IBackgroundTaskBuilder WithOnCompleteCallback(Action<Task> callback)
        {
            _callback = callback;
            return this;
        }

        public IBackgroundTaskBuilder<TResult> WithOnCompleteCallback<TResult>(Action<Task<TResult>> callback)
        {
            var builder = new DefaultBackgroundTaskBuilder<TResult>();
            builder._scheduler = _scheduler;
            builder._options = _options;
            builder._token = _token;
            builder._callback = callback;
            return builder;
        }

        public IBackgroundTaskBuilder WithTask(Action<object> action, CancellationToken? token = null, TaskCreationOptions options = TaskCreationOptions.None)
        {
            _action = action;
            _token = token;
            _options = options;
            return this;
        }

        public IBackgroundTaskBuilder<TResult> WithTask<TResult>(Func<object, TResult> func, CancellationToken? token = null, TaskCreationOptions options = TaskCreationOptions.None)
        {
            var builder = new DefaultBackgroundTaskBuilder<TResult>();
            builder._scheduler = _scheduler;
            builder._options = _options;
            builder._token = _token;
            builder._func = func;
            return builder;
        }
    }

    internal class DefaultBackgroundTaskBuilder<TResult> : IBackgroundTaskBuilder<TResult>
    {
        internal TaskScheduler _scheduler;
        internal Action<Task<TResult>> _callback;
        internal Func<object,TResult> _func;
        internal CancellationToken? _token;
        internal TaskCreationOptions _options;
        public IBackgroundTask<TResult> Build()
        {
            var task = new DefaultBackgroundTask<TResult>();
            task.OnCompletedCallbackScheduler = _scheduler;
            task.Token = _token;
            task.SetCompleteCallback(_callback);
            task.SetTask(new Task<TResult>(_func,_token));
            return task;
        }

        public IBackgroundTaskBuilder<TResult> WithCallbackTaskScheduler(TaskScheduler scheduler)
        {
            _scheduler = scheduler;
            return this;
        }

        public IBackgroundTaskBuilder<TResult> WithOnCompleteCallback(Action<Task<TResult>> callback)
        {
            _callback = callback;
            return this;
        }

        public IBackgroundTaskBuilder<TResult> WithTask(Func<object, TResult> func, CancellationToken? token = null, TaskCreationOptions options = TaskCreationOptions.None)
        {
            _func = func;
            _token = token;
            _options = options;
            return this;
        }
    }
}
