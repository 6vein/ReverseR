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
        public JPath JPath { get; set; }
        public enum NodeType
        {
            Directory,
            CompilationUnit,
            Class, Interface, Field, Constructor, Method, InterfaceMethod,
             Enum, EnumConstant,
            Others,
            __InternalPlaceHolder=-1
        }
        public NodeType Type { get; set; }
        public IClassParser.ModifierType ModifierType { get; set; }

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
            if (Type == NodeType.CompilationUnit && IsExpanded == true)
            {
                if (Children.Count == 1 && Children[0].Type == NodeType.__InternalPlaceHolder)
                {
                    Children.RemoveAt(0);

                    var builder =
                        this.GetIContainer()
                        .Resolve<IBackgroundTaskBuilder<IEnumerable<IClassParser.ParseTreeNode>>>();
                    IBackgroundTask<IEnumerable<IClassParser.ParseTreeNode>> parseTask=builder
                        .WithTask(_ =>
                        {
                            IEnumerable<IClassParser.ParseTreeNode> ret = null;
                            var basedir = Path.GetDirectoryName(JPath.Path);
                            var tempPath = Path.GetTempFileName();
                            File.Delete(tempPath);
                            Directory.CreateDirectory(tempPath);
                            File.Copy(JPath.Path, Path.Combine(tempPath, Path.GetFileName(JPath.Path)));
                            if (JPath.InnerClassPaths != null)
                            {
                                foreach (var p in JPath.InnerClassPaths)
                                {
                                    File.Copy(p, Path.Combine(tempPath, Path.GetFileName(p)));
                                }
                            }
                            IDecompileResult result = null;
                            try
                            {
                                result = this.GetIContainer()
                                    .Resolve<IDecompilerResolver>()
                                    .Resolve<CommonDecompiler>((GlobalUtils.PreferredDecompiler??GlobalUtils.Decompilers[0]).Id)
                                    .Decompile(tempPath, r => { }, null, ParentViewModel.Parent.BaseDirectory + "\\raw.jar");
                                if (result.ResultCode == DecompileResultEnum.Success)
                                {
                                    var files = Directory.GetFiles(result.OutputDir);
#if DEBUG
                                    Debug.Assert(files.Length == 1);
#endif
                                    ret = this.GetIContainer()
                                        .Resolve<IClassParser>()
                                        .Parse(File.ReadAllText(files[0]));
                                }
                                else
                                {
                                    this.GetIContainer().Resolve<IDialogService>().ReportError(result.ResultCode.ToString(), __ => { });
                                }
                            }
                            catch (Exception e)
                            {
                                string message = null;
                                if (e is Win32Exception win32Exception)
                                {
                                    message = $"Unexpected exception:\n{win32Exception.Message}\nHRESULT is {System.Runtime.InteropServices.Marshal.GetHRForLastWin32Error().ToString("x")}";
                                }
                                else message = $"Unexpected exception:\n{e.Message}\n";
                                this.GetIContainer().Resolve<IDialogService>().ReportError(message, r => { }, e.StackTrace);
                            }
                            /*if (token.HasValue)
                            {
                                token.Value.ThrowIfCancellationRequested();
                            }*/
                            Directory.Delete(tempPath, true);
                            return ret;
                        })
                        .WithName("Background:Parsing")
                        .WithDescription("Parsing" + JPath.ClassPath)
                        .Build();
                    parseTask.Start();

                    await parseTask.IsCompletedTask;
                    IEnumerable<IClassParser.ParseTreeNode> root = parseTask.Result;
                    List<SourceTreeNode> nodes=new List<SourceTreeNode>();
                    foreach(var child in root)
                    {
                        nodes.Add(UpdateChildrenInternal(child, "/" + child.Id,JPath));
                    }
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        this.Children = new ObservableCollection<SourceTreeNode>(nodes);
                    });
                }
            }
        }
        internal SourceTreeNode UpdateChildrenInternal(IClassParser.ParseTreeNode root,string classPath,JPath origPath)
        {
            SourceTreeNode node = new SourceTreeNode() { ParentViewModel = this.ParentViewModel };
            List<SourceTreeNode> nodes = new List<SourceTreeNode>();
            foreach(var child in root.Children)
            {
                nodes.Add(UpdateChildrenInternal(child, classPath + "/" + child.Id,origPath));
            }
            switch (root.ItemType)
            {
                case IClassParser.ItemType.Class:node.Type=NodeType.Class; break;
                case IClassParser.ItemType.Interface: node.Type = NodeType.Interface; break;
                case IClassParser.ItemType.Method: node.Type = NodeType.Method; break;
                case IClassParser.ItemType.Field: node.Type = NodeType.Field; break;
                case IClassParser.ItemType.InterfaceMethod: node.Type = NodeType.InterfaceMethod; break;
                case IClassParser.ItemType.Constructor: node.Type = NodeType.Constructor; break;
                case IClassParser.ItemType.Enum: node.Type = NodeType.Enum; break;
                case IClassParser.ItemType.EnumConstant: node.Type = NodeType.EnumConstant; break;
                default:node.Type = NodeType.__InternalPlaceHolder; break;
            }
            node.ModifierType = root.Modifiers.First();
            node.JPath = new JPath(origPath);
            node.JPath.ClassPath = node.JPath.ClassPath.Remove(node.JPath.ClassPath.LastIndexOf('/')) + classPath;
            node.Text = root.Content;
            //Visual Stuffs
            XmlDocument document = new XmlDocument();
            XmlNamespaceManager nsmgr = new XmlNamespaceManager(document.NameTable);
            try
            {
                nsmgr.AddNamespace("wpf", "http://schemas.microsoft.com/winfx/2006/xaml/presentation");
                var info = Application.GetResourceStream(new Uri(IconResource[node.Type.ToString() + node.ModifierType.ToString()]));
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
            }
            catch(Exception ex)
            {

            }
            return node;
        }
        #endregion
    }
}
