using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Prism.Ioc;
using Prism.Modularity;

namespace ReverseR.Common.Modularity
{
    /// <summary>
    /// Base class for every module,inherit this class if you want to configure the <see cref="AppDomainSetup.PrivateBinPath"/> correctly.
    /// The Plugins folder usually looks like this
    /// <list type="table">
    /// <item>
    /// YourModuleName\YourModuleDependencies
    /// </item>
    /// <item>
    /// YourModuleName.dll
    /// </item>
    /// </list>
    /// And you should set <see cref="ModuleName"/> to let it perform correctly
    /// </summary>
    public abstract class ModuleBase : IModule
    {
        public abstract string ModuleName { get; }

        public void OnInitialized(IContainerProvider containerProvider)
        {
            
            Initialized(containerProvider);
        }
        public abstract void Initialized(IContainerProvider containerProvider);
        public abstract void RegisterTypes(IContainerRegistry containerRegistry);

        public ModuleBase()
        {
#pragma warning disable 0618
            var moduleManager = APIHelper.GetIContainer().Resolve<IModuleManager>();
            IModuleInfo thisModuleInfo = moduleManager.Modules.
                FirstOrDefault(mi => mi.ModuleType == GetType().AssemblyQualifiedName);
            //We need to modify it runtime
            AppDomain.CurrentDomain.AppendPrivatePath($"Plugins\\{thisModuleInfo.ModuleName}");
#pragma warning restore 0618
        }
    }
}
