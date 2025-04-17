using System.Diagnostics;
using Microsoft.Extensions.Logging;

namespace ConsoleApp1.Utilities;

/// <summary>
/// Simple Stopwatch with adding addition memory allocations.
/// https://www.meziantou.net/how-to-measure-elapsed-time-without-allocating-a-stopwatch.htm
/// </summary>
/// <example>
/// var stopwatch = ValueStopwatch.StartNew();
/// ... do work
/// var elapsedTime = stopwatch.GetElapsedTime();
/// ... do some more work
/// var timeSinceStart = stopwatch.GetElapsedTime();
/// </example>
public readonly struct ValueStopwatch
{
    private static readonly double TimeStampToTicks = TimeSpan.TicksPerSecond / (double)Stopwatch.Frequency;
    private readonly long _startTimestamp;
    
    /// <summary>
    /// Initializes a new <see cref="T:Sitebulb.Core.Utilities.ValueStopwatch" /> instance and starts measuring elapsed time.
    /// </summary>
    /// <returns>
    /// A <see cref="T:Sitebulb.Core.Utilities.ValueStopwatch" /> that has just begun measuring elapsed time.
    /// </returns>
    public static ValueStopwatch StartNew() => new(GetTimestamp());
    
    /// <summary>
    /// Gets the elapsed time since starting <see cref="T:Sitebulb.Core.Utilities.ValueStopwatch" />.
    /// </summary>
    /// <returns>
    /// A <see cref="T:System.TimeSpan" /> for the elapsed time between the starting timestamp and the time of this call.
    /// </returns>
    public TimeSpan GetElapsedTime() => GetElapsedTime(_startTimestamp, GetTimestamp());

    private ValueStopwatch(long startTimestamp) => _startTimestamp = startTimestamp;
    
    private static long GetTimestamp() => Stopwatch.GetTimestamp();

    private static TimeSpan GetElapsedTime(long startTimestamp, long endTimestamp)
    {
        var timestampDelta = endTimestamp - startTimestamp;
        var ticks = (long)(TimeStampToTicks * timestampDelta);
        return new TimeSpan(ticks);
    }
}