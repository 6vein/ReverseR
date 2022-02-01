using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Prism.Commands;
using Prism.Services.Dialogs;
using ReverseR.Common;
using ReverseR.Common.ViewUtilities;

namespace ReverseR.ViewModels
{
    internal class ConfirmDialogViewModel:DialogViewModelBase
    {
        private string _message;
        public string Message
        {
            get { return _message; }
            set { SetProperty(ref _message, value); }
        }
        private string _yesMessage;
        public string YesMessage
        {
            get { return _yesMessage; }
            set { SetProperty(ref _yesMessage, value); }
        }
        private string _noMessage;
        public string NoMessage
        {
            get { return _noMessage; }
            set { SetProperty(ref _noMessage, value); }
        }
        public ConfirmDialogViewModel()
        {
            Title = "Confirm";
        }
        public override void OnDialogOpened(IDialogParameters parameters)
        {
            Message = parameters.GetValue<string>("message");
            if (parameters.GetValue<string>("yes") != null)
            {
                YesMessage = parameters.GetValue<string>("yes");
            }
            else
            {
                YesMessage = "Yes";
            }
            if(parameters.GetValue<string>("no") != null)
            {
                NoMessage = parameters.GetValue<string>("no");
            }
            else
            {
                NoMessage = "No";
            }
            base.OnDialogOpened(parameters);
        }
        protected override void CloseDialog(string parameter)
        {
        }
        public DelegateCommand YesCommand => new DelegateCommand(() =>
          {
              RaiseRequestClose(new DialogResult(ButtonResult.Yes));
          });
        public DelegateCommand NoCommand => new DelegateCommand(() =>
        {
            RaiseRequestClose(new DialogResult(ButtonResult.No));
        });
    }
}
