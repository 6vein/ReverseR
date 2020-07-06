using Prism.Mvvm;
using Prism.Commands;
using Prism.Regions;
using Prism.Ioc;
using Prism.Events;
using ReverseR.Common;
using ReverseR.Common.Services;
using ReverseR.Common.ViewUtilities;
using ReverseR.Common.Events;
using ReverseR.Internal.Events;
using ReverseR.Common.DecompUtilities;
using System;
using System.Linq;
using System.Collections.ObjectModel;
using System.Windows.Input;
using System.Windows.Media;
using System.Collections.Generic;

namespace ReverseR.ViewModels
{
    public class MainWindowViewModel : BindableBase
    {
        private IRegionManager regionManager;
        private IContainerProvider containerProvider;
        #region Bindings
        private string _title = "ReverseR";
        public string Title
        {
            get { return _title; }
            set { SetProperty(ref _title, value); }
        }
        private string _statusText = "Ready";
        public string StatusText
        {
            get { return _statusText; }
            set { SetProperty(ref _statusText, value); }
        }
        private int _notificationcount = 0;
        public int NotificationCount
        {
            get { return _notificationcount; }
            set { SetProperty(ref _notificationcount, value); }
        }

        private object _activeContent;
        public object ActiveContent
        {
            get => _activeContent;
            set
            {
                _activeContent = value;
                if(_activeContent is System.Windows.FrameworkElement element)
                {
                    if(element.DataContext is IDecompileViewModel viewModel)
                    {
                        containerProvider.Resolve<IEventAggregator>()
                            .GetEvent<ViewActivatedEvent>().Publish(viewModel.Guid);
                    }
                }
            }
        }

        public DelegateCommand OpenFileCommand => new DelegateCommand(() =>
        {
            using (Microsoft.WindowsAPICodePack.Dialogs.CommonOpenFileDialog openFileDialog = new Microsoft.WindowsAPICodePack.Dialogs.CommonOpenFileDialog())
            {
                var group = new Microsoft.WindowsAPICodePack.Dialogs.Controls.CommonFileDialogGroupBox("&Decompiler");
                var comboBox = new Microsoft.WindowsAPICodePack.Dialogs.Controls.CommonFileDialogComboBox("DecompilerCombo");
                foreach (CommonStorage.DecompilerInfo info in CommonStorage.Decompilers)
                {
                    comboBox.Items.Add(new Microsoft.WindowsAPICodePack.Dialogs.Controls.CommonFileDialogComboBoxItem(info.Name));
                }
                group.IsProminent = true;
                group.Items.Add(comboBox);
                comboBox.SelectedIndex = 0;
                openFileDialog.Controls.Add(group);
                openFileDialog.DefaultDirectory = "%UserProfile%";
                openFileDialog.Filters.Add(new Microsoft.WindowsAPICodePack.Dialogs.CommonFileDialogFilter("Java Archive(*.jar;*.war)", "*.jar;*.war"));
                openFileDialog.Filters.Add(new Microsoft.WindowsAPICodePack.Dialogs.CommonFileDialogFilter("Zip file(*.zip)", "*.zip"));
                openFileDialog.Filters.Add(new Microsoft.WindowsAPICodePack.Dialogs.CommonFileDialogFilter("Java Classes(*.class)", "*.class"));
                openFileDialog.AddToMostRecentlyUsedList = true;
                openFileDialog.EnsureFileExists = true;
                openFileDialog.EnsurePathExists = true;
                openFileDialog.Multiselect = true;
                openFileDialog.InitialDirectory = "%UserProfile%";
                openFileDialog.AllowPropertyEditing = true;
                openFileDialog.EnsureValidNames = true;
                openFileDialog.ShowHiddenItems = true;
                if (openFileDialog.ShowDialog(App.Current.MainWindow) == Microsoft.WindowsAPICodePack.Dialogs.CommonFileDialogResult.Ok)
                {
                    object view = null;
                    foreach (string path in openFileDialog.FileNames)
                    {
                        int index = (openFileDialog.Controls["DecompilerCombo"] as Microsoft.WindowsAPICodePack.Dialogs.Controls.CommonFileDialogComboBox).SelectedIndex;
                        if (ActiveContent != null && (ActiveContent.GetType() == CommonStorage.GetViewTypeByIndex(index)))
                        {
                            if(!string.IsNullOrEmpty(((ActiveContent as System.Windows.FrameworkElement).DataContext as IDecompileViewModel).FilePath))
                            {
                                view = ActiveContent;
                            }
                            else
                            {
                                view= CommonStorage.ResolveViewByIndex(index);
                            }
                        }
                        else
                        {
                            view = CommonStorage.ResolveViewByIndex(index);
                        }
                        Guid guid = ((view as System.Windows.FrameworkElement).DataContext as IDecompileViewModel).Guid;
                        regionManager.AddToRegion("RootDockRegion", view);
                        var ea = containerProvider.Resolve<IEventAggregator>();
                        ea.GetEvent<OpenFileEvent>().Publish(new Tuple<string, FileTypes, Guid>
                            (path, openFileDialog.SelectedFileTypeIndex == 2 ? FileTypes.Class : FileTypes.Jar, guid));
                    }
                }
            }
        });
        ObservableCollection<RootMenuItemWrapper> _rootMenus;
        public ObservableCollection<RootMenuItemWrapper> RootMenus { get => _rootMenus; set => SetProperty(ref _rootMenus, value); }
        public ObservableCollection<IAbstractBackgroundTask> BackgroundTasks { get; set; } = new ObservableCollection<IAbstractBackgroundTask>();
        public bool IsInIdle { get => BackgroundTasks.Count == 0; }

