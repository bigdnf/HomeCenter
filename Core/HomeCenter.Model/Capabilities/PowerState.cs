﻿using HomeCenter.Model.Capabilities.Constants;
using HomeCenter.Model.Messages.Commands.Device;
using HomeCenter.Model.ValueTypes;

namespace HomeCenter.Model.Capabilities
{
    public class PowerState : State
    {
        public static string StateName { get; } = nameof(PowerState);

        public PowerState(StringValue ReadWriteMode = default) : base(ReadWriteMode)
        {
            this[StateProperties.StateName] = new StringValue(nameof(PowerState));
            this[StateProperties.CapabilityName] = new StringValue(Constants.Capabilities.PowerController);
            this[StateProperties.Value] = new BooleanValue();
            this[StateProperties.SupportedCommands] = new StringListValue(nameof(TurnOnCommand), nameof(TurnOffCommand));
        }
    }
}