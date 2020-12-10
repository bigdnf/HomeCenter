﻿using HomeCenter.Abstractions;
using HomeCenter.Actors.Core;
using HomeCenter.Messages.Commands.Service;
using HomeCenter.Messages.Events.Device;
using HomeCenter.Messages.Queries.Service;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HomeCenter.Services.Networking
{
    [Proxy]
    public class SerialPortService : Service
    {
        private readonly ISerialDevice _serialDevice;
        private readonly Dictionary<int, RegisterSerialCommand> _messageHandlers = new Dictionary<int, RegisterSerialCommand>();
        private readonly DisposeContainer _disposeContainer = new DisposeContainer();

        protected SerialPortService(ISerialDevice serialDevice)
        {
            _serialDevice = serialDevice ?? throw new ArgumentNullException(nameof(serialDevice));
        }

        protected override async Task OnStarted(Proto.IContext context)
        {
            await base.OnStarted(context);

            //TODO DNF
            //_disposeContainer.Add(_serialDevice.Subscribe(System.Reflection.Metadata.Handle));
            _disposeContainer.Add(_serialDevice);
        }

        [Subscribe]
        protected Task Handle(RegisterSerialCommand registration)
        {
            if (registration.MessageType is null) throw new ArgumentNullException();

            if (_messageHandlers.ContainsKey(registration.MessageType.Value))
            {
                throw new ArgumentException($"Message type {registration.MessageType} is already registered in {nameof(SerialPortService)}");
            }

            _messageHandlers.Add(registration.MessageType.Value, registration);

            return Task.CompletedTask;
        }

        private void Handle(byte[] rawData)
        {
            using (var str = new MemoryStream(rawData))
            using (var reader = new BinaryReader(str))
            {
                var messageBodySize = reader.ReadByte();
                var messageType = reader.ReadByte();

                if (messageType == 0)
                {
                    Logger.LogInformation("Test message from RC");
                }

                if (messageType == 10)
                {
                    var byteArray = reader.ReadBytes(rawData.Length - 2);
                    string message = Encoding.UTF8.GetString(byteArray);
                    Logger.LogInformation(message);
                    return;
                }

                if (!_messageHandlers.TryGetValue(messageType, out RegisterSerialCommand registration))
                {
                    //throw new ArgumentException($"Message type {messageType} is not supported by {nameof(SerialPortService)}");
                    Logger.LogError("Message type {messageType} is not supported by {service}", messageType, nameof(SerialPortService));
                    return;
                }

                if (messageBodySize != registration.MessageSize) throw new ArgumentException($"Message type {messageType} have wrong size");
                var result = ReadData(registration.ResultFormat, reader);

                MessageBroker.Send(result, registration.Actor);
            }
        }

        private SerialResultEvent ReadData(Format[] registration, BinaryReader reader)
        {
            var result = new SerialResultEvent();

            foreach (var format in registration.OrderBy(l => l.Lp))
            {
                if (format.ValueType == typeof(byte))
                {
                    result.SetProperty(format.ValueName, reader.ReadByte());
                }
                else if (format.ValueType == typeof(uint))
                {
                    result.SetProperty(format.ValueName, reader.ReadUInt32());
                }
                else if (format.ValueType == typeof(float))
                {
                    result.SetProperty(format.ValueName, reader.ReadSingle());
                }
                else
                {
                    throw new ArgumentException($"Result of type {format.ValueType} is not supported");
                }
            }

            return result;
        }
    }
}