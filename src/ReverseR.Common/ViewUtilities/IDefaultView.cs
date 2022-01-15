using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace ReverseR.Common.ViewUtilities
{
    /// <summary>
    /// Views implementing this interface must have the <see cref="FrameworkElement.DataContext"/> implementing <see cref="IDefaultViewModel"/>
    /// </summary>
    public interface IDefaultView
    {
        void SetDecompiler(string id);
    }
    /// <summary>
    /// ViewModel for views shared between decompilers
    /// </summary>
    public interface IDefaultViewModel:IDecompileViewModel
    {
        void SetDecompiler(string id);
    }
}
