﻿using HomeCenter.WindowsService.Core.Interop.Enum;
using HomeCenter.WindowsService.Core.Interop.Struct;
using System;
using System.Runtime.InteropServices;

namespace HomeCenter.WindowsService.Core.Interop
{
    [Guid("7991EEC9-7E89-4D85-8390-6C703CEC60C0")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IMMNotificationClient
    {
        void OnDeviceStateChanged([MarshalAs(UnmanagedType.LPWStr)]string deviceId, [MarshalAs(UnmanagedType.I4)]AudioDeviceState newState);

        void OnDeviceAdded([MarshalAs(UnmanagedType.LPWStr)]string deviceId);

        void OnDeviceRemoved([MarshalAs(UnmanagedType.LPWStr)]string deviceId);

        void OnDefaultDeviceChanged(AudioDeviceKind flow, AudioDeviceRole role, [MarshalAs(UnmanagedType.LPWStr)]string deviceId);

        void OnPropertyValueChanged([MarshalAs(UnmanagedType.LPWStr)]string deviceId, PropertyKey key);
    }
}