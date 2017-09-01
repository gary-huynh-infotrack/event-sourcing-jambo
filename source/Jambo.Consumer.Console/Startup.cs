﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Jambo.ServiceBus;
using Newtonsoft.Json;
using MediatR;
using System.Reflection;
using Autofac;
using Microsoft.Extensions.DependencyInjection;
using Autofac.Extensions.DependencyInjection;
using Jambo.Consumer.IoC;
using System.Threading;
using Jambo.Consumer.Application.DomainEventHandlers.Blogs;
using Jambo.Domain.Model;

namespace Jambo.Consumer.Console
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        IServiceProvider serviceProvider;

        public IServiceProvider ConfigureServices(IServiceCollection services)
        {
            services.AddMediatR(typeof(BlogCreatedEventHandler).GetTypeInfo().Assembly);

            ContainerBuilder container = new ContainerBuilder();
            container.Populate(services);

            container.RegisterModule(new ApplicationModule(
                Configuration.GetSection("MongoDB").GetValue<string>("ConnectionString"),
                Configuration.GetSection("MongoDB").GetValue<string>("Database")));

            container.RegisterModule(new BusModule(
                Configuration.GetSection("ServiceBus").GetValue<string>("ConnectionString"),
                Configuration.GetSection("ServiceBus").GetValue<string>("Topic"),
                ProcessDomainEventDelegate));

            serviceProvider = new AutofacServiceProvider(container.Build());

            return serviceProvider;
        }

        private void ProcessDomainEventDelegate(string topic, string key, string value)
        {
            System.Console.WriteLine($"{topic} {key} {value}");

            Type eventType = Type.GetType(key);
            DomainEvent domainEvent = (DomainEvent)JsonConvert.DeserializeObject(value, eventType);

            serviceProvider.GetService<IMediator>().Send(domainEvent).Wait();
        }

        public void Run()
        {
            while (true)
            {
                Thread.Sleep(1000 * 60);
                System.Console.WriteLine(DateTime.Now.ToString());
            }
        }
    }
}