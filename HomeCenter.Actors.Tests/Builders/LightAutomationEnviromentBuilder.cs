﻿using HomeCenter.Model.Actors;
using HomeCenter.Model.Core;
using HomeCenter.Model.Messages.Events.Device;
using HomeCenter.Services.Actors;
using HomeCenter.Services.Configuration.DTO;
using HomeCenter.Services.DI;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Reactive.Testing;
using SimpleInjector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;

namespace HomeCenter.Services.MotionService.Tests
{
    internal class LightAutomationEnviromentBuilder
    {
        private readonly ServiceDTO _serviceConfig;
        private readonly TestScheduler _scheduler = new TestScheduler();
        private readonly Container _container = new Container();
        private readonly List<Recorded<Notification<MotionEnvelope>>> _motionEvents = new List<Recorded<Notification<MotionEnvelope>>>();
        private readonly List<Recorded<Notification<PowerStateChangeEvent>>> _lampEvents = new List<Recorded<Notification<PowerStateChangeEvent>>>();

        private int? _timeDuration;
        private TimeSpan? _periodicCheckTime;
        private readonly bool _useRavenDbLogs;

        public LightAutomationEnviromentBuilder(ServiceDTO serviceConfig, bool useRavenDbLogs)
        {
            _serviceConfig = serviceConfig;
            _useRavenDbLogs = useRavenDbLogs;

            _container.Options.ResolveUnregisteredConcreteTypes = true;
        }

        public LightAutomationEnviromentBuilder WithMotion(params Recorded<Notification<MotionEnvelope>>[] messages)
        {
            _motionEvents.AddRange(messages);
            return this;
        }

        public LightAutomationEnviromentBuilder WithMotions(Dictionary<int, string> motions)
        {
            _motionEvents.AddRange(motions.Select(x => new Recorded<Notification<MotionEnvelope>>(Time.Tics(x.Key), Notification.CreateOnNext(new MotionEnvelope(x.Value)))));

            return this;
        }

        public LightAutomationEnviromentBuilder WithMotions(List<Tuple<int, string>> motions)
        {
            _motionEvents.AddRange(motions.Select(x => new Recorded<Notification<MotionEnvelope>>(Time.Tics(x.Item1), Notification.CreateOnNext(new MotionEnvelope(x.Item2)))));

            return this;
        }

        public LightAutomationEnviromentBuilder WithRepeatedMotions(string roomUid, int numberOfMotions, TimeSpan waitTime)
        {
            long ticks = 0;

            for (int i = 0; i < numberOfMotions; i++)
            {
                ticks += Time.Tics((int)waitTime.TotalMilliseconds);

                _motionEvents.Add(new Recorded<Notification<MotionEnvelope>>(ticks, Notification.CreateOnNext(new MotionEnvelope(roomUid))));
            }

            return this;
        }

        /// <summary>
        /// Add repeat motion to <paramref name="roomUid"/> that takes <paramref name="motionTime"/> and waits between moves <paramref name="waitTime"/>
        /// </summary>
        /// <param name="roomUid"></param>
        /// <param name="motionTime"></param>
        /// <param name="waitTime">Deafault value is 3 seconds</param>
        /// <returns></returns>
        public LightAutomationEnviromentBuilder WithRepeatedMotions(string roomUid, TimeSpan motionTime, TimeSpan? waitTime = null)
        {
            var time = waitTime ?? TimeSpan.FromSeconds(3);

            int num = (int)(motionTime.TotalMilliseconds / time.TotalMilliseconds);

            WithRepeatedMotions(roomUid, num, time);

            return this;
        }

        public LightAutomationEnviromentBuilder WithLampEvents(params Recorded<Notification<PowerStateChangeEvent>>[] messages)
        {
            _lampEvents.AddRange(messages);
            return this;
        }

        public LightAutomationEnviromentBuilder WithPeriodicCheckTime(TimeSpan periodicCheckTimw)
        {
            _periodicCheckTime = periodicCheckTimw;
            return this;
        }

        public LightAutomationEnviromentBuilder WithTimeDuration(int timeDuration)
        {
            _timeDuration = timeDuration;
            return this;
        }

        public ActorEnvironment Build()
        {
            var lampDictionary = CreateFakeLamps();
            var motionEvents = _scheduler.CreateColdObservable(_motionEvents.ToArray());

            var logger = new FakeLogger<LightAutomationServiceProxy>(_scheduler, _useRavenDbLogs);

            _container.RegisterInstance<IConcurrencyProvider>(new TestConcurrencyProvider(_scheduler));
            _container.RegisterInstance<ILogger<LightAutomationServiceProxy>>(logger);
            _container.RegisterInstance<IMessageBroker>(new FakeMessageBroker(motionEvents, lampDictionary));
            _container.RegisterSingleton<DeviceActorMapper>();
            _container.RegisterSingleton<BaseObjectMapper>();
            _container.RegisterSingleton<ClassActivator>();
            _container.RegisterSingleton<IServiceProvider, SimpleInjectorServiceProvider>();

            var sm = _container.GetService<ServiceMapper>();


            LightAutomationServiceProxy actor = sm.Map(_serviceConfig, typeof(LightAutomationServiceProxy)) as LightAutomationServiceProxy;

            var actorContext = new ActorEnvironment(_scheduler, motionEvents, lampDictionary, logger, actor);
            actorContext.IsAlive();

            return actorContext;
        }

        private Dictionary<string, FakeMotionLamp> CreateFakeLamps()
        {
            var lampDictionary = new Dictionary<string, FakeMotionLamp>();

            foreach (var detector in _serviceConfig.ComponentsAttachedProperties)
            {
                var detectorName = detector.Properties[MotionProperties.Lamp];

                lampDictionary.Add(detectorName.ToString(), new FakeMotionLamp(detectorName.ToString()));
            }

            return lampDictionary;
        }
    }
}