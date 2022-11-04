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
using DecompilerCFR.Decompile;
using DecompilerCFR.Decompile.Internal;

namespace DecompilerCFR
{
    public class DecompilerCFRModule:DecompilerModuleBase
    {
        protected override string FriendlyName => "CFR";
        public override string Id => $"6168218c.{nameof(DecompilerCFR)}";
        protected override string DecompilerId => $"org.benf.cfr";

        protected override (Type jvmDecompiler, Type embeddedDecompiler) Decompilers
            => (typeof(JVMCFRDecompiler), typeof(IKVMCFRDecompiler));

        public override void RegisterTypes(IContainerRegistry containerRegistry)
        {
            RegisterDecompilerHelper<CFRPreferences>();
        }
    }
}
