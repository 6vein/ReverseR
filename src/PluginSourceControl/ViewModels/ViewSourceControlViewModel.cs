using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using Prism.Ioc;
using ReverseR.Common;
using ReverseR.Common.ViewUtilities;
using ReverseR.Common.DecompUtilities;
using System.Windows.Controls;
using System.Windows.Interop;
using System.Windows.Media.Imaging;
using System.Windows.Media;
using System.Windows.Markup;
using System.Xml;
using System.Windows.Input;
using System.Text.RegularExpressions;

namespace PluginSourceControl.ViewModels
{
    public class ViewSourceControlViewModel : BindableBase, IPluginViewModel
    {
        IContainerProvider Container { get; }

        public IDecompileViewModel Parent { get; set; }

        private string _title = "Source Control";
        public string Title
        {
            get { return _title; }
            set { SetProperty(ref _title, value); }
        }
        private string classRoot;
        private string JarName => Path.GetFileNameWithoutExtension(Parent.FilePath);
        /*private double _width;
        public double Width
        {
            get { return _width; }
            set { SetProperty(ref _width, value); }
        }

        private double _height;
        public double Height
        {
            get { return _height; }
            set { SetProperty(ref _height, value); }
        }*/

        ObservableCollection<SourceTreeNode> _sourceTree;
        public ObservableCollection<SourceTreeNode> SourceTree { get => _sourceTree; set => SetProperty(ref _sourceTree, value); }

