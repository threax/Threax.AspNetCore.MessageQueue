using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Threax.AspNetCore.MessageQueue
{
    public interface IEventQueue<T>
         where T : class
    {
        /// <summary>
        /// Add a typed event to the queue.
        /// </summary>
        /// <param name="evt"></param>
        void Add(T evt);

        /// <summary>
        /// Fire all the queued events to the actual queue. After firing events they are cleared from the queue.
        /// </summary>
        /// <returns>A task.</returns>
        Task Fire();

        /// <summary>
        /// Fire all the queued events to the actual queue. This will fire on a background thread
        /// and won't block execution of the current thread. After firing events they are cleared from the queue.
        /// </summary>
        void FireAndForget();

        /// <summary>
        /// Returns true if the last fire event was sucessful.
        /// </summary>
        bool Sucessful { get; }

        /// <summary>
        /// Get any errors that occured when firing the queue.
        /// </summary>
        IEnumerable<EventQueueError<T>> Errors { get; }
    }
}
