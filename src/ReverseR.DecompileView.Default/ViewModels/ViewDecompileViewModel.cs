﻿using ReverseR.Common;
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

namespace ReverseR.DecompileView.Default.ViewModels
{
    public class ViewDecompileViewModel : DecompileViewModelBase,IDefaultViewModel
    {
        public CommonDecompiler Decompiler { get; set; }
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

        protected override IDocumentViewModel _InnerOpenDocument(IJPath path)
        {
            DecompileDocumentViewModel viewModel;
            viewModel = Container.Resolve<DecompileDocumentViewModel>();
            viewModel.Parent = this;
            viewModel.Title = Path.GetFileName(path.Path);
            Documents.Add(viewModel);
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
                                newFileName = Path.Combine(basedir+'\\', Path.GetFileNameWithoutExtension(files[0]) + ".java");
                                File.Copy(files[0], newFileName, true);
                                MapSourceToMd5.Add(newFileName, APIHelper.GetMd5Of(newFileName));
                            }
                            StatusMessage = backgroundTask.TaskDescription = "Processing " + path.ClassPath;
                            await viewModel.LoadAsync(newFileName, path);

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
            viewModel.BackgroundTask.Start();
            return viewModel;
        }
        public override IDocumentViewModel ActiveDocument { get; protected set; }
        public override void ActivateDocument(IDocumentViewModel documentViewModel)
        {
            Manager.ActiveContent = documentViewModel;
        }
        #endregion
        #region UserInterface
        public DockingManager Manager { get; set; }
        public Dictionary<IDockablePlugin, LayoutAnchorable> MapPluginAnchor { get; protected set; } = new Dictionary<IDockablePlugin, LayoutAnchorable>();

        #endregion
        #region Extensibility
        protected override void InitalizePlugins()
        {
            foreach(IDockablePlugin plugin in Plugins)
            {
                LayoutAnchorable layoutAnchorable = new LayoutAnchorable();
                layoutAnchorable.Hiding += (s, e) =>
                {
                    RaisePropertyChanged();
                };
                var container = CreatePluginRegion(plugin);
                layoutAnchorable.Content = container;
                MapPluginAnchor[plugin] = layoutAnchorable;
                layoutAnchorable.AddToLayout(Manager, plugin.Side);
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
