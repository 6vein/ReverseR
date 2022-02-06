using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace PluginSourceControl.Views
{
    /// <summary>
    /// Interaction logic for ViewA.xaml
    /// </summary>
    public partial class ViewSourceControl : UserControl
    {
        public ViewSourceControl()
        {
            InitializeComponent();
        }

        private void TreeViewItem_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if(sender is TreeViewItem item)
            {
                if (e.ClickCount % 2 == 0 && item.DataContext is ViewModels.SourceTreeNode node)
                {
                    node.ParentViewModel.ItemDblClickCommand.Execute(e);
                    if (node.ParseTreeNode.ItemType == ReverseR.Common.Code.IClassParser.ItemType.CompilationUnit)
                    {
                        e.Handled = true;
                    }
                }
            }
        }
    }
}
