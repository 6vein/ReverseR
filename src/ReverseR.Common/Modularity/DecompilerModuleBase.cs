using Newtonsoft.Json;
using Prism.Ioc;
using ReverseR.Common.DecompUtilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Prism.Modularity;

namespace ReverseR.Common.Modularity
{
    /// <summary>
    /// Helper and boilerplate class for creating a decompiler module
    /// Register the decompiler with the default view implementation as 
    /// <see cref="GlobalUtils.DecompilerInfo.ViewType"/>
    /// </summary>
    public abstract class DecompilerModuleBase : ModuleBase,IModule
    {
        protected abstract string Id { get; }
        protected abstract string FriendlyName { get; }
        protected abstract (Type jvmDecompiler,Type embeddedDecompiler) Decompilers { get; }
        public override void Initialized(IContainerProvider containerProvider)
        {
        }
        protected void RegisterDecompilerHelper<TPreference>(string jsonConfigPath) where TPreference : ICommonPreferences
        {
            ICommonPreferences preferences;
            if (File.Exists(GlobalUtils.GlobalConfig.ConfigPrefix + jsonConfigPath))
                preferences = JsonConvert.DeserializeObject(File.ReadAllText(GlobalUtils.GlobalConfig.ConfigPrefix + "fernflower,json"), typeof(TPreference)) as ICommonPreferences;
            else preferences = _container.Resolve<TPreference>();
            GlobalUtils.RegisterDecompiler(Id, FriendlyName, preferences, Decompilers);
            _container.Resolve<IDecompilerResolver>()
                .Register(Id, Decompilers.jvmDecompiler, Decompilers.embeddedDecompiler);
        }

    }
}
