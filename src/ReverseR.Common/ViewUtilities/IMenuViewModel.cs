using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows;
using System.ComponentModel;
using System.Windows.Input;
using System.Collections.ObjectModel;
using System.Windows.Media;

namespace ReverseR.Common.ViewUtilities
{
    public interface IMenuViewModel:INotifyPropertyChanged
    {
        /// <summary>
        /// Text to display for this menu item
        /// </summary>
        public string Text { get; set; }
        /// <summary>
        /// Tooltip to show for this menu item
        /// </summary>
        public string Tooltip { get; set; }
        /// <summary>
        /// The Command of this menu item
        /// </summary>
        public ICommand Command { get; set; }
        /// <summary>
        /// Icon to display for this item
        /// </summary>
        public ImageSource Icon { get; set; }
        /// <summary>
        /// See <see cref="MenuItem.InputGestureText"/> for more info
        /// </summary>
        public string GestureText { get; set; }
        /// <summary>
        /// Collection of sub menu items
        /// </summary>
        public ObservableCollection<IMenuViewModel> Children { get; set; }
        /// <summary>
        /// Indicate whether it is a separator
        /// </summary>
        public bool IsSeparator { get; set; }
    }
}
