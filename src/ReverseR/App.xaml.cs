using ReverseR.Views;
using ReverseR.Common.Services;
using ReverseR.Common.DecompUtilities;
using ReverseR.Common.ViewUtilities;
using ReverseR.DecompileView.Default.Views;
using Prism.Ioc;
using Prism.Modularity;
using System.Windows;
using Prism.Regions;
using System;
using System.Reflection;
using System.Runtime.CompilerServices;
using Prism.Services.Dialogs;
using ReverseR.Properties;
using System.Configuration;

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
            containerRegistry.RegisterDialog<ConfirmDialog>();
            containerRegistry.RegisterDialog<SettingsDialog>();
            containerRegistry.RegisterDialogWindow<MetroDialogWindowHost>();

            containerRegistry
                .Register<IDefaultView, ViewDecompile>()
                .Register<IMenuViewModel, ViewModels.DefaultMenuItem>();

            containerRegistry
                .Register<IDecompileResult, Internal.DecompUtilities.DefaultDecompResult>()
                .RegisterSingleton<IDecompilerResolver, Internal.DecompUtilities.DefaultDecompilerResolver>();

            containerRegistry.Register<IBackgroundTaskBuilder, Internal.Services.DefaultBackgroundTaskBuilder>();
            containerRegistry.Register(typeof(IBackgroundTaskBuilder<>), typeof(Internal.Services.DefaultBackgroundTaskBuilder<>));
        }

        protected override void ConfigureModuleCatalog(IModuleCatalog moduleCatalog)
        {
            //moduleCatalog.AddModule<ModuleFernflower.ModuleFernflowerModule>();
            base.ConfigureModuleCatalog(moduleCatalog);
        }

        protected override IModuleCatalog CreateModuleCatalog()
        {
            return new Internal.Modularity.DefaultModuleCatalog();
        }

        protected override void ConfigureRegionAdapterMappings(RegionAdapterMappings regionAdapterMappings)
        {
            base.ConfigureRegionAdapterMappings(regionAdapterMappings);
            regionAdapterMappings.RegisterMapping(typeof(Xceed.Wpf.AvalonDock.DockingManager), Container.Resolve<DockingManagerRegionAdapter>());
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            Common.GlobalUtils.Load(Settings.Default, l => l.JsonConfig);
            base.OnStartup(e);
        }
        protected override void OnExit(ExitEventArgs e)
        {
            Common.GlobalUtils.Save(Settings.Default, l => l.JsonConfig);
            //save to file
            Settings.Default.Save();
            base.OnExit(e);
        }

        private void PrismApplication_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            if(IsProcessingUnhandledException==false)
            {
                IsProcessingUnhandledException = true;
                try
                {
                    Container.Resolve<IDialogService>().ReportError($"Fatal:An unhandled exception:{e.Exception.GetType()}\nMessage:{e.Exception.Message}\nFrom:{e.Exception.Source}", null, e.Exception.StackTrace);
                }
                finally
                {
                    //Environment.FailFast($"An unhandled exception:{e.Exception.Message}", e.Exception);
                    throw new Exception("An unhandled exception:{e.Exception.ToString()}\nMessage:{e.Exception.Message}\nFrom:{e.Exception.Source}");
                }
            }
            
        }
    }
}
