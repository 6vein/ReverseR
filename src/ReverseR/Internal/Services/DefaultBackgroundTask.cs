using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Threading;
using System.Threading;
using System.Threading.Tasks;
using Prism.Mvvm;
using ReverseR.Common.Services;
using ReverseR.Common;
using Prism.Ioc;
using Prism.Events;
using ReverseR.Internal.Events;

namespace ReverseR.Internal.Services
{
    /// <summary>
    /// Use this class if you want your background task to be shown in the status bar
    /// <para>THE USE OF THIS CLASS IS OPTIONAL</para>
    /// </summary>
    internal class DefaultBackgroundTask:BindableBase,IBackgroundTask
    {
        string _taskName;
        public string TaskName { get => _taskName; set => SetProperty(ref _taskName, value); }
        string _taskdescription;
        public string TaskDescription { get => _taskdescription; set => SetProperty(ref _taskdescription, value); }
        bool _isCompleted;
        public bool IsCompleted { get => _isCompleted;protected set => SetProperty(ref _isCompleted, value); }

        protected Task completedCallbackTask;

        protected Action<Task> _oncompletedCallback;
        public Action<Task> OnCompletedCallback
        {
            get => _oncompletedCallback;
            set
            {
                AssertNotDisposed();
                if (Task.Status != TaskStatus.WaitingToRun)
                {
                    throw new InvalidOperationException("The task has already started,can not set completed callback now");
                }
                _oncompletedCallback = value;
            }
        }
        protected TaskScheduler _oncompletedcallbackScheduler = TaskScheduler.Default;
        public TaskScheduler OnCompletedCallbackScheduler
        {
            get => _oncompletedcallbackScheduler;
            set
            {
                AssertNotDisposed();
                if(Task.Status!=TaskStatus.WaitingToRun)
                {
                    throw new InvalidOperationException("The task has already started");
                }
                _oncompletedcallbackScheduler = value;
            }
        }

        public Task Task { get; set; }
        public CancellationToken? Token { get; set; }

        public void WaitUntilComplete(Thread UIThread=null)
        {
            AssertNotDisposed();
            if(Task==null)
            {
                throw new InvalidOperationException("The task of IBackgroundTask has not been created.");
            }
            if (Task.Status != TaskStatus.Running)
                return;
            Dispatcher dispatcher = Dispatcher.FromThread(UIThread??App.Current.Dispatcher.Thread);
#if DEBUG
            System.Diagnostics.Debug.Assert(dispatcher != null);
#endif
            //We are not going to block the ui thread specified
            if(dispatcher.Thread==Thread.CurrentThread)
            {
                dispatcher.Invoke(() =>
                {
                    DispatcherFrame frame = new DispatcherFrame();
                    Func<object, object> callback = obj =>
                    {
                        (obj as DispatcherFrame).Continue = !Task.IsCompleted || !completedCallbackTask.IsCompleted;
                        return null;
                    };
                    dispatcher.BeginInvoke(DispatcherPriority.Loaded, callback, frame);
                    Dispatcher.PushFrame(frame);
                });
            }
            else
            {
                var wait = new SpinWait();
                while (!Task.IsCompleted)
                {
                    wait.SpinOnce();
                }
            }
        }
        /// <summary>
        /// Call this if you want to use <see cref="IBackgroundTask.OnCompletedCallback"/>
        /// </summary>
        public virtual void Start()
        {
            AssertNotDisposed();
            if (Task == null)
                throw new InvalidOperationException("The task of IBackgroundTask has not been created.");
            completedCallbackTask = Task.ContinueWith(task => 
            {
                if (!(Token.HasValue && Token.Value.IsCancellationRequested)) 
                    OnCompletedCallback?.Invoke(task);
                IsCompleted = true;
                this.GetIContainer().Resolve<IEventAggregator>().GetEvent<TaskCompletedEvent>().Publish(this);
                
            },OnCompletedCallbackScheduler);
            this.GetIContainer().Resolve<IEventAggregator>().GetEvent<TaskStartedEvent>().Publish(this);
            Task.Start();
        }

        internal void SetTask(Task task)
        {
            Task = task;
        }

        internal void SetCompleteCallback(Action<Task> callback)
        {
            _oncompletedCallback = callback;
        }

        #region IDisposable Support
        private bool disposedValue = false; // 要检测冗余调用

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: 释放托管状态(托管对象)。
                    Task?.Dispose();
                    Task = null;
                    completedCallbackTask?.Dispose();
                    completedCallbackTask = null;
                    Token = null;
                }

                // TODO: 释放未托管的资源(未托管的对象)并在以下内容中替代终结器。
                // TODO: 将大型字段设置为 null。

                disposedValue = true;
            }
        }

        // TODO: 仅当以上 Dispose(bool disposing) 拥有用于释放未托管资源的代码时才替代终结器。
        // ~DefaultBackgroundTask()
        // {
        //   // 请勿更改此代码。将清理代码放入以上 Dispose(bool disposing) 中。
        //   Dispose(false);
        // }

        // 添加此代码以正确实现可处置模式。
        public void Dispose()
        {
            // 请勿更改此代码。将清理代码放入以上 Dispose(bool disposing) 中。
            Dispose(true);
            // TODO: 如果在以上内容中替代了终结器，则取消注释以下行。
            // GC.SuppressFinalize(this);
        }

        internal void AssertNotDisposed()
        {
            if (disposedValue)
                throw new ObjectDisposedException(nameof(DefaultBackgroundTask));
        }
        #endregion
    }

    internal class DefaultBackgroundTask<TResult>:DefaultBackgroundTask,IBackgroundTask<TResult>
    {
        public TResult Result
        {
            get
            {
                if (Task is Task<TResult> task)
                {
                    return task.Result;
                }
                else
                {
                    throw new InvalidOperationException("Calling get_Result on a non_generic IBackgroundTask!");
                }
            }
        }

        internal void SetTask(Task<TResult> task)
        {
            Task = task;
        }
        internal void SetCompleteCallback(Action<Task<TResult>> callback)
        {
            _oncompletedCallback = (Action<Task>)callback;
        }

        public override void Start()
        {
            AssertNotDisposed();
            if (Task == null || !(Task is Task<TResult>))
                throw new InvalidOperationException("The task of IBackgroundTask has not been created or is invalid.");
            completedCallbackTask = Task.ContinueWith(task =>
            {
                if (!(Token.HasValue && Token.Value.IsCancellationRequested))
                {
                    if (task is Task<TResult> taskT)
                    {
                        if (OnCompletedCallback is Action<Task<TResult>> callback)
                        {
                            callback?.Invoke(taskT);
                        }
                    }
                }
                IsCompleted = true;
                this.GetIContainer().Resolve<IEventAggregator>().GetEvent<TaskCompletedEvent>().Publish(this);

            }, OnCompletedCallbackScheduler);
            this.GetIContainer().Resolve<IEventAggregator>().GetEvent<TaskStartedEvent>().Publish(this);
            Task.Start();
        }
    }
}
