using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Prism.Mvvm;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using ReverseR.Common.ViewUtilities;
using ReverseR.Common.DecompUtilities;

namespace PluginSourceControl.ViewModels
{
    public class SourceTreeNode : BindableBase
    {
        public IDocumentViewModel AssociatedDocument { get; set; }
        public JPath JPath { get; set; }
        public enum NodeType
        {
            Directory,
            JavaClass,
            Others,
            __InternalPlaceHolder=-1
        }
        public NodeType Type { get; set; }

        internal static Dictionary<string, string> IconResource => new Dictionary<string, string>()
        {
            {".java","pack://application:,,,/PluginSourceControl;component/ImageResources/JavaFile_16x.xaml" },
            {"<folder>","pack://application:,,,/PluginSourceControl;component/ImageResources/Folder_16x.xaml" },
            {"<folderopen>","pack://application:,,,/PluginSourceControl;component/ImageResources/FolderOpen_16x.xaml" }
        };

        #region Bindings
        object _icon;
        public object Icon { get => _icon; set => SetProperty(ref _icon, value); }
        object _expandedIcon;
        public object ExpandedIcon { get => _expandedIcon; set => SetProperty(ref _expandedIcon, value); }
        string _text;
        public string Text { get => _text; set => SetProperty(ref _text, value); }
        ObservableCollection<SourceTreeNode> _children;
        public ObservableCollection<SourceTreeNode> Children { get => _children; set => SetProperty(ref _children, value); }
        bool _isSelected;
        public bool IsSelected { get => _isSelected; set => SetProperty(ref _isSelected, value); }
        bool _isExpanded;
        public bool IsExpanded { get => _isExpanded; set => SetProperty(ref _isExpanded, value); }
        bool _isPending;
        public bool IsPending { get => _isPending; set => SetProperty(ref _isPending, value); }
        #endregion
        #region Operations
        public void UpdateChildren()
        {
            if(Type==NodeType.JavaClass)
            {
                
            }
        }
        #endregion
    }
}
