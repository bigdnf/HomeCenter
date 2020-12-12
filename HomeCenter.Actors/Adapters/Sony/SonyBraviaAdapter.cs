﻿using HomeCenter.Abstractions;
using HomeCenter.Actors.Core;
using HomeCenter.Adapters.Sony.Messages;
using HomeCenter.Capabilities;
using HomeCenter.Messages.Commands.Device;
using HomeCenter.Messages.Commands.Service;
using HomeCenter.Messages.Queries.Device;
using Proto;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HomeCenter.Adapters.Sony
{
    [Proxy]
    public class SonyBraviaAdapter : Adapter
    {
        private const int DEFAULT_POOL_INTERVAL = 1000;

        private bool _powerState;
        private double _volume;
        private bool _mute;
        private string? _input;
        private string? _clientId;
        private string? _mac;

        private TimeSpan _poolInterval;
        private string? _hostname;
        private string? _authorisationKey;

        private readonly Dictionary<string, string> _inputSourceMap = new Dictionary<string, string>
        {
            { "HDMI1", "AAAAAgAAABoAAABaAw==" },
            { "HDMI2", "AAAAAgAAABoAAABbAw==" },
            { "HDMI3", "AAAAAgAAABoAAABcAw==" },
            { "HDMI4", "AAAAAgAAABoAAABdAw==" }
        };

        protected override async Task OnStarted(IContext context)
        {
            await base.OnStarted(context);

            _hostname = this.AsString(MessageProperties.Hostname);
            _authorisationKey = this.AsString(MessageProperties.AuthKey);
            _clientId = this.AsString(MessageProperties.ClientID);
            _mac = this.AsString(MessageProperties.MAC);
            _poolInterval = this.AsIntTime(MessageProperties.PoolInterval, DEFAULT_POOL_INTERVAL);

            // await ScheduleDeviceRefresh<RefreshStateJob>(_poolInterval);
        }

        private SonyControlQuery GetControlCommand(string code)
        {
            if (_hostname is null) throw new InvalidOperationException();

            return new SonyControlQuery
            {
                Address = _hostname,
                AuthorisationKey = _authorisationKey,
                Code = code
            };
        }

        private SonyJsonQuery GetJsonCommand(string path, string method, object? parameters = null)
        {
            if (_hostname is null) throw new InvalidOperationException();

            return new SonyJsonQuery
            {
                Address = _hostname,
                AuthorisationKey = _authorisationKey,
                Path = path,
                Method = method,
                Params = parameters
            };
        }

        protected DiscoveryResponse Discover(DiscoverQuery message)
        {
            return new DiscoveryResponse(RequierdProperties(), new PowerState(),
                                                               new VolumeState(),
                                                               new MuteState(),
                                                               new InputSourceState()
                                          );
        }

        protected async Task<string> Handle(SonyRegisterQuery sonyRegisterQuery)
        {
            if (_hostname is null) throw new InvalidOperationException();

            sonyRegisterQuery.Address = _hostname;
            sonyRegisterQuery.ClientID = _clientId;

            var result = await MessageBroker.QueryService<SonyRegisterQuery, string>(sonyRegisterQuery);
            return result;
        }

        protected async Task Handle(RefreshCommand message)
        {
            var cmd = GetJsonCommand("system", "getPowerStatus");
            var power = await MessageBroker.QueryJsonService<SonyJsonQuery, SonyPowerResult>(cmd);

            cmd = GetJsonCommand("audio", "getVolumeInformation");
            var audio = await MessageBroker.QueryJsonService<SonyJsonQuery, SonyAudioResult>(cmd);

            //TODO save audio and power state
            //_powerState = await UpdateState<BooleanValue>(PowerState.StateName, _powerState, power);
        }

        protected async Task Handle(TurnOnCommand message)
        {
            if (_mac is null) throw new InvalidOperationException();

            var command = WakeOnLanCommand.Create(_mac);
            await MessageBroker.SendToService(command);
            //var cmd = GetControlCommand("AAAAAQAAAAEAAAAuAw==");

            _powerState = await UpdateState(PowerState.StateName, _powerState, true);
        }

        protected async Task Handle(TurnOffCommand message)
        {
            var cmd = GetControlCommand("AAAAAQAAAAEAAAAvAw==");
            await MessageBroker.QueryService<SonyControlQuery, string>(cmd);
            _powerState = await UpdateState(PowerState.StateName, _powerState, false);
        }

        protected async Task Handle(VolumeUpCommand command)
        {
            var volume = _volume + command.AsDouble(MessageProperties.ChangeFactor);
            var cmd = GetJsonCommand("audio", "setAudioVolume", new SonyAudioVolumeRequest("speaker", ((int)volume).ToString()));
            await MessageBroker.QueryJsonService<SonyJsonQuery, SonyAudioResult>(cmd);

            _volume = await UpdateState(VolumeState.StateName, _volume, volume);
        }

        protected async Task Handle(VolumeDownCommand command)
        {
            var volume = _volume - command.AsDouble(MessageProperties.ChangeFactor);
            var cmd = GetJsonCommand("audio", "setAudioVolume", new SonyAudioVolumeRequest("speaker", ((int)volume).ToString()));
            await MessageBroker.QueryJsonService<SonyJsonQuery, SonyAudioResult>(cmd);

            _volume = await UpdateState(VolumeState.StateName, _volume, volume);
        }

        protected async Task Handle(VolumeSetCommand command)
        {
            var volume = command.AsDouble(MessageProperties.Value);
            var cmd = GetJsonCommand("audio", "setAudioVolume", new SonyAudioVolumeRequest("speaker", ((int)volume).ToString()));
            await MessageBroker.QueryJsonService<SonyJsonQuery, SonyAudioResult>(cmd);

            _volume = await UpdateState(VolumeState.StateName, _volume, volume);
        }

        protected async Task Handle(MuteCommand message)
        {
            var cmd = GetJsonCommand("audio", "setAudioMute", new SonyAudioMuteRequest(true));
            await MessageBroker.QueryJsonService<SonyJsonQuery, SonyAudioResult>(cmd);

            _mute = await UpdateState(MuteState.StateName, _mute, true);
        }

        protected async Task Handle(UnmuteCommand message)
        {
            var cmd = GetJsonCommand("audio", "setAudioMute", new SonyAudioMuteRequest(false));
            await MessageBroker.QueryJsonService<SonyJsonQuery, SonyAudioResult>(cmd);

            _mute = await UpdateState(MuteState.StateName, _mute, false);
        }

        protected async Task Handle(InputSetCommand message)
        {
            var inputName = message.AsString(MessageProperties.InputSource);
            if (!_inputSourceMap.ContainsKey(inputName)) throw new ArgumentException($"Input {inputName} was not found on available device input sources");

            var code = _inputSourceMap[inputName];

            var cmd = GetControlCommand(code);
            await MessageBroker.QueryService<SonyControlQuery, string>(cmd);

            _input = await UpdateState(InputSourceState.StateName, _input, inputName);
        }
    }
}