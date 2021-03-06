﻿using System;

namespace HomeCenter.Model.Contracts
{
    public interface ISerialDevice : IDisposable
    {
        void Init();

        void Send(byte[] data);

        void Send(string data);

        IDisposable Subscribe(Action<byte[]> handler);
    }
}