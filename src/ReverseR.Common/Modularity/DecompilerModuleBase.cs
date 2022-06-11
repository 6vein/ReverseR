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
        protected abstract string DecompilerId { get; }
        protected abstract string FriendlyName { get; }
        protected abstract (Type jvmDecompiler,Type embeddedDecompiler) Decompilers { get; }
        public override void Initialized(IContainerProvider containerProvider)
        {
        }
        protected void RegisterDecompilerHelper<TPreference>(string jsonConfigFileName) where TPreference : ICommonPreferences
        {
            ICommonPreferences preferences = _container.Resolve<TPreference>();
            if (File.Exists(Path.Combine(GlobalUtils.GlobalConfig.ConfigPrefix, jsonConfigFileName)))
            {
                preferences.DeserializePart(File.ReadAllText(Path.Combine(GlobalUtils.GlobalConfig.ConfigPrefix, jsonConfigFileName)));
            }
            GlobalUtils.RegisterDecompiler(DecompilerId, FriendlyName, preferences, Decompilers);
            _container.Resolve<IDecompilerResolver>()
                .Register(DecompilerId, Decompilers.jvmDecompiler, Decompilers.embeddedDecompiler);
        }

    }
}
