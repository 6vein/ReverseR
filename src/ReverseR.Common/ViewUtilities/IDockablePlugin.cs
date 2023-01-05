using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xceed.Wpf.AvalonDock.Layout;
using ReverseR.Common.Extensibility;
using System.IO;

namespace ReverseR.Common.ViewUtilities
{
    /// <summary>
    /// Classes implementing this interface should create the <see cref="View"/> and assign the <see cref="ViewModel"/> itself
    /// <para>
    /// The <see cref="View"/> is usually injected with <see cref="Prism.Regions.IRegionManager.AddToRegion(string, object)"/>
    /// </para>
    /// <para>
    /// the Region Name is <see cref="IPlugin.Id"/> + <see cref="IDecompileViewModel.Guid"/>
    /// </para>
    /// </summary>
    public interface IDockablePlugin:IPlugin,IDIAble
    {
        /// <summary>
        /// The name shown in the Views menu, supports access text
        /// </summary>
        public string ViewMenuText { get; }
        /// <summary>
        /// The key gesture shown in the Views menu.
        /// </summary>
        public string ViewMenuGesture { get; }
        public AnchorableShowStrategy Side { get; set; }
        public double InitialWidth { get; }
        public double InitialHeight { get; }
        public object View { get; set; }
        public IPluginViewModel ViewModel { get; set; }
    }
}
