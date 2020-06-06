using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Prism.Mvvm;
using ReverseR.Common.ViewUtilities;
using System.Windows.Input;
using System.Collections.ObjectModel;
using System.Windows.Media;

namespace ReverseR.ViewModels
{
    class DefaultMenuItem:BindableBase,IMenuViewModel
    {
        private string _text;
        public string Text { get => _text; set => SetProperty(ref _text, value); }
        private string _tooltip;
        public string Tooltip { get => _tooltip; set => SetProperty(ref _tooltip, value); }
        private ICommand _command;
        public ICommand Command { get => _command; set => SetProperty(ref _command, value); }
        private ObservableCollection<IMenuViewModel> _children = new ObservableCollection<IMenuViewModel>();
        public ObservableCollection<IMenuViewModel> Children { get => _children; set => SetProperty(ref _children, value); }
        private ImageSource _icon;
        public ImageSource Icon { get => _icon; set => SetProperty(ref _icon, value); }
        private string _gesturetext = "";
        public string GestureText { get => _gesturetext; set => SetProperty(ref _gesturetext, value); }

        public bool IsSeparator { get; set; }
    }
}
