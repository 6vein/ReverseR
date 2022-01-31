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
                Decompiler.Decompile(BaseDirectory + "\\raw.jar", (str) => { MessageWhole = str; });
                
            }
            else
            {
                FirePluginNotification(IPlugin.NotifyOptions.ArchiveOpened);
            }
        }

        protected override IDocumentViewModel _InnerOpenDocument(JPath path)
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
            viewModel.AttachDecompileTask(Container.Resolve<IBackgroundTaskBuilder>()
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
                        result = Decompiler.Decompile(tempPath, r => MessageWhole = r, token, BaseDirectory + "\\raw.jar");
                        if (result.ResultCode == DecompileResultEnum.Success)
                        {
                            var files = Directory.GetFiles(result.OutputDir);
#if DEBUG
                            Debug.Assert(files.Length == 1);
#endif
                            string newFileName = files[0];
                            if (EnableDecompiledFileCache)
                            {
                                newFileName = Path.Combine(basedir, Path.GetFileNameWithoutExtension(files[0]) + ".java");
                                File.Copy(files[0], newFileName, true);
                                MapSourceToMd5.Add(newFileName, APIHelper.GetMd5Of(newFileName));
                            }
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
                        }
                    }
                    catch (Exception e)
                    {
                        if (e is OperationCanceledException)
                        {
                            Documents.Remove(viewModel);
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
                        }
                    }
                    if (token.HasValue)
                    {
                        token.Value.ThrowIfCancellationRequested();
                    }
                    Directory.Delete(tempPath, true);
                },viewModel.DecompTaskTokenSource.Token)
                .WithName($"{Path.GetFileName(path.Path)}")
                .WithDescription($"Decompiler {Decompiler.GetDecompilerInfo().FriendlyName}")
                .Build());
            viewModel.BackgroundTask.Start();
            return viewModel;
        }
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

        protected override void InitializeSelf()
        {
        }

        protected override ObservableCollection<IMenuViewModel> _InternalMenuUpdate()
        {
            return new ObservableCollection<IMenuViewModel>();
        }
        #endregion
    }
}
