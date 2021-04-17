﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using HomeCenter.Abstractions;
using HomeCenter.Abstractions.Defaults;
using HomeCenter.Actors.Core;
using HomeCenter.Capabilities;
using HomeCenter.EventAggregator;
using HomeCenter.Extensions;
using HomeCenter.Messages.Commands.Device;
using HomeCenter.Messages.Commands.Service;
using HomeCenter.Messages.Events.Device;
using HomeCenter.Messages.Queries.Device;
using HomeCenter.Messages.Queries.Services;
using Microsoft.Extensions.Logging;
using Proto;

namespace HomeCenter.Adapters.Common
{
    // TODO add reverse state - to interpreted true as false and opposite
    [Proxy]
    public class CCToolsAdapter : Adapter
    {
        private readonly MAX7311Driver _driver = new MAX7311Driver();

        private int _poolDurationWarning;
        private int _i2cAddress;
        private bool _firstPortWriteMode;
        private bool _secondPortWriteMode;

        protected CCToolsAdapter()
        {
            _requierdProperties.Add(MessageProperties.PinNumber);
        }

        protected override async Task OnStarted(IContext context)
        {
            await base.OnStarted(context);

            _poolDurationWarning = this.AsInt(MessageProperties.PollDurationWarningThreshold, 2000);
            _i2cAddress = this.AsInt(MessageProperties.Address);
            _firstPortWriteMode = this.AsBool(MessageProperties.FirstPortWriteMode);
            _secondPortWriteMode = this.AsBool(MessageProperties.SecondPortWriteMode);

            if (ContainsProperty(MessageProperties.InterruptPin) && ContainsProperty(MessageProperties.InterruptSource))
            {
                Subscribe<PinValueChangedEvent>(false, new RoutingFilter(new Dictionary<string, object>()
                {
                    [MessageProperties.MessageSource] = this.AsString(MessageProperties.InterruptSource),
                    [MessageProperties.PinNumber] = this.AsString(MessageProperties.InterruptPin),
                }));
            }

            await ConfigureDriver();
            await FetchState();

            if (_firstPortWriteMode)
            {
                for (int i = 0; i < 8; i++)
                {
                    await SetPortState(i, false);
                }
            }

            if (_secondPortWriteMode)
            {
                for (int i = 8; i < 15; i++)
                {
                    await SetPortState(i, false);
                }
            }
        }

        private Task ConfigureDriver()
        {
            return MessageBroker.SendToService(I2cCommand.Create(_i2cAddress, _driver.Configure(_firstPortWriteMode, _secondPortWriteMode)));
        }

        protected DiscoveryResponse Handle(DiscoverQuery message)
        {
            var pinNumber = message.AsInt(MessageProperties.PinNumber);
            bool pinInWriteMode = IsPinInWriteMode(pinNumber);

            return new DiscoveryResponse(RequierdProperties(), pinInWriteMode ? new PowerState(ReadWriteMode.Write) : new PowerState(ReadWriteMode.Read));
        }

        protected Task Hadle(PinValueChangedEvent pinValueChangedEvent) => FetchState();

        protected Task Handle(RefreshCommand message) => FetchState();

        protected async Task Handle(TurnOnCommand message)
        {
            int pinNumber = ValidatePin(message);

            await SetPortState(pinNumber, true);

            if (message.ContainsProperty(MessageProperties.StateTime))
            {
                await Task.Delay(message.AsIntTime(MessageProperties.StateTime));
                await SetPortState(pinNumber, false);
            }
        }

        protected Task Handle(TurnOffCommand message)
        {
            int pinNumber = ValidatePin(message);

            return SetPortState(pinNumber, false);
        }

        protected Task Handle(SwitchPowerStateCommand message)
        {
            int pinNumber = ValidatePin(message);
            var currentState = _driver.GetState(pinNumber);

            return SetPortState(pinNumber, !currentState);
        }

        protected bool QueryState(StateQuery message)
        {
            var pinNumber = this.AsInt(MessageProperties.PinNumber);
            return _driver.GetState(pinNumber);
        }

        private int ValidatePin(Command message)
        {
            var pinNumber = message.AsInt(MessageProperties.PinNumber);
            if (pinNumber < 0 || pinNumber > 15)
            {
                throw new ArgumentOutOfRangeException(nameof(pinNumber));
            }

            var isPinInFirstPortRange = pinNumber < 8;

            if ((isPinInFirstPortRange && !_firstPortWriteMode) || (!isPinInFirstPortRange && !_secondPortWriteMode))
            {
                throw new ArgumentException($"Pin {pinNumber} on device {Uid} is configured for INPUT");
            }

            return pinNumber;
        }

        private async Task SetPortState(int pinNumber, bool state)
        {
            var newState = _driver.GenerateNewState(pinNumber, state);

            try
            {
                await MessageBroker.SendToService(I2cCommand.Create(_i2cAddress, newState));
                _driver.AcceptNewState();
            }
            catch (Exception)
            {
                _driver.RevertNewState();
                throw;
            }

            Logger.LogInformation("Board [{Uid}] committed state '{state}'", Uid, _driver.GetState().ToBinaryString());
        }

        private async Task FetchState()
        {
            var stopwatch = Stopwatch.StartNew();

            var newState = await ReadFromBus();

            stopwatch.Stop();

            if (!_driver.TrySaveState(newState, out var oldState))
            {
                return;
            }

            var oldStateBits = new BitArray(oldState);
            var newStateBits = new BitArray(newState);

            Logger.LogTrace("[{Uid}] fetched different state [{oldState}->{newState}]", Uid, oldState.ToBinaryString(), newState.ToBinaryString());

            for (int pinNumber = 0; pinNumber < oldStateBits.Length; pinNumber++)
            {
                var oldPinState = oldStateBits.Get(pinNumber);
                var newPinState = newStateBits.Get(pinNumber);
                bool pinInWriteMode = IsPinInWriteMode(pinNumber);

                // When state is the same or change is in port that are set to WRITE we skip event generation
                if (oldPinState == newPinState || pinInWriteMode)
                {
                    continue;
                }

                var properyChangeEvent = PropertyChangedEvent.Create(Uid, PowerState.StateName, oldPinState, newPinState, new Dictionary<string, string>()
                {
                    [MessageProperties.PinNumber] = pinNumber.ToString(),
                });

                await MessageBroker.Publish(properyChangeEvent, Uid);

                Logger.LogTrace("[{Uid}] Pin [{pinNumber}] state changed {oldPinState}->{newPinState}", Uid, pinNumber, oldPinState, newPinState);
            }

            if (stopwatch.ElapsedMilliseconds > _poolDurationWarning)
            {
                Logger.LogWarning("Polling device '{Uid}' took {elapsed}ms.", Uid, stopwatch.ElapsedMilliseconds);
            }
        }

        private bool IsPinInWriteMode(int pinNumber)
        {
            var isPinInFirstPortRange = pinNumber < 8;
            var pinInWriteMode = (isPinInFirstPortRange && _firstPortWriteMode) || (!isPinInFirstPortRange && _secondPortWriteMode);
            return pinInWriteMode;
        }

        private async Task<byte[]> ReadFromBus()
        {
            var query = I2cQuery.Create(_i2cAddress, _driver.GetReadTable(), _driver.BufferSize);
            var result = await MessageBroker.QueryService<I2cQuery, byte[]>(query);
            return result;
        }
    }
}