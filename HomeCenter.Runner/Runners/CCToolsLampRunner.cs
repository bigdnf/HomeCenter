﻿using HomeCenter.Abstractions;
using HomeCenter.Capabilities;
using HomeCenter.Messages.Commands.Device;
using HomeCenter.Messages.Events.Device;
using HomeCenter.Runner.ConsoleExtentions;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HomeCenter.Runner
{
    public class CCToolsLampRunner : Runner
    {
        private int? pinNumber = 10;

        public CCToolsLampRunner(string uid) : base(uid)
        {
            _tasks = new string[] { "TurnOn", "TurnOff", "Refresh", "Switch", "TestMotion", "TestMotion2" };
        }

        public override void RunnerReset()
        {
            base.RunnerReset();

            pinNumber = 10;
        }

        public override async Task RunTask(int taskId)
        {
            if (!pinNumber.HasValue)
            {
                ConsoleEx.WriteWarning("Write PIN number:");
                pinNumber = int.Parse(Console.ReadLine());
            }

            Command cmd = null;
            switch (taskId)
            {
                case 0:
                    cmd = new TurnOnCommand();
                    break;

                case 1:
                    cmd = new TurnOffCommand();
                    break;

                case 2:
                    cmd = new RefreshCommand();
                    cmd.LogLevel = nameof(Microsoft.Extensions.Logging.LogLevel.Information);
                    break;

                case 3:
                    cmd = new SwitchPowerStateCommand();
                    break;

                case 4:
                    var inputUid = "HSPE16InputOnly_2";
                    var properyChangeEvent = PropertyChangedEvent.Create(inputUid, PowerState.StateName, false, true, new Dictionary<string, string>()
                    {
                        [MessageProperties.PinNumber] = 0.ToString()
                    });

                    await MessageBroker.Publish(properyChangeEvent, inputUid);
                    return;

                case 5:
                    var inputUid2 = "HSPE16InputOnly_2";
                    var properyChangeEvent2 = PropertyChangedEvent.Create(inputUid2, PowerState.StateName, true, false, new Dictionary<string, string>()
                    {
                        [MessageProperties.PinNumber] = 0.ToString()
                    });

                    await MessageBroker.Publish(properyChangeEvent2, inputUid2);
                    return;
            }

            if (pinNumber.HasValue && pinNumber.Value < 10)
            {
                cmd.SetProperty(MessageProperties.PinNumber, pinNumber.Value);
            }

            MessageBroker.Send(cmd, Uid);

            //return Task.CompletedTask;
        }
    }
}