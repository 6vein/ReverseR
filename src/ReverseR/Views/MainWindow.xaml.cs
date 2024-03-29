﻿using System.Windows;
using Prism.Events;
using ReverseR.Common.Events;
using ReverseR.Common.ViewUtilities;
using ReverseR.Common;
using Prism.Ioc;

namespace ReverseR.Views
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : MahApps.Metro.Controls.MetroWindow
    {
        IContainerProvider Container { get; set; }
        public MainWindow(IContainerProvider containerProvider)
        {
            Container = containerProvider;
            InitializeComponent();
        }

        private void CommandBinding_CanExecute(object sender, System.Windows.Input.CanExecuteRoutedEventArgs e)
        {
            //stop it from passing on
            e.Handled = true;
            if ((DataContext as ViewModels.MainWindowViewModel).HandleMenuCanExecuteEvent(sender,e))
            {
                return;
            }
            if(((DataContext as ViewModels.MainWindowViewModel).ActiveContent 
                as FrameworkElement)?.DataContext is IDecompileViewModel viewModel)
            {
                Container.Resolve<IEventAggregator>().GetEvent<MenuCanExecuteEvent>().Publish((viewModel.Guid,e));
            }
        }

        private void CommandBinding_Executed(object sender, System.Windows.Input.ExecutedRoutedEventArgs e)
        {
            //stop it from passing on
            e.Handled = true;
            if ((DataContext as ViewModels.MainWindowViewModel).HandleMenuExecutedEvent(sender, e))
            {
                return;
            }
            if (((DataContext as ViewModels.MainWindowViewModel).ActiveContent
                as FrameworkElement).DataContext is IDecompileViewModel viewModel)
            {
                Container.Resolve<IEventAggregator>().GetEvent<MenuExecuteEvent>().Publish((viewModel.Guid,e));
            }
        }
    }
}
