using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Prism.Mvvm;
using Prism.Commands;
using Prism.Services.Dialogs;
using ReverseR.Common;
using ReverseR.Common.ViewUtilities;
using System.Collections.ObjectModel;
using ReverseR.Common.DecompUtilities;
using System.IO;
using System.Reflection;

namespace ReverseR.ViewModels
{
    internal interface ISettingsViewModel
    {
        public bool VerifyData();
        public void Save();
    }
    internal class ModularitySettingsViewModel: BindableBase, ISettingsViewModel
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
        public bool VerifyData()
        {
            return false;
        }
        public void Save()
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
    internal class EnvironmentSettingsViewModel : BindableBase,ISettingsViewModel
    {
        private string _configPrefix;
        public string ConfigPrefix { get=>_configPrefix; set=>SetProperty(ref _configPrefix,value); }
        private string _CachePath; 
        public string CachePath { get=>_CachePath; set=>SetProperty(ref _CachePath,value); }
        private bool _CacheDecompiledFiles; 
        public bool CacheDecompiledFiles { get=>_CacheDecompiledFiles; set=>SetProperty(ref _CacheDecompiledFiles,value); }
        private bool _CacheExtractedFiles; 
        public bool CacheExtractedFiles { get=>_CacheExtractedFiles; set=>SetProperty(ref _CacheExtractedFiles,value); }
        private bool _DecompileWhole; 
        public bool DecompileWhole { get=>_DecompileWhole; set=>SetProperty(ref _DecompileWhole,value); }
        private string _JavaPath; 
        public string JavaPath { get=>_JavaPath; set=>SetProperty(ref _JavaPath,value); }
        private ICommonPreferences.RunTypes _RunType; 
        public ICommonPreferences.RunTypes RunType { get=>_RunType; set=>SetProperty(ref _RunType,value); }
        public bool VerifyData()
        {

            return true;
        }
        public void Save()
        {
            GlobalUtils.GlobalConfig.RunType = RunType;
            GlobalUtils.GlobalConfig.ConfigPrefix = ConfigPrefix;
            GlobalUtils.GlobalConfig.CachePath = CachePath;
            GlobalUtils.GlobalConfig.JavaPath = JavaPath;
            GlobalUtils.GlobalConfig.CacheDecompiledFiles = CacheDecompiledFiles;
            GlobalUtils.GlobalConfig.CacheExtractedFiles = CacheExtractedFiles;
            GlobalUtils.GlobalConfig.DecompileWhole = DecompileWhole;
        }
        public EnvironmentSettingsViewModel()
        {
            RunType = GlobalUtils.GlobalConfig.RunType;
            ConfigPrefix = GlobalUtils.GlobalConfig.ConfigPrefix;
            CachePath = GlobalUtils.GlobalConfig.CachePath;
            JavaPath= GlobalUtils.GlobalConfig.JavaPath;
            CacheDecompiledFiles = GlobalUtils.GlobalConfig.CacheDecompiledFiles;
            CacheExtractedFiles = GlobalUtils.GlobalConfig.CacheExtractedFiles;
            DecompileWhole=GlobalUtils.GlobalConfig.DecompileWhole;
        }
    }
    internal class DecompilerSettingsViewModel : BindableBase, ISettingsViewModel
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
        
        public void Save()
        {
            
        }

        public bool VerifyData()
        {
            return false;
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
        ISettingsViewModel _content;
        public ISettingsViewModel Content { get => _content; set => SetProperty(ref _content, value); }
    }
    internal class SettingsDialogViewModel: DialogViewModelBase
    {
        ObservableCollection<SettingsTreeNode> _treeNodes;
        public ObservableCollection<SettingsTreeNode> TreeNodes 
        { get => TreeNodes; set => SetProperty(ref _treeNodes, value); }
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
        }
        public override void OnDialogOpened(IDialogParameters parameters)
        {
            base.OnDialogOpened(parameters);
        }
        protected override void CloseDialog(string parameter)
        {
        }
        public DelegateCommand OKCommand => new DelegateCommand(() =>
        {
            ApplyChanges();
            RaiseRequestClose(new DialogResult(ButtonResult.OK));
        });
        public DelegateCommand CancelCommand => new DelegateCommand(() =>
        {
            RaiseRequestClose(new DialogResult(ButtonResult.Cancel));
        });
        public DelegateCommand ApplyCommand => new DelegateCommand(ApplyChanges);
        protected void ApplyChanges()
        {
            if (ActiveNode.Content.VerifyData())
            {
                ActiveNode.Content.Save();
            }
        }
    }
}
