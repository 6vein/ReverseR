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
using ReverseR.Common.ViewUtilities;

namespace ReverseR.DecompileView.Default.Views
{
    /// <summary>
    /// Interaction logic for ViewA.xaml
    /// </summary>
    public partial class ViewDecompile : UserControl,IDefaultView
    {
        public ViewDecompile()
        {
            InitializeComponent();
            //I hate Avalondock with so little support of Mvvm
            manager.LayoutUpdateStrategy = new ViewModels.DocumentLayoutUpdateStrategy();
            (DataContext as ViewModels.ViewDecompileViewModel).Manager = manager;
        }

        public void SetDecompiler(string id)
        {
            (DataContext as ViewModels.ViewDecompileViewModel).SetDecompiler(id);
        }
    }
}
