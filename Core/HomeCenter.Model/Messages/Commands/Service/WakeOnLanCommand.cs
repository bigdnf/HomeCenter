﻿using System;
using System.Globalization;
using System.Net;
using System.Text.RegularExpressions;

namespace HomeCenter.Model.Messages.Commands.Service
{
    public class WakeOnLanCommand : UdpCommand
    {
        public WakeOnLanCommand(string macAddress, int port = 9)
        {
            macAddress = Regex.Replace(macAddress, "[-|:]", "");

            int payloadIndex = 0;

            /* The magic packet is a broadcast frame containing anywhere within its payload 6 bytes of all 255 (FF FF FF FF FF FF in hexadecimal), followed by sixteen repetitions of the target computer's 48-bit MAC address, for a total of 102 bytes. */
            byte[] payload = new byte[1024];    // Our packet that we will be broadcasting

            // Add 6 bytes with value 255 (FF) in our payload
            for (int i = 0; i < 6; i++)
            {
                payload[payloadIndex] = 255;
                payloadIndex++;
            }

            // Repeat the device MAC address sixteen times
            for (int j = 0; j < 16; j++)
            {
                for (int k = 0; k < macAddress.Length; k += 2)
                {
                    var s = macAddress.Substring(k, 2);
                    payload[payloadIndex] = byte.Parse(s, NumberStyles.HexNumber);
                    payloadIndex++;
                }
            }

            Body = payload;
            Address = $"255.255.255.255:{port}";
        }
    }
}