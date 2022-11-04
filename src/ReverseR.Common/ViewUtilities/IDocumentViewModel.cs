using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ReverseR.Common.Services;
using ReverseR.Common.Serialization;
using ReverseR.Common.DecompUtilities;
using System.IO;

namespace ReverseR.Common.ViewUtilities
{
    public interface IDocumentViewModel:ITitleSupport
    {
        public string InternalFilePath { get; }
        public IJPath JPath { get; }
        public Task<string> GetContentAsync();
        public Task SelectAsync(int start, int end);
        public IDecompileViewModel Parent { get; set; }
        public void SetJPath(IJPath classPath);
        public Task LoadAsync(string path);
        public void AttachDecompileTask(IBackgroundTask decompileTask);
        public IBackgroundTask GetAttachedDecompileTask();
        /// <summary>
        /// Cleans up self,while the close command closes self from parent
        /// </summary>
        /// <returns>
        /// true if finishes cleaning up,false if not
        /// </returns>
        public bool Close(bool forceClose);
    }
}
