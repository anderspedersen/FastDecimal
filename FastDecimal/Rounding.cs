using System.Runtime.CompilerServices;

namespace FastDecimal;

public static class Rounding
{
    internal static ulong RoundDec(ulong q, UInt128 rem, int digits, bool negative, MidpointRounding rounding)
    {
        if (digits < 19)
        {
            return Round64(q, rem.Lower, Fast128BitDiv.GetDivisorLow(digits), negative, rounding);
        }
        else
        {
            return Round128(q, rem, new UInt128(Fast128BitDiv.GetDivisorHigh(digits),Fast128BitDiv.GetDivisorLow(digits)), negative, rounding);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static uint Round64(ulong q, ulong remainder, ulong divisor, bool negative, MidpointRounding mode)
    {
        var roundUp = mode switch
        {
            MidpointRounding.ToEven => (remainder << 1) + (q % 2) > divisor,
            MidpointRounding.AwayFromZero => (remainder << 1) >= divisor,
            MidpointRounding.ToNegativeInfinity => negative && remainder > 0,
            MidpointRounding.ToPositiveInfinity => !negative && remainder > 0,
            MidpointRounding.ToZero => false,
            _ => throw new ArgumentException($"The value '{mode}' is not valid for this usage of the type {nameof(MidpointRounding)}.", nameof(mode))
        };

        return (roundUp ? 1u : 0u);
    }

    private static uint Round128(ulong q, UInt128 remainder, UInt128 divisor, bool negative, MidpointRounding mode)
    {
        var roundUp = mode switch
        {
            MidpointRounding.ToEven => (remainder << 1) + new UInt128(0,q % 2) > divisor,
            MidpointRounding.AwayFromZero => (remainder << 1) >= divisor,
            MidpointRounding.ToNegativeInfinity => negative && remainder > new UInt128(0,0),
            MidpointRounding.ToPositiveInfinity => !negative && remainder > new UInt128(0,0),
            MidpointRounding.ToZero => false,
            _ => throw new ArgumentException($"The value '{mode}' is not valid for this usage of the type {nameof(MidpointRounding)}.", nameof(mode))
        };

        return (roundUp ? 1u : 0u);
    }
}