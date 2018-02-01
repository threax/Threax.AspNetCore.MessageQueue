using MassTransit;
using MassTransit.ExtensionsDependencyInjectionIntegration;
using MassTransit.RabbitMqTransport;
using MassTransit.Saga;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace Threax.AspNetCore.MessageQueue.MassTransit
{
    enum Lifetime
    {
        Singleton,
        Transient,
        Scoped
    }

    class MassTransitServiceRegistration
    {
        public Type Type { get; set; }

        public Lifetime Lifetime { get; set; }
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
        /// Set this to true to use rabbitmq over ssl.
        /// </summary>
        public bool UseSsl { get; set; }

        /// <summary>
        /// The port to use for ssl connections. Defaults to 5671, which is the rabbitmq default.
        /// </summary>
        public int SslPort { get; set; } = 5671;

        /// <summary>
        /// Set this to an action to be called during the UseSsl call. This will be called after everything else
        /// so the settings applied here will override anything set automatically.
        /// </summary>
        [JsonIgnore]
        public Action<IRabbitMqSslConfigurator> ConfigureSsl;

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

        /// <summary>
        /// The assemblies to auto load consumers and sagas from. Will load all implementations of IConsumer or ISaga.
        /// Defaults to the entry assembly, you can add assemblies to the list, or clear it and customize what you need.
        /// All types found this way will be loaded scoped. If you define a scope for a consumer or saga manually it will keep
        /// that registration.
        /// </summary>
        [JsonIgnore]
        public List<Assembly> AutoLoadAssemblies { get; private set; } = new List<Assembly>() { Assembly.GetEntryAssembly() };

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
                Lifetime = Lifetime.Singleton
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
                Lifetime = Lifetime.Scoped
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
                Lifetime = Lifetime.Transient
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
                Lifetime = Lifetime.Singleton
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
                Lifetime = Lifetime.Scoped
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
                Lifetime = Lifetime.Transient
            });
        }
    }
}
