﻿namespace Countries.Models
{
    public class TemporalBlockedCountry
    {
        public string CountryCode { get; set; }
        public DateTime BlockedUntil { get; set; }
    }
}
