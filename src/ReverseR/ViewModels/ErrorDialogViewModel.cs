using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Prism.Services.Dialogs;
using ReverseR.Common;
using ReverseR.Common.ViewUtilities;

namespace ReverseR.ViewModels
{
    internal class ErrorDialogViewModel : DialogViewModelBase
    {
        private string _message;
        public string Message
        {
            get { return _message; }
            set { SetProperty(ref _message, value); }
        }

        private string _stackTrace;
        public string StackTrace
        {
            get { return _stackTrace; }
            set { SetProperty(ref _stackTrace, value); }
        }

        private bool _isStackTraceAvailable;
        public bool IsStackTraceAvaliable
        {
            get => _isStackTraceAvailable;
            set => SetProperty(ref _isStackTraceAvailable, value);
        }
        public ErrorDialogViewModel()
        {
            Title = "Error";
        }
        public override void OnDialogOpened(IDialogParameters parameters)
        {
            Message = parameters.GetValue<string>("message");
            if(parameters.GetValue<string>("stacktrace")==null)
            {
                IsStackTraceAvaliable = false;
            }
            else
            {
                IsStackTraceAvaliable = true;
                StackTrace = parameters.GetValue<string>("stacktrace");
            }
            base.OnDialogOpened(parameters);
        }
    }
}
