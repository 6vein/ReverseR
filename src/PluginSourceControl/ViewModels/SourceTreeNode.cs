using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Prism.Mvvm;
using Prism.Ioc;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using ReverseR.Common.ViewUtilities;
using ReverseR.Common.Services;
using ReverseR.Common.DecompUtilities;
using ReverseR.Common;
using ReverseR.Common.Code;
using System.IO;
using System.Xml;
using System.Windows.Markup;
using ModifierType = ReverseR.Common.Code.IClassParser.ModifierType;
using NodeType = ReverseR.Common.Code.IClassParser.ItemType;
using Prism.Commands;
using Prism.Services.Dialogs;
using System.ComponentModel;
using System.Diagnostics;

namespace PluginSourceControl.ViewModels
{
    public class SourceTreeNode : BindableBase
    {
        public ViewSourceControlViewModel ParentViewModel { get; set; }
        public IDocumentViewModel AssociatedDocument { get; set; }
        public ParseTreeNode ParseTreeNode { get; set; }
        public SourceTreeNode CompilationUnitNode { get;private set; }
        internal static Dictionary<string, string> IconResource => new Dictionary<string, string>()
        {
            {".java","pack://application:,,,/PluginSourceControl;component/ImageResources/JavaFile_16x.xaml" },
            {"<folder>","pack://application:,,,/PluginSourceControl;component/ImageResources/Folder_16x.xaml" },
            {"<folderopen>","pack://application:,,,/PluginSourceControl;component/ImageResources/FolderOpen_16x.xaml" },
            {nameof(NodeType.Class)+nameof(ModifierType.Public),"pack://application:,,,/PluginSourceControl;component/ImageResources/Class_16x.xaml" },
            {nameof(NodeType.Class)+nameof(ModifierType.Protected),"pack://application:,,,/PluginSourceControl;component/ImageResources/ClassProtected_16x.xaml" },
            {nameof(NodeType.Class)+nameof(ModifierType.Private),"pack://application:,,,/PluginSourceControl;component/ImageResources/ClassPrivate_16x.xaml" },
            {nameof(NodeType.Class)+nameof(ModifierType.Final),"pack://application:,,,/PluginSourceControl;component/ImageResources/ClassSealed_16x.xaml" },
            {nameof(NodeType.Interface)+nameof(ModifierType.Public),"pack://application:,,,/PluginSourceControl;component/ImageResources/Interface_16x.xaml" },
            {nameof(NodeType.Interface)+nameof(ModifierType.Default),"pack://application:,,,/PluginSourceControl;component/ImageResources/InterfaceFriend_16x.xaml" },
            {nameof(NodeType.Field)+nameof(ModifierType.Public),"pack://application:,,,/PluginSourceControl;component/ImageResources/Field_16x.xaml" },
            {nameof(NodeType.Field)+nameof(ModifierType.Protected),"pack://application:,,,/PluginSourceControl;component/ImageResources/FieldProtected_16x.xaml" },
            {nameof(NodeType.Field)+nameof(ModifierType.Private),"pack://application:,,,/PluginSourceControl;component/ImageResources/FieldPrivate_16x.xaml" },
            {nameof(NodeType.Field)+nameof(ModifierType.Final),"pack://application:,,,/PluginSourceControl;component/ImageResources/FieldSealed_16x.xaml" },
            {nameof(NodeType.Constructor)+nameof(ModifierType.Public),"pack://application:,,,/PluginSourceControl;component/ImageResources/Method_16x.xaml" },
            {nameof(NodeType.Constructor)+nameof(ModifierType.Protected),"pack://application:,,,/PluginSourceControl;component/ImageResources/MethodProtect_16x.xaml" },
            {nameof(NodeType.Constructor)+nameof(ModifierType.Private),"pack://application:,,,/PluginSourceControl;component/ImageResources/MethodPrivate_16x.xaml" },
            {nameof(NodeType.Method)+nameof(ModifierType.Public),"pack://application:,,,/PluginSourceControl;component/ImageResources/Method_16x.xaml" },
            {nameof(NodeType.Method)+nameof(ModifierType.Protected),"pack://application:,,,/PluginSourceControl;component/ImageResources/MethodProtect_16x.xaml" },
            {nameof(NodeType.Method)+nameof(ModifierType.Private),"pack://application:,,,/PluginSourceControl;component/ImageResources/MethodPrivate_16x.xaml" },
            {nameof(NodeType.Method)+nameof(ModifierType.Final),"pack://application:,,,/PluginSourceControl;component/ImageResources/MethodSealed_16x.xaml" },
            {nameof(NodeType.InterfaceMethod)+nameof(ModifierType.Public),"pack://application:,,,/PluginSourceControl;component/ImageResources/Method_16x.xaml" },
            {nameof(NodeType.InterfaceMethod)+nameof(ModifierType.Default),"pack://application:,,,/PluginSourceControl;component/ImageResources/MethodFriend_16x.xaml" },
            {nameof(NodeType.Enum)+nameof(ModifierType.Public),"pack://application:,,,/PluginSourceControl;component/ImageResources/Enumerator_16x.xaml" },
            {nameof(NodeType.Enum)+nameof(ModifierType.Protected),"pack://application:,,,/PluginSourceControl;component/ImageResources/EnumProtect_16x.xaml" },
            {nameof(NodeType.Enum)+nameof(ModifierType.Private),"pack://application:,,,/PluginSourceControl;component/ImageResources/EnumPrivate_16x.xaml" },
            {nameof(NodeType.Enum)+nameof(ModifierType.Final),"pack://application:,,,/PluginSourceControl;component/ImageResources/EnumSealed_16x.xaml" },
            {nameof(NodeType.EnumConstant)+nameof(ModifierType.Public),"pack://application:,,,/PluginSourceControl;component/ImageResources/EnumItem_16x.xaml" }
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
        public bool IsExpanded 
        { 
            get => _isExpanded;
            set
            {
                SetProperty(ref _isExpanded, value);
            }
        }
        bool _isPending;
        public bool IsPending { get => _isPending; set => SetProperty(ref _isPending, value); }
        #endregion
        #region Operations
        public DelegateCommand UpdateChildrenCommand=>new DelegateCommand(async () =>
        {
            if (!IsPending)
            {
                IsPending = true;
                await UpdateChildren();
                IsPending = false;
            }
        });
        public async Task UpdateChildren()
        {
            if (ParseTreeNode.ItemType == NodeType.CompilationUnit && IsExpanded == true)
            {
                if (Children.Count == 1 && Children[0].ParseTreeNode.ItemType == NodeType.__InternalPlaceHolder)
                {
                    Children.RemoveAt(0);

                    var root = await ParentViewModel.Parent.GetParseTreeAsync(ParseTreeNode.ClassPath, true);
                    List<SourceTreeNode> nodes = new List<SourceTreeNode>();
                    foreach(var child in root.Children)
                    {
                        nodes.Add(UpdateChildrenInternal(child));
                    }
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        this.Children = new ObservableCollection<SourceTreeNode>(nodes);
                    });
                }
            }
        }
        internal SourceTreeNode UpdateChildrenInternal(ParseTreeNode root)
        {
            SourceTreeNode node = new SourceTreeNode() { ParentViewModel = this.ParentViewModel };
            List<SourceTreeNode> nodes = new List<SourceTreeNode>();
            foreach(var child in root.Children)
            {
                nodes.Add(UpdateChildrenInternal(child));
            }
            node.Text = root.Id;
            node.ParseTreeNode = root;
            node.CompilationUnitNode = this;
            //Visual Stuffs
            XmlDocument document = new XmlDocument();
            XmlNamespaceManager nsmgr = new XmlNamespaceManager(document.NameTable);
            nsmgr.AddNamespace("wpf", "http://schemas.microsoft.com/winfx/2006/xaml/presentation");
            var info = Application.GetResourceStream(
                new Uri(IconResource[node.ParseTreeNode.ItemType.ToString() + node.ParseTreeNode.Modifiers.FirstOrDefault()
                .ToString()]));
            using (info.Stream)
            {
                document.Load(info.Stream);
            }
            XmlNode xmlNode = document.SelectSingleNode("/wpf:Viewbox/wpf:Rectangle/wpf:Rectangle.Fill/wpf:DrawingBrush/wpf:DrawingBrush.Drawing", nsmgr);
            Application.Current.Dispatcher.Invoke(() =>
            {
                DrawingImage drawingImage = new DrawingImage();
                drawingImage.Drawing = XamlReader.Parse(xmlNode.InnerXml) as DrawingGroup;
                node.ExpandedIcon = node.Icon = drawingImage;
                node.Children = new ObservableCollection<SourceTreeNode>(nodes);
            });
            
            return node;
        }
        #endregion
    }
}
