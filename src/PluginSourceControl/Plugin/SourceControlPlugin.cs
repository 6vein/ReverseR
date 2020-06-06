using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Prism.Ioc;
using Prism.Events;
using Prism.Regions;
using ReverseR.Common;
using ReverseR.Common.ViewUtilities;
using ReverseR.Common.Events;
using Xceed.Wpf.AvalonDock.Layout;
using System.Collections.ObjectModel;

namespace PluginSourceControl.Plugin
{
    public class SourceControlPlugin:IDockablePlugin
    {
        protected IContainerProvider Container { get;private set; }
        public AnchorableShowStrategy Side { get; set; }
        public object View { get; set; }
        public IPluginViewModel ViewModel { get; set; }
        public IDecompileViewModel ParentViewModel { get; set; }
        public IDockablePlugin.NotifyOptions NotifyOption => IDockablePlugin.NotifyOptions.ArchiveOpened;
        public string PluginName => "SourceControl";
        public string RegionName { get; set; }
        public double InitialWidth => 45;
        public double InitialHeight { get; }
        public void InitalizePlugin(IDecompileViewModel parent)
        {
            ParentViewModel = parent;
            RegionName = $"{PluginName}{{{ParentViewModel.Guid.ToString()}}}";
            var view = Container.Resolve<Views.ViewSourceControl>();
            ViewModel = view.DataContext as IPluginViewModel;
            ViewModel.Parent = parent;
            View = view;
            Container.Resolve<IRegionManager>().AddToRegion(RegionName, View);
        }

        void OnArchiveOpened(string baseDir)
        {
            (ViewModel as ViewModels.ViewSourceControlViewModel).UpdateSourceTree(baseDir);
        }

        public ObservableCollection<IMenuViewModel> InsertPluginMenu()
        {
            return null;
        }

        public SourceControlPlugin()
        {
            Container = this.GetIContainer();
            Container.Resolve<IEventAggregator>().GetEvent<ArchiveOpenedEvent>().Subscribe(OnArchiveOpened, ThreadOption.BackgroundThread);
        }
    }
}
