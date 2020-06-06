using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReverseR.Common.ViewUtilities
{
    public interface IPluginViewModel:ITitleSupport
    {
        public IDecompileViewModel Parent { get; set; }
        public Action OnPluginUnload { get; }
    }
}
