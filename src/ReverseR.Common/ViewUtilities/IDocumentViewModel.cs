using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ReverseR.Common.Services;
using ReverseR.Common.Serialization;
using ReverseR.Common.DecompUtilities;

namespace ReverseR.Common.ViewUtilities
{
    public interface IDocumentViewModel:ITitleSupport
    {
        public IDecompileViewModel Parent { get; set; }
        public Task LoadAsync(string path, JPath classpath);
        public void AttachDecompileTask(IBackgroundTask decompileTask);
        /// <summary>
        /// Return true for processing
        /// </summary>
        public bool Closing();
    }
}
