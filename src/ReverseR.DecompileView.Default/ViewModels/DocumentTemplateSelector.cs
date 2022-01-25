using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Prism.Regions;
using Prism.Ioc;
using ReverseR.Common;

namespace ReverseR.DecompileView.Default.ViewModels
{
    internal class DocumentTemplateSelector: DataTemplateSelector,IDIAble
    {
        public DataTemplate DocumentTemplate { get; set; }
        public DataTemplate DefaultTemplate { get; set; }
        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            if (item is DecompileDocumentViewModel)
            {
                return DocumentTemplate;
            }
            return null;
        }
    }
}
