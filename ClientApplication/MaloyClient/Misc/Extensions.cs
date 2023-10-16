using System;

namespace MaloyClient.Misc;

public static class Extensions
{
    public static DateTime UnixTimeStampToDateTime(this ulong unixTimeStamp)
    {
        // Unix timestamp is seconds past epoch
        var dateTime = DateTime.UnixEpoch;
        
        dateTime = dateTime.AddSeconds( unixTimeStamp ).ToLocalTime();
        return dateTime;
    }

    public static ulong DateTimeToUnixTimeStamp(this DateTime dateTime)
    {
        return (ulong)(dateTime.ToUniversalTime().Ticks - DateTime.UnixEpoch.Ticks) / 10000000;
    }
}