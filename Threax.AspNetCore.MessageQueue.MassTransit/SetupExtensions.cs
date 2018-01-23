using MassTransit;
using MassTransit.ExtensionsDependencyInjectionIntegration;
using MassTransit.Saga;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Threax.AspNetCore.MessageQueue;
using Threax.AspNetCore.MessageQueue.MassTransit;

namespace Microsoft.Extensions.DependencyInjection.Extensions
{
    public static class SetupExtensions
    {
        private static IServiceProvider serviceProvider;

        /// <summary>
        /// Add the services for MassTransit according to the Threax convention. This will setup a connection to a single queue using MassTransit over RabbitMQ.
        /// </summary>
        /// <param name="services">The service collection to modify.</param>
        /// <param name="configure">The configure callback function.</param>
        /// <returns>The passed in ServiceCollection.</returns>
        public static IServiceCollection AddThreaxMassTransit(this IServiceCollection services, Action<MassTransitOptions> configure = null)
        {
            var options = new MassTransitOptions();
            configure?.Invoke(options);

            services.AddMassTransit(o =>
            {
                var args = new Object[0];
                var addConsumer = o.GetType().GetMethod(nameof(IServiceCollectionConfigurator.AddConsumer));
                IEnumerable<Type> allTypes = new Type[0];
                foreach(var assembly in options.AutoLoadAssemblies)
                {
                    if (assembly != null)
                    {
                        allTypes = allTypes.Concat(assembly.ExportedTypes);
                    }
                }
                foreach(var consumer in options.Consumers)
                {
                    var addConsumerGeneric = addConsumer.MakeGenericMethod(consumer.Type);
                    addConsumerGeneric.Invoke(o, args);
                    switch (consumer.Lifetime)
                    {
                        case Lifetime.Scoped:
                            services.AddScoped(consumer.Type);
                            break;
                        case Lifetime.Singleton:
                            services.AddSingleton(consumer.Type);
                            break;
                        case Lifetime.Transient:
                            services.AddTransient(consumer.Type);
                            break;
                    }
                }

                foreach(var consumerType in allTypes.Where(i => typeof(IConsumer).IsAssignableFrom(i) && !i.GetCustomAttributes(typeof(NoAutoDiscoverMessages), false).Any()))
                {
                    if(!options.Consumers.Any(i => i.Type == consumerType))
                    {
                        var addConsumerGeneric = addConsumer.MakeGenericMethod(consumerType);
                        addConsumerGeneric.Invoke(o, args);
                        services.AddScoped(consumerType);
                    }
                }

                var addSaga = o.GetType().GetMethod(nameof(IServiceCollectionConfigurator.AddSaga));
                foreach (var saga in options.Sagas)
                {
                    var addSagaGeneric = addSaga.MakeGenericMethod(saga.Type);
                    addSagaGeneric.Invoke(o, args);
                    switch (saga.Lifetime)
                    {
                        case Lifetime.Scoped:
                            services.AddScoped(saga.Type);
                            break;
                        case Lifetime.Singleton:
                            services.AddSingleton(saga.Type);
                            break;
                        case Lifetime.Transient:
                            services.AddTransient(saga.Type);
                            break;
                    }
                }

                foreach(var sagaType in allTypes.Where(i => typeof(ISaga).IsAssignableFrom(i) && !i.GetCustomAttributes(typeof(NoAutoDiscoverMessages), false).Any()))
                {
                    if(!options.Consumers.Any(i => i.Type == sagaType))
                    {
                        var addSagaGeneric = addSaga.MakeGenericMethod(sagaType);
                        addSagaGeneric.Invoke(o, args);
                        services.AddScoped(sagaType);
                    }
                }
            });

            if (options.Host == "InMemory")
            {
                services.AddSingleton<IBusControl>(s => Bus.Factory.CreateUsingInMemory(o =>
                {
                    if (options.OpenQueue)
                    {
                        o.ReceiveEndpoint(options.QueueName, e =>
                        {
                            e.LoadFrom(serviceProvider);
                        });
                    }
                }));
            }
            else
            {
                services.AddSingleton<IBusControl>(s => Bus.Factory.CreateUsingRabbitMq(sbc =>
                {
                    var host = sbc.Host(new Uri(options.Host), h =>
                    {
                        h.Username(options.Username);
                        h.Password(options.Password);
                    });

                    if (options.OpenQueue)
                    {
                        sbc.ReceiveEndpoint(host, options.QueueName, e =>
                        {
                            e.LoadFrom(serviceProvider);
                        });
                    }
                }));
            }
            services.AddSingleton<IPublishEndpoint>(s => s.GetRequiredService<IBusControl>());
            services.AddTransient(typeof(IEventQueue<>), typeof(MassTransitEventQueue<>));

            return services;
        }

        /// <summary>
        /// Activate the connection to rabbitmq over mass transit. Due to the way this is implemented it is not thread safe, however
        /// it should really only be called from Configure anyway, so this should be ok.
        /// </summary>
        /// <param name="appBuilder">The app builder for the app.</param>
        /// <returns>The passed in service provider.</returns>
        public static IApplicationBuilder UseThreaxMassTransit(this IApplicationBuilder appBuilder)
        {
            SetupExtensions.serviceProvider = appBuilder.ApplicationServices;

            var bus = serviceProvider.GetRequiredService<IBusControl>();
            bus.Start();

            var appLifetime = serviceProvider.GetRequiredService<IApplicationLifetime>();
            appLifetime.ApplicationStopped.Register(() => bus.Stop());

            SetupExtensions.serviceProvider = null;

            return appBuilder;
        }

        /// <summary>
        /// Shut down any reachable busses in the given service provider.
        /// </summary>
        /// <param name="serviceProvider">The service provider to shutdown busses with.</param>
        public static void StopThreaxMassTransit(this IServiceProvider serviceProvider)
        {
            var bus = serviceProvider.GetRequiredService<IBusControl>();
            bus.Stop();
        }
    }
}
