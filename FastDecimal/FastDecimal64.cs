using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics.X86;
using FastDecimal.FractionalDigits;

namespace FastDecimal;

/// <summary>Represents a 64-bit signed fixed-point decimal number.</summary>
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

    /// <inheritdoc cref="IComparable.CompareTo(object)" />
    public int CompareTo(object? value)
    {
        if (value == null)
            return 1;
        if (!(value is FastDecimal64<T>))
            throw new ArgumentException($"Object must be of type FastDecimal64<{typeof(T).Name}>.");

        var other = (FastDecimal64<T>)value;
        
        return CompareTo(other);
    }

    /// <inheritdoc cref="IComparable{T}.CompareTo(T)" />
    public int CompareTo(FastDecimal64<T> other)
    {
        return _value.CompareTo(other._value);
    }

    /// <inheritdoc cref="object.Equals(object?)" />
    public override bool Equals(object? obj)
    {
        return obj is FastDecimal64<T> other && Equals(other);
    }

    /// <inheritdoc cref="IEquatable{T}.Equals(T)" />
    public bool Equals(FastDecimal64<T> other)
    {
        return _value == other._value;
    }

    /// <inheritdoc cref="object.GetHashCode()" />
    public override int GetHashCode()
    {
        return _value.GetHashCode();
    }

    /// <inheritdoc cref="object.ToString()" />
    public override string ToString()
    {
        return ((decimal) this).ToString();
    }

    /// <inheritdoc cref="IFormattable.ToString(string?,System.IFormatProvider?)" />
    public string ToString(string? format, IFormatProvider? formatProvider)
    {
        return ((decimal) this).ToString(format, formatProvider);
    }

    /// <inheritdoc cref="ISpanFormattable.TryFormat(Span&lt;char&gt;,out int, ReadOnlySpan&lt;char&gt;, IFormatProvider?)" />
    public bool TryFormat(Span<char> destination, out int charsWritten, ReadOnlySpan<char> format, IFormatProvider? provider)
    {
        return ((decimal) this).TryFormat(destination, out charsWritten, format, provider);
    }

    /// <inheritdoc cref="IParsable{TSelf}.Parse(string, IFormatProvider?)" />
    public static FastDecimal64<T> Parse(string s, IFormatProvider? provider)
    {
        return (FastDecimal64<T>) decimal.Parse(s, provider);
    }


    /// <inheritdoc cref="INumberBase{TSelf}.Parse(string, NumberStyles, IFormatProvider?)" />
    public static FastDecimal64<T> Parse(string s, NumberStyles style, IFormatProvider? provider)
    {
        var dec = decimal.Parse(s, style, provider);
        if (TryCastDecimal(dec, out var o))
            return o;
        
        throw new OverflowException();
    }

    /// <inheritdoc cref="ISpanParsable{TSelf}.Parse(ReadOnlySpan&lt;char&gt;, IFormatProvider?)" />
    public static FastDecimal64<T> Parse(ReadOnlySpan<char> s, IFormatProvider? provider)
    {
        return (FastDecimal64<T>) decimal.Parse(s, provider);
    }

    /// <inheritdoc cref="INumberBase{TSelf}.Parse(ReadOnlySpan&lt;char&gt;, NumberStyles, IFormatProvider?)" />
    public static FastDecimal64<T> Parse(ReadOnlySpan<char> s, NumberStyles style, IFormatProvider? provider)
    {
        var dec = decimal.Parse(s, style, provider);
        if (TryCastDecimal(dec, out var o))
            return o;
        
        throw new OverflowException();
    }

    /// <inheritdoc cref="IParsable{TSelf}.TryParse(string?, IFormatProvider?, out TSelf)" />
    public static bool TryParse([NotNullWhen(true)] string? s, IFormatProvider? provider, out FastDecimal64<T> result)
    {
        if(decimal.TryParse(s, provider, out var dec) && TryCastDecimal(dec, out result))
        {
            return true;
        }
        result = default;
        return false;
    }

    /// <inheritdoc cref="ISpanParsable{TSelf}.TryParse(ReadOnlySpan&lt;char&gt;, IFormatProvider?, out TSelf)" />
    public static bool TryParse(ReadOnlySpan<char> s, IFormatProvider? provider, out FastDecimal64<T> result)
    {
        if (decimal.TryParse(s, provider, out var dec) && TryCastDecimal(dec, out result))
        {
            return true;
        }
        result = default;
        return false;
    }

    /// <inheritdoc cref="INumberBase{TSelf}.TryParse(string?, NumberStyles, IFormatProvider?, out TSelf)" />
    public static bool TryParse([NotNullWhen(true)] string? s, NumberStyles style, IFormatProvider? provider, out FastDecimal64<T> result)
    {
        result = default;
        return (decimal.TryParse(s, style, provider, out var dec) && TryCastDecimal(dec, out result)) ;
        
    }

    /// <inheritdoc cref="INumberBase{TSelf}.TryParse(ReadOnlySpan&lt;char&gt;, NumberStyles, IFormatProvider?, out TSelf)" />
    public static bool TryParse(ReadOnlySpan<char> s, NumberStyles style, IFormatProvider? provider, out FastDecimal64<T> result)
    {
        var success = decimal.TryParse(s, style, provider, out var dec);
        result = (FastDecimal64<T>) dec;
        return success;
    }

    //
    // Explicit Conversions From FastDecimal64
    //

    /// <summary>Explicitly converts a <see cref="FastDecimal64{T}" /> to a <see cref="FastDecimal32{T}" /> value.</summary>
    /// <param name="value">The value to convert.</param>
    /// <returns><paramref name="value" /> converted to a <see cref="FastDecimal32{T}" />.</returns>
    public static explicit operator FastDecimal32<T>(FastDecimal64<T> value)
    {
        return new FastDecimal32<T>((int) value._value);
    }

    /// <summary>Explicitly converts a <see cref="FastDecimal64{T}" /> to a <see cref="FastDecimal32{T}" /> value.</summary>
    /// <param name="value">The value to convert.</param>
    /// <returns><paramref name="value" /> converted to a <see cref="FastDecimal32{T}" />.</returns>
    /// <exception cref="OverflowException"><paramref name="value" /> is not representable by <see cref="FastDecimal32{T}" />.</exception>
    public static explicit operator checked FastDecimal32<T>(FastDecimal64<T> value)
    {
        return new FastDecimal32<T>(checked((int) value._value));
    }

    /// <summary>Explicitly converts a <see cref="FastDecimal64{T}" /> to a <see cref="decimal" /> value.</summary>
    /// <param name="value">The value to convert.</param>
    /// <returns><paramref name="value" /> converted to a <see cref="decimal" />.</returns>
    public static explicit operator decimal(FastDecimal64<T> value)
    {
        var scale = GetFractionalDigits();
        ConvertToUnsigned(value, out var unsignedValue, out var negative);

        return new decimal((int) unsignedValue, (int) (unsignedValue >> 32), 0, negative, (byte) scale);
    }

    //
    // Explicit Conversions To FastDecimal64
    //

    /// <summary>Explicitly converts a <see cref="long" /> to a <see cref="FastDecimal64{T}" /> value.</summary>
    /// <param name="value">The value to convert.</param>
    /// <returns><paramref name="value" /> converted to a <see cref="FastDecimal64{T}" />.</returns>
    public static explicit operator FastDecimal64<T>(long value)
    {
        return new FastDecimal64<T>(value * (long) GetDivisor());
    }

    /// <summary>Explicitly converts a <see cref="long" /> to a <see cref="FastDecimal64{T}" /> value, throwing an overflow exception for any values that fall outside the representable range.</summary>
    /// <param name="value">The value to convert.</param>
    /// <returns><paramref name="value" /> converted to a <see cref="FastDecimal64{T}" />.</returns>
    /// <exception cref="OverflowException"><paramref name="value" /> is not representable by <see cref="FastDecimal64{T}" />.</exception>
    public static explicit operator checked FastDecimal64<T>(long value)
    {
        return new FastDecimal64<T>(checked(value * unchecked((long) GetDivisor())));
    }

    /// <summary>Explicitly converts a <see cref="decimal" /> to a <see cref="FastDecimal64{T}" /> value.</summary>
    /// <param name="value">The value to convert.</param>
    /// <returns><paramref name="value" /> converted to a <see cref="FastDecimal64{T}" />.</returns>
    public static explicit operator FastDecimal64<T>(decimal value)
    {
        var dec = Unsafe.As<decimal, DecimalStruct>(ref value);
        var scale = dec.Scale;
        var fractionalDigits = GetFractionalDigits();

        if (scale == fractionalDigits)
        {
            return new FastDecimal64<T>(dec.Negative ? -(long) dec.Low64 : (long) dec.Low64);
        } 
        
        if (scale < fractionalDigits)
        {
            var high = Math.BigMul(dec.Low64, Fast128BitDiv.GetDivisorLow(fractionalDigits - scale), out var low);
            return new FastDecimal64<T>(dec.Negative ? -(long) low : (long) low);
        }
        
        var (q,r) = Fast128BitDiv.DecDivRem128By128(dec.High32, dec.Low64, scale - fractionalDigits);
        q += new UInt128(0,Rounding.RoundDec(q.Lower, r, scale - fractionalDigits, dec.Negative, MidpointRounding.ToEven));
        return new FastDecimal64<T>(dec.Negative ? -(long) q.Lower : (long) q.Lower);
    }

    /// <summary>Explicitly converts a <see cref="decimal" /> to a <see cref="FastDecimal64{T}" /> value, throwing an overflow exception for any values that fall outside the representable range.</summary>
    /// <param name="value">The value to convert.</param>
    /// <returns><paramref name="value" /> converted to a <see cref="FastDecimal64{T}" />.</returns>
    /// <exception cref="OverflowException"><paramref name="value" /> is not representable by <see cref="FastDecimal64{T}" />.</exception>
    public static explicit operator checked FastDecimal64<T>(decimal value)
    {
        if (TryCastDecimal(value, out var fd))
        {
            return fd;
        }

        throw new OverflowException();
    }

    //
    // IAdditionOperators
    //

    /// <inheritdoc cref="IAdditionOperators{TSelf, TOther, TResult}.op_Addition(TSelf, TOther)" />
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static FastDecimal64<T> operator +(FastDecimal64<T> left, FastDecimal64<T> right)
    {
        return new FastDecimal64<T>(left._value + right._value);
    }

    /// <inheritdoc cref="IAdditionOperators{TSelf, TOther, TResult}.op_CheckedAddition(TSelf, TOther)" />
    public static FastDecimal64<T> operator checked +(FastDecimal64<T> left, FastDecimal64<T> right)
    {
        return new FastDecimal64<T>(checked(left._value + right._value));
    }

    //
    // IAdditiveIdentity
    //

    /// <inheritdoc cref="IAdditiveIdentity{TSelf, TResult}.AdditiveIdentity" />
    public static FastDecimal64<T> AdditiveIdentity { get; } = default;
    
    //
    // IComparisonOperators
    //

    /// <inheritdoc cref="IComparisonOperators{TSelf, TOther, TResult}.op_GreaterThan(TSelf, TOther)" />
    public static bool operator >(FastDecimal64<T> left, FastDecimal64<T> right) => left._value > right._value;

    /// <inheritdoc cref="IComparisonOperators{TSelf, TOther, TResult}.op_GreaterThanOrEqual(TSelf, TOther)" />
    public static bool operator >=(FastDecimal64<T> left, FastDecimal64<T> right) => left._value >= right._value;

    /// <inheritdoc cref="IComparisonOperators{TSelf, TOther, TResult}.op_LessThan(TSelf, TOther)" />
    public static bool operator <(FastDecimal64<T> left, FastDecimal64<T> right) => left._value < right._value;

    /// <inheritdoc cref="IComparisonOperators{TSelf, TOther, TResult}.op_LessThanOrEqual(TSelf, TOther)" />
    public static bool operator <=(FastDecimal64<T> left, FastDecimal64<T> right) => left._value <= right._value;

    //
    // IDecrementOperators
    //

    /// <inheritdoc cref="IDecrementOperators{TSelf}.op_Decrement(TSelf)" />
    public static FastDecimal64<T> operator --(FastDecimal64<T> value)
    {
        var div = GetDivisor();
        return new FastDecimal64<T>(value._value - (long) div);
    }

    /// <inheritdoc cref="IDecrementOperators{TSelf}.op_CheckedDecrement(TSelf)" />
    public static FastDecimal64<T> operator checked --(FastDecimal64<T> value)
    {
        var div = GetDivisor();
        return new FastDecimal64<T>(checked(value._value - (long) div));
    }

    //
    // IDivisionOperators
    //

    /// <inheritdoc cref="IDivisionOperators{TSelf, TOther, TResult}.op_Division(TSelf, TOther)" />
    [MethodImpl(MethodImplOptions.NoInlining)]
    public static FastDecimal64<T> operator /(FastDecimal64<T> left, FastDecimal64<T> right)
    {
        return DivideInternal(left, right, MidpointRounding.ToEven, false);
    }

    /// <inheritdoc cref="IDivisionOperators{TSelf, TOther, TResult}.op_CheckedDivision(TSelf, TOther)" />
    [MethodImpl(MethodImplOptions.NoInlining)]
    public static FastDecimal64<T> operator checked /(FastDecimal64<T> left, FastDecimal64<T> right)
    {
        return DivideInternal(left, right, MidpointRounding.ToEven, true);
    }

    /// <summary>Divides two values together to compute their quotient.</summary>
    /// <param name="left">The value which <paramref name="right" /> divides.</param>
    /// <param name="right">The value which divides <paramref name="left" />.</param>
    /// <param name="mode">One of the enumeration values that specifies which rounding strategy to use.</param>;
    /// <returns>The quotient of <paramref name="left" /> divided-by <paramref name="right" />.</returns>
    /// <exception cref="ArgumentException"><paramref name="mode" /> is not a MidpointRounding value.</exception>
    [MethodImpl(MethodImplOptions.NoInlining)]
    public static FastDecimal64<T> Divide(FastDecimal64<T> left, FastDecimal64<T> right, MidpointRounding mode)
    {
        return DivideInternal(left, right, mode, false);
    }

    /// <summary>Divides two values together to compute their quotient.</summary>
    /// <param name="left">The value which <paramref name="right" /> divides.</param>
    /// <param name="right">The value which divides <paramref name="left" />.</param>
    /// <param name="mode">One of the enumeration values that specifies which rounding strategy to use.</param>;
    /// <returns>The quotient of <paramref name="left" /> divided-by <paramref name="right" />.</returns>
    /// <exception cref="ArgumentException"><paramref name="mode" /> is not a MidpointRounding value.</exception>
    /// <exception cref="OverflowException">The quotient of <paramref name="left" /> divided-by <paramref name="right" /> is not representable by <href name="FastDecimal64{T}" />.</exception>
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

    //
    // IEqualityOperators
    //

    /// <inheritdoc cref="IEqualityOperators{TSelf, TOther, TResult}.op_Equality(TSelf, TOther)" />
    public static bool operator ==(FastDecimal64<T> left, FastDecimal64<T> right) => left._value == right._value;

    /// <inheritdoc cref="IEqualityOperators{TSelf, TOther, TResult}.op_Inequality(TSelf, TOther)" />
    public static bool operator !=(FastDecimal64<T> left, FastDecimal64<T> right) => left._value != right._value;
    
    //
    // IIncrementOperators
    //

    /// <inheritdoc cref="IIncrementOperators{TSelf}.op_Increment(TSelf)" />
    public static FastDecimal64<T> operator ++(FastDecimal64<T> value)
    {
        var div = GetDivisor();
        return new FastDecimal64<T>(value._value + (long) div);
    }

    /// <inheritdoc cref="IIncrementOperators{TSelf}.op_CheckedIncrement(TSelf)" />
    public static FastDecimal64<T> operator checked ++(FastDecimal64<T> value)
    {
        var div = GetDivisor();
        return new FastDecimal64<T>(checked(value._value + unchecked((long) div)));
    }

    //
    // IMinMaxValue
    //

    /// <inheritdoc cref="IMinMaxValue{TSelf}.MaxValue" />
    public static FastDecimal64<T> MaxValue => new FastDecimal64<T>(long.MaxValue);

    /// <inheritdoc cref="IMinMaxValue{TSelf}.MinValue" />
    public static FastDecimal64<T> MinValue => new FastDecimal64<T>(long.MinValue);

    //
    // IModulusOperators
    //

    /// <inheritdoc cref="IModulusOperators{TSelf, TOther, TResult}.op_Modulus(TSelf, TOther)" />
    public static FastDecimal64<T> operator %(FastDecimal64<T> left, FastDecimal64<T> right)
    {
        return new FastDecimal64<T>(left._value % right._value);
    }

    //
    // IMultiplicativeIdentity
    //

    /// <inheritdoc cref="IMultiplicativeIdentity{TSelf, TResult}.MultiplicativeIdentity" />
    public static FastDecimal64<T> MultiplicativeIdentity => new FastDecimal64<T>((long) GetDivisor());
    
    //
    // IMultiplyOperators
    //

    /// <inheritdoc cref="IMultiplyOperators{TSelf, TOther, TResult}.op_Multiply(TSelf, TOther)" />
    [MethodImpl(MethodImplOptions.NoInlining)]
    public static FastDecimal64<T> operator *(FastDecimal64<T> left, FastDecimal64<T> right)
    {
        return MultiplyInternal(left, right, MidpointRounding.ToEven, isChecked: false);
    }

    /// <inheritdoc cref="IMultiplyOperators{TSelf, TOther, TResult}.op_CheckedMultiply(TSelf, TOther)" />
    [MethodImpl(MethodImplOptions.NoInlining)]
    public static FastDecimal64<T> operator checked *(FastDecimal64<T> left, FastDecimal64<T> right)
    {
        return MultiplyInternal(left, right, MidpointRounding.ToEven, isChecked: true);
    }

    /// <summary>Multiplies two values together to compute their product.</summary>
    /// <param name="left">The value which <paramref name="right" /> multiplies.</param>
    /// <param name="right">The value which multiplies <paramref name="left" />.</param>
    /// <param name="mode">One of the enumeration values that specifies which rounding strategy to use.</param>
    /// <returns>The product of <paramref name="left" /> multiplied-by <paramref name="right" />.</returns>
    /// <exception cref="ArgumentException"><paramref name="mode" /> is not a MidpointRounding value.</exception>
    [MethodImpl(MethodImplOptions.NoInlining)]
    public static FastDecimal64<T> Multiply(FastDecimal64<T> left, FastDecimal64<T> right, MidpointRounding mode)
    {
        return MultiplyInternal(left, right, mode, false);
    }

    /// <summary>Multiplies two values together to compute their product.</summary>
    /// <param name="left">The value which <paramref name="right" /> multiplies.</param>
    /// <param name="right">The value which multiplies <paramref name="left" />.</param>
    /// <param name="mode">One of the enumeration values that specifies which rounding strategy to use.</param>
    /// <returns>The product of <paramref name="left" /> multiplied-by <paramref name="right" />.</returns>
    /// <exception cref="ArgumentException"><paramref name="mode" /> is not a MidpointRounding value.</exception>
    /// <exception cref="OverflowException">The product of <paramref name="left" /> multiplied-by <paramref name="right" /> is not representable by <seef cref="FastDecimal64{T}" />.</exception>

    [MethodImpl(MethodImplOptions.NoInlining)]
    public static FastDecimal64<T> MultiplyChecked(FastDecimal64<T> left, FastDecimal64<T> right, MidpointRounding mode)
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

    //
    // INumberBase
    //
    
    /// <inheritdoc cref="INumberBase{TSelf}.One" />
    public static FastDecimal64<T> One => new((long) GetDivisor());
    
    /// <inheritdoc cref="INumberBase{TSelf}.Radix" />
    static int INumberBase<FastDecimal64<T>>.Radix => 10;

    /// <inheritdoc cref="INumberBase{TSelf}.Zero" />
    public static FastDecimal64<T> Zero => default;


    /// <inheritdoc cref="INumberBase{TSelf}.Abs(TSelf)" />
    public static FastDecimal64<T> Abs(FastDecimal64<T> value)
    {
        return new FastDecimal64<T>(long.Abs(value._value));
    }
    
    /// <inheritdoc cref="INumberBase{TSelf}.IsCanonical(TSelf)" />
    static bool INumberBase<FastDecimal64<T>>.IsCanonical(FastDecimal64<T> value) => true;

    /// <inheritdoc cref="INumberBase{TSelf}.IsComplexNumber(TSelf)" />
    static bool INumberBase<FastDecimal64<T>>.IsComplexNumber(FastDecimal64<T> value) => false;

    /// <inheritdoc cref="INumberBase{TSelf}.IsEvenInteger(TSelf)" />
    public static bool IsEvenInteger(FastDecimal64<T> value)
    {
        return value._value % (2 * (long) GetDivisor()) == 0;
    }

    /// <inheritdoc cref="INumberBase{TSelf}.IsFinite(TSelf)" />
    static bool INumberBase<FastDecimal64<T>>.IsFinite(FastDecimal64<T> value) => true;

    /// <inheritdoc cref="INumberBase{TSelf}.IsImaginaryNumber(TSelf)" />
    static bool INumberBase<FastDecimal64<T>>.IsImaginaryNumber(FastDecimal64<T> value) => false;

    /// <inheritdoc cref="INumberBase{TSelf}.IsInfinity(TSelf)" />
    static bool INumberBase<FastDecimal64<T>>.IsInfinity(FastDecimal64<T> value) => false;

    /// <inheritdoc cref="INumberBase{TSelf}.IsInteger(TSelf)" />
    public static bool IsInteger(FastDecimal64<T> value)
    {
        var divisor = GetDivisor();
        return value._value % (long) divisor == 0;
    }

    /// <inheritdoc cref="INumberBase{TSelf}.IsNaN(TSelf)" />
    static bool INumberBase<FastDecimal64<T>>.IsNaN(FastDecimal64<T> value) => false;

    /// <inheritdoc cref="INumberBase{TSelf}.IsNegative(TSelf)" />
    public static bool IsNegative(FastDecimal64<T> value) => long.IsNegative(value._value);

    /// <inheritdoc cref="INumberBase{TSelf}.IsNegativeInfinity(TSelf)" />
    static bool INumberBase<FastDecimal64<T>>.IsNegativeInfinity(FastDecimal64<T> value) => false;

    /// <inheritdoc cref="INumberBase{TSelf}.IsNormal(TSelf)" />
    public static bool IsNormal(FastDecimal64<T> value)
    {
        return value._value != 0;
    }

    /// <inheritdoc cref="INumberBase{TSelf}.IsOddInteger(TSelf)" />
    public static bool IsOddInteger(FastDecimal64<T> value)
    {
        var (q, r) = Math.DivRem(value._value, (long) GetDivisor());
        return r == 0 && (q & 1) != 0;
    }
    
    /// <inheritdoc cref="INumberBase{TSelf}.IsPositive(TSelf)" />
    public static bool IsPositive(FastDecimal64<T> value) => long.IsPositive(value._value);

    /// <inheritdoc cref="INumberBase{TSelf}.IsPositiveInfinity(TSelf)" />
    static bool INumberBase<FastDecimal64<T>>.IsPositiveInfinity(FastDecimal64<T> value) => false;

    /// <inheritdoc cref="INumberBase{TSelf}.IsRealNumber(TSelf)" />
    static bool INumberBase<FastDecimal64<T>>.IsRealNumber(FastDecimal64<T> value) => true;

    /// <inheritdoc cref="INumberBase{TSelf}.IsSubnormal(TSelf)" />
    static bool INumberBase<FastDecimal64<T>>.IsSubnormal(FastDecimal64<T> value) => false;

    /// <inheritdoc cref="INumberBase{TSelf}.IsZero(TSelf)" />
    public static bool IsZero(FastDecimal64<T> value) => value._value == 0;

    /// <inheritdoc cref="INumberBase{TSelf}.MaxMagnitude(TSelf, TSelf)" />
    public static FastDecimal64<T> MaxMagnitude(FastDecimal64<T> x, FastDecimal64<T> y)
    {
        return new FastDecimal64<T>(long.MaxMagnitude(x._value, y._value));
    }

    /// <inheritdoc cref="INumberBase{TSelf}.MaxMagnitudeNumber(TSelf, TSelf)" />
    static FastDecimal64<T> INumberBase<FastDecimal64<T>>.MaxMagnitudeNumber(FastDecimal64<T> x, FastDecimal64<T> y) 
        => MaxMagnitude(x, y);

    /// <inheritdoc cref="INumberBase{TSelf}.MinMagnitude(TSelf, TSelf)" />
    public static FastDecimal64<T> MinMagnitude(FastDecimal64<T> x, FastDecimal64<T> y)
    {
        return new FastDecimal64<T>(long.MinMagnitude(x._value, y._value));
    }

    /// <inheritdoc cref="INumberBase{TSelf}.MinMagnitudeNumber(TSelf, TSelf)" />
    static FastDecimal64<T> INumberBase<FastDecimal64<T>>.MinMagnitudeNumber(FastDecimal64<T> x, FastDecimal64<T> y) 
        => MinMagnitude(x, y);
    
    /// <inheritdoc cref="INumberBase{TSelf}.TryConvertFromChecked{TOther}(TOther, out TSelf)" />
    public static bool TryConvertFromChecked<TOther>(TOther value, out FastDecimal64<T> result) where TOther : INumberBase<TOther>
    {
        result = default;
        return false;
    }

    /// <inheritdoc cref="INumberBase{TSelf}.TryConvertFromSaturating{TOther}(TOther, out TSelf)" />
    public static bool TryConvertFromSaturating<TOther>(TOther value, out FastDecimal64<T> result) where TOther : INumberBase<TOther>
    {
        result = default;
        return false;
    }

    /// <inheritdoc cref="INumberBase{TSelf}.TryConvertFromTruncating{TOther}(TOther, out TSelf)" />
    public static bool TryConvertFromTruncating<TOther>(TOther value, out FastDecimal64<T> result) where TOther : INumberBase<TOther>
    {
        result = default;
        return false;
    }

    /// <inheritdoc cref="INumberBase{TSelf}.TryConvertToChecked{TOther}(TSelf, out TOther?)" />
    public static bool TryConvertToChecked<TOther>(FastDecimal64<T> value, out TOther? result) where TOther : INumberBase<TOther>
    {
        result = default;
        return false;
    }

    /// <inheritdoc cref="INumberBase{TSelf}.TryConvertToSaturating{TOther}(TSelf, out TOther?)" />
    public static bool TryConvertToSaturating<TOther>(FastDecimal64<T> value, out TOther? result) where TOther : INumberBase<TOther>
    {
        result = default;
        return false;
    }

    /// <inheritdoc cref="INumberBase{TSelf}.TryConvertToTruncating{TOther}(TSelf, out TOther?)" />
    public static bool TryConvertToTruncating<TOther>(FastDecimal64<T> value, out TOther? result) where TOther : INumberBase<TOther>
    {
        result = default;
        return false;
    }

    //
    // ISignedNumber
    //

    /// <inheritdoc cref="ISignedNumber{TSelf}.NegativeOne" />
    public static FastDecimal64<T> NegativeOne => -One;

    //
    // ISubtractionOperators
    //

    /// <inheritdoc cref="ISubtractionOperators{TSelf, TOther, TResult}.op_Subtraction(TSelf, TOther)" />
    public static FastDecimal64<T> operator -(FastDecimal64<T> left, FastDecimal64<T> right)
    {
        return new FastDecimal64<T>(left._value - right._value);
    }

    /// <inheritdoc cref="ISubtractionOperators{TSelf, TOther, TResult}.op_CheckedSubtraction(TSelf, TOther)" />
    public static FastDecimal64<T> operator checked -(FastDecimal64<T> left, FastDecimal64<T> right)
    {
        return new FastDecimal64<T>(checked(left._value - right._value));
    }

    //
    // IUnaryNegationOperators
    //

    /// <inheritdoc cref="IUnaryNegationOperators{TSelf, TResult}.op_UnaryNegation(TSelf)" />
    public static FastDecimal64<T> operator -(FastDecimal64<T> value) => new FastDecimal64<T>(-value._value);

    /// <inheritdoc cref="IUnaryNegationOperators{TSelf, TResult}.op_CheckedUnaryNegation(TSelf)" /> 
    public static FastDecimal64<T> operator checked -(FastDecimal64<T> value) => new FastDecimal64<T>(checked(-value._value));

    //
    // IUnaryPlusOperators
    //

    /// <inheritdoc cref="IUnaryPlusOperators{TSelf, TResult}.op_UnaryPlus(TSelf)" />
    public static FastDecimal64<T> operator +(FastDecimal64<T> value) => value;

    /// <summary>Converts the current <see cref="FastDecimal64{T}" /> to a <see cref="FastDecimal64{TResult}" /> of a different precision, throwing an overflow exception for any values that fall outside the representable range.</summary>
    /// <param name="mode">One of the enumeration values that specifies which rounding strategy to use.</param>
    /// <returns>The current <see cref="FastDecimal64{T}" /> converted to a <see cref="FastDecimal64{TResult}" />.</returns>
    /// <exception cref="ArgumentException"><paramref name="mode" /> is not a MidpointRounding value.</exception>
    /// <exception cref="OverflowException">The current <see cref="FastDecimal64{T}" /> is not representable by <see cref="FastDecimal64{TResult}" />.</exception>
    public FastDecimal64<TResult> ChangePrecision<TResult>(MidpointRounding mode = MidpointRounding.ToEven) where TResult : struct, IFractionalDigits
    {
        var dstDigits = FastDecimal64<TResult>.GetFractionalDigits();
        var srcDigits = GetFractionalDigits();
        
        if (dstDigits >= srcDigits)
            return new FastDecimal64<TResult>(checked(_value * (long) Fast128BitDiv.GetDivisorLow(dstDigits - srcDigits)));

        ConvertToUnsigned(this, out var unsignedValue, out var negative);

        var (q, r) = Math.DivRem(unsignedValue, Fast128BitDiv.GetDivisorLow(srcDigits - dstDigits));
        q += Rounding.Round64(q, r, Fast128BitDiv.GetDivisorLow(srcDigits - dstDigits), negative, mode);

        return new FastDecimal64<TResult>(negative ? -(long) q : (long) q);
    }

    /// <summary>Tries to cast a <see cref="decimal" /> to a <see cref="FastDecimal64{T}" /> value.</summary>
    /// <param name="value">The value to convert.</param>
    /// <param name="fd"><paramref name="value" /> converted to a <see cref="FastDecimal64{T}" /> if <paramref name="value" /> is representable by <see cref="FastDecimal64{T}" />; otherwise, the default value.</param>
    /// <returns>true if <paramref name="value" /> is representable by <see cref="FastDecimal64{T}" />; otherwise, false.</returns>
    public static bool TryCastDecimal(decimal value, out FastDecimal64<T> fd)
    {
        var dec = Unsafe.As<decimal, DecimalStruct>(ref value);
        var scale = dec.Scale;
        var fractionalDigits = GetFractionalDigits();

        if (scale == fractionalDigits)
        {
            if (dec.High32 == 0 && TryCast(dec.Low64, dec.Negative, out var signed))
            {
                fd = new FastDecimal64<T>(signed);
                return true;
            }
        } 
        else if (scale < fractionalDigits && dec.High32 == 0)
        {
            var high = Math.BigMul(dec.Low64, Fast128BitDiv.GetDivisorLow(fractionalDigits - scale), out var low);
            if (high == 0 && TryCast(low, dec.Negative, out var signed))
            {
                fd = new FastDecimal64<T>(signed);
                return true;
            }
        } 
        else if (scale > fractionalDigits)
        {
            var (q,r) = Fast128BitDiv.DecDivRem128By128(dec.High32, dec.Low64, scale - fractionalDigits);
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

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
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

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
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

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static ulong GetDivisor() => Fast128BitDiv.GetDivisorLow(GetFractionalDigits());

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static int GetFractionalDigits() => new T().FractionalDigits;
}
