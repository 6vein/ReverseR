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
        public abstract string VerifyData();
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
        public virtual DelegateCommand BrowseFileCommand => new DelegateCommand(() =>
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
            public GlobalUtils.ModuleInfo ModuleInfo { get; set; }
            public bool Enabled { get => ModuleInfo.Enabled; set { ModuleInfo.Enabled = value;RaisePropertyChanged(); }}
        }
        private string _ModulePath;
        public string ModuleDirectory { get => _ModulePath; set => SetProperty(ref _ModulePath, value); }
        private ObservableCollection<ModuleEnableInfo> _ModuleNames;
        public ObservableCollection<ModuleEnableInfo> ModuleInfos { get => _ModuleNames; set => SetProperty(ref _ModuleNames, value); }
        public GlobalUtils.ModuleInfo CreateModuleInfo(string path)
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
                                return new GlobalUtils.ModuleInfo()
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
        public override string VerifyData()
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
                ModuleInfo = new GlobalUtils.ModuleInfo(info)
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

        public override string VerifyData()
        {
            string errorReport = "";
            if (!File.Exists(Environment.ExpandEnvironmentVariables(ConfigPrefix)))
            {
                errorReport += "Config directory is not a valid path!\n";
            }
            if (!File.Exists(Environment.ExpandEnvironmentVariables(CachePath)))
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
        private ICommonPreferences.RunTypes _RunType;
        public ICommonPreferences.RunTypes RunType { get => _RunType; set => SetProperty(ref _RunType, value); }
        public string[] RunTypes = new string[]
        { ICommonPreferences.RunTypes.JVM.ToString(),ICommonPreferences.RunTypes.IKVM.ToString() };

        private int _preferredDecompilerIndex = 0;
        public int PreferredDecompilerIndex { get => _preferredDecompilerIndex; set=>SetProperty(ref _preferredDecompilerIndex, value); }
        public string PreferredDecompilerId => Decompilers[_preferredDecompilerIndex].Id;
        public List<GlobalUtils.DecompilerInfo> Decompilers => GlobalUtils.Decompilers;
        public override string VerifyData()
        {

            return null;
        }
        public override void Save()
        {
            GlobalUtils.GlobalConfig.RunType = RunType;
            GlobalUtils.GlobalConfig.PreferredDecompilerId = PreferredDecompilerId;
            GlobalUtils.GlobalConfig.JavaPath = JavaPath;
            GlobalUtils.GlobalConfig.DecompileWhole = DecompileWhole;
        }
        public DecompileGeneralViewModel()
        {
            RunType = GlobalUtils.GlobalConfig.RunType;
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
        internal class ObservableArgument : BindableBase
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
        }
        public DecompilerSettingsViewModel(int index)
        {
            decompilerInfoIndex = index;
        }
        public override void Save()
        {
            
        }

        public override string VerifyData()
        {
            return null;
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
            TreeNodes.Add(new SettingsTreeNode()
            {
                Text = "Decompile",
                Children = new ObservableCollection<SettingsTreeNode>()
                {
                    new SettingsTreeNode()
                    {
                        Text="General",
                        Content=new DecompileGeneralViewModel()
                    }
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
                string str = node.Content.VerifyData();
                if (string.IsNullOrEmpty(str))
                {
                    node.Content.Save();
                }
                else
                {
                    this.GetIContainer().Resolve<IDialogService>()
                        .ReportError($"Error saving settings:\n{str}", _ => { });
                    return false;
                }
            }
            return true;
        }
    }
}
