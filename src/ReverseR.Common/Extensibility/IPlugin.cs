using ReverseR.Common.ViewUtilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.ObjectModel;

namespace ReverseR.Common.Extensibility
{
    public interface IPlugin
    {
        [Flags]
        public enum NotifyOptions
        {
            DonotNotfiy = 0,
            ArchiveOpened = 1 << 1,
            TextEdited = 1 << 2
        }

        public NotifyOptions NotifyOption { get; }

        public IDecompileViewModel ParentViewModel { get; }

        void InitalizePlugin(IDecompileViewModel parent);
        void UnloadPlugin();
        ObservableCollection<IMenuViewModel> InsertPluginMenu();
    }
}
