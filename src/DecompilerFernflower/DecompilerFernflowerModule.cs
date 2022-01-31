using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Prism.Ioc;
using ReverseR.Common.Modularity;
using ReverseR.Common.DecompUtilities;
using ReverseR.Common;
using Newtonsoft.Json;
using System.IO;
using Prism.Modularity;
using DecompilerFernflower.Decompile;
using DecompilerFernflower.Decompile.Internal;

namespace DecompilerFernflower
{
    public class DecompilerFernflowerModule : DecompilerModuleBase,IModule
    {
        public override string DependencyPath => "DecompilerFernflower";
        protected override string FriendlyName => "Fernflower";
        protected override string Id => typeof(FernflowerDecompiler).FullName;

        protected override (Type jvmDecompiler, Type embeddedDecompiler) Decompilers
            => (typeof(JVMFernflowerDecompiler), typeof(IKVMFernflowerDecompiler));

        public override void RegisterTypes(IContainerRegistry containerRegistry)
        {
            RegisterDecompilerHelper<FernflowerPreferences>("fernflower.json");
        }
    }
}
