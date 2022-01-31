using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Prism.Ioc;
using Prism.Services.Dialogs;
using ReverseR.Common;
using System.Windows;

namespace ReverseR.Common.Services
{
    public static class DialogServiceExtensions
    {
        public static void ReportError(this IDialogService dialogService,string message,Action<IDialogResult> callback,string stacktrace=null)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                IDialogParameters parameters = new DialogParameters();
                parameters.Add("message", message);
                parameters.Add("stacktrace", stacktrace);
                dialogService.ShowDialog("ErrorDialog", parameters, callback);
            });
        }
        public static void PresentConfirmation(this IDialogService dialogService,string message, Action<IDialogResult> callback)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                IDialogParameters parameters = new DialogParameters();
                parameters.Add("message", message);
                dialogService.ShowDialog("ErrorDialog", parameters, callback);
            });
        }
    }
}
