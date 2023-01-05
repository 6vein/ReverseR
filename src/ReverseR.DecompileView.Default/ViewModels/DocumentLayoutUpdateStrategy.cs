using ReverseR.Common.ViewUtilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xceed.Wpf.AvalonDock;
using Xceed.Wpf.AvalonDock.Layout;

namespace ReverseR.DecompileView.Default.ViewModels
{
    internal class DocumentLayoutUpdateStrategy : ILayoutUpdateStrategy
    {
        public void AfterInsertAnchorable(LayoutRoot layout, LayoutAnchorable anchorableShown)
        {
        }

        public void AfterInsertDocument(LayoutRoot layout, LayoutDocument documentShown)
        {
            if(documentShown.Content is IDocumentViewModel viewModel)
            {
                documentShown.ContentId = viewModel.JPath.ClassPath;
            }
        }

        public bool BeforeInsertAnchorable(LayoutRoot layout, LayoutAnchorable anchorableToShow, ILayoutContainer destinationContainer)
        {
            return false;
        }

        public bool BeforeInsertDocument(LayoutRoot layout, LayoutDocument documentToShow, ILayoutContainer destinationContainer)
        {
            return false;
        }
    }
}
