﻿using HomeCenter.Broker;
using HomeCenter.Model.Core;
using HomeCenter.Model.Messages;
using HomeCenter.Model.Messages.Commands;
using HomeCenter.Model.Messages.Commands.Device;
using HomeCenter.Model.Messages.Events;
using HomeCenter.Model.Messages.Events.Device;
using HomeCenter.Model.Messages.Queries;
using HomeCenter.Model.Messages.Queries.Device;
using HomeCenter.Model.Messages.Queries.Services;
using HomeCenter.Model.Messages.Scheduler;
using Microsoft.Reactive.Testing;
using Proto;
using Quartz;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace HomeCenter.Services.MotionService.Tests
{
    public class FakeMessageBroker : IMessageBroker
    {
        private readonly ITestableObservable<MotionEnvelope> _motionData;
        private readonly Dictionary<string, FakeMotionLamp> _lamps;
        public FakeMessageBroker(ITestableObservable<MotionEnvelope> motionData, Dictionary<string, FakeMotionLamp> lamps)
        {
            _motionData = motionData;
            _lamps = lamps;
        }

        public PID GetPID(string uid, string address = null)
        {
            throw new NotImplementedException();
        }

        public IObservable<IMessageEnvelope<T>> Observe<T>(RoutingFilter routingFilter = null) where T : Event
        {
            if (typeof(T) == typeof(MotionEvent))
            {
                return (IObservable<IMessageEnvelope<T>>)_motionData;
            }
            throw new NotImplementedException();
        }

        public Task Publish<T>(T message, RoutingFilter routingFilter = null) where T : ActorMessage
        {
            throw new NotImplementedException();
        }

        public Task<Event> PublishWithTranslate(ActorMessage source, ActorMessage destination, RoutingFilter filter = null)
        {
            throw new NotImplementedException();
        }

        public Task<R> QueryJsonService<T, R>(T query, RoutingFilter filter = null)
            where T : Query
        {
            throw new NotImplementedException();
        }

        public Task<R> QueryService<T, R>(T query, RoutingFilter filter = null)
            where T : Query
        {
            if (query is SunriseQuery)
            {
                return Task.FromResult((TimeSpan?)new TimeSpan(6, 0, 0)) as Task<R>;
            }
            else if (query is SunsetQuery)
            {
                return Task.FromResult((TimeSpan?)new TimeSpan(18, 0, 0)) as Task<R>;
            }
            else
            {
                throw new NotImplementedException();
            }
        }

        public Task<bool> QueryServiceWithVerify<T, Q, R>(T query, R expectedResult, RoutingFilter filter = null)
            where T : Query, IMessageResult<Q, R>
            where Q : class
        {
            throw new NotImplementedException();
        }

        public Task<R> Request<T, R>(T message, PID actor) where T : ActorMessage
        {
            throw new NotImplementedException();
        }

        public Task<R> Request<T, R>(T message, string uid) where T : ActorMessage
        {
            throw new NotImplementedException();
        }

        public Task<JobKey> ScheduleInterval<M>(M message, PID actor, TimeSpan interval, CancellationToken token = default(CancellationToken)) where M : ActorMessage
        {
            throw new NotImplementedException();
        }

        public void Send(object message, PID destination)
        {
            throw new NotImplementedException();
        }

        public void Send(object message, string uid, string address = null)
        {
            if (message is TurnOnCommand)
            {
                _lamps[uid].SetState(true);
            }
            else if (message is TurnOffCommand)
            {
                _lamps[uid].SetState(false);
            }
            else
            {
                throw new NotImplementedException();
            }
        }

        public Task SendAfterDelay(ActorMessageContext message, TimeSpan delay, bool cancelExisting = true, CancellationToken token = default(CancellationToken))
        {
            throw new NotImplementedException();
        }

        public Task SendAtTime(ActorMessageContext message, DateTimeOffset time, CancellationToken token = default(CancellationToken))
        {
            throw new NotImplementedException();
        }

        public Task SendDailyAt(ActorMessageContext message, TimeSpan time, CancellationToken token = default(CancellationToken), string calendar = null)
        {
            throw new NotImplementedException();
        }

        public Task SendToService<T>(T command, RoutingFilter filter = null) where T : Command
        {
            throw new NotImplementedException();
        }

        public Task SendWithCronRepeat(ActorMessageContext message, string cronExpression, CancellationToken token = default(CancellationToken), string calendar = null)
        {
            throw new NotImplementedException();
        }

        public Task SendWithSimpleRepeat(ActorMessageContext message, TimeSpan interval, CancellationToken token = default(CancellationToken))
        {
            throw new NotImplementedException();
        }

        public void SendWithTranslate(ActorMessage source, ActorMessage destination, string address)
        {
            throw new NotImplementedException();
        }

        public SubscriptionToken SubscribeForEvent<T>(Action<IMessageEnvelope<T>> action, RoutingFilter filter = null) where T : Event
        {
            throw new NotImplementedException();
        }

        public SubscriptionToken SubscribeForEvent<T>(Func<IMessageEnvelope<T>, Task> action, RoutingFilter filter = null) where T : Event
        {
            throw new NotImplementedException();
        }

        public SubscriptionToken SubscribeForMessage<T>(PID subscriber, bool subscribeOnParent, RoutingFilter filter = null) where T : ActorMessage
        {
            return SubscriptionToken.Empty;
        }

        public SubscriptionToken SubscribeForQuery<T, R>(PID subscriber, bool subscribeOnParent, RoutingFilter filter = null) where T : Query
        {
            throw new NotImplementedException();
        }

       
    }
}