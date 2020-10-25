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
using DecompilerFernflower.Decompile;

namespace DecompilerFernflower
{
    class DecompilerFernflowerModule : DecompilerModuleBase<FernflowerPreferences>
    {
        public override string DependencyPath => "DecompilerFernflower";
        public override string FriendlyName => "Fernflower";

        public override void Initialized(IContainerProvider containerProvider)
        {
            ICommonPreferences preferences;
            if (File.Exists(GlobalUtils.GlobalConfig.ConfigPrefix + "fernflower,json"))
                preferences = JsonConvert.DeserializeObject(File.ReadAllText(GlobalUtils.GlobalConfig.ConfigPrefix + "fernflower,json"), typeof(FernflowerPreferences)) as FernflowerPreferences;
            else preferences = new FernflowerPreferences();
            GlobalUtils.RegisterDecompiler(FriendlyName, preferences);
        }
    }
}
