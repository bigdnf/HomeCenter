﻿using System.Collections.Generic;

namespace HomeCenter.Adapters.Denon
{
    public class DenonDeviceInfo
    {
        public string Surround { get; set; }
        public string Model { get; set; }
        public string FriendlyName { get; set; }
        public Dictionary<string, string> InputMap { get; set; }
        public Dictionary<string, string> InputSources { get; set; }

        public string TranslateInputName(string inputName, string zone)
        {
            string input = "";

            // If inputName is renamed value we changed it to original
            if (InputSources.ContainsKey(inputName))
            {
                inputName = InputSources[inputName];
            }
            // Search for mapping
            if (InputMap.ContainsKey(inputName))
            {
                input = InputMap[inputName];
            }
            // If there is no mapping maybe value is already mapped value
            if (InputMap.ContainsValue(inputName))
            {
                input = inputName;
            }

            if (!string.IsNullOrWhiteSpace(input))
            {
                if (zone == "1")
                {
                    input = $"SI{input}";
                }
                else
                {
                    input = $"Z{zone}{input}";
                }
            }

            return input;
        }
    }
}