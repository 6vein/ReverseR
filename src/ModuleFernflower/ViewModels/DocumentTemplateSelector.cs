using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace ModuleFernflower.ViewModels
{
    internal class DocumentTemplateSelector: DataTemplateSelector
    {
        public DataTemplate DocumentTemplate { get; set; }
        [Obsolete]
        public DataTemplate DefaultTemplate { get; set; }
        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            if(item is FernflowerDocumentViewModel)
            {
                return DocumentTemplate;
            }
            return null;
        }
    }
}
