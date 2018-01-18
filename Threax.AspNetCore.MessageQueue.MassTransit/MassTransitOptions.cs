using MassTransit;
using MassTransit.ExtensionsDependencyInjectionIntegration;
using MassTransit.Saga;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Threax.AspNetCore.MessageQueue.MassTransit
{
    enum RegistrationType
    {
        Singleton,
        Transient,
        Scoped
    }

    class MassTransitServiceRegistration
    {
        public Type Type { get; set; }

        public RegistrationType RegistrationType { get; set; }
    }

    /// <summary>
    /// Options for mass transit. Also makes it easier to register consumers and sagas vs the default implementation.
    /// </summary>
    public class MassTransitOptions
    {
        /// <summary>
        /// The url of the rabbitmq host. If you set this to "InMemory" (case sensitive) the in memory queue will be used. This is useful if you
        /// cannot run the RabbitMQ service for some reason.
        /// </summary>
        public String Host { get; set; }

        /// <summary>
        /// The username.
        /// </summary>
        public String Username { get; set; }

        /// <summary>
        /// The password.
        /// </summary>
        public String Password { get; set; }

        /// <summary>
        /// The name of the queue in RabbitMQ for this application. Ideally set this to the application scope name, but
        /// you can customize it if you need to.
        /// </summary>
        public String QueueName { get; set; }

        [JsonIgnore]
        internal bool OpenQueue
        {
            get
            {
                return Consumers.Count != 0 || Sagas.Count != 0;
            }
        }

        [JsonIgnore]
        internal List<MassTransitServiceRegistration> Consumers { get; private set; } = new List<MassTransitServiceRegistration>();

        [JsonIgnore]
        internal List<MassTransitServiceRegistration> Sagas { get; private set; } = new List<MassTransitServiceRegistration>();

        /// <summary>
        /// Add a consumer as a singleton. It must inherit from IConsumer&lt;T&gt; where T is the message you want to respond ot.
        /// </summary>
        /// <typeparam name="T">The type of the consumer to add.</typeparam>
        public void AddSingletonConsumer<T>() where T : class, IConsumer
        {
            Consumers.Add(new MassTransitServiceRegistration()
            {
                Type = typeof(T),
                RegistrationType = RegistrationType.Singleton
            });
        }

        /// <summary>
        /// Add a consumer as a scoped instance. It must inherit from IConsumer&lt;T&gt; where T is the message you want to respond ot.
        /// </summary>
        /// <typeparam name="T">The type of the consumer to add.</typeparam>
        public void AddScopedConsumer<T>() where T : class, IConsumer
        {
            Consumers.Add(new MassTransitServiceRegistration()
            {
                Type = typeof(T),
                RegistrationType = RegistrationType.Scoped
            });
        }

        /// <summary>
        /// Add a consumer as a transient instance. It must inherit from IConsumer&lt;T&gt; where T is the message you want to respond ot.
        /// </summary>
        /// <typeparam name="T">The type of the consumer to add.</typeparam>
        public void AddTransientConsumer<T>() where T : class, IConsumer
        {
            Consumers.Add(new MassTransitServiceRegistration()
            {
                Type = typeof(T),
                RegistrationType = RegistrationType.Transient
            });
        }

        /// <summary>
        /// Add a saga as a singleton instance. It must inherit from IConsumer&lt;T&gt; where T is the message you want to respond ot.
        /// </summary>
        /// <typeparam name="T">The type of the saga to add.</typeparam>
        public void AddSingletonSaga<T>() where T : class, ISaga
        {
            Sagas.Add(new MassTransitServiceRegistration()
            {
                Type = typeof(T),
                RegistrationType = RegistrationType.Singleton
            });
        }

        /// <summary>
        /// Add a saga as a scoped instance. It must inherit from IConsumer&lt;T&gt; where T is the message you want to respond ot.
        /// </summary>
        /// <typeparam name="T">The type of the saga to add.</typeparam>
        public void AddScopedSaga<T>() where T : class, ISaga
        {
            Sagas.Add(new MassTransitServiceRegistration()
            {
                Type = typeof(T),
                RegistrationType = RegistrationType.Scoped
            });
        }

        /// <summary>
        /// Add a saga as a transient instance. It must inherit from IConsumer&lt;T&gt; where T is the message you want to respond ot.
        /// </summary>
        /// <typeparam name="T">The type of the saga to add.</typeparam>
        public void AddTransientSaga<T>() where T : class, ISaga
        {
            Sagas.Add(new MassTransitServiceRegistration()
            {
                Type = typeof(T),
                RegistrationType = RegistrationType.Transient
            });
        }
    }
}
