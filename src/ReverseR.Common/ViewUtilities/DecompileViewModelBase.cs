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

namespace ReverseR.Common.ViewUtilities
{
    /// <summary>
    /// Default implemention of <see cref="IDecompileViewModel"/>
    /// This implemention extracts all the content into the cache,and automatically check if it's modified
    /// </summary>
    public abstract class DecompileViewModelBase:BindableBase,IDecompileViewModel
    {
        #region PrismIoc
        protected IContainerProvider Container { get;private set; }
        #endregion
        #region Bindings
        private bool _isWholeLoaderOpen;
        public bool IsWholeLoaderOpen
        {
            get { return _isWholeLoaderOpen; }
            set { SetProperty(ref _isWholeLoaderOpen, value); }
        }

        private string _messageWhole;
        public string MessageWhole
        {
            get { return _messageWhole; }
            set { SetProperty(ref _messageWhole, value); }
        }
        //TimeSpan _loaderWaitTime;
        //public TimeSpan LoaderWaitTime { get => _loaderWaitTime; set => SetProperty(ref _loaderWaitTime, value); }
        #endregion
        #region FileOperations
        public string FilePath { get => _filePath; set => SetProperty(ref _filePath, value); }
        public FileTypes FileType { get; set; }
        public string Md5 { get; set; }
        /// <summary>
        /// name of the base directory,doesn't include "\\"
        /// </summary>
        public string BaseDirectory { get; set; }
        public string ContentDirectory { get; set; }
        public Dictionary<string, string> MapSourceToMd5 { get;private set; }
        public Action PreOpenfileCallback { get; set; }
        public Action AfterOpenfileCallback { get; set; }
        public abstract string DecompileViewName { get; protected set; }
        public void OnOpenFile(Tuple<string,FileTypes,Guid> tuple)
        {
            IsWholeLoaderOpen = true;
            FilePath = tuple.Item1;
            FileType = tuple.Item2;
            Title = Path.GetFileName(FilePath) + $"[{DecompileViewName}]";
            //TitleTooltip = FilePath;
            try
            {
                Md5 = APIHelper.GetMd5Of(FilePath);
                BaseDirectory = GlobalUtils.GlobalConfig.CachePath + $"\\{Md5}";
                ContentDirectory = BaseDirectory + "\\Content";
                Directory.CreateDirectory(BaseDirectory);
                Directory.CreateDirectory(ContentDirectory);
                if (FileType == FileTypes.Jar)
                {
                    File.Copy(FilePath, BaseDirectory + "\\raw.jar", true);
                    FastZipEvents events = new FastZipEvents();
                    events.CompletedFile = (s, e) =>
                    {
                    };
                    (new FastZip(events)).ExtractZip(BaseDirectory + "\\raw.jar", ContentDirectory, "");
                    //need background worker
                    /*using (var zipArchive = System.IO.Compression.ZipFile.OpenRead(BaseDirectory + "\\raw.jar"))
                    {
                        foreach(var entry in zipArchive.Entries)
                        {
                            string path = ContentDirectory + $"\\{entry.FullName.Replace('/', '\\')}";
                            var stream = entry.Open();
                            if(stream!=null)
                            {
                                stream.Close();
                                if (MapPathToMd5.ContainsKey(path))
                                {
                                    if (GetMd5Of(path) != MapPathToMd5[path])
                                    {
                                        entry.ExtractToFile(path, true);
                                    }
                                }
                                else
                                {
                                    entry.ExtractToFile(path, true);
                                    MapPathToMd5.Add(path, GetMd5Of(path));
                                }
                            }
                            else
                            {
                                Directory.CreateDirectory(path);
                            }
                        }
                        File.WriteAllText(BaseDirectory + "\\Md5.json", JsonConvert.SerializeObject(MapPathToMd5, Formatting.Indented));
                    }*/
                }
                else
                {
                    File.Copy(FilePath, ContentDirectory + "\\" + Path.GetFileName(FilePath));
                }
                if(File.Exists(BaseDirectory+"\\sourceMap.json"))
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
                Container.Resolve<IDialogService>().ReportError($"An error has occoured:\n{e.Message}", r => { }, e.StackTrace);
            }
            PreOpenfileCallback?.Invoke();
            this.HandleOpenFile();
            AfterOpenfileCallback?.Invoke();
            IsWholeLoaderOpen = false;
        }

        public abstract void HandleOpenFile();


        #endregion
        #region UserInterface
        private ObservableCollection<IDocumentViewModel> _documents = new ObservableCollection<IDocumentViewModel>();
        public ObservableCollection<IDocumentViewModel> Documents { get => _documents; set => SetProperty(ref _documents, value); }
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
        public List<IPlugin> Plugins { get;protected set; } = new List<IPlugin>();
        protected abstract IDocumentViewModel _InnerOpenDocument(JPath path);
        public IDocumentViewModel OpenDocument(JPath path)
        {
            return _InnerOpenDocument(path);
        }
        protected abstract void InitalizePlugins();
        protected ContentControl CreatePluginRegion(IDockablePlugin plugin)
        {
            ContentControl contentControl = new ContentControl();
            RegionManager.SetRegionName(contentControl, $"{plugin.PluginName}{{{Guid.ToString()}}}");
            RegionManager.SetRegionManager(contentControl, Container.Resolve<IRegionManager>());
            return contentControl;
        }
        protected abstract void InitializeSelf();
        public void Initalize()
        {
            InitalizePlugins();
            InitializeSelf();
        }

        protected void FirePluginNotification(IDockablePlugin.NotifyOptions option)
        {
            foreach(IDockablePlugin plugin in Plugins)
            {
                if(plugin.NotifyOption.HasFlag(option))
                {
                    Container.Resolve<IEventAggregator>().GetEvent<ArchiveOpenedEvent>().Publish(BaseDirectory);
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
                if(menu!=null)
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

        public virtual void OnUnload()
        {
            throw new NotImplementedException();
        }
        public abstract void ActivateDocument(IDocumentViewModel documentViewModel);
        public void CloseDocument(IDocumentViewModel documentViewModel, bool ForceClose = false)
        {
            if (documentViewModel.Closing()||ForceClose)
                Documents.Remove(documentViewModel);
        }
        #endregion
        public DecompileViewModelBase()
        {
            //LoaderWaitTime = new TimeSpan(0, 0, 1);
            MessageWhole = "Loading...";
            Container = this.GetIContainer();
            Container.Resolve<IEventAggregator>().GetEvent<OpenFileEvent>().Subscribe(OnOpenFile, ThreadOption.BackgroundThread,false,filter=>filter.Item3==Guid);
            Container.Resolve<IEventAggregator>().GetEvent<ViewActivatedEvent>().Subscribe(guid => OnActivated(), ThreadOption.UIThread, false, filter => filter == this.Guid);
            foreach(GlobalUtils.DockablePluginInfo info in GlobalUtils.DockablePlugins)
            {
                Plugins.Add(Container.Resolve(info.PluginType) as IDockablePlugin);
            }
        }
    }
}
