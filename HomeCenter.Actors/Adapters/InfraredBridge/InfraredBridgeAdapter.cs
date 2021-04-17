﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using HomeCenter.Abstractions;
using HomeCenter.Actors.Core;
using HomeCenter.Capabilities;
using HomeCenter.Messages.Commands.Device;
using HomeCenter.Messages.Commands.Service;
using HomeCenter.Messages.Events.Device;
using HomeCenter.Messages.Queries.Device;
using HomeCenter.Messages.Queries.Service;
using Proto;

namespace HomeCenter.Adapters.InfraredBridge
{
    [Proxy]
    public class InfraredBridgeAdapter : Adapter
    {
        private const int DEAFULT_REPEAT = 3;
        private int _i2cAddress;

        protected override async Task OnStarted(IContext context)
        {
            await base.OnStarted(context);

            _i2cAddress = this.AsInt(MessageProperties.Address);

            var registration = new RegisterSerialCommand(Self, 3, new Format[]
               {
                new Format(1, typeof(byte), "System"),
                new Format(2, typeof(uint), "Code"),
                new Format(3, typeof(byte), "Bits"),
               });
            await MessageBroker.SendToService(registration);
        }

        protected DiscoveryResponse Discover(DiscoverQuery message)
        {
            return new DiscoveryResponse(new InfraredReceiverState(), new InfraredSenderState());
        }

        protected Task Handle(SerialResultEvent serialResultCommand)
        {
            var system = serialResultCommand.AsByte("System");
            var code = serialResultCommand.AsUint("Code");

            return MessageBroker.Publish(InfraredEvent.Create(Uid, system, code), Uid);
        }

        protected Task Handle(SendCodeCommand message)
        {
            var commandCode = message.AsUint(MessageProperties.Code);
            var system = message.AsInt(MessageProperties.System);
            var bits = message.AsInt(MessageProperties.Bits);
            var repeat = message.AsInt(MessageProperties.Repeat, DEAFULT_REPEAT);

            var package = new List<byte>
            {
                3,
                (byte)repeat,
                (byte)system,
                (byte)bits,
            };
            package.AddRange(BitConverter.GetBytes(commandCode));
            var code = package.ToArray();

            var cmd = I2cCommand.Create(_i2cAddress, package.ToArray());
            return MessageBroker.SendToService(cmd);
        }
    }
}