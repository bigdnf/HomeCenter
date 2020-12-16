﻿using Microsoft.Extensions.Logging;
using System;
using System.Reactive.Concurrency;

namespace HomeCenter.Actors.Tests.Fakes
{
    public sealed class FakeLoggerProvider<T> : ILoggerProvider
    {
        private readonly IScheduler _scheduler;
        private readonly bool _useRavenDB;
        private Lazy<FakeLogger<T>> _logger;

        public FakeLoggerProvider(IScheduler scheduler, bool useRavenDB)
        {
            _scheduler = scheduler;
            _useRavenDB = useRavenDB;
            _logger = new Lazy<FakeLogger<T>>(() => new FakeLogger<T>(_scheduler, _useRavenDB));
        }

        public ILogger CreateLogger(string categoryName) => _logger.Value;

        public void Dispose()
        {
            _logger.Value.Dispose();
        }
    }
}