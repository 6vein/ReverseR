using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReverseR.Common.Services
{
    public interface IBackgroundTaskBuilder
    {
        IBackgroundTask CreateWithoutFunc();
        IBackgroundTask<T> CreateWithoutFunc<T>();
        /// <summary>
        /// The parameter of <paramref name="action"/> is always the cancellation token,which can be null
        /// </summary>
        /// <param name="action"></param>
        /// <returns></returns>
        IBackgroundTask Create(Action<object> action);
        /// <summary>
        /// The parameter of <paramref name="action"/> is always the cancellation token,which can be null
        /// </summary>
        IBackgroundTask<T> Create<T>(Func<object,T> action);
    }
}
