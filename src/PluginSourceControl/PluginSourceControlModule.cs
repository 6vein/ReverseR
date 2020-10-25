using PluginSourceControl.Views;
using Prism.Ioc;
using Prism.Modularity;
using Prism.Regions;
using ReverseR.Common;
using ReverseR.Common.Modularity;

namespace PluginSourceControl
{
    public class PluginSourceControlModule : ModuleBase
    {
        public override string DependencyPath => "PluginSourceControl";
        public override void Initialized(IContainerProvider containerProvider)
        {
 
        }

        public override void RegisterTypes(IContainerRegistry containerRegistry)
        {
            GlobalUtils.RegisterDockablePlugin(typeof(Plugin.SourceControlPlugin));
        }
    }
}