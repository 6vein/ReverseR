using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media;
using Prism.Ioc;
using Prism.Mvvm;

namespace ReverseR.Common.ViewUtilities
{
    public static class ViewModelExtensions
    {
        public static IMenuViewModel CreateMenu(this BindableBase _, string text, ICommand command, string inputgesture = "", ImageSource icon = null, string tooltip = null)
        {
#pragma warning disable CS0612 // 类型或成员已过时
            IMenuViewModel vm = APIHelper.GetIContainer().Resolve<IMenuViewModel>();
#pragma warning restore CS0612 // 类型或成员已过时
            vm.Text = text;
            vm.Command = command;
            vm.Icon = icon;
            vm.GestureText = inputgesture;
            vm.Tooltip = tooltip;
            return vm;
        }
        public static IMenuViewModel CreateMenuSeparator(this BindableBase _)
        {
#pragma warning disable CS0612 // 类型或成员已过时
            IMenuViewModel vm = APIHelper.GetIContainer().Resolve<IMenuViewModel>();
#pragma warning restore CS0612 // 类型或成员已过时
            vm.IsSeparator = true;
            return vm;
        }
    }
}
