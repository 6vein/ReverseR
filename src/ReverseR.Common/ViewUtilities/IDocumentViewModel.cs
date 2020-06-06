using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ReverseR.Common.Services;
using ReverseR.Common.Serialization;

namespace ReverseR.Common.ViewUtilities
{
    public interface IDocumentViewModel:ITitleSupport
    {
        public IDecompileViewModel Parent { get; set; }
        public void Load(string path);
        public void AttachDecompileTask(IBackgroundTask decompileTask);
        /// <summary>
        /// Return true for processing
        /// </summary>
        public bool Closing();
    }
}
