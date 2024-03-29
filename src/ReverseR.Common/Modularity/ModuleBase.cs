﻿using System;
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
    /// If you want to change the behavior, you should set <see cref="DependencyPath"/> to let it perform correctly
    /// </summary>
    public abstract class ModuleBase : IModule
    {
        public virtual string DependencyPath { get; }

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
            string catalogModuleName = thisModuleInfo.ModuleName.EndsWith("Module")
                ? thisModuleInfo.ModuleName.Remove(thisModuleInfo.ModuleName.Length - 6) : thisModuleInfo.ModuleName;
            string modulePath = string.IsNullOrEmpty(DependencyPath) ? catalogModuleName : DependencyPath;
            //We need to modify it runtime
            AppDomain.CurrentDomain.
                AppendPrivatePath($"Plugins\\{modulePath}");
#pragma warning restore 0618
        }
    }
}
