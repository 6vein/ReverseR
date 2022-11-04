using ICSharpCode.SharpZipLib.Zip;
using Newtonsoft.Json;
using Prism.Events;
using Prism.Ioc;
using Prism.Mvvm;
using Prism.Regions;
using Prism.Services.Dialogs;
using ReverseR.Common.DecompUtilities;
using ReverseR.Common.Events;
using ReverseR.Common.Extensibility;
using ReverseR.Common.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Controls;
using Xceed.Wpf.AvalonDock.Layout;
using ReverseR.Common.Code;
using System.Diagnostics;
using System.ComponentModel;
using System.Text.RegularExpressions;
using ReverseR.Common.Collections;

namespace ReverseR.Common.ViewUtilities
{
    /// <summary>
    /// Default implemention of <see cref="IDecompileViewModel"/>
    /// This implemention extracts all the content into the cache,and automatically check if it's modified
    /// </summary>
    public abstract class DecompileViewModelBase : BindableBase, IDecompileViewModel
    {
        #region PrismIoc
        protected IContainerProvider Container { get; private set; }
        #endregion
        #region Bindings
        private bool _isWholeLoaderOpen;
        public bool IsWholeLoaderOpen
        {
            get { return _isWholeLoaderOpen; }
            set { SetProperty(ref _isWholeLoaderOpen, value); }
        }

        private string _statusMessage;
        public string StatusMessage
        {
            get { return _statusMessage; }
            set { SetProperty(ref _statusMessage, value); }
        }
        //TimeSpan _loaderWaitTime;
        //public TimeSpan LoaderWaitTime { get => _loaderWaitTime; set => SetProperty(ref _loaderWaitTime, value); }
        #endregion
        #region FileOperations
        protected bool EnableExtractionCache { get; set; }
        protected bool EnableDecompiledFileCache => GlobalUtils.GlobalConfig.CacheDecompiledFiles;
        public string FilePath { get => _filePath; set => SetProperty(ref _filePath, value); }
        public abstract CommonDecompiler Decompiler { get; protected set; }
        public FileTypes FileType { get; set; }
        public string Md5 { get; set; }
        /// <summary>
        /// name of the base directory,doesn't include "\\"
        /// </summary>
        public string BaseDirectory { get; set; }
        public string ContentDirectory { get; set; }
        public Dictionary<string, string> MapSourceToMd5 { get; private set; }
        public Action PreOpenfileCallback { get; set; }
        public Action AfterOpenfileCallback { get; set; }
        public abstract string DecompileViewName { get; protected set; }
        public ParseTreeNode ASTEntry { get; protected set; }
        public void OnOpenFile(Tuple<string, FileTypes, Guid> tuple)
        {
            IsWholeLoaderOpen = true;
            FilePath = tuple.Item1;
            FileType = tuple.Item2;
            StatusMessage = "Opening " + FilePath;
            Title = Path.GetFileName(FilePath) + $"[{DecompileViewName}]";
            EnableExtractionCache = GlobalUtils.GlobalConfig.CacheExtractedFiles;
            //TitleTooltip = FilePath;
            try
            {
                Md5 = APIHelper.GetMd5OfIncludingPath(FilePath);
                if (EnableExtractionCache)
                    BaseDirectory = GlobalUtils.GlobalConfig.CachePath + $"\\{Md5}";
                else
                    BaseDirectory = Path.GetTempPath();
                ContentDirectory = BaseDirectory + "\\Content";
                Directory.CreateDirectory(BaseDirectory);
                Directory.CreateDirectory(ContentDirectory);
                ASTEntry = new ParseTreeNode();
                ASTEntry.ItemType = IClassParser.ItemType.Directory;
                ASTEntry.Id = "<entry>";
                ASTEntry.SetPaths(ContentDirectory, "");
                if (FileType == FileTypes.Jar)
                {
#if DEBUG
                    File.Copy(FilePath, BaseDirectory + "\\raw.jar", true);
                    FastZipEvents events = new FastZipEvents();
                    (new FastZip(events)).ExtractZip(BaseDirectory + "\\raw.jar", ContentDirectory, FastZip.Overwrite.Never, _ => false, "", "", false); ;
#else
                    File.Copy(FilePath, BaseDirectory + "\\raw.jar", true);
                    FastZipEvents events = new FastZipEvents();
                    (new FastZip(events)).ExtractZip(BaseDirectory + "\\raw.jar", ContentDirectory, "");
#endif
                    //scan
                    ContentDirectory += '\\';//simple workaround to avoid classpath leading by '/'
                    ASTEntry.Children = new List<ParseTreeNode>(ScanContent(ContentDirectory));
                    ContentDirectory.Remove(ContentDirectory.Length - 1);
                }
                else
                {
                    File.Copy(FilePath, ContentDirectory + "\\" + Path.GetFileName(FilePath));

                }
                if (File.Exists(BaseDirectory + "\\sourceMap.json"))
                {
                    MapSourceToMd5 = JsonConvert.DeserializeObject<Dictionary<string, string>>(File.ReadAllText(BaseDirectory + "\\sourceMap.json"));
                }
                else
                {
                    MapSourceToMd5 = new Dictionary<string, string>();
                }
            }
            catch (Exception e)
            {
                Container.Resolve<IDialogService>().ReportError(e, r => { }, e.StackTrace);
            }
            System.Windows.Application.Current.Dispatcher.Invoke(() =>
            {
                Initialize();
            });
            PreOpenfileCallback?.Invoke();
            this.HandleOpenFile();
            AfterOpenfileCallback?.Invoke();
            IsWholeLoaderOpen = false;
        }

