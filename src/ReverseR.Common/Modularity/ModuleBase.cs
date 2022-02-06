using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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
    /// If you want to change the behavior, you should set <see cref="DependencyPath"/> to let it perform correctly
    /// </summary>
    public abstract class ModuleBase : IModule
    {
        protected IContainerProvider _container;
        [Obsolete]
        public virtual string DependencyPath { get; }
        public abstract string Id { get; }
        public virtual string Name => GetType().Namespace.Substring(GetType().Namespace.LastIndexOf('.') + 1);
        public virtual string Description => "";

        public void OnInitialized(IContainerProvider containerProvider)
        {
            GlobalUtils.Modules.Where(info => info.Id == Id).First().Name = Name;
            GlobalUtils.Modules.Where(info => info.Id == Id).First().Description = Description;
            Initialized(containerProvider);
        }


        public virtual void Initialized(IContainerProvider containerProvider) { }
        public abstract void RegisterTypes(IContainerRegistry containerRegistry);

        public ModuleBase()
        {
#pragma warning disable 0618
#pragma warning disable 0612
            _container = APIHelper.GetIContainer();
            var moduleManager = _container.Resolve<IModuleManager>();
            IModuleInfo thisModuleInfo = moduleManager.Modules.
                FirstOrDefault(mi => mi.ModuleType == GetType().AssemblyQualifiedName);
            string catalogModuleName = thisModuleInfo.ModuleName.EndsWith("Module")
                ? thisModuleInfo.ModuleName.Remove(thisModuleInfo.ModuleName.Length - 6) : thisModuleInfo.ModuleName;
            string modulePath = string.IsNullOrEmpty(DependencyPath) ? catalogModuleName : DependencyPath;
            //We need to modify it runtime
            AppDomain.CurrentDomain.
                AppendPrivatePath($"Plugins\\{modulePath}");
#pragma warning restore 0612
#pragma warning restore 0618
        }
    }
}
