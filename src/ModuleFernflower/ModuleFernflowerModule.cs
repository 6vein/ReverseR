using ModuleFernflower.Views;
using ModuleFernflower.Decompile;
using Prism.Ioc;
using Prism.Modularity;
using Prism.Regions;
using ReverseR.Common;
using ReverseR.Common.DecompUtilities;
using System.IO;
using Newtonsoft.Json;
using ReverseR.Common.Modularity;

namespace ModuleFernflower
{
    public class ModuleFernflowerModule : ModuleBase
    {
        public override string DependencyPath => "ModuleFernflower";
        public override void Initialized(IContainerProvider containerProvider)
        {
        }

        public override void RegisterTypes(IContainerRegistry containerRegistry)
        {
            containerRegistry.Register<FernflowerDecompiler,Decompile.Internal.JVMFernflowerDecompiler>(CommonDecompiler.RunTypes.JVM.ToString());
            ICommonPreferences preferences;
            if (File.Exists(GlobalUtils.GlobalConfig.ConfigPrefix + "fernflower,json"))
                preferences = JsonConvert.DeserializeObject(File.ReadAllText(GlobalUtils.GlobalConfig.ConfigPrefix + "fernflower,json")) as FernflowerPreferences;
            else preferences = new FernflowerPreferences();
            //Actually we should register default Decompiler based on default preference's runtype
            containerRegistry.Register<FernflowerDecompiler, Decompile.Internal.JVMFernflowerDecompiler>();
            GlobalUtils.RegisterDecompiler("Fernflower",preferences);
        }
    }
}