using ReverseR.Common;
using ReverseR.Common.Services;
using ReverseR.Common.ViewUtilities;
using ReverseR.Common.DecompUtilities;
using ReverseR.Common.Extensibility;
using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using ICSharpCode.SharpZipLib.Zip;
using Prism.Ioc;
using Prism.Regions;
using Prism.Services.Dialogs;
using Xceed.Wpf.AvalonDock.Layout;
using Xceed.Wpf.AvalonDock;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Input;
using System.Windows.Media;
using System.Threading;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Windows.Data;
using System.Diagnostics;
using ReverseR.Common.Controls;
using System.Xml;
using System.Text;
using ReverseR.Common.Code;

namespace ReverseR.DecompileView.Default.ViewModels
{
    public class ViewDecompileViewModel : DecompileViewModelBase,IDefaultViewModel
    {
        public override CommonDecompiler Decompiler { get;protected set; }
        public void SetDecompiler(string id)
        {
            if (id == Decompiler?.Id)
                return;
            Decompiler = Container
                .Resolve<IDecompilerResolver>()
                .Resolve<CommonDecompiler>(id);
            DecompileViewName = GlobalUtils.Decompilers.Find(item => item.Id == id).FriendlyName;
        }
        public ViewDecompileViewModel()
        {
            SetDecompiler((GlobalUtils.PreferredDecompiler??GlobalUtils.Decompilers[0]).Id);
            IsWholeLoaderOpen = false;
        }
        #region FileOperations
        public override string DecompileViewName { get; protected set; }
        public override void HandleOpenFile()
        {
            if(GlobalUtils.GlobalConfig.DecompileWhole)
            {
                Decompiler.Decompile(BaseDirectory + "\\raw.jar", (str) => { StatusMessage = str; });
                
            }
            else
            {
                FirePluginNotification(IPlugin.NotifyOptions.ArchiveOpened);
            }
        }
        IDocumentViewModel CreateDocument(IJPath path)
        {
            DecompileDocumentViewModel viewModel;
            viewModel = Container.Resolve<DecompileDocumentViewModel>();
            viewModel.Parent = this;
            viewModel.Title = Path.GetFileName(path.Path);
            /*viewModel.TokenSource = new CancellationTokenSource();
            viewModel.DecompileTask = new Task(() =>
              {
                  if (MapSourceToMd5.ContainsKey(path.Path))
                  {
                      if (APIHelper.GetMd5Of(path.Path) == MapSourceToMd5[path.Path])
                      {
                          //TODO  
                      }
                  }
                  var basedir = Path.GetDirectoryName(path.Path);
                  var tempPath = Path.GetTempFileName();
                  File.Delete(tempPath);
                  Directory.CreateDirectory(tempPath);
                  File.Copy(path.Path, Path.Combine(tempPath, Path.GetFileName(path.Path)));
                  foreach(var p in path.InnerClassPaths)
                  {
                      File.Copy(p, Path.Combine(tempPath, Path.GetFileName(p)));
                  }
                  var result = Decompiler.Decompile(tempPath, r => MessageWhole = r, viewModel.TokenSource.Token, BaseDirectory + "\\raw.jar");
                  
                  if (result.ResultCode == CommonDecompiler.DecompileResultEnum.DecompileSuccess)
                  {
                      viewModel.Load(result.Output);
                  }
              });
            viewModel.DecompileTask.Start();*/
            viewModel.SetJPath(path);
            viewModel.DecompTaskTokenSource = new CancellationTokenSource();
            IBackgroundTask backgroundTask = null;
            backgroundTask = Container.Resolve<IBackgroundTaskBuilder>()
                .WithTask(async obj =>
                {
                    var token = obj as CancellationToken?;
                    if (MapSourceToMd5.ContainsKey(path.Path))
                    {
                        if (APIHelper.GetMd5Of(path.Path) == MapSourceToMd5[path.Path])
                        {
                            //TODO  
                        }
                    }
                    var basedir = Path.GetDirectoryName(path.Path);
                    var tempPath = Path.GetTempFileName();
                    File.Delete(tempPath);
                    Directory.CreateDirectory(tempPath);
                    File.Copy(path.Path, Path.Combine(tempPath, Path.GetFileName(path.Path)));
                    if (path.InnerClassPaths != null)
                    {
                        foreach (var p in path.InnerClassPaths)
                        {
                            File.Copy(p, Path.Combine(tempPath, Path.GetFileName(p)));
                        }
                    }
                    IDecompileResult result = null;
                    try
                    {
                        result = Decompiler.Decompile(tempPath,
                            r => { if (backgroundTask != null) backgroundTask.TaskDescription = r; },
                            token, BaseDirectory + "\\raw.jar");
                        if (result.ResultCode == DecompileResultEnum.Success)
                        {
                            var files = Directory.GetFiles(result.OutputDir);
#if DEBUG
                            Debug.Assert(files.Length == 1);
#endif
                            string newFileName = files[0];
                            if (EnableDecompiledFileCache)
                            {
                                newFileName = Path.Combine(basedir + '\\', Path.GetFileNameWithoutExtension(files[0]) + ".java");
                                File.Copy(files[0], newFileName, true);
                                MapSourceToMd5.Add(newFileName, APIHelper.GetMd5Of(newFileName));
                            }
                            StatusMessage = backgroundTask.TaskDescription = "Processing " + path.ClassPath;
                            await viewModel.LoadAsync(newFileName);

                            Application.Current.Dispatcher.Invoke(() =>
                            {
                                if (!(Manager.ActiveContent is IDocumentViewModel))
                                    ActivateDocument(viewModel);
                            });
                        }
                        else
                        {
                            Container.Resolve<IDialogService>().ReportError(result.ResultCode.ToString(), _ => { });
                            Application.Current.Dispatcher.Invoke(() =>
                            {
                                Documents.Remove(viewModel);
                            });
                        }
                    }
                    catch (Exception e)
                    {
                        if (e is OperationCanceledException)
                        {
                            Application.Current.Dispatcher.Invoke(() =>
                            {
                                Documents.Remove(viewModel);
                            });
                        }
                        else
                        {
                            string message = null;
                            if (e is Win32Exception win32Exception)
                            {
                                message = $"Unexpected exception:\n{win32Exception.Message}\nHRESULT is {System.Runtime.InteropServices.Marshal.GetHRForLastWin32Error().ToString("x")}";
                            }
                            else message = $"Unexpected exception:\n{e.Message}\n";
                            Container.Resolve<IDialogService>().ReportError(message, r => { }, e.StackTrace);
                            Application.Current.Dispatcher.Invoke(() =>
                            {
                                Documents.Remove(viewModel);
                            });
                        }
                    }
                    if (token.HasValue)
                    {
                        token.Value.ThrowIfCancellationRequested();
                    }
                    Directory.Delete(tempPath, true);
                    StatusMessage = "Ready";
                }, viewModel.DecompTaskTokenSource.Token)
                .WithName($"{Decompiler.GetDecompilerInfo().FriendlyName}:{Path.GetFileName(path.Path)}")
                .WithDescription($"Decompiler started")
                .Build();
            viewModel.AttachDecompileTask(backgroundTask);
            return viewModel;
        }
        protected override IDocumentViewModel _InnerOpenDocument(IJPath path)
        {
            IDocumentViewModel viewModel = CreateDocument(path);
            viewModel.GetAttachedDecompileTask().Start();
            Documents.Add(viewModel);
            return viewModel;
        }
        public override IDocumentViewModel ActiveDocument { get; protected set; }
        public override void ActivateDocument(IDocumentViewModel documentViewModel)
        {
            Manager.ActiveContent = documentViewModel;
        }
        #endregion
        #region UserInterface
        [Serializable]
        class DockOptions
        {
            public AnchorableShowStrategy Side { get; set; }
            public double Width { get; set; }
            public double Height { get; set; }
        }
        public DockingManager Manager { get; set; }
        public Dictionary<IDockablePlugin, LayoutAnchorable> MapPluginAnchor { get; protected set; } = new Dictionary<IDockablePlugin, LayoutAnchorable>();

