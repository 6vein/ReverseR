using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Prism.Mvvm;
using Prism.Ioc;
using Prism.Commands;
using Prism.Services.Dialogs;
using ReverseR.Common;
using ReverseR.Common.ViewUtilities;
using ReverseR.Common.Services;
using System.Collections.ObjectModel;
using ReverseR.Common.DecompUtilities;
using System.IO;
using System.Reflection;

namespace ReverseR.ViewModels
{
    internal abstract class SettingsViewModelBase:BindableBase
    {
        /// <summary>
        /// Verifies the data,return null or empty to indicate success.
        /// </summary>
        /// <returns></returns>
        public abstract string GetVerifyState();
        public abstract void Save();
        public DelegateCommand<System.Windows.Controls.TextBox> BrowseFolderCommand =>
            new DelegateCommand<System.Windows.Controls.TextBox>(text =>
            {
                using (System.Windows.Forms.FolderBrowserDialog dialog
                = new System.Windows.Forms.FolderBrowserDialog())
                {
                    if(Directory.Exists(text.Text))
                        dialog.SelectedPath = text.Text;
                    dialog.ShowNewFolderButton = true;
                    dialog.ShowDialog();
                    if (CheckFolderWriteAccess(dialog.SelectedPath))
                        text.Text = dialog.SelectedPath;
                    else
                    {
                        this.GetIContainer()
                            .Resolve<IDialogService>()
                            .ReportError($"The Path {dialog.SelectedPath} is not accessible!", _ => { });
                    }
                }
            });
        public virtual DelegateCommand<System.Windows.Controls.TextBox> BrowseFileCommand => new DelegateCommand<System.Windows.Controls.TextBox>(text =>
          {

          });
        protected bool CheckFolderWriteAccess(string dir)
        {
            try
            {
                using(FileStream fs = File.Create(
                    Path.Combine(dir, Path.GetRandomFileName()), 1, FileOptions.DeleteOnClose))
                {
                    fs.WriteByte(1);
                }
            }
            catch
            {
                return false;
            }
            return true;
        }
    }
    internal class ModularitySettingsViewModel: SettingsViewModelBase
    {
        public class ModuleEnableInfo:BindableBase
        {
            public GlobalUtils.RRModuleInfo ModuleInfo { get; set; }
            public bool Enabled { get => ModuleInfo.Enabled; set { ModuleInfo.Enabled = value;RaisePropertyChanged(); }}
        }
        private string _ModulePath;
        public string ModuleDirectory { get => _ModulePath; set => SetProperty(ref _ModulePath, value); }
        private ObservableCollection<ModuleEnableInfo> _ModuleNames;
        public ObservableCollection<ModuleEnableInfo> ModuleInfos { get => _ModuleNames; set => SetProperty(ref _ModuleNames, value); }
        public GlobalUtils.RRModuleInfo CreateModuleInfo(string path)
        {
            if (File.Exists(path))
            {
                AppDomain childDomain = Internal.Modularity.DefaultModuleCatalog.BuildChildDomain(AppDomain.CurrentDomain);
                try
                {
                    List<string> loadedAssemblies = new List<string>();

                    var assemblies = (
                                         from Assembly assembly in AppDomain.CurrentDomain.GetAssemblies()
                                         where !(assembly is System.Reflection.Emit.AssemblyBuilder)
                                            && assembly.GetType().FullName != "System.Reflection.Emit.InternalAssemblyBuilder"
                                            // TODO: Do this in a less hacky way... probably never gonna happen
                                            && !assembly.GetName().Name.StartsWith("xunit")
                                            && !string.IsNullOrEmpty(assembly.Location)
                                         select assembly.Location
                                     );

                    loadedAssemblies.AddRange(assemblies);

                    Type loaderType = typeof(Internal.Modularity.DefaultModuleCatalog.InnerModuleInfoLoader);

                    if (loaderType.Assembly != null)
                    {
                        var loader =
                            (Internal.Modularity.DefaultModuleCatalog.InnerModuleInfoLoader)
                            childDomain.CreateInstanceFrom(loaderType.Assembly.Location, loaderType.FullName).Unwrap();
                        loader.LoadAssemblies(loadedAssemblies);
                        var items = loader.GetModuleInfos(new string[] { path });
                        if(items != null && items.Length > 0)
                        {
                            string name = loader.GetModuleId(items[0]);
                            if(name!= null)
                            {
                                return new GlobalUtils.RRModuleInfo()
                                {
                                    Id = name,
                                    Path = path,
                                    Enabled = true
                                };
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    throw ex;
                }
                finally
                {
                    AppDomain.Unload(childDomain);
                }
                throw new ArgumentException($"{path} does not contain a valid Module!");
            }
            else
            {
                throw new ArgumentException($"{path} is not a valid path!");
            }
        }
        public override string GetVerifyState()
        {
            return null;//normally it will be processed when adding modules to the catalog.
        }
        public override void Save()
        {
            GlobalUtils.GlobalConfig.ModuleDirectory = ModuleDirectory;
            GlobalUtils.GlobalConfig.ModuleInfos = ModuleInfos.Select(info => info.ModuleInfo).ToArray();
        }
        ModularitySettingsViewModel()
        {
            ModuleDirectory = GlobalUtils.GlobalConfig.ModuleDirectory;
            ModuleInfos = new ObservableCollection<ModuleEnableInfo>(
                GlobalUtils.GlobalConfig.ModuleInfos.Select(info => new ModuleEnableInfo()
            {
                ModuleInfo = new GlobalUtils.RRModuleInfo(info)
            }));
        }
    }
    internal class EnvironmentSettingsViewModel : SettingsViewModelBase
    {
        private string _configPrefix;
        public string ConfigPrefix { get=>_configPrefix; set=>SetProperty(ref _configPrefix,value); }
        private string _CachePath; 
        public string CachePath { get=>_CachePath; set=>SetProperty(ref _CachePath,value); }
        private bool _CacheDecompiledFiles; 
        public bool CacheDecompiledFiles { get=>_CacheDecompiledFiles; set=>SetProperty(ref _CacheDecompiledFiles,value); }
        private bool _CacheExtractedFiles; 
        public bool CacheExtractedFiles { get=>_CacheExtractedFiles; set=>SetProperty(ref _CacheExtractedFiles,value); }

        public override string GetVerifyState()
        {
            string errorReport = "";
            if (!Directory.Exists(Environment.ExpandEnvironmentVariables(ConfigPrefix)))
            {
                errorReport += "Config directory is not a valid path!\n";
            }
            if (!Directory.Exists(Environment.ExpandEnvironmentVariables(CachePath)))
            {
                errorReport += "Config directory is not a valid path!\n";
            }
            return errorReport;
        }
        public override void Save()
        {
            GlobalUtils.GlobalConfig.ConfigPrefix = ConfigPrefix;
            GlobalUtils.GlobalConfig.CachePath = CachePath;
            GlobalUtils.GlobalConfig.CacheDecompiledFiles = CacheDecompiledFiles;
            GlobalUtils.GlobalConfig.CacheExtractedFiles = CacheExtractedFiles;
        }
        public EnvironmentSettingsViewModel()
        {
            ConfigPrefix = GlobalUtils.GlobalConfig.ConfigPrefix;
            CachePath = GlobalUtils.GlobalConfig.CachePath;
            CacheDecompiledFiles = GlobalUtils.GlobalConfig.CacheDecompiledFiles;
            CacheExtractedFiles = GlobalUtils.GlobalConfig.CacheExtractedFiles;
        }
    }
    internal class DecompileGeneralViewModel : SettingsViewModelBase
    {
        private bool _DecompileWhole;
        public bool DecompileWhole { get => _DecompileWhole; set => SetProperty(ref _DecompileWhole, value); }
        private string _JavaPath;
        public string JavaPath { get => _JavaPath; set => SetProperty(ref _JavaPath, value); }
        private int _runtypeIndex = 0;
        public int RunTypeIndex { get => _runtypeIndex; set => SetProperty(ref _runtypeIndex, value); }
        private int _RunTypeIndex;
        public int RunType { get => _RunTypeIndex; set => SetProperty(ref _RunTypeIndex, value); }
        public string[] RunTypes { get; } = new string[]
        { ICommonPreferences.RunTypes.JVM.ToString(),ICommonPreferences.RunTypes.IKVM.ToString() };

        private int _preferredDecompilerIndex = 0;
        public int PreferredDecompilerIndex { get => _preferredDecompilerIndex; set=>SetProperty(ref _preferredDecompilerIndex, value); }
        public string PreferredDecompilerId => DecompilerIds[_preferredDecompilerIndex];
        public string[] DecompilerIds { get; } = GlobalUtils.Decompilers.Select(it => it.Id).ToArray();
        public override string GetVerifyState()
        {
            return null;
        }
        public override DelegateCommand<System.Windows.Controls.TextBox> BrowseFileCommand => new DelegateCommand<System.Windows.Controls.TextBox>(text =>
        {
            using (System.Windows.Forms.OpenFileDialog dialog
                = new System.Windows.Forms.OpenFileDialog())
            {
                if (File.Exists(text.Text))
                    dialog.InitialDirectory = Directory.GetDirectoryRoot(text.Text);
                dialog.CheckFileExists = true;
                dialog.CheckPathExists = true;
                dialog.AutoUpgradeEnabled = true;
                dialog.Filter = "Java.exe|java.exe";
                if(dialog.ShowDialog()==System.Windows.Forms.DialogResult.OK)
                {
                    if (File.Exists(dialog.FileName))
                        text.Text = dialog.FileName;
                    else
                    {
                        this.GetIContainer()
                            .Resolve<IDialogService>()
                            .ReportError($"The file {dialog.FileName} is not accessible!", _ => { });
                    }
                }
            }
        });
        public override void Save()
        {
            GlobalUtils.GlobalConfig.RunType = RunType == 0 ? ICommonPreferences.RunTypes.JVM : ICommonPreferences.RunTypes.IKVM;
            GlobalUtils.GlobalConfig.PreferredDecompilerId = PreferredDecompilerId;
            GlobalUtils.GlobalConfig.JavaPath = JavaPath;
            GlobalUtils.GlobalConfig.DecompileWhole = DecompileWhole;
        }
        public DecompileGeneralViewModel()
        {
            RunType = (int)GlobalUtils.GlobalConfig.RunType;
            PreferredDecompilerIndex = GlobalUtils.Decompilers
                .FindIndex(info => info.Id == GlobalUtils.GlobalConfig.PreferredDecompilerId);
            JavaPath = GlobalUtils.GlobalConfig.JavaPath;
            DecompileWhole = GlobalUtils.GlobalConfig.DecompileWhole;
        }
    }
    internal class DecompilerSettingsViewModel : SettingsViewModelBase
    {
        int decompilerInfoIndex;
        public string Name => GlobalUtils.Decompilers[decompilerInfoIndex].FriendlyName;
        public ICommonPreferences Preferences => GlobalUtils.Decompilers[decompilerInfoIndex].Options;
        internal class ObservableArgument : BindableBase,ICommonPreferences.IArgument
        {
            public string Name { get; set; }
            public string Description { get; set; }
            /// <summary>
            /// List of available values, if <see cref="ValueIndex"/> == -1,
            /// then the first element of the item is the selected value,
            /// which also indicates the argument is of type 'string'
            /// </summary>
            public string[] AvailableValues { get; }
            private int _valueIndex;
            public int ValueIndex { get=> _valueIndex;set=>SetProperty(ref _valueIndex,value); }
            public string Content { get => AvailableValues[0]; set => SetProperty(ref AvailableValues[0], value); }
            public ObservableArgument(ICommonPreferences.IArgument argument)
            {
                Name = argument.Name;
                Description = argument.Description;
                AvailableValues = argument.AvailableValues.ToArray();
                ValueIndex = argument.ValueIndex;
            }

            public string GetArgument()
            {
                throw new NotImplementedException();
            }
        }
        ObservableCollection<ObservableArgument> _arguments;
        public ObservableCollection<ObservableArgument> Arguments { get => _arguments; set => SetProperty(ref _arguments,value); }
        public DecompilerSettingsViewModel(int index)
        {
            decompilerInfoIndex = index;
            Arguments = new ObservableCollection<ObservableArgument>(Preferences.GetArguments().Select(it => new ObservableArgument(it)));
        }
        public override void Save()
        {
            Preferences.SetArguments(Arguments);
        }

        public override string GetVerifyState()
        {
            var invalids = Preferences.GetInvalidArguments(Arguments);
            if (invalids.Count() == 0)
            {
                return null;
            }
            else
            {
                return string.Join(",", invalids.Select(it => it.Name)) + " not valid!";
            }
        }
    }
    internal class SettingsTreeNode : BindableBase
    {
        #region Bindings
        string _text;
        public string Text { get => _text; set => SetProperty(ref _text, value); }
        ObservableCollection<SettingsTreeNode> _children;
        public ObservableCollection<SettingsTreeNode> Children { get => _children; set => SetProperty(ref _children, value); }
        bool _isSelected;
        public bool IsSelected { get => _isSelected; set => SetProperty(ref _isSelected, value); }
        bool _isExpanded;
        public bool IsExpanded { get => _isExpanded; set => SetProperty(ref _isExpanded, value); }
        #endregion
        SettingsViewModelBase _content;
        public SettingsViewModelBase Content { get => _content; set => SetProperty(ref _content, value); }
    }
    internal class LazyLoadTreeNode : SettingsTreeNode
    {
        public bool Loaded { get; set; } = false;
        public Action<LazyLoadTreeNode> LoadAction { get; set; }
        public void Load()
        {
            LoadAction?.Invoke(this);
            Loaded = true;
        }
    }
    internal class SettingsDialogViewModel: DialogViewModelBase
    {
        ObservableCollection<SettingsTreeNode> _treeNodes;
        public ObservableCollection<SettingsTreeNode> TreeNodes 
        { get => _treeNodes; set => SetProperty(ref _treeNodes, value); }
        SettingsTreeNode _activeNode;
        public SettingsTreeNode ActiveNode { get => _activeNode; set => SetProperty(ref _activeNode, value); }
        public SettingsDialogViewModel()
        {
            Title = "Preferences";

            TreeNodes = new ObservableCollection<SettingsTreeNode>();
            TreeNodes.Add(new SettingsTreeNode()
            {
                Text = "Environment",
                Children = new ObservableCollection<SettingsTreeNode>()
                {
                    new SettingsTreeNode()
                    {
                        Text="General",
                        Content=new EnvironmentSettingsViewModel()
                    }
                }
            });
            TreeNodes.Add(new LazyLoadTreeNode()
            {
                Text = "Decompile",
                Children = new ObservableCollection<SettingsTreeNode>(
                    new List<SettingsTreeNode>()
                    {
                        new SettingsTreeNode()
                        {
                            Text="General",
                            Content=new DecompileGeneralViewModel()
                        }
                    }),
                LoadAction = node =>
                {
                    node.Children = new ObservableCollection<SettingsTreeNode>(
                        node.Children.Concat(GlobalUtils.Decompilers.Select((it, i) => new SettingsTreeNode()
                        {
                            Text = it.FriendlyName,
                            Content = new DecompilerSettingsViewModel(i)
                        })));
                }
            });
            TreeNodes[0].IsSelected = true;
            SetActiveNode(TreeNodes[0]);
        }
        public override void OnDialogOpened(IDialogParameters parameters)
        {
            base.OnDialogOpened(parameters);
        }
        protected override void CloseDialog(string parameter)
        {
        }
        public DelegateCommand<RoutedPropertyChangedEventArgs<object>> SelectChangedCommand 
            => new DelegateCommand<RoutedPropertyChangedEventArgs<object>>(e =>
          {
              SetActiveNode(e.NewValue as SettingsTreeNode);
          });
        protected void SetActiveNode(SettingsTreeNode node)
        {
            ActiveNode = node;
            if(node is LazyLoadTreeNode lazyNode)
            {
                lazyNode.Load();
            }
            if (ActiveNode.Children != null && ActiveNode.Children.Count > 0)
            {
                ActiveNode.IsExpanded = true;
                ActiveNode = ActiveNode.Children[0];
            }
        }
        public DelegateCommand OKCommand => new DelegateCommand(() =>
        {
            if (ApplyChanges())
                RaiseRequestClose(new DialogResult(ButtonResult.OK));
        });
        public DelegateCommand CancelCommand => new DelegateCommand(() =>
        {
            RaiseRequestClose(new DialogResult(ButtonResult.Cancel));
        });
        protected bool ApplyChanges()
        {
            Queue<SettingsTreeNode> nodes = new Queue<SettingsTreeNode>(TreeNodes);
            while(nodes.Count > 0)
            {
                var node = nodes.Dequeue();
                if(node.Children!=null&&node.Children.Count > 0)
                {
                    foreach (var child in node.Children)
                        nodes.Enqueue(child);
                    continue;
                }
                string str = node.Content.GetVerifyState();
                if (string.IsNullOrEmpty(str))
                {
                    node.Content.Save();
                }
                else
                {
                    this.GetIContainer().Resolve<IDialogService>()
                        .ReportError($"Error in {node.Text} \n {str}", _ => { });
                    return false;
                }
            }
            return true;
        }
    }
}
