﻿using System;
using System.Reflection;

using Autofac.Extras.IocManager;

using Stove.Configuration;
using Stove.Events.Bus;

namespace Stove
{
    public static class StoveRegistrationExtensions
    {
        public static IIocBuilder UseStove(this IIocBuilder builder)
        {
            RegisterDefaults(builder);
            return builder;
        }

        private static void RegisterDefaults(IIocBuilder builder)
        {
            builder.RegisterServices(r => r.RegisterAssemblyByConvention(Assembly.GetExecutingAssembly()));
            builder.RegisterServices(r => r.Register<IGuidGenerator>(context => SequentialGuidGenerator.Instance));
            builder.RegisterServices(r => r.Register<IStoveStartupConfiguration, StoveStartupConfiguration>(Lifetime.Singleton));
        }

        public static IIocBuilder UseStove(this IIocBuilder builder, Func<IStoveStartupConfiguration, IStoveStartupConfiguration> configurationAction)
        {
            RegisterDefaults(builder);
            builder.RegisterServices(r => r.Register(context => configurationAction(new StoveStartupConfiguration(context.Resolver)), Lifetime.Singleton));
            return builder;
        }

        public static IIocBuilder UseDefaultEventBus(this IIocBuilder builder)
        {
            builder.RegisterServices(r => r.Register<IEventBus>(context => EventBus.Default));
            return builder;
        }

        public static IIocBuilder UseEventBus(this IIocBuilder builder)
        {
            builder.RegisterServices(r => r.Register<IEventBus, EventBus>());
            return builder;
        }
    }
}