using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Prism.Ioc;
using ReverseR.Common.Code;
using ReverseR.Common.Modularity;

namespace AntlrParser
{
    public class AntlrParserModule:ModuleBase
    {
        public override void Initialized(IContainerProvider containerProvider)
        {
        }
        public override void RegisterTypes(IContainerRegistry containerRegistry)
        {
            containerRegistry.Register<IClassParser, AntlrParser>();
        }
    }
}
