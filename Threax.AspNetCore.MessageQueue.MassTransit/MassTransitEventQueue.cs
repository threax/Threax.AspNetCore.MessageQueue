using MassTransit;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Threax.AspNetCore.MessageQueue.MassTransit
{
    /// <summary>
    /// An IEventQueue that uses mass transit to publish.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class MassTransitEventQueue<T> : IEventQueue<T>
        where T : class
    {
        private List<T> events = new List<T>();
        private List<EventQueueError<T>> errors = new List<EventQueueError<T>>();
        private IPublishEndpoint publisher;

        public MassTransitEventQueue(IPublishEndpoint publisher)
        {
            this.publisher = publisher;
        }

        public void Add(T evt)
        {
            events.Add(evt);
        }

        public async Task Fire()
        {
            errors.Clear();
            foreach (var evt in events)
            {
                try
                {
                    await this.publisher.Publish<T>(evt);
                }
                catch (Exception ex)
                {
                    errors.Add(new EventQueueError<T>()
                    {
                        Event = evt,
                        Exception = ex
                    });
                }
            }
            events.Clear();
        }

        public bool Sucessful
        {
            get
            {
                return errors.Count == 0;
            }
        }

        public IEnumerable<EventQueueError<T>> Errors
        {
            get
            {
                return errors;
            }
        }

        public void FireAndForget()
        {
            Task.Run(Fire);
        }
    }
}
