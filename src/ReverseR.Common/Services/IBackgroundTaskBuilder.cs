using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

namespace ReverseR.Common.Services
{
    public interface IBackgroundTaskBuilder
    {
        /// <summary>
        /// Set the action to execute for this task,the parameter of <paramref name="action"/> being nullable
        /// </summary>
        /// <param name="action">the action to execute,the first parameter being the <paramref name="token"/></param>
        /// <param name="token">the <see cref="CancellationToken"/>,can be null</param>
        /// <returns></returns>
        IBackgroundTaskBuilder WithTask(Action<object> action, CancellationToken? token=null, 
            TaskCreationOptions options = TaskCreationOptions.None);
        /// <summary>
        /// Set the action to execute for this task,the parameter of <paramref name="action"/> being nullable
        /// <para>
        /// WARNING:After calling this,you need to call <see cref="IBackgroundTaskBuilder{TResult}.WithOnCompleteCallback(Action{Task{TResult}})"/>
        /// as it will be reset to null.
        /// </para>
        /// </summary>
        /// <param name="func">the action to execute,the first parameter being the <paramref name="token"/></param>
        /// <param name="token">the <see cref="CancellationToken"/>,can be null</param>
        /// <returns></returns>
        IBackgroundTaskBuilder<TResult> WithTask<TResult>(Func<object, TResult> func, CancellationToken? token = null,
            TaskCreationOptions options = TaskCreationOptions.None);
        /// <summary>
        /// Set the callback when the work ends,regardless of whether it's success or not
        /// </summary>
        /// <param name="callback">Will be called whenever the execution completed</param>
        /// <returns></returns>
        IBackgroundTaskBuilder WithOnCompleteCallback(Action<Task> callback);
        /// <summary>
        /// Set the callback when the work ends,regardless of whether it's success or not
        /// <para>
        /// WARNING:After calling this,you need to call <see cref="IBackgroundTaskBuilder{TResult}.WithTask(Func{TResult, object}, CancellationToken?, TaskCreationOptions)"/>
        /// as it will be reset to null.
        /// </para>
        /// </summary>
        /// <param name="callback">Will be called whenever the execution completed</param>
        /// <returns></returns>
        IBackgroundTaskBuilder<TResult> WithOnCompleteCallback<TResult>(Action<Task<TResult>> callback);
        /// <summary>
        /// Set the <see cref="TaskScheduler"/> for the creation of callback task
        /// </summary>
        /// <param name="scheduler">the scheduler,<see cref="TaskScheduler.Default"/> by default</param>
        /// <returns></returns>
        IBackgroundTaskBuilder WithCallbackTaskScheduler(TaskScheduler scheduler);
        /// <summary>
        /// Set the <see cref="IBackgroundTask.TaskName"/>
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        IBackgroundTaskBuilder WithName(string name);
        /// <summary>
        /// Set the <see cref="IBackgroundTask.TaskDescription"/>
        /// </summary>
        /// <param name="description"></param>
        /// <returns></returns>
        IBackgroundTaskBuilder WithDescription(string description);
        IBackgroundTask Build();
    }

    /// <summary>
    /// The generic version of <see cref="IBackgroundTaskBuilder"/>,but must be created by <see cref="IBackgroundTaskBuilder.WithOnCompleteCallback{TResult}(Action{Task{TResult}})"/>
    /// or <see cref="IBackgroundTaskBuilder.WithTask{TResult}(Func{TResult, object}, CancellationToken?, TaskCreationOptions)"/>
    /// </summary>
    /// <typeparam name="TResult"></typeparam>
    public interface IBackgroundTaskBuilder<TResult>
    {
        /// <summary>
        /// Set the action to execute for this task
        /// </summary>
        /// <param name="func">the action to execute,the first parameter being the <paramref name="token"/></param>
        /// <param name="token">the <see cref="CancellationToken?"/></param>
        /// <returns></returns>
        IBackgroundTaskBuilder<TResult> WithTask(Func<object, TResult> func, CancellationToken? token = null,
            TaskCreationOptions options = TaskCreationOptions.None);
        /// <summary>
        /// Set the callback when the work ends,regardless of whether it's success or not
        /// </summary>
        /// <param name="callback">Will be called whenever the execution completed</param>
        /// <returns></returns>
        IBackgroundTaskBuilder<TResult> WithOnCompleteCallback(Action<Task<TResult>> callback);
        /// <summary>
        /// Set the <see cref="TaskScheduler"/> for the creation of callback task
        /// </summary>
        /// <param name="scheduler">the scheduler,<see cref="TaskScheduler.Default"/> by default</param>
        /// <returns></returns>
        IBackgroundTaskBuilder<TResult> WithCallbackTaskScheduler(TaskScheduler scheduler);
        /// <summary>
        /// Set the <see cref="IBackgroundTask{TResult}.TaskName"/>
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        IBackgroundTaskBuilder<TResult> WithName(string name);
        /// <summary>
        /// Set the <see cref="IBackgroundTask{TResult}.TaskDescription"/>
        /// </summary>
        /// <param name="description"></param>
        /// <returns></returns>
        IBackgroundTaskBuilder<TResult> WithDescription(string description);
        IBackgroundTask<TResult> Build();

    }
}