        #endregion
        #region Extensibility
        protected override void LoadLayout()
        {
            try
            {
                using (var reader = new XmlTextReader(Path.Combine(BaseDirectory, "layout.xml")))
                {
                    Manager.Layout.ReadXml(reader);
                }
            }
            catch(Exception _)
            {
            }
        }
        protected override void SaveLayout()
        {
            try
            {
                using (var writer = new XmlTextWriter(Path.Combine(BaseDirectory, "layout.xml"),Encoding.UTF8))
                {
                    writer.WriteStartElement("DockLayouts");
                    Manager.Layout.WriteXml(writer);
                    writer.WriteEndElement();
                }
            }
            catch(Exception _) { }
        }
        protected override void InitalizePlugins()
        {
            foreach(IDockablePlugin plugin in Plugins)
            {
                LayoutAnchorable layoutAnchorable = Manager.Layout.Descendents()
                    .FirstOrDefault(elem => elem is LayoutAnchorable anchorable &&
                        anchorable.ContentId == plugin.Id) as LayoutAnchorable;
                if (layoutAnchorable == null)
                {
                    layoutAnchorable = new LayoutAnchorable();
                    layoutAnchorable.AddToLayout(Manager, plugin.Side);
                }
                layoutAnchorable.Hiding += (s, e) =>
                {
                    RaisePropertyChanged();
                };
                var container = CreatePluginRegion(plugin);
                layoutAnchorable.Content = container;
                layoutAnchorable.ContentId = plugin.Id;
                MapPluginAnchor[plugin] = layoutAnchorable;
                //(layoutAnchorable.Parent as LayoutAnchorablePane).DockMinWidth = plugin.InitialWidth;
                //(layoutAnchorable.Parent as LayoutAnchorablePane).DockMinHeight = plugin.InitialHeight;
                plugin.InitalizePlugin(this);
                Binding binding = new Binding("Title") { Source = plugin.ViewModel, Mode = BindingMode.OneWay };
                BindingOperations.SetBinding(container, ViewRegionControl.TitleProperty, binding);
            }
            foreach(IPlugin plugin in Plugins)
            {
                if(!(plugin is IDockablePlugin))
                {
                    plugin.InitalizePlugin(this);
                }
            }
        }
        protected override void UnloadPlugins()
        {
            foreach (IDockablePlugin plugin in Plugins)
            {
                //TODO:Save plugin states
                BindingOperations.ClearBinding(MapPluginAnchor[plugin].Content as DependencyObject, ViewRegionControl.TitleProperty);
                MapPluginAnchor[plugin].Hiding -= (s, e) =>
                 {
                     RaisePropertyChanged();
                 };
            }
            foreach(IPlugin plugin in Plugins)
            {
                plugin.UnloadPlugin();
            }
        }
        EventHandler ActiveContentChanged => (s, e) =>
         {
             if (Manager.ActiveContent is IDocumentViewModel vm)
             {
                 ActiveDocument = vm;
                 if (vm.GetAttachedDecompileTask() != null && !vm.GetAttachedDecompileTask().IsStarted)
                     vm.GetAttachedDecompileTask().Start();
             }
         };
        EventHandler<DocumentClosingEventArgs> DocumentClosing => (s, e) =>
             {
                 e.Cancel = !CloseDocument(e.Document.Content as IDocumentViewModel);
             };
        EventHandler<DocumentClosedEventArgs> DocumentClosed => (s, e) =>
         {
             Documents.Remove(e.Document.Content as IDocumentViewModel);
         };
        protected override void InitializeSelf()
        {
            List<IDocumentViewModel> documents = new List<IDocumentViewModel>();
            List<LayoutDocument> invalidDocuments = new List<LayoutDocument>();
            IJPath activeDocumentPath = null;
            foreach (var document in Manager.Layout.Descendents().Where(it => it is LayoutDocument)
                .Select(it=>it as LayoutDocument))
            {
                string classPath = document.ContentId;
                LinkedList<string> periods = new LinkedList<string>(classPath.Split('/'));
                ParseTreeNode root = ASTEntry;
                //locate the node
                while (periods.Count > 0 && root.Children.Count > 0)
                {
                    root = root.Children.First(item => item.Id == periods.First());
                    periods.RemoveFirst();
                }
                if (document.IsSelected)
                {
                    activeDocumentPath = root;
                }
                if (periods.Count == 0)
                {
                    IDocumentViewModel viewModel = CreateDocument(root);
                    documents.Add(viewModel);
                    document.Content = viewModel;
                }
                invalidDocuments.Add(document);
            }
            foreach(var document in invalidDocuments)
            {
                document.Parent.RemoveChild(document);
            }
            Documents.AddRange(documents);
            if (activeDocumentPath != null)
            {
                Documents.FirstOrDefault(it => it.JPath == activeDocumentPath)?.GetAttachedDecompileTask()?.Start();
            }
            Manager.ActiveContentChanged += ActiveContentChanged;
            Manager.DocumentClosing += DocumentClosing;
            Manager.DocumentClosed += DocumentClosed;
        }
        protected override void UnloadSelf()
        {
            Manager.ActiveContentChanged -= ActiveContentChanged;
            Manager.DocumentClosing -= DocumentClosing;
            Manager.DocumentClosed -= DocumentClosed;
        }
        protected override void HandleMenuCanExecuteEvent(CanExecuteRoutedEventArgs e)
        {
            if(ActiveDocument is DecompileDocumentViewModel viewModel)
            {
                if (!(viewModel.GetAttachedDecompileTask()?.IsCompleted==true) || (viewModel.IsLoading == true))
                {
                    e.CanExecute = false;
                    e.Handled= true;
                    return;
                }
                if (e.Command == ApplicationCommands.Undo)
                {
                    e.CanExecute = viewModel.EditorControl.CanUndo;
                }
                else if (e.Command == ApplicationCommands.Redo)
                {
                    e.CanExecute = viewModel.EditorControl.CanRedo;
                }
                else if (e.Command == ApplicationCommands.Cut || e.Command == ApplicationCommands.Copy || e.Command == ApplicationCommands.Paste) 
                {
                    e.CanExecute = true;
                }
                e.Handled = true;
            }
            else
            {
                e.CanExecute = false;
                e.Handled = true;
            }
        }
        protected override void HandleMenuExecuteEvent(ExecutedRoutedEventArgs e)
        {
            if (ActiveDocument is DecompileDocumentViewModel viewModel)
            {
                if (!(viewModel.GetAttachedDecompileTask()?.IsCompleted == true) || (viewModel.IsLoading == true))
                {
                    e.Handled = true;
                    return;
                }
                if (e.Command == ApplicationCommands.Undo)
                {
                    viewModel.EditorControl.Undo();
                }
                else if (e.Command == ApplicationCommands.Redo)
                {
                    viewModel.EditorControl.Redo();
                }
                else if (e.Command == ApplicationCommands.Cut)
                {
                    viewModel.EditorControl.Cut();
                }
                else if(e.Command == ApplicationCommands.Copy)
                {
                    viewModel.EditorControl.Copy();
                }
                else if(e.Command != ApplicationCommands.Paste)
                {
                    viewModel.EditorControl.Paste();
                }
                e.Handled = true;
            }
            else
            {
                e.Handled = true;
            }
        }
        protected override ObservableCollection<IMenuViewModel> _InternalMenuUpdate()
        {
            return new ObservableCollection<IMenuViewModel>();
        }
        #endregion
    }
}
