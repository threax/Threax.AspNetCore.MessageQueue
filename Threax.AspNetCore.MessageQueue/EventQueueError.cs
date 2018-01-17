using System;
using System.Collections.Generic;
using System.Text;

namespace Threax.AspNetCore.MessageQueue
{
    public class EventQueueError<T>
    {
        public T Event { get; set; }

        public Exception Exception { get; set; }
    }
}
