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
        protected override string FriendlyName => "Fernflower";
        public override string Id => $"6168218c.{nameof(DecompilerFernflower)}";
        protected override string DecompilerId => $"org.jetbrains.fernflower";

        protected override (Type jvmDecompiler, Type embeddedDecompiler) Decompilers
            => (typeof(JVMFernflowerDecompiler), typeof(IKVMFernflowerDecompiler));

        public override void RegisterTypes(IContainerRegistry containerRegistry)
        {
            RegisterDecompilerHelper<FernflowerPreferences>("fernflower.json");
        }
    }
}
