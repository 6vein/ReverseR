using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.ComponentModel;

namespace ReverseR.Common.Services
{
    /// <summary>
    /// A background task that supports cancelling,and will be automatically added to the background task list
    /// Do not create instance of this interface manually,use <see cref="IBackgroundTaskBuilder"/> instead
    /// </summary>
    public interface IBackgroundTask:IDisposable
    {
        #region Bindings
        public string TaskName { get; set; }
        public string TaskDescription { get; set; }
        public bool IsCompleted { get; }
        #endregion
        /// <summary>
        /// Get the complete status of the task.ONLY USE THIS FOR SYNCHRONIZE
        /// </summary>
        public Task<bool> IsCompletedTask { get; }
        /// <summary>
        /// Calling this method to wait for the task and its completion callback to complete.
        /// Mostly used after calling <see cref="CancellationTokenSource.Cancel"/>
        /// <para>Default behavior:</para>
        /// <list type="bullet">
        /// If <paramref name="UIThread"/> is this thread,
        /// will "block" this thread with <see cref="System.Windows.Threading.Dispatcher.PushFrame(System.Windows.Threading.DispatcherFrame)"/>
        /// </list>
        /// <list type="bullet">
        /// Will <see cref="SpinWait"/> otherwise
        /// </list>
        /// </summary>
        /// <param name="UIThread">
        /// The UI thread,won't be blocked by simply calling <see cref="Task.Wait"/>.If the argument is null,will specify the main thread
        /// </param>
        public void WaitUntilComplete(Thread UIThread=null);
        public void Start();
    }

    /// <inheritdoc/>
    /// <typeparam name="TResult">Type for the Task to return</typeparam>
    public interface IBackgroundTask<TResult>:IBackgroundTask,IDisposable
    {
        /// <summary>
        /// Get the result of the task,blocking the thread if the execution is still running
        /// <para>
        /// NOTE:<see cref="Task{TResult}.Result"/> is called in default implementation.
        /// </para>
        /// </summary>
        /// <exception cref="InvalidOperationException">
        /// Occours when the object is not a generic instance
        /// </exception>
        public TResult Result { get; }
    }
}
