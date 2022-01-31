﻿using System;
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
        public JPath JPath { get; }
        public Task<string> GetContentAsync();
        public IDecompileViewModel Parent { get; set; }
        public Task LoadAsync(string path, JPath classpath);
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
