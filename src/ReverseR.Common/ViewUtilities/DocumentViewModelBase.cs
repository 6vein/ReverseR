using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Prism.Mvvm;
using Prism.Commands;
using Prism.Ioc;
using Prism.Services.Dialogs;
using Newtonsoft.Json;
using ReverseR.Common.Services;
using ReverseR.Common;
using ReverseR.Common.Code;
using System.Threading;
using System.Windows.Threading;
using System.Diagnostics;
using ReverseR.Common.DecompUtilities;

namespace ReverseR.Common.ViewUtilities
{
    /// <summary>
    /// Default implementation of <see cref="IDocumentViewModel"/>,supports cancelling and 
    /// <para>Classes inheriting this class should implement text editor themselves</para>
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    public abstract class DocumentViewModelBase : BindableBase, IDocumentViewModel
    {
        public IDecompileViewModel Parent { get; set; }
        private string _path;
        /// <summary>
        /// Path shown on the title, can be used for recovering
        /// </summary>
        [JsonProperty]
        public string Path { get => _path; set => SetProperty(ref _path, value); }
        protected IContainerProvider Container { get; set; }
        #region Bindings
        private string _title;
        [JsonProperty]
        public string Title { get => _title; set => SetProperty(ref _title, value); }
        private bool _isLoading;
        public bool IsLoading { get => _isLoading; set => SetProperty(ref _isLoading, value); }
        private bool _hasCancelled;
        public bool HasCancelled { get => _hasCancelled; set => SetProperty(ref _hasCancelled, value); }
        private string _LoaderText;
        public string LoaderText { get => _LoaderText; set => SetProperty(ref _LoaderText, value); }
        /// <summary>
        /// Return true to proceed the close operation
        /// </summary>
        public Func<bool> PreClosingCallback { get; set; }

        /// <summary>
        /// Cancel command,also serves as a close command
        /// </summary>
        public DelegateCommand CancelCommand => new DelegateCommand(() =>
        {
            IsLoading = true;
            HasCancelled = true;
            LoaderText = "Cancelling...";
            //Cleanup();
            Parent.CloseDocument(this);
        });
        public DelegateCommand CloseCommand => new DelegateCommand(() =>
          {
              //Cleanup();
              Parent.CloseDocument(this);
          });
        public virtual void AttachDecompileTask(IBackgroundTask decompileTask)
        {
            IsLoading = true;
            LoaderText = "Loading...";
            BackgroundTask = decompileTask;
        }
        public virtual IBackgroundTask GetAttachedDecompileTask()
        {
            return BackgroundTask;
        }
        #endregion
        #region FileOperations
        public JPath JPath { get; protected set; }
        public string InternalFilePath { get; protected set; }
        protected ICodeCompletion CompletionProvider { get; set; }
        protected Task LoadTask { get; set; }
        public abstract Task<string> GetContentAsync();
        public virtual Task LoadAsync(string path, JPath classPath)
        {
            JPath = classPath;
            InternalFilePath = path;
            LoadTask = _InnerLoadAsync(path, classPath);
            return LoadTask;
        }
        public abstract Task _InnerLoadAsync(string path, JPath classPath);

        /// <summary>
        /// Cancels the background task
        /// </summary>
        public virtual void Cleanup()
        {
            /*if (TokenSource != null && DecompileTask != null && DecompileTask.Status == TaskStatus.Running) 
            {
                try
                {
                    TokenSource.Cancel();
                    DispatcherFrame frame = new DispatcherFrame();
                    Func<object, object> callback = obj =>
                     {
                         (obj as DispatcherFrame).Continue = !DecompileTask.IsCompleted;
                         return null;
                     };
                    Dispatcher.CurrentDispatcher.BeginInvoke(DispatcherPriority.Loaded, callback, frame);
                    Dispatcher.PushFrame(frame);
                }
                catch (Exception)
                {
                    if(DecompileTask.IsFaulted)
                    {
                        Container.Resolve<IDialogService>().
                        ReportError($"Uncaught exception:{DecompileTask.Exception.ToString()}\nMessage:{DecompileTask.Exception.Message}",
                        r => { }, DecompileTask.Exception.StackTrace);
                    }
                }
            }*/
            DecompTaskTokenSource?.Cancel();
            BackgroundTask.WaitUntilComplete(Thread.CurrentThread);
            BackgroundTask = null;
        }
        public bool Close(bool forceClose)
        {
            if (((PreClosingCallback?.Invoke()) ?? true) || forceClose)
            {
                Cleanup();
                return true;
            }
            return false;
        }
        #endregion
        #region Threading
        public CancellationTokenSource DecompTaskTokenSource { get; set; }
        public IBackgroundTask BackgroundTask { get; set; }
        #endregion
        public DocumentViewModelBase()
        {
            Container = this.GetIContainer();
            CompletionProvider = Container.Resolve<ICodeCompletion>();
            LoaderText = "Loading...";
        }
    }
}
