﻿using System.Collections.Generic;
using System.Threading.Tasks;
using HomeCenter.Abstractions;
using HomeCenter.Abstractions.Defaults;
using HomeCenter.Actors.Core;
using HomeCenter.Capabilities;
using HomeCenter.Messages.Commands.Service;
using HomeCenter.Messages.Events.Device;
using HomeCenter.Messages.Queries.Device;
using HomeCenter.Messages.Queries.Service;
using Proto;

namespace HomeCenter.Adapters.TemperatureBridge
{
    [Proxy]
    public class TemperatureBridgeAdapter : Adapter
    {
        private const int I2C_ACTION_TEMPERATURE = 1;
        private readonly Dictionary<int, double> _state = new Dictionary<int, double>();
        private int _i2cAddress;

        protected TemperatureBridgeAdapter()
        {
            _requierdProperties.Add(MessageProperties.PinNumber);
        }

        protected override async Task OnStarted(IContext context)
        {
            await base.OnStarted(context);

            _i2cAddress = this.AsInt(MessageProperties.Address);

            var registration = new RegisterSerialCommand(Self, I2C_ACTION_TEMPERATURE, new Format[]
            {
                new Format(1, typeof(byte), MessageProperties.PinNumber),
                new Format(2, typeof(float), MessageProperties.Value),
            });
            await MessageBroker.SendToService(registration);
        }

        protected async Task Handle(SerialResultEvent serialResult)
        {
            var pin = serialResult.AsByte(MessageProperties.PinNumber);
            var temperature = serialResult.AsDouble(MessageProperties.Value);

            if (_state.ContainsKey(pin))
            {
                var oldValue = _state[pin];

                _state[pin] = await UpdateState(TemperatureState.StateName, oldValue, temperature, new Dictionary<string, string>() { [MessageProperties.PinNumber] = pin.ToString() });
            }
        }

        protected DiscoveryResponse Discover(DiscoverQuery message)
        {
            RegisterPinNumber(message);

            return new DiscoveryResponse(RequierdProperties(), new TemperatureState(ReadWriteMode.Read));
        }

        private void RegisterPinNumber(DiscoverQuery message)
        {
            var pin = message.AsByte(MessageProperties.PinNumber);
            var registrationMessage = new byte[] { I2C_ACTION_TEMPERATURE, pin };

            if (!_state.ContainsKey(pin))
            {
                _state.Add(pin, 0);
            }

            MessageBroker.SendToService(I2cCommand.Create(_i2cAddress, registrationMessage));
        }
    }
}