using System.Globalization;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics.X86;
using FastDecimal.FractionalDigits;

namespace FastDecimal;

public readonly struct FastDecimal64<T> : 
    INumber<FastDecimal64<T>>,
    ISignedNumber<FastDecimal64<T>>,
    IMinMaxValue<FastDecimal64<T>>
    where T : struct, IFractionalDigits 
{
    private readonly long _value;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal FastDecimal64(long value)
    {
        if (GetFractionalDigits() is > 18 or < 0)
            throw new InvalidOperationException("FastDecimal64 only supports between 0 and 18 fractional digits.");
        _value = value;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static FastDecimal64<T> operator +(FastDecimal64<T> left, FastDecimal64<T> right)
    {
        return new FastDecimal64<T>(left._value + right._value);
    }

    public static FastDecimal64<T> operator checked +(FastDecimal64<T> left, FastDecimal64<T> right)
    {
        return new FastDecimal64<T>(checked(left._value + right._value));
    }

    public FastDecimal64<T2> ChangePrecision<T2>(MidpointRounding mode = MidpointRounding.ToEven) where T2 : struct, IFractionalDigits
    {
        var dstDigits = FastDecimal64<T2>.GetFractionalDigits();
        var srcDigits = GetFractionalDigits();
        
        if (dstDigits >= srcDigits)
            return new FastDecimal64<T2>(checked(_value * (long) Fast128BitDiv.GetDivisorLow(dstDigits - srcDigits)));

        ConvertToUnsigned(this, out var unsignedValue, out var negative);

        var (q, r) = Math.DivRem(unsignedValue, Fast128BitDiv.GetDivisorLow(srcDigits - dstDigits));
        q += Rounding.Round64(q, r, Fast128BitDiv.GetDivisorLow(srcDigits - dstDigits), negative, mode);

        return new FastDecimal64<T2>(negative ? -(long) q : (long) q);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    public static FastDecimal64<T> Multiply(FastDecimal64<T> left, FastDecimal64<T> right, MidpointRounding mode = MidpointRounding.ToEven)
    {
        return MultiplyInternal(left, right, mode, false);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    public static FastDecimal64<T> MultiplyChecked(FastDecimal64<T> left, FastDecimal64<T> right, MidpointRounding mode = MidpointRounding.ToEven)
    {
        return MultiplyInternal(left, right, mode, true);
    }


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static FastDecimal64<T> MultiplyInternal(FastDecimal64<T> left, FastDecimal64<T> right, MidpointRounding mode, bool isChecked)
    {
        ConvertToUnsigned(left, right, out var leftUnsigned, out var rightUnsigned, out var negative);

        var divisor = GetDivisor();

        var high = Math.BigMul(leftUnsigned, rightUnsigned, out var low);

        if (high == 0)
        {
            var (q, r) = Math.DivRem(low, divisor);
            q += Rounding.Round64(q, r, divisor, negative, mode);
            if (isChecked)
            {
                if (TryCast(q, negative, out var signed))
                {
                    return new FastDecimal64<T>(signed);
                }
                throw new OverflowException();
            }
            else
            {
                return new FastDecimal64<T>(negative ?  - (long) q : (long) q);
            }
        }
        else
        {
            var (q, r) = Fast128BitDiv.DecDivRem128By128(high, low, GetFractionalDigits());
            q += new UInt128(0,Rounding.Round64(q.Lower, r.Lower, divisor, negative, mode));
            
            if (isChecked)
            {
                if (q.Upper == 0 && TryCast(q.Lower, negative, out var signed))
                {
                    return new FastDecimal64<T>(signed);
                }
                throw new OverflowException();
            }
            else
            {
                return new FastDecimal64<T>(negative ?  - (long) q.Lower : (long) q.Lower);
            }
        }
    } 

    public static explicit operator FastDecimal64<T>(long value)
    {
        return new FastDecimal64<T>(value * (long) GetDivisor());
    }

    public static explicit operator decimal(FastDecimal64<T> value)
    {
        var scale = GetFractionalDigits();
        ConvertToUnsigned(value, out var unsignedValue, out var negative);

        return new decimal((int) unsignedValue, (int) (unsignedValue >> 32), 0, negative, (byte) scale);
    }


    public override string ToString()
    {
        return ((decimal) this).ToString();
    }

    public int CompareTo(object? value)
    {
        if (value == null)
            return 1;
        if (!(value is FastDecimal64<T>))
            throw new ArgumentException($"Object must be of type FastDecimal64<{typeof(T).Name}>.");

        var other = (FastDecimal64<T>)value;
        
        return CompareTo(other);
    }

    public int CompareTo(FastDecimal64<T> other)
    {
        return _value.CompareTo(other._value);
    }

    public string ToString(string? format, IFormatProvider? formatProvider)
    {
        return ((decimal) this).ToString(format, formatProvider);
    }

    public bool TryFormat(Span<char> destination, out int charsWritten, ReadOnlySpan<char> format, IFormatProvider? provider)
    {
        return ((decimal) this).TryFormat(destination, out charsWritten, format, provider);
    }

    public static FastDecimal64<T> AdditiveIdentity { get; } = new FastDecimal64<T>(0);

    public static bool operator ==(FastDecimal64<T> left, FastDecimal64<T> right) => left._value == right._value;

    public static bool operator !=(FastDecimal64<T> left, FastDecimal64<T> right) => left._value != right._value;

    public static bool operator >(FastDecimal64<T> left, FastDecimal64<T> right) => left._value > right._value;

    public static bool operator >=(FastDecimal64<T> left, FastDecimal64<T> right) => left._value >= right._value;

    public static bool operator <(FastDecimal64<T> left, FastDecimal64<T> right) => left._value < right._value;

    public static bool operator <=(FastDecimal64<T> left, FastDecimal64<T> right) => left._value <= right._value;


    [MethodImpl(MethodImplOptions.NoInlining)]
    public static FastDecimal64<T> operator /(FastDecimal64<T> left, FastDecimal64<T> right)
    {
        return DivideInternal(left, right, MidpointRounding.ToEven, false);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    public static FastDecimal64<T> operator checked /(FastDecimal64<T> left, FastDecimal64<T> right)
    {
        return DivideInternal(left, right, MidpointRounding.ToEven, true);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    public static FastDecimal64<T> Divide(FastDecimal64<T> left, FastDecimal64<T> right, MidpointRounding mode)
    {
        return DivideInternal(left, right, mode, false);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    public static FastDecimal64<T> DivideChecked(FastDecimal64<T> left, FastDecimal64<T> right, MidpointRounding mode)
    {
        return DivideInternal(left, right, mode, true);
    }


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static FastDecimal64<T> DivideInternal(FastDecimal64<T> left, FastDecimal64<T> right, MidpointRounding mode, bool isChecked)
    {
        ConvertToUnsigned(left, right, out var dividend, out var divisor, out var negative);

        var precisionDivisor = GetDivisor();

        var scaledDividendHigh = Math.BigMul(dividend, precisionDivisor, out var scaledDividendLow);

        if (scaledDividendHigh == 0)
        {
            var (q, r) = Math.DivRem(scaledDividendLow, divisor);
            q += Rounding.Round64(q, r, divisor, negative, mode);

            if (isChecked)
            {
                if (TryCast(q, negative, out var signed))
                {
                    return new FastDecimal64<T>(signed);
                }
                throw new OverflowException();
            }
            else
            {
                return new FastDecimal64<T>(negative ? -(long) q : (long) q);
            }
        }
        else
        {
            if (isChecked && scaledDividendHigh >= divisor)
                throw new OverflowException();
            
            UInt128 q;
            ulong r;
            # if NET8_0_OR_GREATER
            if (X86Base.X64.IsSupported && scaledDividendHigh < divisor)
            {
                (var qLow, r) = X86Base.X64.DivRem(scaledDividendLow, scaledDividendHigh, divisor);
                q = new UInt128(0, qLow);
            }
            else
            #endif
            {
                (q, var rem) = UInt128.DivRem(new UInt128(scaledDividendHigh, scaledDividendLow), new UInt128(0,divisor));
                r = rem.Lower;
            }
            
            q += new UInt128(0, Rounding.Round64(q.Lower, r, divisor, negative, mode));

            if (isChecked)
            {
                if (q.Upper == 0 && TryCast(q.Lower, negative, out var signed))
                {
                    return new FastDecimal64<T>(signed);
                }
                throw new OverflowException();
            }

            return new FastDecimal64<T>(negative ? -(long) q.Lower : (long) q.Lower);
        }
    }

    private static void ConvertToUnsigned(FastDecimal64<T> fd1, FastDecimal64<T> fd2, out ulong unsignedValue1, out ulong unsignedValue2,
        out bool negative)
    {
        negative = false;
        if (fd1._value >= 0)
        {
            unsignedValue1 = (ulong) fd1._value;
        }
        else
        {
            unsignedValue1 = (ulong) -fd1._value;
            negative = !negative;
        }

        if (fd2._value >= 0)
        {
            unsignedValue2 = (ulong) fd2._value;
        }
        else
        {
            unsignedValue2 = (ulong) -fd2._value;
            negative = !negative;
        }
    }

    private static void ConvertToUnsigned(FastDecimal64<T> fd, out ulong unsignedValue, out bool negative)
    {
        negative = false;
        if (fd._value >= 0)
        {
            unsignedValue = (ulong) fd._value;
        }
        else
        {
            unsignedValue = (ulong) -fd._value;
            negative = !negative;
        }
    }

    public static FastDecimal64<T> operator ++(FastDecimal64<T> value)
    {
        var div = GetDivisor();
        return new FastDecimal64<T>(value._value + (long) div);
    }

    public static FastDecimal64<T> operator checked ++(FastDecimal64<T> value)
    {
        var div = GetDivisor();
        return new FastDecimal64<T>(checked(value._value + (long) div));
    }

    public static FastDecimal64<T> operator --(FastDecimal64<T> value)
    {
        var div = GetDivisor();
        return new FastDecimal64<T>(value._value - (long) div);
    }

    public static FastDecimal64<T> operator checked --(FastDecimal64<T> value)
    {
        var div = GetDivisor();
        return new FastDecimal64<T>(checked(value._value - (long) div));
    }

    public static FastDecimal64<T> MaxValue => new FastDecimal64<T>(long.MaxValue);

    public static FastDecimal64<T> MinValue => new FastDecimal64<T>(long.MinValue);

    public static FastDecimal64<T> MultiplicativeIdentity => new FastDecimal64<T>((long) GetDivisor());

    [MethodImpl(MethodImplOptions.NoInlining)]
    public static FastDecimal64<T> operator *(FastDecimal64<T> left, FastDecimal64<T> right)
    {
        return MultiplyInternal(left, right, MidpointRounding.ToEven, isChecked: false);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    public static FastDecimal64<T> operator checked *(FastDecimal64<T> left, FastDecimal64<T> right)
    {
        return MultiplyInternal(left, right, MidpointRounding.ToEven, isChecked: true);
    }

    public static FastDecimal64<T> operator -(FastDecimal64<T> left, FastDecimal64<T> right)
    {
        return new FastDecimal64<T>(left._value - right._value);
    }

    public static FastDecimal64<T> operator checked -(FastDecimal64<T> left, FastDecimal64<T> right)
    {
        return new FastDecimal64<T>(checked(left._value - right._value));
    }

    public bool Equals(FastDecimal64<T> other)
    {
        return _value == other._value;
    }

    public static FastDecimal64<T> operator +(FastDecimal64<T> value) => value;

    public static FastDecimal64<T> operator %(FastDecimal64<T> left, FastDecimal64<T> right)
    {
        return new FastDecimal64<T>(left._value % right._value);
    }

    public override bool Equals(object? obj)
    {
        return obj is FastDecimal64<T> other && Equals(other);
    }

    public override int GetHashCode()
    {
        return _value.GetHashCode();
    }

    public static FastDecimal64<T> Parse(string s, IFormatProvider? provider)
    {
        return (FastDecimal64<T>) decimal.Parse(s, provider);
    }

    public static bool TryParse(string? s, IFormatProvider? provider, out FastDecimal64<T> result)
    {
        if(decimal.TryParse(s, provider, out var dec) && TryCastDecimal(dec, out result))
        {
            return true;
        }
        result = default;
        return false;
    }

    public static FastDecimal64<T> Parse(ReadOnlySpan<char> s, IFormatProvider? provider)
    {
        return (FastDecimal64<T>) decimal.Parse(s, provider);
    }

    public static bool TryParse(ReadOnlySpan<char> s, IFormatProvider? provider, out FastDecimal64<T> result)
    {
        if (decimal.TryParse(s, provider, out var dec) && TryCastDecimal(dec, out result))
        {
            return true;
        }
        result = default;
        return false;
    }

    public static FastDecimal64<T> operator -(FastDecimal64<T> value)
    {
        return new FastDecimal64<T>(-value._value);
    }

    public static FastDecimal64<T> operator checked -(FastDecimal64<T> value)
    {
        return new FastDecimal64<T>(checked(-value._value));
    }

    public static FastDecimal64<T> Abs(FastDecimal64<T> value)
    {
        return new FastDecimal64<T>(long.Abs(value._value));
    }

    static bool INumberBase<FastDecimal64<T>>.IsCanonical(FastDecimal64<T> value) => true;

    static bool INumberBase<FastDecimal64<T>>.IsComplexNumber(FastDecimal64<T> value) => false;

    public static bool IsEvenInteger(FastDecimal64<T> value)
    {
        return value._value % (2 * (long) GetDivisor()) == 0;
    }

    static bool INumberBase<FastDecimal64<T>>.IsFinite(FastDecimal64<T> value) => true;

    static bool INumberBase<FastDecimal64<T>>.IsImaginaryNumber(FastDecimal64<T> value) => false;

    static bool INumberBase<FastDecimal64<T>>.IsInfinity(FastDecimal64<T> value) => false;

    public static bool IsInteger(FastDecimal64<T> value)
    {
        var divisor = GetDivisor();
        return value._value % (long) divisor == 0;
    }

    static bool INumberBase<FastDecimal64<T>>.IsNaN(FastDecimal64<T> value) => false;

    public static bool IsNegative(FastDecimal64<T> value) => long.IsNegative(value._value);

    static bool INumberBase<FastDecimal64<T>>.IsNegativeInfinity(FastDecimal64<T> value) => false;

    public static bool IsNormal(FastDecimal64<T> value)
    {
        return value._value != 0;
    }

    public static bool IsOddInteger(FastDecimal64<T> value)
    {
        var (q, r) = Math.DivRem(value._value, (long) GetDivisor());
        return r == 0 && (q & 1) != 0;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static ulong GetDivisor() => Fast128BitDiv.GetDivisorLow(GetFractionalDigits());

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static int GetFractionalDigits() => new T().FractionalDigits;

    public static bool IsPositive(FastDecimal64<T> value) => long.IsPositive(value._value);

    static bool INumberBase<FastDecimal64<T>>.IsPositiveInfinity(FastDecimal64<T> value) => false;

    static bool INumberBase<FastDecimal64<T>>.IsRealNumber(FastDecimal64<T> value) => true;

    static bool INumberBase<FastDecimal64<T>>.IsSubnormal(FastDecimal64<T> value) => false;

    public static bool IsZero(FastDecimal64<T> value) => value._value == 0;

    public static FastDecimal64<T> MaxMagnitude(FastDecimal64<T> x, FastDecimal64<T> y)
    {
        return new FastDecimal64<T>(long.MaxMagnitude(x._value, y._value));
    }

    static FastDecimal64<T> INumberBase<FastDecimal64<T>>.MaxMagnitudeNumber(FastDecimal64<T> x, FastDecimal64<T> y) 
        => MaxMagnitude(x, y);

    public static FastDecimal64<T> MinMagnitude(FastDecimal64<T> x, FastDecimal64<T> y)
    {
        return new FastDecimal64<T>(long.MinMagnitude(x._value, y._value));
    }

    static FastDecimal64<T> INumberBase<FastDecimal64<T>>.MinMagnitudeNumber(FastDecimal64<T> x, FastDecimal64<T> y) 
        => MinMagnitude(x, y);

    public static FastDecimal64<T> Parse(ReadOnlySpan<char> s, NumberStyles style, IFormatProvider? provider)
    {
        var dec = decimal.Parse(s, style, provider);
        if (TryCastDecimal(dec, out var o))
            return o;
        
        throw new OverflowException();
    }

    public static FastDecimal64<T> Parse(string s, NumberStyles style, IFormatProvider? provider)
    {
        var dec = decimal.Parse(s, style, provider);
        if (TryCastDecimal(dec, out var o))
            return o;
        
        throw new OverflowException();
    }

    public static bool TryConvertFromChecked<TOther>(TOther value, out FastDecimal64<T> result) where TOther : INumberBase<TOther>
    {
        result = default;
        return false;
    }

    public static bool TryConvertFromSaturating<TOther>(TOther value, out FastDecimal64<T> result) where TOther : INumberBase<TOther>
    {
        result = default;
        return false;
    }

    public static bool TryConvertFromTruncating<TOther>(TOther value, out FastDecimal64<T> result) where TOther : INumberBase<TOther>
    {
        result = default;
        return false;
    }

    public static bool TryConvertToChecked<TOther>(FastDecimal64<T> value, out TOther? result) where TOther : INumberBase<TOther>
    {
        result = default;
        return false;
    }

    public static bool TryConvertToSaturating<TOther>(FastDecimal64<T> value, out TOther? result) where TOther : INumberBase<TOther>
    {
        result = default;
        return false;
    }

    public static bool TryConvertToTruncating<TOther>(FastDecimal64<T> value, out TOther? result) where TOther : INumberBase<TOther>
    {
        result = default;
        return false;
    }

    public static bool TryParse(ReadOnlySpan<char> s, NumberStyles style, IFormatProvider? provider, out FastDecimal64<T> result)
    {
        var success = decimal.TryParse(s, style, provider, out var dec);
        result = (FastDecimal64<T>) dec;
        return success;
    }

    public static bool TryParse(string? s, NumberStyles style, IFormatProvider? provider, out FastDecimal64<T> result)
    {
        result = default;
        return (decimal.TryParse(s, style, provider, out var dec) && TryCastDecimal(dec, out result)) ;
        
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool TryCast(ulong ul, bool negative, out long l)
    {
        if (!negative && ul <= long.MaxValue)
        {
            l = (long) ul;
            return true;
        }

        if (negative && ul <= (ulong) long.MaxValue + 1)
        {
            l = -(long) ul;
            return true;
        }

        l = default;
        return false;
    }

    public static explicit operator FastDecimal64<T>(decimal value)
    {
        var dec = Unsafe.As<decimal, DecimalStruct>(ref value);
        var scale = dec.Scale;
        var fractionalDigits = GetFractionalDigits();

        if (scale == fractionalDigits)
        {
            return new FastDecimal64<T>(dec.Negative ? -(long) dec.Low : (long) dec.Low);
        } 
        
        if (scale < fractionalDigits)
        {
            var high = Math.BigMul(dec.Low, Fast128BitDiv.GetDivisorLow(fractionalDigits - scale), out var low);
            return new FastDecimal64<T>(dec.Negative ? -(long) low : (long) low);
        }
        
        var (q,r) = Fast128BitDiv.DecDivRem128By128(dec.High, dec.Low, scale - fractionalDigits);
        q += new UInt128(0,Rounding.RoundDec(q.Lower, r, scale - fractionalDigits, dec.Negative, MidpointRounding.ToEven));
        return new FastDecimal64<T>(dec.Negative ? -(long) q.Lower : (long) q.Lower);
    }

    public static explicit operator checked FastDecimal64<T>(decimal value)
    {
        if (TryCastDecimal(value, out var fd))
        {
            return fd;
        }

        throw new OverflowException();
    }

    public static bool TryCastDecimal(decimal value, out FastDecimal64<T> fd)
    {
        var dec = Unsafe.As<decimal, DecimalStruct>(ref value);
        var scale = dec.Scale;
        var fractionalDigits = GetFractionalDigits();

        if (scale == fractionalDigits)
        {
            if (dec.High == 0 && TryCast(dec.Low, dec.Negative, out var signed))
            {
                fd = new FastDecimal64<T>(signed);
                return true;
            }
        } 
        else if (scale < fractionalDigits && dec.High == 0)
        {
            var high = Math.BigMul(dec.Low, Fast128BitDiv.GetDivisorLow(fractionalDigits - scale), out var low);
            if (high == 0 && TryCast(low, dec.Negative, out var signed))
            {
                fd = new FastDecimal64<T>(signed);
                return true;
            }
        } 
        else if (scale > fractionalDigits)
        {
            var (q,r) = Fast128BitDiv.DecDivRem128By128(dec.High, dec.Low, scale - fractionalDigits);
            q += new UInt128(0,Rounding.RoundDec(q.Lower, r, scale - fractionalDigits, dec.Negative, MidpointRounding.ToEven));

            if (q.Upper == 0 && TryCast(q.Lower, dec.Negative, out var signed))
            {
                fd = new FastDecimal64<T>(signed);
                return true;
            }
        }
        
        fd = default;
        return false;
    }

    public static FastDecimal64<T> One => new((long) GetDivisor());
    public static FastDecimal64<T> NegativeOne => -One;
    static int INumberBase<FastDecimal64<T>>.Radix => 10;

    public static FastDecimal64<T> Zero => new FastDecimal64<T>(0);
}
