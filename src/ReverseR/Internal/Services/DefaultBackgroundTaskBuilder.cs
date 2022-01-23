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
        string _name;
        string _description;
        public IBackgroundTask Build()
        {
            var task = new DefaultBackgroundTask();
            task.TaskName = _name;
            task.TaskDescription = _description;
            if (_scheduler != null)
                task.OnCompletedCallbackScheduler = _scheduler;
            task.Token = _token;
            task.SetCompleteCallback(_callback);
            if (_token.HasValue)
                task.SetTask(new Task(_action, _token, _token.Value));
            else task.SetTask(new Task(_action, _token));
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

        public IBackgroundTaskBuilder WithName(string name)
        {
            _name = name;
            return this;
        }
        public IBackgroundTaskBuilder WithDescription(string description)
        {
            _description = description;
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
        internal string _name;
        internal string _description;
        public IBackgroundTask<TResult> Build()
        {
            var task = new DefaultBackgroundTask<TResult>();
            task.TaskName = _name;
            task.TaskDescription = _description;
            if (_scheduler != null)
                task.OnCompletedCallbackScheduler = _scheduler;
            task.Token = _token;
            task.SetCompleteCallback(_callback);
            if (_token.HasValue)
                task.SetTask(new Task<TResult>(_func, _token, _token.Value));
            else
                task.SetTask(new Task<TResult>(_func, _token));
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
        public IBackgroundTaskBuilder<TResult> WithName(string name)
        {
            _name = name;
            return this;
        }
        public IBackgroundTaskBuilder<TResult> WithDescription(string description)
        {
            _description = description;
            return this;
        }
    }
}
