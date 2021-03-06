﻿using System.Threading.Tasks;

namespace HomeCenter.Broker.Behaviors
{
    public class AsyncBehavior : Behavior
    {
        public AsyncBehavior()
        {
            Priority = 50;
        }

        public override Task<R> HandleAsync<T, R>(IMessageEnvelope<T> message)
        {
            return Task.Run(() => _asyncCommandHandler.HandleAsync<T, R>(message));
        }
    }
}