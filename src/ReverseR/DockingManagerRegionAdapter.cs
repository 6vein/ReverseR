using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Prism.Regions;
using Prism.Events;
using Xceed.Wpf.AvalonDock;
using Xceed.Wpf.AvalonDock.Layout;
using Xceed.Wpf.AvalonDock.Controls;
using ReverseR.Common;
using ReverseR.Common.Events;
using ReverseR.Common.ViewUtilities;
using System.Windows.Data;
using Prism.Ioc;
using Prism.Services.Dialogs;
using ReverseR.Common.Services;
using System.IO;

namespace ReverseR
{
    internal class DockingManagerRegionAdapter : RegionAdapterBase<DockingManager>
    {
        object ActiveContent;
        SubscriptionToken token = null;
        IContainerProvider Container { get; set; }
        #region Constructor

        public DockingManagerRegionAdapter(IRegionBehaviorFactory factory,IContainerProvider containerProvider)
            : base(factory)
        {
            Container = containerProvider;
        }

        #endregion  //Constructor


        #region Overrides

        protected override IRegion CreateRegion()
        {
            return new AllActiveRegion();
        }

        protected override void Adapt(IRegion region, DockingManager regionTarget)
        {
            region.Views.CollectionChanged += delegate (
                Object sender, NotifyCollectionChangedEventArgs e)
            {
                this.OnViewsCollectionChanged(sender, e, region, regionTarget);
            };

            regionTarget.ActiveContentChanged += (s, e) =>
            {
                ActiveContent = regionTarget.ActiveContent;
                if(ActiveContent!=null)
                {
                    object vm = (ActiveContent as FrameworkElement).DataContext;
                    if (vm is IDecompileViewModel viewModel)
                    {
                        token = Container.Resolve<IEventAggregator>().GetEvent<MenuUpdatedEvent>().
                        Subscribe((regionTarget.DataContext as ViewModels.MainWindowViewModel).OnMenuUpdated, ThreadOption.UIThread, false, r => r.guid == viewModel.Guid);
                        (regionTarget.DataContext as ViewModels.MainWindowViewModel).ActiveContent = ActiveContent;
                    }
                }
                else
                {
                    if(token!=null)
                    {
                        Container.Resolve< IEventAggregator>().GetEvent<MenuUpdatedEvent>().Unsubscribe(token);
                        token = null;
                    }
                }
            };

            regionTarget.DocumentClosed += delegate (
                            object sender, DocumentClosedEventArgs e)
            {
                this.OnDocumentClosedEventArgs(sender, e, region);
            };
        }

        #endregion  //Overrides


        #region Event Handlers

        /// <summary>
        /// Handles the NotifyCollectionChangedEventArgs event.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The event.</param>
        /// <param name="region">The region.</param>
        /// <param name="regionTarget">The region target.</param>
        void OnViewsCollectionChanged(object sender, NotifyCollectionChangedEventArgs e, IRegion region, DockingManager regionTarget)
        {
            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                foreach (FrameworkElement item in e.NewItems)
                {
                    UIElement view = item as UIElement;

                    if (view != null)
                    {
                        //Create a new layout document to be included in the LayoutDocuemntPane (defined in xaml)
                        LayoutDocument newLayoutDocument = new LayoutDocument();
                        //Set the content of the LayoutDocument
                        newLayoutDocument.Content = item;

                        /*IDecompileViewModel viewModel = item.DataContext as IDecompileViewModel;

                        if (viewModel != null)
                        {
                            Binding binding = new Binding("Title") { Source = viewModel,Mode=BindingMode.OneWay };
                            BindingOperations.SetBinding(newLayoutDocument, LayoutDocument.TitleProperty, binding);
                        }*/

                        //Store all LayoutDocuments already pertaining to the LayoutDocumentPane (defined in xaml)
                        List<LayoutDocument> oldLayoutDocuments = new List<LayoutDocument>();
                        //Get the current ILayoutDocumentPane ... Depending on the arrangement of the views this can be either 
                        //a simple LayoutDocumentPane or a LayoutDocumentPaneGroup
                        ILayoutDocumentPane currentILayoutDocumentPane = (ILayoutDocumentPane)regionTarget.Layout.RootPanel.Children[0];

                        if (currentILayoutDocumentPane.GetType() == typeof(LayoutDocumentPaneGroup))
                        {
                            //If the current ILayoutDocumentPane turns out to be a group
                            //Get the children (LayoutDocuments) of the first pane
                            
                            LayoutDocumentPane oldLayoutDocumentPane = (LayoutDocumentPane)currentILayoutDocumentPane.Children.ToList()[0];
                            foreach (LayoutDocument child in oldLayoutDocumentPane.Children)
                            {
                                oldLayoutDocuments.Insert(0, child);
                            }
                        }
                        else if (currentILayoutDocumentPane.GetType() == typeof(LayoutDocumentPane))
                        {
                            //If the current ILayoutDocumentPane turns out to be a simple pane
                            //Get the children (LayoutDocuments) of the single existing pane.
                            foreach (LayoutDocument child in currentILayoutDocumentPane.Children)
                            {
                                oldLayoutDocuments.Insert(0, child);
                            }
                        }

                        //Create a new LayoutDocumentPane and inserts your new LayoutDocument
                        LayoutDocumentPane newLayoutDocumentPane = new LayoutDocumentPane();
                        newLayoutDocumentPane.InsertChildAt(0, newLayoutDocument);

                        //Append to the new LayoutDocumentPane the old LayoutDocuments
                        foreach (LayoutDocument doc in oldLayoutDocuments)
                        {
                            newLayoutDocumentPane.InsertChildAt(0, doc);
                        }

                        //Traverse the visual tree of the xaml and replace the LayoutDocumentPane (or LayoutDocumentPaneGroup) in xaml
                        //with your new LayoutDocumentPane (or LayoutDocumentPaneGroup)
                        if (currentILayoutDocumentPane.GetType() == typeof(LayoutDocumentPane))
                            regionTarget.Layout.RootPanel.ReplaceChildAt(0, newLayoutDocumentPane);
                        else if (currentILayoutDocumentPane.GetType() == typeof(LayoutDocumentPaneGroup))
                        {
                            currentILayoutDocumentPane.ReplaceChild(currentILayoutDocumentPane.Children.ToList()[0], newLayoutDocumentPane);
                            regionTarget.Layout.RootPanel.ReplaceChildAt(0, currentILayoutDocumentPane);
                        }
                        newLayoutDocument.IsActive = true;
                    }
                }
            }
        }

        /// <summary>
        /// Handles the DocumentClosedEventArgs event raised by the DockingNanager when
        /// one of the LayoutContent it hosts is closed.
        /// </summary>
        /// <param name="sender">The sender</param>
        /// <param name="e">The event.</param>
        /// <param name="region">The region.</param>
        void OnDocumentClosedEventArgs(object sender, DocumentClosedEventArgs e, IRegion region)
        {
            if((e.Document.Content as FrameworkElement)?.DataContext is IDecompileViewModel viewModel)
            {
                bool closed = false;
                if (!viewModel.Shutdown(false)) 
                {
                    Container.Resolve<IDialogService>()
                        .PresentConfirmation($"File {Path.GetFileName(viewModel.FilePath)}"
                        + "didn't seem to close in time.\nWould you force to close it?",
                        r =>
                        {
                            if (r?.Result == ButtonResult.Yes)
                            {
                                viewModel.Shutdown(true);
                                closed = true;
                            }
                        });
                }
                else
                {
                    closed = true;
                }
                if (closed) region.Remove(e.Document.Content);
            }
        }

        #endregion   
    }
}
