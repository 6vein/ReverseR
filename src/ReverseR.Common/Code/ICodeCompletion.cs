using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.CodeCompletion;
using ICSharpCode.AvalonEdit.Editing;
using ICSharpCode.AvalonEdit.Document;
using ReverseR.Common.Services;

namespace ReverseR.Common.Code
{
    public interface ICodeCompletion
    {
        /// <summary>
        /// Base directory for AST building
        /// </summary>
        public string BaseDirectory { get; set; }
        public IList<ICompletionData> Complete(string text,string path, CancellationToken? token = null);
        //        public IList<ICompletionData> Complete(string text,TextDocument document, CancellationToken? token = null);
        public IBackgroundTask<IList<ICompletionData>> CompleteAsync(string text, string path, CancellationToken? token = null, Action<Task<IList<ICompletionData>>> completedCallback = null);
//        public Task<IList<ICompletionData>> CompleteAsync(string text, TextDocument document, CancellationToken? token = null);
    }
}
