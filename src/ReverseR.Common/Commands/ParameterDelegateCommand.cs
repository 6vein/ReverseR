using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace ReverseR.Common.Commands
{
    public class ParameterDelegateCommand<TParameter> : ICommand
    {
        private Action<TParameter> _executeMethod;

        private Func<TParameter, bool> _canExecuteMethod;
        private TParameter _payload;
        public event EventHandler CanExecuteChanged;

        public ParameterDelegateCommand(TParameter value,Action<TParameter> executeMethod)
            : this(value,executeMethod, _ => true)
        {
        }
        public ParameterDelegateCommand(TParameter value,Action<TParameter> executeMethod, Func<TParameter,bool> canExecuteMethod)
        {
            if (executeMethod == null || canExecuteMethod == null)
            {
                throw new ArgumentNullException("executeMethod", $"{nameof(ParameterDelegateCommand<TParameter>)} cannot have a null execute method.");
            }

            _payload = value;
            _executeMethod = executeMethod;
            _canExecuteMethod = canExecuteMethod;
        }
        public bool CanExecute(object parameter)
        {
            return _canExecuteMethod(_payload);
        }

        public void Execute(object parameter)
        {
            _executeMethod(_payload);
        }
    }
}