        public abstract void HandleOpenFile();


#endregion
#region UserInterface
        private PartiallyObservableCollection<IDocumentViewModel> _documents = new PartiallyObservableCollection<IDocumentViewModel>();
        public PartiallyObservableCollection<IDocumentViewModel> Documents { get => _documents; set => SetProperty(ref _documents, value); }
        string _title;
        public string Title
        {
            get { return _title; }
            set { SetProperty(ref _title, value); }
        }
        string _filePath;
        //public string TitleTooltip { get => _titletooltip; set => SetProperty(ref _titletooltip, value); }
#endregion
#region Extensibility
        public Guid Guid { get; private set; } = Guid.NewGuid();
        public List<IPlugin> Plugins { get; protected set; } = new List<IPlugin>();
        protected abstract IDocumentViewModel _InnerOpenDocument(IJPath path);
        public IDocumentViewModel OpenDocument(IJPath path)
        {
            return _InnerOpenDocument(path);
        }
        protected abstract void InitalizePlugins();
        protected abstract void UnloadPlugins();
        protected Controls.ViewRegionControl CreatePluginRegion(IDockablePlugin plugin)
        {
            Controls.ViewRegionControl contentControl = new Controls.ViewRegionControl();
            RegionManager.SetRegionName(contentControl, $"{plugin.PluginName}{{{Guid.ToString()}}}");
            RegionManager.SetRegionManager(contentControl, Container.Resolve<IRegionManager>());
            return contentControl;
        }
        protected abstract void InitializeSelf();
        protected abstract void UnloadSelf();
        protected abstract void LoadLayout();
        protected abstract void SaveLayout();
        public void Initialize()
        {
            LoadLayout();
            InitalizePlugins();
            InitializeSelf();
        }
        public void Unload()
        {
            SaveLayout();
            UnloadPlugins();
            UnloadSelf();
            //clean up
            if (Directory.Exists(BaseDirectory))
            {
                if (!EnableExtractionCache)
                {
                    Directory.Delete(BaseDirectory, true);
                }
                else if (EnableDecompiledFileCache)
                {
                    //save states
                    string json = JsonConvert.SerializeObject(MapSourceToMd5);
                    File.WriteAllText(BaseDirectory + "\\sourceMap.json", json);
                }
            }
        }

        protected void FirePluginNotification(IDockablePlugin.NotifyOptions option)
        {
            foreach (IDockablePlugin plugin in Plugins)
            {
                if (plugin.NotifyOption.HasFlag(option))
                {
                    Container.Resolve<IEventAggregator>().GetEvent<ArchiveOpenedEvent>().Publish((BaseDirectory, Guid));
                }
            }
        }

        public virtual void PublishMenuUpdate()
        {
            ObservableCollection<IMenuViewModel> menus = new ObservableCollection<IMenuViewModel>();


            menus.AddRange(_InternalMenuUpdate());

            foreach (IPlugin plugin in Plugins)
            {
                var menu = plugin.InsertPluginMenu();
                if (menu != null)
                {
                    menus.AddRange(menu);
                }
            }

            Container.Resolve<IEventAggregator>().GetEvent<MenuUpdatedEvent>().
                Publish((menus, Guid));
        }


