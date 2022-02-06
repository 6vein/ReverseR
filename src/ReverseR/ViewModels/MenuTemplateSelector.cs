using ReverseR.Common.ViewUtilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Input;

namespace ReverseR.ViewModels
{
    class MenuTemplateSelector:DataTemplateSelector
    {
        public DataTemplate RootMenuTemplate { get; set; }
        public DataTemplate MenuTemplate { get; set; }
        public DataTemplate SeparatorTemplate { get; set; }
        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            Window mainWindow = App.Current.MainWindow;
            if(item is IMenuViewModel viewModel)
            {
                if (viewModel.IsSeparator)
                {
                    return SeparatorTemplate;
                }
                else
                {
                    if (item is RootMenuItemWrapper) return RootMenuTemplate;
                    return MenuTemplate;
                }
            }
            else return null;
        }
    }
}
