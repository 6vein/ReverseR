using Newtonsoft.Json;
using Prism.Ioc;
using ReverseR.Common.DecompUtilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReverseR.Common.Modularity
{
    /// <summary>
    /// Helper class for creating a decompiler module
    /// Register the decompiler with the default view implementation as 
    /// <see cref="ReverseR.Common.CommonStorage.DecompilerInfo.ViewType"/>
    /// </summary>
    public abstract class DecompilerModuleBase<TPreference> : ModuleBase where TPreference:ICommonPreferences
    {
        public abstract string FriendlyName { get; }
        protected abstract (Type jvmDecompiler,Type embeddedDecompiler) Decompilers { get; }

        private IContainerProvider _container;

        public override void Initialized(IContainerProvider containerProvider)
        {
            _container = containerProvider;
        }

        public override void RegisterTypes(IContainerRegistry containerRegistry)
        {
            ICommonPreferences preferences;
            if (File.Exists(GlobalUtils.GlobalConfig.ConfigPrefix + "fernflower,json"))
                preferences = JsonConvert.DeserializeObject(File.ReadAllText(GlobalUtils.GlobalConfig.ConfigPrefix + "fernflower,json"), typeof(TPreference)) as ICommonPreferences;
            else preferences = _container.Resolve<TPreference>();
            GlobalUtils.RegisterDecompiler(FriendlyName, preferences);
            _container.Resolve<IDecompilerResolver>()
                .Register(FriendlyName, Decompilers.jvmDecompiler, Decompilers.embeddedDecompiler);
        }
    }
}
