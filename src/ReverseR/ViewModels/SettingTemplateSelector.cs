using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace ReverseR.ViewModels
{
    internal class SettingTemplateSelector: DataTemplateSelector
    {
        public DataTemplate EnvironmentSettings { get; set; }
        public DataTemplate AppearenceSettings { get; set; }
        public DataTemplate ModularitySettings { get; set; }
        public DataTemplate DecompilerGeneralSettings { get; set; }
        public DataTemplate DecompilerSettings { get; set; }
        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            return base.SelectTemplate(item, container);
        }
    }
}
