﻿using System.Threading.Tasks;

namespace HomeCenter.Broker.Behaviors
{
    public class RetryBehavior : Behavior
    {
        private int _retryCount;

        public RetryBehavior(int retryCount = 3)
        {
            Priority = 40;
            _retryCount = retryCount;
        }

        public override async Task<R> HandleAsync<T, R>(IMessageEnvelope<T> message)
        {
            while (true)
            {
                try
                {
                    return await _asyncCommandHandler.HandleAsync<T, R>(message);
                }
                catch when (_retryCount-- > 0) { }
            }
        }
    }
}