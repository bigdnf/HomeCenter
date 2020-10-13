﻿using HomeCenter.Model.Actors;
using HomeCenter.Model.Extensions;
using HomeCenter.Model.Messages;
using HomeCenter.Services.Configuration.DTO;
using Proto;
using System;
using System.Text.Json;

namespace HomeCenter.Services.Actors
{
    internal class DeviceActorMapper : ITypeMapper<DeviceActorDTO>
    {
        private readonly ClassActivator _classActivator;

        public DeviceActorMapper(ClassActivator classActivator)
        {
            _classActivator = classActivator;
        }

        public IActor Create(DeviceActorDTO config, Type destinationType)
        {
            destinationType.MustDeriveFrom<IActor>();

            if (_classActivator.Create(destinationType) is not DeviceActor instance)
            {
                throw new InvalidCastException($"Type {destinationType} is not {typeof(DeviceActor).Name}");
            }

            instance.SetProperty(MessageProperties.Uid, config.Uid);
            instance.SetProperty(MessageProperties.Type, config.Type);
            instance.SetProperty(MessageProperties.IsEnabled, config.IsEnabled);
            instance.SetProperty(MessageProperties.Tags, config.Tags);
            SetProperties(config, instance);

            return instance as IActor;
        }

        private static void SetProperties(DeviceActorDTO config, DeviceActor instance)
        {
            foreach (var property in config.Properties)
            {
                if (property.Value is JsonElement element)
                {
                    if (element.ValueKind == JsonValueKind.Number)
                    {
                        if (element.TryGetInt32(out int intValue))
                        {
                            instance.SetProperty(property.Key, intValue);
                        }
                        if (element.TryGetDouble(out double douleValue))
                        {
                            instance.SetProperty(property.Key, douleValue);
                        }
                        else
                        {
                            throw new NotImplementedException();
                        }
                    }
                    else if (element.ValueKind == JsonValueKind.True)
                    {
                        instance.SetProperty(property.Key, true);
                    }
                    else if (element.ValueKind == JsonValueKind.False)
                    {
                        instance.SetProperty(property.Key, false);
                    }
                    else if (element.ValueKind == JsonValueKind.String)
                    {
                        var stringValue = element.GetString();
                        if (TimeSpan.TryParse(stringValue, out TimeSpan timeValue))
                        {
                            instance.SetProperty(property.Key, timeValue);
                        }
                        else if (DateTime.TryParse(stringValue, out DateTime dateValue))
                        {
                            instance.SetProperty(property.Key, timeValue);
                        }
                        else if (Guid.TryParse(stringValue, out Guid guidValue))
                        {
                            instance.SetProperty(property.Key, timeValue);
                        }
                    }
                    else
                    {
                        throw new NotImplementedException();
                    }
                }
                else
                {
                    throw new NotImplementedException();
                }
            }
        }
    }

}