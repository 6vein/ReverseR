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
using ReverseR.Common.Code;

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
            IEnumerable<SourceTreeNode> collection = UpdateSourceTreeInternal(Parent.GetParseTreeAsync("").Result);
            System.Windows.Application.Current.Dispatcher.Invoke(() =>
            {
                SourceTree.AddRange(collection);
            });
            Title = "Source Control";
        }

        internal IEnumerable<SourceTreeNode> UpdateSourceTreeInternal(ParseTreeNode root)
        {
            List<SourceTreeNode> lstRet = new List<SourceTreeNode>();
            XmlDocument document = new XmlDocument();
            XmlNamespaceManager nsmgr = new XmlNamespaceManager(document.NameTable);
            nsmgr.AddNamespace("wpf", "http://schemas.microsoft.com/winfx/2006/xaml/presentation");
            foreach (var item in root.Children)
            {
                SourceTreeNode node = new SourceTreeNode() { ParentViewModel = this };
                node.Text = item.Id;
                node.ParseTreeNode = item;
                if (item.ItemType == IClassParser.ItemType.Directory)
                {
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
                    var child = UpdateSourceTreeInternal(item);
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        DrawingImage drawingImage = new DrawingImage();
                        drawingImage.Drawing = XamlReader.Parse(xmlNode.InnerXml) as DrawingGroup;
                        node.Icon = drawingImage;
                        DrawingImage imageOpen = new DrawingImage();
                        imageOpen.Drawing = XamlReader.Parse(xmlNodeopen.InnerXml) as DrawingGroup;
                        node.ExpandedIcon = imageOpen;
                        node.Children = new ObservableCollection<SourceTreeNode>(child);
                    });
                    lstRet.Add(node);
                }
                else if (item.ItemType == IClassParser.ItemType.CompilationUnit)
                {
                    var info = Application.GetResourceStream(new Uri(SourceTreeNode.IconResource[".java"]));
                    using (info.Stream)
                    {
                        document.Load(info.Stream);
                    }
                    XmlNode xmlNode = document.SelectSingleNode("/wpf:Viewbox/wpf:Rectangle/wpf:Rectangle.Fill/wpf:DrawingBrush/wpf:DrawingBrush.Drawing", nsmgr);
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        DrawingImage drawingImage = new DrawingImage();
                        drawingImage.Drawing = XamlReader.Parse(xmlNode.InnerXml) as DrawingGroup;
                        node.Icon = node.ExpandedIcon = drawingImage;
                        node.Children = new ObservableCollection<SourceTreeNode>() { new SourceTreeNode() { ParseTreeNode=item.Children[0] } };
                    });
                    lstRet.Add(node);
                }
                else if (item.ItemType == IClassParser.ItemType.Others)
                {
                    using (var icon = System.Drawing.Icon.ExtractAssociatedIcon(item.Path))
                    {
                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            var ico = Imaging.CreateBitmapSourceFromHIcon(icon.Handle, new Int32Rect(0, 0, icon.Width, icon.Height), BitmapSizeOptions.FromEmptyOptions());
                            node.Icon = node.ExpandedIcon = ico;
                        });
                    }                        
                    lstRet.Add(node);
                }
                else
                {
                    throw new NotImplementedException();
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
                      if (node.ParseTreeNode.ItemType == IClassParser.ItemType.CompilationUnit)
                      {
                          if (node.AssociatedDocument == null||!Parent.Documents.Contains(node.AssociatedDocument))
                              node.AssociatedDocument = Parent.OpenDocument(node.ParseTreeNode);
                          else
                              Parent.ActivateDocument(node.AssociatedDocument);
                      }
                      else if (node.ParseTreeNode.ItemType != IClassParser.ItemType.Directory && node.ParseTreeNode.ItemType != IClassParser.ItemType.__InternalPlaceHolder)
                      {
                          if (node.CompilationUnitNode.AssociatedDocument == null || !Parent.Documents.Contains(node.CompilationUnitNode.AssociatedDocument))
                          {
                              node.CompilationUnitNode.AssociatedDocument = Parent.OpenDocument(node.CompilationUnitNode.ParseTreeNode);
                          }
                          else
                              Parent.ActivateDocument(node.CompilationUnitNode.AssociatedDocument);
                          Task.Run(async () =>
                          {
                              await node.CompilationUnitNode.AssociatedDocument.SelectAsync(node.ParseTreeNode.Start, node.ParseTreeNode.End);
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
