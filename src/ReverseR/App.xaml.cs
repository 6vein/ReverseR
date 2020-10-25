using ReverseR.Views;
using ReverseR.Common.Services;
using ReverseR.Common.DecompUtilities;
using ReverseR.Common.ViewUtilities;
using ReverseR.DecompileView.Default.ViewModels;
using Prism.Ioc;
using Prism.Modularity;
using System.Windows;
using Prism.Regions;
using System;
using System.Reflection;
using System.Runtime.CompilerServices;
using Prism.Services.Dialogs;

namespace ReverseR
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App
    {
        bool IsProcessingUnhandledException = false;
        public App()
        {
            uint nLanguage = 1;
            ReverseR.Common.APIHelper.SetProcessPreferredUILanguages(ReverseR.Common.APIHelper.MUI_LANGUAGE_NAME, "en-US",ref nLanguage);
        }

        protected override Window CreateShell()
        {
            return Container.Resolve<MainWindow>();
        }

        protected override void RegisterTypes(IContainerRegistry containerRegistry)
        {
            containerRegistry.RegisterDialog<ErrorDialog>();
            containerRegistry.RegisterDialogWindow<MetroDialogWindowHost>();

            containerRegistry.Register<IDecompileViewModel, ViewDecompileViewModel>();
            containerRegistry.Register<IMenuViewModel, ViewModels.DefaultMenuItem>();

            containerRegistry.Register<IDecompileResult, Internal.DecompUtilities.DefaultDecompResult>();
            containerRegistry.Register<IBackgroundTaskBuilder, Internal.Services.DefaultBackgroundTaskBuilder>();
        }

        protected override void ConfigureModuleCatalog(IModuleCatalog moduleCatalog)
        {
            //moduleCatalog.AddModule<ModuleFernflower.ModuleFernflowerModule>();
            base.ConfigureModuleCatalog(moduleCatalog);
        }

        protected override IModuleCatalog CreateModuleCatalog()
        {
            return new DirectoryModuleCatalog() { ModulePath = AppDomain.CurrentDomain.BaseDirectory + "\\Plugins\\" };
        }

        protected override void ConfigureRegionAdapterMappings(RegionAdapterMappings regionAdapterMappings)
        {
            base.ConfigureRegionAdapterMappings(regionAdapterMappings);
            regionAdapterMappings.RegisterMapping(typeof(Xceed.Wpf.AvalonDock.DockingManager), Container.Resolve<DockingManagerRegionAdapter>());
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            Common.GlobalUtils.Load();
            base.OnStartup(e);
        }

        private void PrismApplication_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            if(IsProcessingUnhandledException==false)
            {
                IsProcessingUnhandledException = true;
                try
                {
                    Container.Resolve<IDialogService>().ReportError($"Fatal:An unhandled exception:{e.Exception.ToString()}\nMessage:{e.Exception.Message}\nFrom:{e.Exception.Source}", null, e.Exception.StackTrace);
                }
                finally
                {
                    Environment.FailFast($"An unhandled exception:{e.Exception.Message}", e.Exception);
                }
            }
            
        }
    }
}