        ObservableCollection<InputBinding> _inputBindings = new ObservableCollection<InputBinding>();
        public ObservableCollection<InputBinding> InputBindings { get => _inputBindings; set => SetProperty(ref _inputBindings, value); }
        #endregion
        public MainWindowViewModel(IRegionManager region)
        {
            regionManager = region;
            containerProvider = (App.Current as App).Container;
            containerProvider.Resolve<IEventAggregator>().GetEvent<TaskStartedEvent>().Subscribe(payload => BackgroundTasks.Add(payload), ThreadOption.UIThread);
            containerProvider.Resolve<IEventAggregator>().GetEvent<TaskCompletedEvent>().Subscribe(payload => BackgroundTasks.Remove(payload), ThreadOption.UIThread);
            BackgroundTasks.CollectionChanged += (s, e) => { RaisePropertyChanged(nameof(IsInIdle)); };
            RestoreMenu();
        }
        #region Methods
        /// <summary>
        /// Restore the menu and return the index to insert the plugin menus in
        /// </summary>
        internal int RestoreMenu()
        {
            InputBindings.Clear();
            RootMenus = new ObservableCollection<RootMenuItemWrapper>();
            RootMenuItemWrapper file = new RootMenuItemWrapper()
            { Model = containerProvider.Resolve<IMenuViewModel>(), Alignment = System.Windows.TextAlignment.Center, Width = 56 };
            file.Text = "_File";
            file.Children = new ObservableCollection<IMenuViewModel>();
            file.Children.Add(_InternalCreateMenu("_Open", OpenFileCommand, null, "Ctrl+O", "Open a new file"));
            RootMenus.Add(file);
            return 1;
        }

        internal void OnMenuUpdated(Tuple<IEnumerable<IMenuViewModel>,Guid> tuple)
        {
            RestoreMenu();
            foreach(IMenuViewModel item in tuple.Item1)
            {
                RootMenus.Insert(1, new RootMenuItemWrapper { Alignment = System.Windows.TextAlignment.Center, Width = 56, Model = item });
                foreach(IMenuViewModel child in item.Children)
                {
                    if (child.Command != null || !string.IsNullOrEmpty(child.GestureText))
                    {
                        InputBindings.Add(new InputBinding(child.Command, (new KeyGestureConverter()).ConvertFrom(child.GestureText) as InputGesture));
                    }
                }
            }
        }

        protected IMenuViewModel _InternalCreateMenu(string text, ICommand command, ImageSource icon, string inputgesture, string tooltip = null)
        {
            IMenuViewModel vm = containerProvider.Resolve<IMenuViewModel>();
            vm.Text = text;
            vm.Command = command;
            vm.Icon = icon;
            vm.GestureText = inputgesture;
            vm.Tooltip = tooltip;
            if (command != null && !string.IsNullOrEmpty(inputgesture)) 
            {
                InputBindings.Add(new InputBinding(command, (new KeyGestureConverter()).ConvertFrom(inputgesture) as InputGesture));
            }
            return vm;
        }
        #endregion
    }
}
