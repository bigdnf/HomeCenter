﻿using FluentAssertions;
using HomeCenter.Actors.Tests.Builders;
using HomeCenter.Actors.Tests.Helpers;
using Microsoft.Reactive.Testing;
using System.Collections.Generic;
using Xunit;

namespace HomeCenter.Services.MotionService.Tests
{
    public class MoveTests : ReactiveTest
    {
        // *[Confusion], ^[Resolved]
        //  ___________________________________________   __________________________
        // |        |                |                       |                      |
        // |        |            3                                                  |
        // |        |                |                       |                      |
        // |                         |___   ______           |                      |
        // |        |                |            |          |                      |
        // |        |                |            |          |                      |
        // |        |                |            |          |______________________|
        // |        |                |            |          |                      |
        // |        |                |            |            1                    |
        // |        |                |            |____  ____|                      |
        // |        |                |            |          |                      |
        // |        |                |            |    0     |                      |
        // |        |                |            |          |                      |
        // |________|________________|____________|__________|______________________|
        [Fact(DisplayName = "Move on separate rooms should turn on light")]
        public void Move1()
        {
            using var env = EnviromentBuilder.Create(s => s.WithDefaultRooms())
                .WithMotions(new Dictionary<string, string>
            {
                { "500", Detectors.toilet },
                { "1500", Detectors.kitchen },
                { "2000", Detectors.livingRoom }
            }).Build();

            env.AdvanceToEnd();

            env.LampState(Detectors.toilet).Should().BeTrue();
            env.LampState(Detectors.kitchen).Should().BeTrue();
            env.LampState(Detectors.livingRoom).Should().BeTrue();
        }
    }
}