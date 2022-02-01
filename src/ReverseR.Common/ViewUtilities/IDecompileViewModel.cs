using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ReverseR.Common.Extensibility;
using ReverseR.Common.DecompUtilities;
using System.Threading;

namespace ReverseR.Common.ViewUtilities
{
    /// <summary>
    /// Classes implementing this interface should Publish the <see cref="Events.MenuUpdatedEvent"/> to change the menu
    /// The views associated with this viewmodel should call <see cref="Initalize"/> after creation(for non-Mvvm capability)
    /// </summary>
    public interface IDecompileViewModel:ITitleSupport
    {
        /// <summary>
        /// Initalize the view and its plugins
        /// </summary>
        public void Initalize();
        /// <summary>
        /// Close the view
        /// </summary>
        /// <returns></returns>
        public bool Shutdown(bool ForceClose);
        /// <summary>
        /// Open a document,Please use it with <see cref="System.ComponentModel.BackgroundWorker"/>
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public IDocumentViewModel OpenDocument(IJPath path);
        public void ActivateDocument(IDocumentViewModel documentViewModel);
        /// <summary>
        /// Closes the document,will call <see cref="IDocumentViewModel.Close(bool)"/>
        /// </summary>
        /// <param name="documentViewModel"></param>
        public bool CloseDocument(IDocumentViewModel documentViewModel, bool ForceClose = false);
        /// <summary>
        /// Directory where the cache storages
        /// </summary>
        public string BaseDirectory { get; set; }
        /// <summary>
        /// Publish the <see cref="Events.MenuUpdatedEvent"/> to notify the shell to update menu
        /// </summary>
        public void PublishMenuUpdate();
        /// <summary>
        /// Gets the parse tree,empty or null <paramref name="classPath"/> for the entry root.
        /// Every opened decompile view has a entry, such as <see cref="DecompileViewModelBase.ASTEntry"/>
        /// </summary>
        /// <param name="classPath"></param>
        /// <param name="parseCompilationUnit"></param>
        /// <returns></returns>
        public Task<Code.ParseTreeNode> GetParseTreeAsync(string classPath, bool parseCompilationUnit = false);
        public Guid Guid { get; }
        /// <summary>
        /// A list that manages the plugins(<see cref="IPlugin"/> and <see cref="IDockablePlugin"/>).
        /// The latter one has a <see cref="Xceed.Wpf.AvalonDock.Layout.LayoutAnchorable"/> to display content
        /// </summary>
        public List<IPlugin> Plugins { get; }
        public IDocumentViewModel ActiveDocument { get; }
        public ObservableCollection<IDocumentViewModel> Documents { get; set; }
        public string FilePath { get; }
        public string StatusMessage { get; }
    }
}
