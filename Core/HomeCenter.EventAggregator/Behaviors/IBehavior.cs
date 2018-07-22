﻿namespace HomeCenter.Core.EventAggregator
{
    public interface IBehavior : IAsyncCommandHandler
    {
        void SetNextNode(IAsyncCommandHandler asyncCommandHandler);
        int Priority { get; }
    }
}