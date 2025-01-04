using System.Diagnostics;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace FastDecimal;

/// <summary>Represents a 128-bit unsigned integer.</summary>
[CLSCompliant(false)]
[StructLayout(LayoutKind.Sequential)]
internal readonly struct UInt128 
{
    internal const int Size = 16;

    /// <summary>Initializes a new instance of the <see cref="UInt128" /> struct.</summary>
    /// <param name="upper">The upper 64-bits of the 128-bit value.</param>
    /// <param name="lower">The lower 64-bits of the 128-bit value.</param>
    [CLSCompliant(false)]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public UInt128(ulong upper, ulong lower)
    {
        Lower = lower;
        Upper = upper;
    }
    
    internal ulong Lower
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get;
    }

    internal ulong Upper
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get;
    }
    
    //
    // IAdditionOperators
    //

    /// <inheritdoc cref="IAdditionOperators{TSelf, TOther, TResult}.op_Addition(TSelf, TOther)" />
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static UInt128 operator +(UInt128 left, UInt128 right)
    {
        // For unsigned addition, we can detect overflow by checking `(x + y) < x`
        // This gives us the carry to add to upper to compute the correct result

        ulong lower = left.Lower + right.Lower;
        ulong carry = (lower < left.Lower) ? 1UL : 0UL;

        ulong upper = left.Upper + right.Upper + carry;
        return new UInt128(upper, lower);
    }

    /// <inheritdoc cref="IBinaryInteger{TSelf}.LeadingZeroCount(TSelf)" />
    public static int LeadingZeroCount(UInt128 value)
    {
        if (value.Upper == 0)
        {
            return 64 + (int) ulong.LeadingZeroCount(value.Lower);
        }

        return (int) ulong.LeadingZeroCount(value.Upper);
    }

    public static (UInt128 Quotient, ulong Remainder) DivRem(UInt128 left, ulong right)
    {
        UInt128 quotient = left / right;
        return (quotient, (left - quotient * right).Lower);
    }
    
    public static UInt128 operator /(UInt128 left, ulong right)
        {
            return DivideSlow(left, right);

            static uint AddDivisor(Span<uint> left, ReadOnlySpan<uint> right)
            {
                Debug.Assert(left.Length >= right.Length);

                // Repairs the dividend, if the last subtract was too much

                ulong carry = 0UL;

                for (int i = 0; i < right.Length; i++)
                {
                    ref uint leftElement = ref left[i];
                    ulong digit = (leftElement + carry) + right[i];

                    leftElement = unchecked((uint)digit);
                    carry = digit >> 32;
                }

                return (uint)carry;
            }

            static bool DivideGuessTooBig(ulong q, ulong valHi, uint valLo, uint divHi, uint divLo)
            {
                Debug.Assert(q <= 0xFFFFFFFF);

                // We multiply the two most significant limbs of the divisor
                // with the current guess for the quotient. If those are bigger
                // than the three most significant limbs of the current dividend
                // we return true, which means the current guess is still too big.

                ulong chkHi = divHi * q;
                ulong chkLo = divLo * q;

                chkHi += (chkLo >> 32);
                chkLo = (uint)(chkLo);

                return (chkHi > valHi) || ((chkHi == valHi) && (chkLo > valLo));
            }

            unsafe static UInt128 DivideSlow(UInt128 quotient, ulong divisor)
            {
                // This is the same algorithm currently used by BigInteger so
                // we need to get a Span<uint> containing the value represented
                // in the least number of elements possible.

                // We need to ensure that we end up with 4x uints representing the bits from
                // least significant to most significant so the math will be correct on both
                // little and big endian systems. So we'll just allocate the relevant buffer
                // space and then write out the four parts using the native endianness of the
                // system.

                uint* pLeft = stackalloc uint[Size / sizeof(uint)];

                Unsafe.WriteUnaligned(ref *(byte*)(pLeft + 0), (uint)(quotient.Lower >> 00));
                Unsafe.WriteUnaligned(ref *(byte*)(pLeft + 1), (uint)(quotient.Lower >> 32));

                Unsafe.WriteUnaligned(ref *(byte*)(pLeft + 2), (uint)(quotient.Upper >> 00));
                Unsafe.WriteUnaligned(ref *(byte*)(pLeft + 3), (uint)(quotient.Upper >> 32));

                Span<uint> left = new Span<uint>(pLeft, (Size / sizeof(uint)) - (LeadingZeroCount(quotient) / 32));

                // Repeat the same operation with the divisor

                uint* pRight = stackalloc uint[sizeof(ulong) / sizeof(uint)];

                Unsafe.WriteUnaligned(ref *(byte*)(pRight + 0), (uint)(divisor >> 00));
                Unsafe.WriteUnaligned(ref *(byte*)(pRight + 1), (uint)(divisor >> 32));

                Span<uint> right = new Span<uint>(pRight, (sizeof(ulong) / sizeof(uint)) - ((int)ulong.LeadingZeroCount(divisor) / 32));

                Span<uint> rawBits = stackalloc uint[Size / sizeof(uint)];
                rawBits.Clear();
                Span<uint> bits = rawBits.Slice(0, left.Length - right.Length + 1);

                Debug.Assert(left.Length >= 1);
                Debug.Assert(right.Length >= 1);
                Debug.Assert(left.Length >= right.Length);

                // Executes the "grammar-school" algorithm for computing q = a / b.
                // Before calculating q_i, we get more bits into the highest bit
                // block of the divisor. Thus, guessing digits of the quotient
                // will be more precise. Additionally we'll get r = a % b.

                uint divHi = right[right.Length - 1];
                uint divLo = right.Length > 1 ? right[right.Length - 2] : 0;

                // We measure the leading zeros of the divisor
                int shift = BitOperations.LeadingZeroCount(divHi);
                int backShift = 32 - shift;

                // And, we make sure the most significant bit is set
                if (shift > 0)
                {
                    uint divNx = right.Length > 2 ? right[right.Length - 3] : 0;

                    divHi = (divHi << shift) | (divLo >> backShift);
                    divLo = (divLo << shift) | (divNx >> backShift);
                }

                // Then, we divide all of the bits as we would do it using
                // pen and paper: guessing the next digit, subtracting, ...
                for (int i = left.Length; i >= right.Length; i--)
                {
                    int n = i - right.Length;
                    uint t = ((uint)(i) < (uint)(left.Length)) ? left[i] : 0;

                    ulong valHi = ((ulong)(t) << 32) | left[i - 1];
                    uint valLo = (i > 1) ? left[i - 2] : 0;

                    // We shifted the divisor, we shift the dividend too
                    if (shift > 0)
                    {
                        uint valNx = i > 2 ? left[i - 3] : 0;

                        valHi = (valHi << shift) | (valLo >> backShift);
                        valLo = (valLo << shift) | (valNx >> backShift);
                    }

                    // First guess for the current digit of the quotient,
                    // which naturally must have only 32 bits...
                    ulong digit = valHi / divHi;

                    if (digit > 0xFFFFFFFF)
                    {
                        digit = 0xFFFFFFFF;
                    }

                    // Our first guess may be a little bit to big
                    while (DivideGuessTooBig(digit, valHi, valLo, divHi, divLo))
                    {
                        --digit;
                    }

                    if (digit > 0)
                    {
                        // Now it's time to subtract our current quotient
                        uint carry = SubtractDivisor(left.Slice(n), right, digit);

                        if (carry != t)
                        {
                            Debug.Assert(carry == (t + 1));

                            // Our guess was still exactly one too high
                            carry = AddDivisor(left.Slice(n), right);

                            --digit;
                            Debug.Assert(carry == 1);
                        }
                    }

                    // We have the digit!
                    if ((uint)(n) < (uint)(bits.Length))
                    {
                        bits[n] = (uint)(digit);
                    }

                    if ((uint)(i) < (uint)(left.Length))
                    {
                        left[i] = 0;
                    }
                }

                return new UInt128(
                    ((ulong)(rawBits[3]) << 32) | rawBits[2],
                    ((ulong)(rawBits[1]) << 32) | rawBits[0]
                );
            }
            
            

            static uint SubtractDivisor(Span<uint> left, ReadOnlySpan<uint> right, ulong q)
            {
                Debug.Assert(left.Length >= right.Length);
                Debug.Assert(q <= 0xFFFFFFFF);

                // Combines a subtract and a multiply operation, which is naturally
                // more efficient than multiplying and then subtracting...

                ulong carry = 0UL;

                for (int i = 0; i < right.Length; i++)
                {
                    carry += right[i] * q;

                    uint digit = (uint)(carry);
                    carry >>= 32;

                    ref uint leftElement = ref left[i];

                    if (leftElement < digit)
                    {
                        ++carry;
                    }
                    leftElement -= digit;
                }

                return (uint)(carry);
            }
        }

    //
    // IComparisonOperators
    //

    /// <inheritdoc cref="IComparisonOperators{TSelf, TOther, TResult}.op_LessThan(TSelf, TOther)" />
    public static bool operator <(UInt128 left, UInt128 right)
    {
        return (left.Upper < right.Upper)
               || (left.Upper == right.Upper) && (left.Lower < right.Lower);
    }

    /// <inheritdoc cref="IComparisonOperators{TSelf, TOther, TResult}.op_LessThanOrEqual(TSelf, TOther)" />
    public static bool operator <=(UInt128 left, UInt128 right)
    {
        return (left.Upper < right.Upper)
               || (left.Upper == right.Upper) && (left.Lower <= right.Lower);
    }

    /// <inheritdoc cref="IComparisonOperators{TSelf, TOther, TResult}.op_GreaterThan(TSelf, TOther)" />
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator >(UInt128 left, UInt128 right)
    {
        return (left.Upper > right.Upper)
               || (left.Upper == right.Upper) && (left.Lower > right.Lower);
    }

    /// <inheritdoc cref="IComparisonOperators{TSelf, TOther, TResult}.op_GreaterThanOrEqual(TSelf, TOther)" />
    public static bool operator >=(UInt128 left, UInt128 right)
    {
        return (left.Upper > right.Upper)
               || (left.Upper == right.Upper) && (left.Lower >= right.Lower);
    }


    /// <inheritdoc cref="IEqualityOperators{TSelf, TOther, TResult}.op_Equality(TSelf, TOther)" />
    public static bool operator ==(UInt128 left, UInt128 right) =>
        (left.Lower == right.Lower) && (left.Upper == right.Upper);

    /// <inheritdoc cref="IEqualityOperators{TSelf, TOther, TResult}.op_Inequality(TSelf, TOther)" />
    public static bool operator !=(UInt128 left, UInt128 right) =>
        (left.Lower != right.Lower) || (left.Upper != right.Upper);


    //
    // IMultiplyOperators
    //

    /// <inheritdoc cref="IMultiplyOperators{TSelf, TOther, TResult}.op_Multiply(TSelf, TOther)" />
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static UInt128 operator *(UInt128 left, UInt128 right)
    {
        ulong upper = Math.BigMul(left.Lower, right.Lower, out ulong lower);
        upper += (left.Upper * right.Lower) + (left.Lower * right.Upper);
        return new UInt128(upper, lower);
    }
    
    /// <inheritdoc cref="IMultiplyOperators{TSelf, TOther, TResult}.op_Multiply(TSelf, TOther)" />
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static UInt128 operator *(UInt128 left, ulong right)
    {
        ulong upper = Math.BigMul(left.Lower, right, out ulong lower);
        upper += left.Upper * right;
        return new UInt128(upper, lower);
    }

    //
    // IShiftOperators
    //

    /// <inheritdoc cref="IShiftOperators{TSelf, TOther, TResult}.op_LeftShift(TSelf, TOther)" />
    public static UInt128 operator <<(UInt128 value, int shiftAmount)
    {
        // C# automatically masks the shift amount for UInt64 to be 0x3F. So we
        // need to specially handle things if the 7th bit is set.

        shiftAmount &= 0x7F;

        if ((shiftAmount & 0x40) != 0)
        {
            // In the case it is set, we know the entire lower bits must be zero
            // and so the upper bits are just the lower shifted by the remaining
            // masked amount

            ulong upper = value.Lower << shiftAmount;
            return new UInt128(upper, 0);
        }
        else if (shiftAmount != 0)
        {
            // Otherwise we need to shift both upper and lower halves by the masked
            // amount and then or that with whatever bits were shifted "out" of lower

            ulong lower = value.Lower << shiftAmount;
            ulong upper = (value.Upper << shiftAmount) | (value.Lower >> (64 - shiftAmount));

            return new UInt128(upper, lower);
        }
        else
        {
            return value;
        }
    }

    /// <inheritdoc cref="IShiftOperators{TSelf, TOther, TResult}.op_RightShift(TSelf, TOther)" />
    public static UInt128 operator >> (UInt128 value, int shiftAmount) => value >>> shiftAmount;

    /// <inheritdoc cref="IShiftOperators{TSelf, TOther, TResult}.op_UnsignedRightShift(TSelf, TOther)" />
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static UInt128 operator >>> (UInt128 value, int shiftAmount)
    {
        // C# automatically masks the shift amount for UInt64 to be 0x3F. So we
        // need to specially handle things if the 7th bit is set.

        shiftAmount &= 0x7F;

        if ((shiftAmount & 0x40) != 0)
        {
            // In the case it is set, we know the entire upper bits must be zero
            // and so the lower bits are just the upper shifted by the remaining
            // masked amount

            ulong lower = value.Upper >> shiftAmount;
            return new UInt128(0, lower);
        }
        else if (shiftAmount != 0)
        {
            // Otherwise we need to shift both upper and lower halves by the masked
            // amount and then or that with whatever bits were shifted "out" of upper

            ulong lower = (value.Lower >> shiftAmount) | (value.Upper << (64 - shiftAmount));
            ulong upper = value.Upper >> shiftAmount;

            return new UInt128(upper, lower);
        }
        else
        {
            return value;
        }
    }

    /// <inheritdoc cref="ISubtractionOperators{TSelf, TOther, TResult}.op_Subtraction(TSelf, TOther)" />
    /// 
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static UInt128 operator -(UInt128 left, UInt128 right)
    {
        // For unsigned subtract, we can detect overflow by checking `(x - y) > x`
        // This gives us the borrow to subtract from upper to compute the correct result

        ulong lower = left.Lower - right.Lower;
        ulong borrow = (lower > left.Lower) ? 1UL : 0UL;

        ulong upper = left.Upper - right.Upper - borrow;
        return new UInt128(upper, lower);
    }
}