        /// <summary>
        /// Creates and updates the basic tree view of the output
        /// </summary>
        /// <param name="baseDir">
        /// Base directory,see <see cref="ReverseR.Common.ViewUtilities.DecompileViewModelBase.BaseDirectory"/> for more info
        /// </param>
        public void UpdateSourceTree(string baseDir)
        {
            Title = "Source Control - Loading...";
            SourceTree = new ObservableCollection<SourceTreeNode>();
            classRoot = baseDir + "\\Content";
            /*string contentRoot = baseDir + "\\Content";
            var directories = Directory.GetDirectories(contentRoot);
            foreach (var directory in directories)
            {
                SourceTreeNode node = Container.Resolve<SourceTreeNode>();
                node.Text = Path.GetFileName(directory);
                node.Type = SourceTreeNode.NodeType.Directory;
                node.JPath = directory;
                XmlDocument document = new XmlDocument();
                XmlNamespaceManager nsmgr = new XmlNamespaceManager(document.NameTable);
                nsmgr.AddNamespace("wpf", "http://schemas.microsoft.com/winfx/2006/xaml/presentation");
                var info = Application.GetResourceStream(new Uri(SourceTreeNode.IconResource["<folder>"]));
                using (info.Stream)
                {
                    document.Load(info.Stream);
                }
                XmlNode xmlNode = document.SelectSingleNode("/wpf:Viewbox/wpf:Rectangle/wpf:Rectangle.Fill/wpf:DrawingBrush/wpf:DrawingBrush.Drawing", nsmgr);
                var infoopen = Application.GetResourceStream(new Uri(SourceTreeNode.IconResource["<folderopen>"]));
                using (infoopen.Stream)
                {
                    document.Load(infoopen.Stream);
                }
                XmlNode xmlNodeopen = document.SelectSingleNode("/wpf:Viewbox/wpf:Rectangle/wpf:Rectangle.Fill/wpf:DrawingBrush/wpf:DrawingBrush.Drawing", nsmgr);
                var child = UpdateSourceTreeInternal(directory);
                Application.Current.Dispatcher.Invoke(() =>
                {
                    DrawingImage drawingImage = new DrawingImage();
                    drawingImage.Drawing = XamlReader.Parse(xmlNode.InnerXml) as DrawingGroup;
                    node.Icon = drawingImage;
                    DrawingImage imageOpen = new DrawingImage();
                    imageOpen.Drawing = XamlReader.Parse(xmlNodeopen.InnerXml) as DrawingGroup;
                    node.ExpandedIcon = imageOpen;
                    node.Children = new ObservableCollection<SourceTreeNode>(child);
                    SourceTree.Add(node);
                });
            }
            var fileIgnore = new List<string>();
            var files = Directory.GetFiles(contentRoot).ToList();
            foreach (var path in files)
            {
                SourceTreeNode node = Container.Resolve<SourceTreeNode>();
                if (fileIgnore.Contains(path))
                {
                    continue;
                }
                if (Path.GetExtension(path).ToLower() == ".class")
                {
                    Regex regex = new Regex(@"(\w+)\$\w+");
                    Match match = regex.Match(Path.GetFileNameWithoutExtension(path));
                    if (match.Success)
                    {
                        Regex engine = new Regex($@"{match.Result("$1")}\$\w+");
                        node.Type = SourceTreeNode.NodeType.CompilationUnit;
                        node.JPath = new JPath(Path.Combine(Path.GetDirectoryName(path), match.Result("$1") + Path.GetExtension(path)),
                            files.Where(s => engine.Match(Path.GetFileNameWithoutExtension(s)).Success));
                        node.Text = Path.GetFileNameWithoutExtension(node.JPath.Path);
                        //add to the ignore list to prevent from duplicating
                        fileIgnore.Add(node.JPath.Path);
                        fileIgnore.AddRange(node.JPath.InnerClassPaths);
                    }
                    else
                    {
                        node.Type = SourceTreeNode.NodeType.CompilationUnit;
                        node.JPath = path;
                        node.Text = Path.GetFileNameWithoutExtension(path);
                    }
                    XmlDocument document = new XmlDocument();
                    XmlNamespaceManager nsmgr = new XmlNamespaceManager(document.NameTable);
                    nsmgr.AddNamespace("wpf", "http://schemas.microsoft.com/winfx/2006/xaml/presentation");
                    var info = Application.GetResourceStream(new Uri(SourceTreeNode.IconResource[".java"]));
                    document.Load(info.Stream);
                    XmlNode xmlNode = document.SelectSingleNode("/wpf:Viewbox/wpf:Rectangle/wpf:Rectangle.Fill/wpf:DrawingBrush/wpf:DrawingBrush.Drawing", nsmgr);
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        DrawingImage drawingImage = new DrawingImage();
                        drawingImage.Drawing = XamlReader.Parse(xmlNode.InnerXml) as DrawingGroup;
                        DrawingImage imageOpen = new DrawingImage();
                        imageOpen.Drawing = XamlReader.Parse(xmlNode.InnerXml) as DrawingGroup;
                        node.ExpandedIcon = imageOpen;
                        node.Icon = drawingImage;
                        node.Children = new ObservableCollection<SourceTreeNode>() { new SourceTreeNode() { Type = SourceTreeNode.NodeType.__InternalPlaceHolder } };
                        SourceTree.Add(node);
                    });
                }
                else
                {
                    node.Type = SourceTreeNode.NodeType.Others;
                    node.JPath = path;
                    node.Text = Path.GetFileName(path);
                    using (var icon = System.Drawing.Icon.ExtractAssociatedIcon(path))
                    {
                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            var ico = Imaging.CreateBitmapSourceFromHIcon(icon.Handle, new Int32Rect(0, 0, icon.Width, icon.Height), BitmapSizeOptions.FromEmptyOptions());
                            node.Icon = node.ExpandedIcon = ico;
                            SourceTree.Add(node);
                        });
                    }
                }
            }*/
            IEnumerable<SourceTreeNode> collection = UpdateSourceTreeInternal(baseDir + "\\Content");
            System.Windows.Application.Current.Dispatcher.Invoke(() =>
            {
                SourceTree.AddRange(collection);
            });
            Title = "Source Control";
        }

