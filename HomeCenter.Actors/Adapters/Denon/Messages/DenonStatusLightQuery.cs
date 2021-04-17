﻿using System.Linq;
using System.Xml.Linq;
using HomeCenter.Abstractions;
using HomeCenter.Extensions;
using HomeCenter.Messages.Queries.Services;

namespace HomeCenter.Adapters.Denon.Messages
{
    internal class DenonStatusLightQuery : HttpGetQuery, IFormatableMessage<DenonStatusLightQuery>
    {
        public string Zone { get; set; } = "1";

        private float? NormalizeVolume(float? volume) => volume == null ? null : volume + 80.0f;

        public override object Parse(string rawHttpResult)
        {
            var xml = XDocument.Parse(rawHttpResult);

            return new DenonStatus
            {
                ActiveInput = xml.Descendants("InputFuncSelect").FirstOrDefault()?.Value?.Trim(),
                PowerStatus = xml.Descendants("Power").FirstOrDefault()?.Value?.Trim().ToLower() == "on" ? true : false,
                MasterVolume = NormalizeVolume(xml.Descendants("MasterVolume").FirstOrDefault()?.Value?.Trim().ToFloat()),
                Mute = xml.Descendants("Mute").FirstOrDefault()?.Value?.Trim().ToLower() == "on",
            };
        }

        public DenonStatusLightQuery FormatMessage()
        {
            if (Zone == "1")
            {
                Address = $"http://{Address}/goform/formMainZone_MainZoneXmlStatusLite.xml";
            }
            else
            {
                Address = $"http://{Address}/goform/formZone{Zone}_Zone{Zone}XmlStatusLite.xml";
            }

            return this;
        }

        public DenonStatusLightQuery()
        {
            LogLevel = nameof(Microsoft.Extensions.Logging.LogLevel.Trace);
        }
    }
}