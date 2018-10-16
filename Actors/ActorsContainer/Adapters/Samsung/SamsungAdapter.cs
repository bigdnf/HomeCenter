﻿using HomeCenter.CodeGeneration;
using HomeCenter.Model.Adapters;
using HomeCenter.Model.Capabilities;
using HomeCenter.Model.Exceptions;
using HomeCenter.Model.Extensions;
using HomeCenter.Model.Messages.Commands;
using HomeCenter.Model.Messages.Commands.Device;
using HomeCenter.Model.Messages.Queries.Device;
using HomeCenter.Model.ValueTypes;
using Proto;
using System.Threading.Tasks;

namespace HomeCenter.Adapters.Samsung
{
    [ProxyCodeGenerator]
    public abstract class SamsungAdapter : Adapter
    {
        private string _hostname;

        private BooleanValue _powerState;

        //private DoubleValue _volume;
        private BooleanValue _mute;

        private StringValue _input;

        protected override async Task OnStarted(IContext context)
        {
            await base.OnStarted(context).ConfigureAwait(false);

            _hostname = this[AdapterProperties.Hostname].AsString();
        }

        protected DiscoveryResponse Discover(DiscoverQuery message)
        {
            //TODO Add read only state
            return new DiscoveryResponse(RequierdProperties(), new PowerState(),
                                                               new VolumeState(),
                                                               new MuteState(),
                                                               new InputSourceState()
                                          );
        }

        protected Task TurnOn(TurnOnCommand message)
        {
            //TODO ADD infrared message
            return Task.CompletedTask;
        }

        protected async Task TurnOff(TurnOffCommand message)
        {
            //await _eventAggregator.QueryAsync<SamsungControlCommand, string>(new SamsungControlCommand
            //{
            //    Address = _hostname,
            //    Code = "KEY_POWEROFF"
            //}).ConfigureAwait(false);
            _powerState = await UpdateState(PowerState.StateName, _powerState, new BooleanValue(false)).ConfigureAwait(false);
        }

        protected async Task VolumeUp(VolumeUpCommand command)
        {
            //return _eventAggregator.QueryAsync<SamsungControlCommand, string>(new SamsungControlCommand
            //{
            //    Address = _hostname,
            //    Code = "KEY_VOLUP"
            //});
        }

        protected async Task VolumeDown(VolumeDownCommand command)
        {
            //return _eventAggregator.QueryAsync<SamsungControlCommand, string>(new SamsungControlCommand
            //{
            //    Address = _hostname,
            //    Code = "KEY_VOLDOWN"
            //});
        }

        protected async Task Mute(MuteCommand message)
        {
            //await _eventAggregator.QueryAsync<SamsungControlCommand, string>(new SamsungControlCommand
            //{
            //    Address = _hostname,
            //    Code = "KEY_MUTE"
            //}).ConfigureAwait(false);

            _mute = await UpdateState(MuteState.StateName, _mute, new BooleanValue(!_mute)).ConfigureAwait(false);
        }

        protected async Task InputSet(InputSetCommand message)
        {
            var inputName = (StringValue)message[CommandProperties.InputSource];

            var source = "";
            if (inputName == "HDMI")
            {
                source = "KEY_HDMI";
            }
            else if (inputName == "AV")
            {
                source = "KEY_AV1";
            }
            else if (inputName == "COMPONENT")
            {
                source = "KEY_COMPONENT1";
            }
            else if (inputName == "TV")
            {
                source = "KEY_TV";
            }

            if (source?.Length == 0) throw new UnsupportedPropertyStateException($"Input {inputName} was not found on Samsung available device input sources");

            //await _eventAggregator.QueryAsync<SamsungControlCommand, string>(new SamsungControlCommand
            //{
            //    Address = _hostname,
            //    Code = source
            //}).ConfigureAwait(false);

            _input = await UpdateState(InputSourceState.StateName, _input, inputName).ConfigureAwait(false);
        }
    }
}