using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.ComponentModel;

namespace ReverseR.Common.Services
{
    public interface IAbstractBackgroundTask : INotifyPropertyChanged
    {
        #region Bindings
        public string TaskName { get; set; }
        public string TaskDescription { get; set; }
        public bool IsCompleted { get; set; }
        #endregion
    }
    /// <summary>
    /// A background task that supports cancelling,and will be automatically added to the background task list
    /// Do not create instance of this interface manually,use <see cref="IBackgroundTaskBuilder"/> instead
    /// </summary>
    public interface IBackgroundTask:IAbstractBackgroundTask,IDisposable
    {
        /// <summary>
        /// Task to run.Callers should create and configure it themselves
        /// </summary>
        public Task Task { get; set; }
        /// <summary>
        /// Will be called whenever the execution completed
        /// </summary>
        public Action<Task> OnCompletedCallback { get; set; }
        public TaskScheduler OnCompletedCallbackScheduler { get; set; }
        public CancellationTokenSource TokenSource { get; set; }
        /// <summary>
        /// Cancel this task,blocking the current thread,mostly use <see cref="System.Windows.Threading.Dispatcher.PushFrame(System.Windows.Threading.DispatcherFrame)"/>
        /// <para>Default behavior:</para>
        /// <list type="bullet">
        /// Will block this thread with <see cref="System.Windows.Threading.Dispatcher.PushFrame(System.Windows.Threading.DispatcherFrame)"/> if it's on main thread
        /// </list>
        /// <list type="bullet">
        /// Will <see cref="SpinWait"/> otherwise
        /// </list>
        /// </summary>
        /// <param name="UIThread">
        /// The UI thread,if null,will specify the main thread
        /// </param>
        public void CancelWithSync(Thread UIThread=null);
        public void Start();
    }

    /// <inheritdoc/>
    /// <typeparam name="T">Type for the Task to return</typeparam>
    public interface IBackgroundTask<T>:IAbstractBackgroundTask,IDisposable
    {
        /// <summary>
        /// Task to run.Callers should create and configure it themselves
        /// </summary>
        public Task<T> Task { get; set; }
        /// <summary>
        /// Will be called whenever the execution completed
        /// </summary>
        public Action<Task<T>> OnCompletedCallback { get; set; }
        public TaskScheduler OnCompletedCallbackScheduler { get; set; }
        public CancellationTokenSource TokenSource { get; set; }
        /// <summary>
        /// Cancel this task,blocking the current thread,mostly use <see cref="System.Windows.Threading.Dispatcher.PushFrame(System.Windows.Threading.DispatcherFrame)"/>
        /// <para>Default behavior:</para>
        /// <list type="bullet">
        /// Will block this thread with <see cref="System.Windows.Threading.Dispatcher.PushFrame(System.Windows.Threading.DispatcherFrame)"/> if it's on main thread
        /// </list>
        /// <list type="bullet">
        /// Will <see cref="SpinWait"/> otherwise
        /// </list>
        /// </summary>
        /// <param name="UIThread">
        /// The UI thread,if null,will specify the main thread
        /// </param>
        public void CancelWithSync(Thread UIThread = null);
        public void Start();
    }
}