        internal IEnumerable<SourceTreeNode> UpdateSourceTreeInternal(string root)
        {
            List<SourceTreeNode> lstRet = new List<SourceTreeNode>();
            string contentRoot = root;
            var directories = Directory.GetDirectories(contentRoot);
            foreach (var directory in directories)
            {
                SourceTreeNode node = new SourceTreeNode() { ParentViewModel = this };
                node.Text = Path.GetFileName(directory);
                node.Type = SourceTreeNode.NodeType.Directory;
                node.JPath = new JPath(directory, directory.Replace(classRoot, JarName));
                XmlDocument document = new XmlDocument();
                XmlNamespaceManager nsmgr = new XmlNamespaceManager(document.NameTable);
                nsmgr.AddNamespace("wpf", "http://schemas.microsoft.com/winfx/2006/xaml/presentation");
                var info = Application.GetResourceStream(new Uri(SourceTreeNode.IconResource["<folder>"]));
                using (info.Stream)
                {
                    document.Load(info.Stream);
                }
                XmlNode xmlNode = document.SelectSingleNode("/wpf:Viewbox/wpf:Rectangle/wpf:Rectangle.Fill/wpf:DrawingBrush/wpf:DrawingBrush.Drawing", nsmgr);
                var infoopen = Application.GetResourceStream(new Uri(SourceTreeNode.IconResource["<folderopen>"]));
                using (infoopen.Stream)
                {
                    document.Load(infoopen.Stream);
                }
                XmlNode xmlNodeopen = document.SelectSingleNode("/wpf:Viewbox/wpf:Rectangle/wpf:Rectangle.Fill/wpf:DrawingBrush/wpf:DrawingBrush.Drawing", nsmgr);
                var child = UpdateSourceTreeInternal(directory);
                Application.Current.Dispatcher.Invoke(() =>
                {
                    DrawingImage drawingImage = new DrawingImage();
                    drawingImage.Drawing = XamlReader.Parse(xmlNode.InnerXml) as DrawingGroup;
                    node.Icon = drawingImage;
                    DrawingImage imageOpen = new DrawingImage();
                    imageOpen.Drawing = XamlReader.Parse(xmlNodeopen.InnerXml) as DrawingGroup;
                    node.ExpandedIcon = imageOpen;
                    node.Children = new ObservableCollection<SourceTreeNode>(child);
                    lstRet.Add(node);
                });
            }
            var fileIgnore = new List<string>();
            var files = Directory.GetFiles(contentRoot).ToList();
            foreach (var path in files)
            {
                SourceTreeNode node = new SourceTreeNode() { ParentViewModel = this };
                if (fileIgnore.Contains(path))
                {
                    continue;
                }
                if (Path.GetExtension(path).ToLower() == ".class")
                {
                    Regex regex = new Regex(@"(\w+)\$\w+");
                    Match match = regex.Match(Path.GetFileNameWithoutExtension(path));
                    if (match.Success)
                    {
                        Regex engine = new Regex($@"{match.Result("$1")}\$\w+");
                        node.Type = SourceTreeNode.NodeType.CompilationUnit;
                        node.JPath = new JPath(Path.Combine(Path.GetDirectoryName(path), match.Result("$1") + Path.GetExtension(path)),
                            path.Replace(classRoot, JarName)
                            , files.Where(s => engine.Match(Path.GetFileNameWithoutExtension(s)).Success));
                        node.Text = Path.GetFileNameWithoutExtension(node.JPath.Path);
                        //add to the ignore list to prevent from duplicating
                        fileIgnore.Add(node.JPath.Path);
                        fileIgnore.AddRange(node.JPath.InnerClassPaths);
                    }
                    else
                    {
                        node.Type = SourceTreeNode.NodeType.CompilationUnit;
                        node.JPath = new JPath(path, path.Replace(classRoot, JarName));
                        node.Text = Path.GetFileNameWithoutExtension(path);
                    }
                    XmlDocument document = new XmlDocument();
                    XmlNamespaceManager nsmgr = new XmlNamespaceManager(document.NameTable);
                    nsmgr.AddNamespace("wpf", "http://schemas.microsoft.com/winfx/2006/xaml/presentation");
                    var info = Application.GetResourceStream(new Uri(SourceTreeNode.IconResource[".java"]));
                    document.Load(info.Stream);
                    XmlNode xmlNode = document.SelectSingleNode("/wpf:Viewbox/wpf:Rectangle/wpf:Rectangle.Fill/wpf:DrawingBrush/wpf:DrawingBrush.Drawing", nsmgr);
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        DrawingImage drawingImage = new DrawingImage();
                        drawingImage.Drawing = XamlReader.Parse(xmlNode.InnerXml) as DrawingGroup;
                        DrawingImage imageOpen = new DrawingImage();
                        imageOpen.Drawing = XamlReader.Parse(xmlNode.InnerXml) as DrawingGroup;
                        node.ExpandedIcon = imageOpen;
                        node.Icon = drawingImage;
                        node.Children = new ObservableCollection<SourceTreeNode>() { new SourceTreeNode() { Type = SourceTreeNode.NodeType.__InternalPlaceHolder } };
                        lstRet.Add(node);
                    });
                }
                else
                {
                    node.Type = SourceTreeNode.NodeType.Others;
                    node.JPath = new JPath(path, "");
                    node.Text = Path.GetFileName(path);
                    using (var icon = System.Drawing.Icon.ExtractAssociatedIcon(path))
                    {
                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            var ico = Imaging.CreateBitmapSourceFromHIcon(icon.Handle, new Int32Rect(0, 0, icon.Width, icon.Height), BitmapSizeOptions.FromEmptyOptions());
                            node.Icon = node.ExpandedIcon = ico;
                            lstRet.Add(node);
                        });
                    }
                }
            }
            return lstRet;
        }

        public DelegateCommand<MouseButtonEventArgs> ItemDblClickCommand => new DelegateCommand<MouseButtonEventArgs>(e =>
          {
              if (e.OriginalSource is FrameworkElement element)
              {
                  /*
                   * //This statement check if the mouse is on the expander
                  if (e.OriginalSource is System.Windows.Shapes.Path)
                  {
                      //Clicked on the expand button
                      return;
                  }
                  else if (e.OriginalSource is Grid grid)
                  {
                      if (VisualTreeHelper.GetParent(grid) != null)
                      {
                          if ((VisualTreeHelper.GetParent(grid) as FrameworkElement).Name == "Expander")
                          {
                              return;
                          }
                      }
                  }
                   */
                  var item = element.FindParent<TreeViewItem>();
                  if (item != null)
                  {
                      var node = (item.DataContext as SourceTreeNode);
                      if (node.Type == SourceTreeNode.NodeType.CompilationUnit)
                      {
                          if (node.AssociatedDocument == null||!Parent.Documents.Contains(node.AssociatedDocument))
                              node.AssociatedDocument = Parent.OpenDocument(node.JPath);
                          else
                              Parent.ActivateDocument(node.AssociatedDocument);
                      }
                      else if (node.Type != SourceTreeNode.NodeType.Directory && node.Type != SourceTreeNode.NodeType.__InternalPlaceHolder)
                      {
                          if (node.CompilationUnitNode.AssociatedDocument == null || !Parent.Documents.Contains(node.CompilationUnitNode.AssociatedDocument))
                          {
                              node.CompilationUnitNode.AssociatedDocument = Parent.OpenDocument(node.CompilationUnitNode.JPath);
                          }
                          else
                              Parent.ActivateDocument(node.CompilationUnitNode.AssociatedDocument);
                          Task.Run(async () =>
                          {
                              await node.CompilationUnitNode.AssociatedDocument.SelectAsync(node.Start, node.Stop);
                          });
                      }
                  }
                  //prevent from expanding/collapsing
                  e.Handled = true;
              }
          });

        public Action OnPluginUnload => () => { };

        public ViewSourceControlViewModel()
        {
            Container = this.GetIContainer();
        }
    }
}