        protected abstract ObservableCollection<IMenuViewModel> _InternalMenuUpdate();

        public virtual void OnActivated()
        {
            PublishMenuUpdate();
        }
        public virtual bool Shutdown(bool forceClose)
        {
            if (forceClose)
            {
                foreach (IDocumentViewModel document in Documents)
                {
                    CloseDocument(document, forceClose);
                }
                Unload();
                return true;
            }
            else
            {
                bool closed = true;
                foreach (IDocumentViewModel document in Documents)
                {
                    closed &= CloseDocument(document, forceClose);
                }
                if (closed) Unload();
                return closed;
            }
        }
        public abstract IDocumentViewModel ActiveDocument { get; protected set; }
        public abstract void ActivateDocument(IDocumentViewModel documentViewModel);
        public bool CloseDocument(IDocumentViewModel documentViewModel, bool ForceClose = false)
        {
            if (documentViewModel.Close(ForceClose) || ForceClose)
            {
                return true;
            }
            return false;
        }
#endregion
        public DecompileViewModelBase()
        {
            //LoaderWaitTime = new TimeSpan(0, 0, 1);
            StatusMessage = "Loading...";
            Container = this.GetIContainer();
            Container.Resolve<IEventAggregator>().GetEvent<OpenFileEvent>().Subscribe(OnOpenFile, ThreadOption.BackgroundThread, false, filter => filter.Item3 == Guid);
            Container.Resolve<IEventAggregator>().GetEvent<ViewActivatedEvent>().Subscribe(guid => OnActivated(), ThreadOption.UIThread, false, filter => filter == this.Guid);
            Container.Resolve<IEventAggregator>().GetEvent<MenuCanExecuteEvent>().Subscribe(payload => HandleMenuCanExecuteEvent(payload.Item2)
            , ThreadOption.PublisherThread, false, filter => filter.Item1 == Guid);
            Container.Resolve<IEventAggregator>().GetEvent<MenuExecuteEvent>().Subscribe(payload => HandleMenuExecuteEvent(payload.Item2)
            , ThreadOption.PublisherThread, false, filter => filter.Item1 == Guid);
            foreach (GlobalUtils.DockablePluginInfo info in GlobalUtils.DockablePlugins)
            {
                Plugins.Add(Container.Resolve(info.PluginType) as IDockablePlugin);
            }
        }
        protected abstract void HandleMenuCanExecuteEvent(CanExecuteRoutedEventArgs e);
        protected abstract void HandleMenuExecuteEvent(ExecutedRoutedEventArgs e);
        protected async virtual Task<IEnumerable<ParseTreeNode>> ParseCompilationUnitAsync(IJPath jPath)
        {
            var builder =
                        this.GetIContainer()
                        .Resolve<IBackgroundTaskBuilder<IEnumerable<ParseTreeNode>>>();
            IBackgroundTask<IEnumerable<ParseTreeNode>> parseTask = builder
                .WithTask(_ =>
                {
                    IEnumerable<ParseTreeNode> ret = null;
                    var basedir = Path.GetDirectoryName(jPath.Path);
                    var tempPath = Path.GetTempFileName();
                    File.Delete(tempPath);
                    Directory.CreateDirectory(tempPath);
                    File.Copy(jPath.Path, Path.Combine(tempPath, Path.GetFileName(jPath.Path)));
                    if (jPath.InnerClassPaths != null)
                    {
                        foreach (var p in jPath.InnerClassPaths)
                        {
                            File.Copy(p, Path.Combine(tempPath, Path.GetFileName(p)));
                        }
                    }
                    IDecompileResult result = null;
                    try
                    {
                        result = this.GetIContainer()
                            .Resolve<IDecompilerResolver>()
                            .Resolve<CommonDecompiler>((GlobalUtils.PreferredDecompiler ?? GlobalUtils.Decompilers[0]).Id)
                            .Decompile(tempPath, r => { }, null, BaseDirectory + "\\raw.jar");
                        if (result.ResultCode == DecompileResultEnum.Success)
                        {
                            var files = Directory.GetFiles(result.OutputDir);
#if DEBUG
                            Debug.Assert(files.Length == 1);
#endif
                            ret = Container
                                .Resolve<IClassParser>()
                                .SetBasePath(jPath)
                                .Parse(File.ReadAllText(files[0]));
                        }
                        else
                        {
                            this.GetIContainer().Resolve<IDialogService>().ReportError(result.ResultCode.ToString(), __ => { });
                        }
                    }
                    catch (Exception e)
                    {
                        string message = null;
                        if (e is Win32Exception win32Exception)
                        {
                            message = $"Unexpected exception:\n{win32Exception.Message}\nHRESULT is {System.Runtime.InteropServices.Marshal.GetHRForLastWin32Error().ToString("x")}";
                        }
                        else message = $"Unexpected exception:\n{e.Message}\n";
                        this.GetIContainer().Resolve<IDialogService>().ReportError(message, r => { }, e.StackTrace);
                    }
                    Directory.Delete(tempPath, true);
                    return ret;
                })
                .WithName("Background:Parsing")
                .WithDescription("Parsing " + jPath.ClassPath)
                .Build();
            parseTask.Start();

            await parseTask.IsCompletedTask;
            return parseTask.Result;
        }
        IEnumerable<ParseTreeNode> ScanContent(string root)
        {
            IList<ParseTreeNode> nodes=new List<ParseTreeNode>();
            foreach(string directory in Directory.GetDirectories(root))
            {
                ParseTreeNode node = new ParseTreeNode();
                node.ItemType = IClassParser.ItemType.Directory;
                node.Id = Path.GetFileName(directory);
                node.SetPaths(directory, directory.Replace(ContentDirectory, ""));
                node.Children = new List<ParseTreeNode>(ScanContent(directory));
                nodes.Add(node);
            }
            var fileIgnore = new List<string>();
            var files = Directory.GetFiles(root);
            foreach (string path in files)
            {
                ParseTreeNode node = new ParseTreeNode();
                if (fileIgnore.Contains(path))
                    continue;
                if (Path.GetExtension(path).ToLower() == ".class")
                {
                    Regex regex = new Regex(@"(\w+)\$\w+");
                    Match match = regex.Match(Path.GetFileNameWithoutExtension(path));
                    if (match.Success)
                    {
                        Regex engine = new Regex($@"{match.Result("$1")}\$\w+");
                        node.ItemType = IClassParser.ItemType.CompilationUnit;
                        node.SetPaths(Path.Combine(Path.GetDirectoryName(path), match.Result("$1") + Path.GetExtension(path)),
                            Path.Combine(Path.GetDirectoryName(path).Replace(ContentDirectory, ""), Path.GetFileNameWithoutExtension(path)));
                        node.SetInnerClasses(files.Where(s => engine.Match(Path.GetFileNameWithoutExtension(s)).Success));
                        node.Id = match.Result("$1");
                        node.Children.Add(new ParseTreeNode() { ItemType = IClassParser.ItemType.__InternalPlaceHolder });
                        //add to the ignore list to prevent from duplicating
                        fileIgnore.Add((string)node.Path);
                        fileIgnore.AddRange(node.InnerClassPaths);
                    }
                    else
                    {
                        node.ItemType = IClassParser.ItemType.CompilationUnit;
                        node.SetPaths(path, Path.Combine(Path.GetDirectoryName(path).Replace(ContentDirectory, ""), Path.GetFileNameWithoutExtension(path)));
                        node.Id = Path.GetFileNameWithoutExtension(path);
                        node.Children.Add(new ParseTreeNode() { ItemType = IClassParser.ItemType.__InternalPlaceHolder });
                    }
                }
                else
                {
                    node.ItemType = IClassParser.ItemType.Others;
                    node.SetPaths(path, "");
                    node.Id = Path.GetFileName(path);
                }
                nodes.Add(node);
            }
            return nodes;
        }
        public async Task<ParseTreeNode> GetParseTreeAsync(string classPath, bool parseCompilationUnit = false)
        {
            if(classPath == "")
            {
                return ASTEntry;
            }
            LinkedList<string> periods = new LinkedList<string>(classPath.Split('/'));
            ParseTreeNode root = ASTEntry;
            //locate the node
            while(periods.Count>0&&root.Children.Count > 0)
            {
                root = root.Children.First(item => item.Id == periods.First());
                periods.RemoveFirst();
            }

            if (root.ItemType==IClassParser.ItemType.CompilationUnit
                &&root.Children.Count == 1 
                && root.Children[0].ItemType == IClassParser.ItemType.__InternalPlaceHolder
                &&parseCompilationUnit)
            {
                root.Children.RemoveAt(0);
                root.Children = (await ParseCompilationUnitAsync(root)).ToList();
            }

            return root;
        }
    }
}
