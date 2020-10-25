using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Prism.Ioc;
using Prism.Modularity;
using ReverseR.Common.Code;
using ReverseR.Common.Modularity;

namespace BasicCodeCompletion
{
    public class BasicCodeCompletionModule : ModuleBase
    {
        public override string DependencyPath => "BasicCodeCompletion";
        public override void Initialized(IContainerProvider containerProvider)
        {
        }

        public override void RegisterTypes(IContainerRegistry containerRegistry)
        {
            containerRegistry.Register<ICodeCompletion, BasicCodeCompletion>();
            containerRegistry.Register<ICodeCompletion, BasicCodeCompletion>("Basic");
        }
    }
}
