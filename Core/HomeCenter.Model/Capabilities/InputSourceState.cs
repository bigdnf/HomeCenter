﻿using HomeCenter.Model.Capabilities.Constants;
using HomeCenter.Model.Messages.Commands.Device;
using HomeCenter.Model.ValueTypes;

namespace HomeCenter.Model.Capabilities
{
    public class InputSourceState : State
    {
        public static string StateName { get; } = nameof(InputSourceState);

        public InputSourceState(StringValue ReadWriteMode = default) : base(ReadWriteMode)
        {
            this[StateProperties.Value] = new StringValue();
            this[StateProperties.StateName] = new StringValue(nameof(InputSourceState));
            this[StateProperties.CapabilityName] = new StringValue(Constants.Capabilities.InputController);
            this[StateProperties.SupportedCommands] = new StringListValue(nameof(InputSetCommand));
        }
    }
}