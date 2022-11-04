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
using System.Xml.Serialization;
using System.IO;
using System.Runtime.Remoting.Messaging;

namespace PluginSourceControl.Plugin
{
    public class SourceControlPlugin : IDockablePlugin
    {
        public string Id => "6168218c.SourceControlPlugin";
        protected IContainerProvider Container { get; private set; }
        public AnchorableShowStrategy Side { get; set; }
        public object View { get; set; }
        public IPluginViewModel ViewModel { get; set; }
        public IDecompileViewModel ParentViewModel { get; set; }
        public IDockablePlugin.NotifyOptions NotifyOption => IDockablePlugin.NotifyOptions.ArchiveOpened;
        public string PluginName => "SourceControl";
        public string RegionName { get; set; }
        public double InitialWidth => 45;
        public double InitialHeight => 56;
        public void InitalizePlugin(IDecompileViewModel parent)
        {
            ParentViewModel = parent;
            RegionName = $"{PluginName}{{{ParentViewModel.Guid}}}";
            var view = Container.Resolve<Views.ViewSourceControl>();
            ViewModel = view.DataContext as IPluginViewModel;
            ViewModel.Parent = parent;
            View = view;
            Container.Resolve<IRegionManager>().AddToRegion(RegionName, View);
        }
        public void UnloadPlugin()
        {
            Container.Resolve<IRegionManager>().Regions[RegionName].RemoveAll();
        }

        void OnArchiveOpened((string baseDir, Guid guid) payload)
        {
            (ViewModel as ViewModels.ViewSourceControlViewModel).UpdateSourceTree(payload.baseDir);
        }

        public ObservableCollection<IMenuViewModel> InsertPluginMenu()
        {
            return null;
        }

        public SourceControlPlugin()
        {
            Container = this.GetIContainer();
            Container.Resolve<IEventAggregator>().GetEvent<ArchiveOpenedEvent>()
                .Subscribe(OnArchiveOpened, ThreadOption.BackgroundThread, false,
                payload => payload.Item2 == ParentViewModel?.Guid
                );
            //TODO
            Side = AnchorableShowStrategy.Left;
        }
    }
}
