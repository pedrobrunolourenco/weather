﻿namespace CloudWeather.DataLoader.Models
{
    public class TemperatureModel
    {
        public DateTime CreatedOn { get; set; }
        public decimal TempHighF { get; set; }
        public decimal TempLowF { get; set; }
        public string ZipCode { get; set; }

    }
}
