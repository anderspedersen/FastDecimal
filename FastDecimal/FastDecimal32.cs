using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Numerics;
using System.Runtime.CompilerServices;
using FastDecimal.FractionalDigits;

namespace FastDecimal;

/// <summary>Represents a 32-bit signed fixed-point decimal number.</summary>
public readonly struct FastDecimal32<T> : 
    INumber<FastDecimal32<T>>,
    ISignedNumber<FastDecimal32<T>>,
    IMinMaxValue<FastDecimal32<T>>
    where T : struct, IFractionalDigits 
{
    private readonly int _value;

    /// <summary>Initializes a new instance o new instance of the <see cref="FastDecimal32{T}" /> struct using the supplied <see cref="decimal" /> value.</summary>
    /// <param name="value">The <see cref="decimal" /> to store as a <see cref="FastDecimal32{T}" /> struct.</param>
    /// <exception cref="OverflowException"><paramref name="value" /> is not representable by <see cref="FastDecimal32{T}" />.</exception>
    public FastDecimal32(decimal value)
    {
        this = checked((FastDecimal32<T>) value);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal FastDecimal32(int value)
    {
        if (GetFractionalDigits() is > 9 or < 0)
            throw new InvalidOperationException("FastDecimal32 only supports between 0 and 9 fractional digits.");
        _value = value;
    }

    /// <inheritdoc cref="IComparable.CompareTo(object)" />
    public int CompareTo(object? value)
    {
        if (value == null)
            return 1;
        if (!(value is FastDecimal32<T>))
            throw new ArgumentException($"Object must be of type FastDecimal32<{typeof(T).Name}>.");

        var other = (FastDecimal32<T>)value;
        
        return CompareTo(other);
    }

    /// <inheritdoc cref="IComparable{T}.CompareTo(T)" />
    public int CompareTo(FastDecimal32<T> other)
    {
        return _value.CompareTo(other._value);
    }

    /// <inheritdoc cref="object.Equals(object?)" />
    public override bool Equals(object? obj)
    {
        return obj is FastDecimal32<T> other && Equals(other);
    }

    /// <inheritdoc cref="IEquatable{T}.Equals(T)" />
    public bool Equals(FastDecimal32<T> other)
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
    public static FastDecimal32<T> Parse(string s, IFormatProvider? provider)
    {
        return (FastDecimal32<T>) decimal.Parse(s, provider);
    }


    /// <inheritdoc cref="INumberBase{TSelf}.Parse(string, NumberStyles, IFormatProvider?)" />
    public static FastDecimal32<T> Parse(string s, NumberStyles style, IFormatProvider? provider)
    {
        var dec = decimal.Parse(s, style, provider);
        if (TryCastDecimal(dec, out var o))
            return o;
        
        throw new OverflowException();
    }

    /// <inheritdoc cref="ISpanParsable{TSelf}.Parse(ReadOnlySpan&lt;char&gt;, IFormatProvider?)" />
    public static FastDecimal32<T> Parse(ReadOnlySpan<char> s, IFormatProvider? provider)
    {
        return (FastDecimal32<T>) decimal.Parse(s, provider);
    }

    /// <inheritdoc cref="INumberBase{TSelf}.Parse(ReadOnlySpan&lt;char&gt;, NumberStyles, IFormatProvider?)" />
    public static FastDecimal32<T> Parse(ReadOnlySpan<char> s, NumberStyles style, IFormatProvider? provider)
    {
        var dec = decimal.Parse(s, style, provider);
        if (TryCastDecimal(dec, out var o))
            return o;
        
        throw new OverflowException();
    }

    /// <inheritdoc cref="IParsable{TSelf}.TryParse(string?, IFormatProvider?, out TSelf)" />
    public static bool TryParse([NotNullWhen(true)] string? s, IFormatProvider? provider, out FastDecimal32<T> result)
    {
        if(decimal.TryParse(s, provider, out var dec) && TryCastDecimal(dec, out result))
        {
            return true;
        }
        result = default;
        return false;
    }

    /// <inheritdoc cref="ISpanParsable{TSelf}.TryParse(ReadOnlySpan&lt;char&gt;, IFormatProvider?, out TSelf)" />
    public static bool TryParse(ReadOnlySpan<char> s, IFormatProvider? provider, out FastDecimal32<T> result)
    {
        if (decimal.TryParse(s, provider, out var dec) && TryCastDecimal(dec, out result))
        {
            return true;
        }
        result = default;
        return false;
    }

    /// <inheritdoc cref="INumberBase{TSelf}.TryParse(string?, NumberStyles, IFormatProvider?, out TSelf)" />
    public static bool TryParse([NotNullWhen(true)] string? s, NumberStyles style, IFormatProvider? provider, out FastDecimal32<T> result)
    {
        result = default;
        return (decimal.TryParse(s, style, provider, out var dec) && TryCastDecimal(dec, out result)) ;
        
    }

    /// <inheritdoc cref="INumberBase{TSelf}.TryParse(ReadOnlySpan&lt;char&gt;, NumberStyles, IFormatProvider?, out TSelf)" />
    public static bool TryParse(ReadOnlySpan<char> s, NumberStyles style, IFormatProvider? provider, out FastDecimal32<T> result)
    {
        var success = decimal.TryParse(s, style, provider, out var dec);
        result = (FastDecimal32<T>) dec;
        return success;
    }

    //
    // Explicit Conversions From FastDecimal32
    //

    /// <summary>Explicitly converts a <see cref="FastDecimal32{T}" /> to a <see cref="decimal" /> value.</summary>
    /// <param name="value">The value to convert.</param>
    /// <returns><paramref name="value" /> converted to a <see cref="decimal" />.</returns>
    public static explicit operator decimal(FastDecimal32<T> value)
    {
        var scale = GetFractionalDigits();
        ConvertToUnsigned(value, out var unsignedValue, out var negative);

        return new decimal((int) unsignedValue, 0, 0, negative, (byte) scale);
    }

    //
    // Explicit Conversions To FastDecimal32
    //

    /// <summary>Explicitly converts a <see cref="long" /> to a <see cref="FastDecimal32{T}" /> value.</summary>
    /// <param name="value">The value to convert.</param>
    /// <returns><paramref name="value" /> converted to a <see cref="FastDecimal32{T}" />.</returns>
    public static explicit operator FastDecimal32<T>(long value)
    {
        return new FastDecimal32<T>((int) (value * (long) GetDivisor()));
    }

    /// <summary>Explicitly converts a <see cref="long" /> to a <see cref="FastDecimal32{T}" /> value, throwing an overflow exception for any values that fall outside the representable range.</summary>
    /// <param name="value">The value to convert.</param>
    /// <returns><paramref name="value" /> converted to a <see cref="FastDecimal32{T}" />.</returns>
    /// <exception cref="OverflowException"><paramref name="value" /> is not representable by <see cref="FastDecimal32{T}" />.</exception>
    public static explicit operator checked FastDecimal32<T>(long value)
    {
        return new FastDecimal32<T>(checked((int) (value * unchecked((long) GetDivisor()))));
    }

    /// <summary>Explicitly converts a <see cref="decimal" /> to a <see cref="FastDecimal32{T}" /> value.</summary>
    /// <param name="value">The value to convert.</param>
    /// <returns><paramref name="value" /> converted to a <see cref="FastDecimal32{T}" />.</returns>
    public static explicit operator FastDecimal32<T>(decimal value)
    {
        var dec = Unsafe.As<decimal, DecimalStruct>(ref value);
        var scale = dec.Scale;
        var fractionalDigits = GetFractionalDigits();

        if (scale == fractionalDigits)
        {
            return new FastDecimal32<T>(dec.Negative ? -(int) dec.Low64 : (int) dec.Low64);
        } 
        
        if (scale < fractionalDigits)
        {
            var high = Math.BigMul(dec.Low64, Fast128BitDiv.GetDivisorLow(fractionalDigits - scale), out var low);
            return new FastDecimal32<T>(dec.Negative ? -(int) low : (int) low);
        }
        
        var (q,r) = Fast128BitDiv.DecDivRem128By128(dec.High32, dec.Low64, scale - fractionalDigits);
        q += new UInt128(0,Rounding.RoundDec(q.Lower, r, scale - fractionalDigits, dec.Negative, MidpointRounding.ToEven));
        return new FastDecimal32<T>(dec.Negative ? -(int) q.Lower : (int) q.Lower);
    }

    /// <summary>Explicitly converts a <see cref="decimal" /> to a <see cref="FastDecimal32{T}" /> value, throwing an overflow exception for any values that fall outside the representable range.</summary>
    /// <param name="value">The value to convert.</param>
    /// <returns><paramref name="value" /> converted to a <see cref="FastDecimal32{T}" />.</returns>
    /// <exception cref="OverflowException"><paramref name="value" /> is not representable by <see cref="FastDecimal32{T}" />.</exception>
    public static explicit operator checked FastDecimal32<T>(decimal value)
    {
        if (TryCastDecimal(value, out var fd))
        {
            return fd;
        }

        throw new OverflowException();
    }
    
    //
    // Implicit Conversions From FastDecimal32
    //
    
    /// <summary>Implicitly converts a <see cref="FastDecimal32{T}" /> to a <see cref="FastDecimal64{T}" /> value.</summary>
    /// <param name="value">The value to convert.</param>
    /// <returns><paramref name="value" /> converted to a <see cref="FastDecimal64{T}" />.</returns>
    public static implicit operator FastDecimal64<T>(FastDecimal32<T> value)
    {
        return new FastDecimal64<T>(value._value);
    }

    //
    // IAdditionOperators
    //

    /// <inheritdoc cref="IAdditionOperators{TSelf, TOther, TResult}.op_Addition(TSelf, TOther)" />
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static FastDecimal32<T> operator +(FastDecimal32<T> left, FastDecimal32<T> right)
    {
        return new FastDecimal32<T>(left._value + right._value);
    }

    /// <inheritdoc cref="IAdditionOperators{TSelf, TOther, TResult}.op_CheckedAddition(TSelf, TOther)" />
    public static FastDecimal32<T> operator checked +(FastDecimal32<T> left, FastDecimal32<T> right)
    {
        return new FastDecimal32<T>(checked(left._value + right._value));
    }

    //
    // IAdditiveIdentity
    //

    /// <inheritdoc cref="IAdditiveIdentity{TSelf, TResult}.AdditiveIdentity" />
    public static FastDecimal32<T> AdditiveIdentity { get; } = default;
    
    //
    // IComparisonOperators
    //

    /// <inheritdoc cref="IComparisonOperators{TSelf, TOther, TResult}.op_GreaterThan(TSelf, TOther)" />
    public static bool operator >(FastDecimal32<T> left, FastDecimal32<T> right) => left._value > right._value;

    /// <inheritdoc cref="IComparisonOperators{TSelf, TOther, TResult}.op_GreaterThanOrEqual(TSelf, TOther)" />
    public static bool operator >=(FastDecimal32<T> left, FastDecimal32<T> right) => left._value >= right._value;

    /// <inheritdoc cref="IComparisonOperators{TSelf, TOther, TResult}.op_LessThan(TSelf, TOther)" />
    public static bool operator <(FastDecimal32<T> left, FastDecimal32<T> right) => left._value < right._value;

    /// <inheritdoc cref="IComparisonOperators{TSelf, TOther, TResult}.op_LessThanOrEqual(TSelf, TOther)" />
    public static bool operator <=(FastDecimal32<T> left, FastDecimal32<T> right) => left._value <= right._value;

    //
    // IDecrementOperators
    //

    /// <inheritdoc cref="IDecrementOperators{TSelf}.op_Decrement(TSelf)" />
    public static FastDecimal32<T> operator --(FastDecimal32<T> value)
    {
        var div = GetDivisor();
        return new FastDecimal32<T>(value._value - (int) div);
    }

    /// <inheritdoc cref="IDecrementOperators{TSelf}.op_CheckedDecrement(TSelf)" />
    public static FastDecimal32<T> operator checked --(FastDecimal32<T> value)
    {
        var div = GetDivisor();
        return new FastDecimal32<T>(checked(value._value - unchecked((int) div)));
    }

    //
    // IDivisionOperators
    //

    /// <inheritdoc cref="IDivisionOperators{TSelf, TOther, TResult}.op_Division(TSelf, TOther)" />
    [MethodImpl(MethodImplOptions.NoInlining)]
    public static FastDecimal32<T> operator /(FastDecimal32<T> left, FastDecimal32<T> right)
    {
        return DivideInternal(left, right, MidpointRounding.ToEven, false);
    }

    /// <inheritdoc cref="IDivisionOperators{TSelf, TOther, TResult}.op_CheckedDivision(TSelf, TOther)" />
    [MethodImpl(MethodImplOptions.NoInlining)]
    public static FastDecimal32<T> operator checked /(FastDecimal32<T> left, FastDecimal32<T> right)
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
    public static FastDecimal32<T> Divide(FastDecimal32<T> left, FastDecimal32<T> right, MidpointRounding mode)
    {
        return DivideInternal(left, right, mode, false);
    }

    /// <summary>Divides two values together to compute their quotient.</summary>
    /// <param name="left">The value which <paramref name="right" /> divides.</param>
    /// <param name="right">The value which divides <paramref name="left" />.</param>
    /// <param name="mode">One of the enumeration values that specifies which rounding strategy to use.</param>;
    /// <returns>The quotient of <paramref name="left" /> divided-by <paramref name="right" />.</returns>
    /// <exception cref="ArgumentException"><paramref name="mode" /> is not a MidpointRounding value.</exception>
    /// <exception cref="OverflowException">The quotient of <paramref name="left" /> divided-by <paramref name="right" /> is not representable by <href name="FastDecimal32{T}" />.</exception>
    [MethodImpl(MethodImplOptions.NoInlining)]
    public static FastDecimal32<T> DivideChecked(FastDecimal32<T> left, FastDecimal32<T> right, MidpointRounding mode)
    {
        return DivideInternal(left, right, mode, true);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static FastDecimal32<T> DivideInternal(FastDecimal32<T> left, FastDecimal32<T> right, MidpointRounding mode, bool isChecked)
    {
        ConvertToUnsigned(left, right, out var dividend, out var divisor, out var negative);

        var precisionDivisor = GetDivisor();

        var scaledDividend = dividend * precisionDivisor;

        var (q, r) = Math.DivRem(scaledDividend, divisor);
        q += Rounding.Round64(q, r, divisor, negative, mode);

        if (isChecked)
        {
            if (TryCast(checked((uint) q), negative, out var signed))
            {
                return new FastDecimal32<T>(signed);
            }
            throw new OverflowException();
        }
        else
        {
            return new FastDecimal32<T>(negative ? -(int) q : (int) q);
        }
    }

    //
    // IEqualityOperators
    //

    /// <inheritdoc cref="IEqualityOperators{TSelf, TOther, TResult}.op_Equality(TSelf, TOther)" />
    public static bool operator ==(FastDecimal32<T> left, FastDecimal32<T> right) => left._value == right._value;

    /// <inheritdoc cref="IEqualityOperators{TSelf, TOther, TResult}.op_Inequality(TSelf, TOther)" />
    public static bool operator !=(FastDecimal32<T> left, FastDecimal32<T> right) => left._value != right._value;
    
    //
    // IIncrementOperators
    //

    /// <inheritdoc cref="IIncrementOperators{TSelf}.op_Increment(TSelf)" />
    public static FastDecimal32<T> operator ++(FastDecimal32<T> value)
    {
        var div = GetDivisor();
        return new FastDecimal32<T>(value._value + (int) div);
    }

    /// <inheritdoc cref="IIncrementOperators{TSelf}.op_CheckedIncrement(TSelf)" />
    public static FastDecimal32<T> operator checked ++(FastDecimal32<T> value)
    {
        var div = GetDivisor();
        return new FastDecimal32<T>(checked(value._value + unchecked((int) div)));
    }

    //
    // IMinMaxValue
    //

    /// <inheritdoc cref="IMinMaxValue{TSelf}.MaxValue" />
    public static FastDecimal32<T> MaxValue => new FastDecimal32<T>(int.MaxValue);

    /// <inheritdoc cref="IMinMaxValue{TSelf}.MinValue" />
    public static FastDecimal32<T> MinValue => new FastDecimal32<T>(int.MinValue);

    //
    // IModulusOperators
    //

    /// <inheritdoc cref="IModulusOperators{TSelf, TOther, TResult}.op_Modulus(TSelf, TOther)" />
    public static FastDecimal32<T> operator %(FastDecimal32<T> left, FastDecimal32<T> right)
    {
        return new FastDecimal32<T>(left._value % right._value);
    }

    //
    // IMultiplicativeIdentity
    //

    /// <inheritdoc cref="IMultiplicativeIdentity{TSelf, TResult}.MultiplicativeIdentity" />
    public static FastDecimal32<T> MultiplicativeIdentity => new FastDecimal32<T>((int) GetDivisor());
    
    //
    // IMultiplyOperators
    //

    /// <inheritdoc cref="IMultiplyOperators{TSelf, TOther, TResult}.op_Multiply(TSelf, TOther)" />
    public static FastDecimal32<T> operator *(FastDecimal32<T> left, FastDecimal32<T> right)
    {
        return MultiplyInternal(left, right, MidpointRounding.ToEven, isChecked: false);
    }

    /// <inheritdoc cref="IMultiplyOperators{TSelf, TOther, TResult}.op_CheckedMultiply(TSelf, TOther)" />
    public static FastDecimal32<T> operator checked *(FastDecimal32<T> left, FastDecimal32<T> right)
    {
        return MultiplyInternal(left, right, MidpointRounding.ToEven, isChecked: true);
    }
    
    /// <summary>Multiplies two values together to compute their product.</summary>
    /// <param name="left">The value which <paramref name="right" /> multiplies.</param>
    /// <param name="right">The value which multiplies <paramref name="left" />.</param>
    /// <returns>The product of <paramref name="left" /> multiplied-by <paramref name="right" />.</returns>
    public static FastDecimal32<T> operator *(FastDecimal32<T> left, int right)
    {
        return new FastDecimal32<T>(left._value * right);
    }
    
    /// <summary>Multiplies two values together to compute their product.</summary>
    /// <param name="left">The value which <paramref name="right" /> multiplies.</param>
    /// <param name="right">The value which multiplies <paramref name="left" />.</param>
    /// <returns>The product of <paramref name="left" /> multiplied-by <paramref name="right" />.</returns>
    /// <exception cref="OverflowException">The product of <paramref name="left" /> multiplied-by <paramref name="right" /> is not representable by <typeparamref name="TResult" />.</exception>
    public static FastDecimal32<T> operator checked *(FastDecimal32<T> left, int right)
    {
        return new FastDecimal32<T>(checked(left._value * right));
    }
    
    /// <summary>Multiplies two values together to compute their product.</summary>
    /// <param name="left">The value which <paramref name="right" /> multiplies.</param>
    /// <param name="right">The value which multiplies <paramref name="left" />.</param>
    /// <returns>The product of <paramref name="left" /> multiplied-by <paramref name="right" />.</returns>
    public static FastDecimal32<T> operator *(int left, FastDecimal32<T> right)
    {
        return new FastDecimal32<T>(left * right._value);
    }
    
    /// <summary>Multiplies two values together to compute their product.</summary>
    /// <param name="left">The value which <paramref name="right" /> multiplies.</param>
    /// <param name="right">The value which multiplies <paramref name="left" />.</param>
    /// <returns>The product of <paramref name="left" /> multiplied-by <paramref name="right" />.</returns>
    /// <exception cref="OverflowException">The product of <paramref name="left" /> multiplied-by <paramref name="right" /> is not representable by <typeparamref name="TResult" />.</exception>
    public static FastDecimal32<T> operator checked *(int left, FastDecimal32<T> right)
    {
        return new FastDecimal32<T>(checked(left * right._value));
    }

    /// <summary>Multiplies two values together to compute their product.</summary>
    /// <param name="left">The value which <paramref name="right" /> multiplies.</param>
    /// <param name="right">The value which multiplies <paramref name="left" />.</param>
    /// <param name="mode">One of the enumeration values that specifies which rounding strategy to use.</param>
    /// <returns>The product of <paramref name="left" /> multiplied-by <paramref name="right" />.</returns>
    /// <exception cref="ArgumentException"><paramref name="mode" /> is not a MidpointRounding value.</exception>
    public static FastDecimal32<T> Multiply(FastDecimal32<T> left, FastDecimal32<T> right, MidpointRounding mode)
    {
        return MultiplyInternal(left, right, mode, false);
    }

    /// <summary>Multiplies two values together to compute their product.</summary>
    /// <param name="left">The value which <paramref name="right" /> multiplies.</param>
    /// <param name="right">The value which multiplies <paramref name="left" />.</param>
    /// <param name="mode">One of the enumeration values that specifies which rounding strategy to use.</param>
    /// <returns>The product of <paramref name="left" /> multiplied-by <paramref name="right" />.</returns>
    /// <exception cref="ArgumentException"><paramref name="mode" /> is not a MidpointRounding value.</exception>
    /// <exception cref="OverflowException">The product of <paramref name="left" /> multiplied-by <paramref name="right" /> is not representable by <seef cref="FastDecimal32{T}" />.</exception>
    public static FastDecimal32<T> MultiplyChecked(FastDecimal32<T> left, FastDecimal32<T> right, MidpointRounding mode)
    {
        return MultiplyInternal(left, right, mode, true);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static FastDecimal32<T> MultiplyInternal(FastDecimal32<T> left, FastDecimal32<T> right, MidpointRounding mode, bool isChecked)
    {
        ConvertToUnsigned(left, right, out var leftUnsigned, out var rightUnsigned, out var negative);

        var divisor = GetDivisor();

        var product = (ulong) leftUnsigned * rightUnsigned;

        var (q, r) = Math.DivRem(product, divisor);
        
        q += Rounding.Round64(q, r, divisor, negative, mode);
        
        if (isChecked)
        {
            if (TryCast(checked((uint) q), negative, out var signed))
            {
                return new FastDecimal32<T>(signed);
            }
            throw new OverflowException();
        }
        else
        {
            return new FastDecimal32<T>(negative ?  - (int) q : (int) q);
        }
    }

    //
    // INumberBase
    //
    
    /// <inheritdoc cref="INumberBase{TSelf}.One" />
    public static FastDecimal32<T> One => new((int) GetDivisor());
    
    /// <inheritdoc cref="INumberBase{TSelf}.Radix" />
    static int INumberBase<FastDecimal32<T>>.Radix => 10;

    /// <inheritdoc cref="INumberBase{TSelf}.Zero" />
    public static FastDecimal32<T> Zero => default;


    /// <inheritdoc cref="INumberBase{TSelf}.Abs(TSelf)" />
    public static FastDecimal32<T> Abs(FastDecimal32<T> value)
    {
        return new FastDecimal32<T>(int.Abs(value._value));
    }
    
    /// <inheritdoc cref="INumberBase{TSelf}.IsCanonical(TSelf)" />
    static bool INumberBase<FastDecimal32<T>>.IsCanonical(FastDecimal32<T> value) => true;

    /// <inheritdoc cref="INumberBase{TSelf}.IsComplexNumber(TSelf)" />
    static bool INumberBase<FastDecimal32<T>>.IsComplexNumber(FastDecimal32<T> value) => false;

    /// <inheritdoc cref="INumberBase{TSelf}.IsEvenInteger(TSelf)" />
    public static bool IsEvenInteger(FastDecimal32<T> value)
    {
        return value._value % (2 * (long) GetDivisor()) == 0;
    }

    /// <inheritdoc cref="INumberBase{TSelf}.IsFinite(TSelf)" />
    static bool INumberBase<FastDecimal32<T>>.IsFinite(FastDecimal32<T> value) => true;

    /// <inheritdoc cref="INumberBase{TSelf}.IsImaginaryNumber(TSelf)" />
    static bool INumberBase<FastDecimal32<T>>.IsImaginaryNumber(FastDecimal32<T> value) => false;

    /// <inheritdoc cref="INumberBase{TSelf}.IsInfinity(TSelf)" />
    static bool INumberBase<FastDecimal32<T>>.IsInfinity(FastDecimal32<T> value) => false;

    /// <inheritdoc cref="INumberBase{TSelf}.IsInteger(TSelf)" />
    public static bool IsInteger(FastDecimal32<T> value)
    {
        var divisor = GetDivisor();
        return value._value % (long) divisor == 0;
    }

    /// <inheritdoc cref="INumberBase{TSelf}.IsNaN(TSelf)" />
    static bool INumberBase<FastDecimal32<T>>.IsNaN(FastDecimal32<T> value) => false;

    /// <inheritdoc cref="INumberBase{TSelf}.IsNegative(TSelf)" />
    public static bool IsNegative(FastDecimal32<T> value) => long.IsNegative(value._value);

    /// <inheritdoc cref="INumberBase{TSelf}.IsNegativeInfinity(TSelf)" />
    static bool INumberBase<FastDecimal32<T>>.IsNegativeInfinity(FastDecimal32<T> value) => false;

    /// <inheritdoc cref="INumberBase{TSelf}.IsNormal(TSelf)" />
    public static bool IsNormal(FastDecimal32<T> value)
    {
        return value._value != 0;
    }

    /// <inheritdoc cref="INumberBase{TSelf}.IsOddInteger(TSelf)" />
    public static bool IsOddInteger(FastDecimal32<T> value)
    {
        var (q, r) = Math.DivRem(value._value, (int) GetDivisor());
        return r == 0 && (q & 1) != 0;
    }
    
    /// <inheritdoc cref="INumberBase{TSelf}.IsPositive(TSelf)" />
    public static bool IsPositive(FastDecimal32<T> value) => long.IsPositive(value._value);

    /// <inheritdoc cref="INumberBase{TSelf}.IsPositiveInfinity(TSelf)" />
    static bool INumberBase<FastDecimal32<T>>.IsPositiveInfinity(FastDecimal32<T> value) => false;

    /// <inheritdoc cref="INumberBase{TSelf}.IsRealNumber(TSelf)" />
    static bool INumberBase<FastDecimal32<T>>.IsRealNumber(FastDecimal32<T> value) => true;

    /// <inheritdoc cref="INumberBase{TSelf}.IsSubnormal(TSelf)" />
    static bool INumberBase<FastDecimal32<T>>.IsSubnormal(FastDecimal32<T> value) => false;

    /// <inheritdoc cref="INumberBase{TSelf}.IsZero(TSelf)" />
    public static bool IsZero(FastDecimal32<T> value) => value._value == 0;

    /// <inheritdoc cref="INumberBase{TSelf}.MaxMagnitude(TSelf, TSelf)" />
    public static FastDecimal32<T> MaxMagnitude(FastDecimal32<T> x, FastDecimal32<T> y)
    {
        return new FastDecimal32<T>(int.MaxMagnitude(x._value, y._value));
    }

    /// <inheritdoc cref="INumberBase{TSelf}.MaxMagnitudeNumber(TSelf, TSelf)" />
    static FastDecimal32<T> INumberBase<FastDecimal32<T>>.MaxMagnitudeNumber(FastDecimal32<T> x, FastDecimal32<T> y) 
        => MaxMagnitude(x, y);

    /// <inheritdoc cref="INumberBase{TSelf}.MinMagnitude(TSelf, TSelf)" />
    public static FastDecimal32<T> MinMagnitude(FastDecimal32<T> x, FastDecimal32<T> y)
    {
        return new FastDecimal32<T>(int.MinMagnitude(x._value, y._value));
    }

    /// <inheritdoc cref="INumberBase{TSelf}.MinMagnitudeNumber(TSelf, TSelf)" />
    static FastDecimal32<T> INumberBase<FastDecimal32<T>>.MinMagnitudeNumber(FastDecimal32<T> x, FastDecimal32<T> y) 
        => MinMagnitude(x, y);
    
    /// <inheritdoc cref="INumberBase{TSelf}.TryConvertFromChecked{TOther}(TOther, out TSelf)" />
    public static bool TryConvertFromChecked<TOther>(TOther value, out FastDecimal32<T> result) where TOther : INumberBase<TOther>
    {
        result = default;
        return false;
    }

    /// <inheritdoc cref="INumberBase{TSelf}.TryConvertFromSaturating{TOther}(TOther, out TSelf)" />
    public static bool TryConvertFromSaturating<TOther>(TOther value, out FastDecimal32<T> result) where TOther : INumberBase<TOther>
    {
        result = default;
        return false;
    }

    /// <inheritdoc cref="INumberBase{TSelf}.TryConvertFromTruncating{TOther}(TOther, out TSelf)" />
    public static bool TryConvertFromTruncating<TOther>(TOther value, out FastDecimal32<T> result) where TOther : INumberBase<TOther>
    {
        result = default;
        return false;
    }

    /// <inheritdoc cref="INumberBase{TSelf}.TryConvertToChecked{TOther}(TSelf, out TOther?)" />
    public static bool TryConvertToChecked<TOther>(FastDecimal32<T> value, out TOther? result) where TOther : INumberBase<TOther>
    {
        result = default;
        return false;
    }

    /// <inheritdoc cref="INumberBase{TSelf}.TryConvertToSaturating{TOther}(TSelf, out TOther?)" />
    public static bool TryConvertToSaturating<TOther>(FastDecimal32<T> value, out TOther? result) where TOther : INumberBase<TOther>
    {
        result = default;
        return false;
    }

    /// <inheritdoc cref="INumberBase{TSelf}.TryConvertToTruncating{TOther}(TSelf, out TOther?)" />
    public static bool TryConvertToTruncating<TOther>(FastDecimal32<T> value, out TOther? result) where TOther : INumberBase<TOther>
    {
        result = default;
        return false;
    }

    //
    // ISignedNumber
    //

    /// <inheritdoc cref="ISignedNumber{TSelf}.NegativeOne" />
    public static FastDecimal32<T> NegativeOne => -One;

    //
    // ISubtractionOperators
    //

    /// <inheritdoc cref="ISubtractionOperators{TSelf, TOther, TResult}.op_Subtraction(TSelf, TOther)" />
    public static FastDecimal32<T> operator -(FastDecimal32<T> left, FastDecimal32<T> right)
    {
        return new FastDecimal32<T>(left._value - right._value);
    }

    /// <inheritdoc cref="ISubtractionOperators{TSelf, TOther, TResult}.op_CheckedSubtraction(TSelf, TOther)" />
    public static FastDecimal32<T> operator checked -(FastDecimal32<T> left, FastDecimal32<T> right)
    {
        return new FastDecimal32<T>(checked(left._value - right._value));
    }

    //
    // IUnaryNegationOperators
    //

    /// <inheritdoc cref="IUnaryNegationOperators{TSelf, TResult}.op_UnaryNegation(TSelf)" />
    public static FastDecimal32<T> operator -(FastDecimal32<T> value) => new FastDecimal32<T>(-value._value);

    /// <inheritdoc cref="IUnaryNegationOperators{TSelf, TResult}.op_CheckedUnaryNegation(TSelf)" /> 
    public static FastDecimal32<T> operator checked -(FastDecimal32<T> value) => new FastDecimal32<T>(checked(-value._value));

    //
    // IUnaryPlusOperators
    //

    /// <inheritdoc cref="IUnaryPlusOperators{TSelf, TResult}.op_UnaryPlus(TSelf)" />
    public static FastDecimal32<T> operator +(FastDecimal32<T> value) => value;

    /// <summary>Converts the current <see cref="FastDecimal32{T}" /> to a <see cref="FastDecimal32{TResult}" /> of a different precision, throwing an overflow exception for any values that fall outside the representable range.</summary>
    /// <param name="mode">One of the enumeration values that specifies which rounding strategy to use.</param>
    /// <returns>The current <see cref="FastDecimal32{T}" /> converted to a <see cref="FastDecimal32{TResult}" />.</returns>
    /// <exception cref="ArgumentException"><paramref name="mode" /> is not a MidpointRounding value.</exception>
    /// <exception cref="OverflowException">The current <see cref="FastDecimal32{T}" /> is not representable by <see cref="FastDecimal32{TResult}" />.</exception>
    public FastDecimal32<TResult> ChangePrecision<TResult>(MidpointRounding mode = MidpointRounding.ToEven) where TResult : struct, IFractionalDigits
    {
        var dstDigits = FastDecimal32<TResult>.GetFractionalDigits();
        var srcDigits = GetFractionalDigits();
        
        if (dstDigits >= srcDigits)
            return new FastDecimal32<TResult>(checked(_value * (int) Fast128BitDiv.GetDivisorLow(dstDigits - srcDigits)));

        ConvertToUnsigned(this, out var unsignedValue, out var negative);

        var (q, r) = Math.DivRem(unsignedValue, Fast128BitDiv.GetDivisorLow(srcDigits - dstDigits));
        q += Rounding.Round64(q, r, Fast128BitDiv.GetDivisorLow(srcDigits - dstDigits), negative, mode);

        return new FastDecimal32<TResult>(negative ? -(int) q : (int) q);
    }

    /// <summary>Tries to cast a <see cref="decimal" /> to a <see cref="FastDecimal32{T}" /> value.</summary>
    /// <param name="value">The value to convert.</param>
    /// <param name="fd"><paramref name="value" /> converted to a <see cref="FastDecimal32{T}" /> if <paramref name="value" /> is representable by <see cref="FastDecimal32{T}" />; otherwise, the default value.</param>
    /// <returns>true if <paramref name="value" /> is representable by <see cref="FastDecimal32{T}" />; otherwise, false.</returns>
    public static bool TryCastDecimal(decimal value, out FastDecimal32<T> fd)
    {
        var dec = Unsafe.As<decimal, DecimalStruct>(ref value);
        var scale = dec.Scale;
        var fractionalDigits = GetFractionalDigits();

        if (scale == fractionalDigits)
        {
            if (dec.High64 == 0 && TryCast(dec.Low32, dec.Negative, out var signed))
            {
                fd = new FastDecimal32<T>(signed);
                return true;
            }
        } 
        else if (scale < fractionalDigits && dec.High32 == 0)
        {
            var product = dec.Low32 * Fast128BitDiv.GetDivisorLow(fractionalDigits - scale);
            if (product <= uint.MaxValue && TryCast((uint) product, dec.Negative, out var signed))
            {
                fd = new FastDecimal32<T>(signed);
                return true;
            }
        } 
        else if (scale > fractionalDigits)
        {
            var (q,r) = Fast128BitDiv.DecDivRem128By128(dec.High32, dec.Low64, scale - fractionalDigits);
            q += new UInt128(0,Rounding.RoundDec(q.Lower, r, scale - fractionalDigits, dec.Negative, MidpointRounding.ToEven));

            if (q.Upper == 0 && q.Lower <= uint.MaxValue && TryCast((uint) q.Lower, dec.Negative, out var signed))
            {
                fd = new FastDecimal32<T>(signed);
                return true;
            }
        }
        
        fd = default;
        return false;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool TryCast(uint ul, bool negative, out int l)
    {
        if (!negative && ul <= int.MaxValue)
        {
            l = (int) ul;
            return true;
        }

        if (negative && ul <= (uint) int.MaxValue + 1)
        {
            l = -(int) ul;
            return true;
        }

        l = default;
        return false;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void ConvertToUnsigned(FastDecimal32<T> fd, out uint unsignedValue, out bool negative)
    {
        negative = false;
        if (fd._value >= 0)
        {
            unsignedValue = (uint) fd._value;
        }
        else
        {
            unsignedValue = (uint) -fd._value;
            negative = !negative;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void ConvertToUnsigned(FastDecimal32<T> fd1, FastDecimal32<T> fd2, out uint unsignedValue1, out uint unsignedValue2,
        out bool negative)
    {
        negative = false;
        if (fd1._value >= 0)
        {
            unsignedValue1 = (uint) fd1._value;
        }
        else
        {
            unsignedValue1 = (uint) -fd1._value;
            negative = !negative;
        }

        if (fd2._value >= 0)
        {
            unsignedValue2 = (uint) fd2._value;
        }
        else
        {
            unsignedValue2 = (uint) -fd2._value;
            negative = !negative;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static ulong GetDivisor() => Fast128BitDiv.GetDivisorLow(GetFractionalDigits());

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static int GetFractionalDigits() => new T().FractionalDigits;
}
