using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Prism.Mvvm;
using ReverseR.Common.ViewUtilities;
using System.Windows.Controls;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Collections.ObjectModel;

namespace ReverseR.ViewModels
{
    public class RootMenuItemWrapper : BindableBase, IMenuViewModel
    {
        internal IMenuViewModel Model;
        private TextAlignment _alignment;
        public TextAlignment Alignment { get => _alignment; set => SetProperty(ref _alignment, value); }
        private double _width;
        public double Width { get => _width; set => SetProperty(ref _width, value); }
        public string Text { get => Model.Text; set => Model.Text = value; }
        public string Tooltip { get => Model.Tooltip; set => Model.Tooltip = value; }
        public ICommand Command { get => Model.Command; set => Model.Command = value; }
        public ImageSource Icon { get => Model.Icon; set => Model.Icon = value; }
        public string GestureText { get => Model.GestureText; set => Model.GestureText = value; }
        public ObservableCollection<IMenuViewModel> Children { get => Model.Children; set => Model.Children = value; }

        public bool IsSeparator { get => Model.IsSeparator; set => Model.IsSeparator = value; }
    }
}
