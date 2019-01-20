﻿using HomeCenter.Model.Messages.Events.Device;
using System.Collections.Generic;
using System.Reactive;

namespace HomeCenter.Services.MotionService.Model
{
    internal interface IEventDecoder
    {
        void DecodeMessage(IList<Timestamped<PowerStateChangeEvent>> powerStateEvents);
        void Init(Room room);
    }
}