using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using ReverseR.Common.DecompUtilities;

namespace ReverseR.ViewModels
{
    internal class SettingTemplateSelector: DataTemplateSelector
    {
        public DataTemplate EnvironmentSettings { get; set; }
        public DataTemplate AppearenceSettings { get; set; }
        public DataTemplate ModularitySettings { get; set; }
        public DataTemplate DecompileGeneralSettings { get; set; }
        public DataTemplate DecompilerSettings { get; set; }
        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            if(item is EnvironmentSettingsViewModel)
            {
                return EnvironmentSettings;
            }
            else if(item is DecompileGeneralViewModel)
            {
                return DecompileGeneralSettings;
            }
            else if(item is DecompilerSettingsViewModel)
            {
                return DecompilerSettings;
            }
            return null;
        }
    }
    internal class DecompileComboBoxTemplateSelector: DataTemplateSelector
    {
        public DataTemplate NumericItemTemplate { get; set; }
        public DataTemplate StringItemTemplate { get; set; }
        public DataTemplate ComboItemTemplate { get; set; }
        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            if (item is ICommonPreferences.IArgument argument)
            {
                if (argument.ValueIndex == -1 && argument.AvailableValues.Count() == 1)
                {
                    return StringItemTemplate;
                }
                else if (argument.AvailableValues.Count() == 0)
                {
                    return NumericItemTemplate;
                }
                else
                {
                    return ComboItemTemplate;
                }
            }
            return null;
        }
    }
}
