﻿using Microsoft.Reactive.Testing;
using System;

namespace HomeCenter.Actors.Tests.Helpers
{
    public static class TestExtensions
    {
        public static void AdvanceTo(this TestScheduler scheduler, TimeSpan time, long timeAfter = 0, bool roundUp = true)
        {
            var motionEnd = time.Ticks + Time.Tics(timeAfter);
            if (roundUp)
            {
                var round = TimeSpan.FromSeconds(Math.Ceiling(TimeSpan.FromTicks(motionEnd).TotalSeconds));
                motionEnd = round.Ticks;
            }

            scheduler.AdvanceTo(motionEnd);
        }
    }
}