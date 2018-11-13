﻿using HomeCenter.Controller.NativeServices;
using HomeCenter.Model.Native;
using HomeCenter.Services.Bootstrapper;
using HomeCenter.Services.Configuration;

namespace HomeCenter.Controller
{
    internal class RaspberryBootstrapper : Bootstrapper
    {
        protected override void RegisterNativeServices()
        {
            _container.RegisterSingleton<IGpioController, RaspberryGpioController>();
            _container.RegisterSingleton<II2cBus, RaspberryI2cBus>();
            _container.RegisterSingleton<ISerialDevice, RaspberrySerialDevice>();
            _container.RegisterSingleton<ISoundPlayer, RaspberrySoundPlayer>();
            _container.RegisterSingleton<IStorage, RaspberryStorage>();
        }

        protected override void RegisterControllerOptions()
        {
            _container.RegisterInstance<IControllerOptions>(new ControllerOptions
            {
                AdapterMode = AdapterMode.Embedded
            });
        }
    }
}