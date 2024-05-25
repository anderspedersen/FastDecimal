using System.Globalization;
using FastDecimal.FractionalDigits;

namespace FastDecimal.Tests;

public class FastDecimal64Tests
{
    [Fact]
    public void ShouldAddTwoDecimals()
    {
        var result = new FastDecimal64<Four>(34_2945) +
                     new FastDecimal64<Four>(324534_2945);
        
        Assert.Equal(new FastDecimal64<Four>(324568_5890), result);
    }

    [Fact]
    public void ShouldOverflowInUncheckedEnvironmentWhenAddResultIsTooBig()
    {
        var result = new FastDecimal64<Four>(922337147478957_3302) +
                     new FastDecimal64<Four>(434544389_9843);
        
        Assert.Equal(new FastDecimal64<Four>(-922336825347607_8471), result);
    }
    

    [Fact]
    public void ShouldThrowExceptionInCheckedEnvironmentWhenAddResultIsTooBig()
    {
        Assert.Throws<OverflowException>(() => 
            checked(new FastDecimal64<Four>(922337147478957_3302) +
                     new FastDecimal64<Four>(434544389_9843)));
    }
    
    [Fact]
    public void ShouldSubtractTwoDecimals()
    {
        var result = new FastDecimal64<Four>(34_2945) -
                     new FastDecimal64<Four>(324534_2945);
        
        Assert.Equal(new FastDecimal64<Four>(-324500_0000), result);
    }

    [Fact]
    public void ShouldOverflowInUncheckedEnvironmentWhenSubtractResultIsTooBig()
    {
        var result = new FastDecimal64<Four>(-922337147478957_3302) -
                     new FastDecimal64<Four>(434544389_9843);
        
        Assert.Equal(new FastDecimal64<Four>(922336825347607_8471), result);
    }

    [Fact]
    public void ShouldThrowExceptionInCheckedEnvironmentWhenSubtractResultIsTooBig()
    {
        Assert.Throws<OverflowException>(() => 
            checked(new FastDecimal64<Four>(-922337147478957_3302) -
                    new FastDecimal64<Four>(434544389_9843)));
    }

    [Fact]
    public void ShouldMultiplyWhenIntermediateResultIsLessThan64Bit()
    {
        // To multiply fixed point decimals with four fractional digits, we calculate
        // (a*b)/1000. Point of this test is that the intermediate result (a*b) fits in
        // 64 bits
        var result = new FastDecimal64<Four>(83_4959) *
                     new FastDecimal64<Four>(23_4546);
        
        Assert.Equal(new FastDecimal64<Four>(1958_3629), result);
    }

    [Fact]
    public void ShouldMultiplyWhenIntermediateResultIsLargerThan64Bit()
    {
        // To multiply fixed point decimals with four fractional digits, we calculate
        // (a*b)/1000. Point of this test is that the intermediate result (a*b) does not
        // fit in 64 bits (but the final result does, after division, does)
        
        var result = new FastDecimal64<Four>(142984_4538) *
                     new FastDecimal64<Four>(4454543_2198);
        
        Assert.Equal(new FastDecimal64<Four>(636930429211_5963), result);
    }
    
    [Fact]
    public void ShouldOverflowInUncheckedEnvironmentWhenMultiplyResultIsLargerThan64Bit()
    {
        var result = new FastDecimal64<Four>(14263984_4538) *
                     new FastDecimal64<Four>(444554543_2198);
        
        Assert.Equal(new FastDecimal64<Four>(807095871240521_9115), result);
    }
    
    [Fact]
    public void ShouldThrowExceptionInCheckedEnvironmentWhenMultiplyResultIsLargerThan64Bit()
    {
        Assert.Throws<OverflowException>(() => 
            checked(new FastDecimal64<Four>(14263984_4538) *
                    new FastDecimal64<Four>(444554543_2198)));
    }
    
    [Theory]
    [InlineData(MidpointRounding.ToEven, 1_5000, 0_0002)]
    [InlineData(MidpointRounding.ToEven, 2_5000, 0_0002)]
    [InlineData(MidpointRounding.ToEven, 1_4999, 0_0001)] 
    [InlineData(MidpointRounding.ToEven, 2_5001, 0_0003)]
    [InlineData(MidpointRounding.ToEven, -1_5000, -0_0002)]
    [InlineData(MidpointRounding.ToEven, -2_5000, -0_0002)]
    [InlineData(MidpointRounding.ToEven, -1_4999, -0_0001)]
    [InlineData(MidpointRounding.ToEven, -2_5001, -0_0003)]
    [InlineData(MidpointRounding.AwayFromZero, 2_5000, 0_0003)]
    [InlineData(MidpointRounding.AwayFromZero, 2_4999, 0_0002)]
    [InlineData(MidpointRounding.AwayFromZero, -2_5000, -0_0003)]
    [InlineData(MidpointRounding.AwayFromZero, -2_4999, -0_0002)]
    [InlineData(MidpointRounding.ToNegativeInfinity, 2_9999, 0_0002)]
    [InlineData(MidpointRounding.ToNegativeInfinity, 3_0000, 0_0003)]
    [InlineData(MidpointRounding.ToNegativeInfinity, -2_0001, -0_0003)]
    [InlineData(MidpointRounding.ToNegativeInfinity, -2_0000, -0_0002)]
    [InlineData(MidpointRounding.ToZero, 2_9999, 0_0002)]
    [InlineData(MidpointRounding.ToZero, 3_0000, 0_0003)]
    [InlineData(MidpointRounding.ToZero, -2_9999, -0_0002)]
    [InlineData(MidpointRounding.ToZero, -3_0000, -0_0003)]
    [InlineData(MidpointRounding.ToPositiveInfinity, 3_0000, 0_0003)]
    [InlineData(MidpointRounding.ToPositiveInfinity, 3_0001, 0_0004)]
    [InlineData(MidpointRounding.ToPositiveInfinity, -2_9999, -0_0002)]
    [InlineData(MidpointRounding.ToPositiveInfinity, -3_0000, -0_0003)]
    public void ShouldRoundUsingTheMidpointRoundingParameter(MidpointRounding midpointRounding, long roundingNumber, long expectedNumber)
    {
        var result = FastDecimal64<Four>.Multiply(
            new FastDecimal64<Four>(roundingNumber),
            new FastDecimal64<Four>(0_0001),
            midpointRounding);
        
        Assert.Equal(new FastDecimal64<Four>(expectedNumber), result);
    }
    
    [Fact]
    public void ShouldDivideWhenIntermediateResultIsLessThan64Bit()
    {
        // To multiply fixed point decimals with four fractional digits, we calculate
        // (a*1000)/b. Point of this test is that the intermediate result a*1000 fits in
        // 64 bits
        var result = new FastDecimal64<Four>(47_0000) /
                     new FastDecimal64<Four>(2_0000);
        
        Assert.Equal(new FastDecimal64<Four>(23_5000), result);
    }
    
    
    [Fact]
    public void ShouldDivideWhenIntermediateResultIsLargerThan64Bit()
    {
        // To multiply fixed point decimals with four fractional digits, we calculate
        // (a*1000)/b. Point of this test is that the intermediate result a*1000 does not
        // fit in 64 bits (but the final result does, after division, does)
        var result = new FastDecimal64<Four>(470000000000000_0000) /
                     new FastDecimal64<Four>(2_0000);
        
        Assert.Equal(new FastDecimal64<Four>(235000000000000_0000), result);
    }
    
    [Fact]
    public void ShouldOverflowInUncheckedEnvironmentWhenDivideResultIsLargerThan64Bit()
    {
        var result = new FastDecimal64<Four>(470000000000000_0000) /
                     new FastDecimal64<Four>(0_0001);
        
        Assert.Equal(new FastDecimal64<Four>(-230389981193751_7568), result);
    }
    
    [Fact]
    public void ShouldThrowExceptionInCheckedEnvironmentWhenDivideResultIsLargerThan64Bit()
    {
        Assert.Throws<OverflowException>(() => 
            checked(new FastDecimal64<Four>(470000000000000_0000) /
                    new FastDecimal64<Four>(0_0001)));
    }
    
    [Theory]
    [InlineData(MidpointRounding.ToEven, 1_5000, 0_0002)]
    [InlineData(MidpointRounding.ToEven, 2_5000, 0_0002)]
    [InlineData(MidpointRounding.ToEven, 1_4999, 0_0001)] 
    [InlineData(MidpointRounding.ToEven, 2_5001, 0_0003)]
    [InlineData(MidpointRounding.ToEven, -1_5000, -0_0002)]
    [InlineData(MidpointRounding.ToEven, -2_5000, -0_0002)]
    [InlineData(MidpointRounding.ToEven, -1_4999, -0_0001)]
    [InlineData(MidpointRounding.ToEven, -2_5001, -0_0003)]
    [InlineData(MidpointRounding.AwayFromZero, 2_5000, 0_0003)]
    [InlineData(MidpointRounding.AwayFromZero, 2_4999, 0_0002)]
    [InlineData(MidpointRounding.AwayFromZero, -2_5000, -0_0003)]
    [InlineData(MidpointRounding.AwayFromZero, -2_4999, -0_0002)]
    [InlineData(MidpointRounding.ToNegativeInfinity, 2_9999, 0_0002)]
    [InlineData(MidpointRounding.ToNegativeInfinity, 3_0000, 0_0003)]
    [InlineData(MidpointRounding.ToNegativeInfinity, -2_0001, -0_0003)]
    [InlineData(MidpointRounding.ToNegativeInfinity, -2_0000, -0_0002)]
    [InlineData(MidpointRounding.ToZero, 2_9999, 0_0002)]
    [InlineData(MidpointRounding.ToZero, 3_0000, 0_0003)]
    [InlineData(MidpointRounding.ToZero, -2_9999, -0_0002)]
    [InlineData(MidpointRounding.ToZero, -3_0000, -0_0003)]
    [InlineData(MidpointRounding.ToPositiveInfinity, 3_0000, 0_0003)]
    [InlineData(MidpointRounding.ToPositiveInfinity, 3_0001, 0_0004)]
    [InlineData(MidpointRounding.ToPositiveInfinity, -2_9999, -0_0002)]
    [InlineData(MidpointRounding.ToPositiveInfinity, -3_0000, -0_0003)]
    public void ShouldRoundUsingTheMidpointRoundingParameterWhenDividing(MidpointRounding midpointRounding, long roundingNumber, long expectedNumber)
    {
        var result = FastDecimal64<Four>.Divide(
            new FastDecimal64<Four>(roundingNumber),
            new FastDecimal64<Four>(10000_0000),
            midpointRounding);
        
        Assert.Equal(new FastDecimal64<Four>(expectedNumber), result);
    }

    [Theory]
    [InlineData(0_0000, 1_0000)]
    [InlineData(32423_5875, 32424_5875)]
    [InlineData(-32423_5875, -32422_5875)]
    [InlineData(-0_5875, 0_4125)]
    public void ShouldIncreaseValueByOneWhenUsingIncrementOperator(int beforeIncrease, int expectedAfterIncrease)
    {
        var fd = new FastDecimal64<Four>(beforeIncrease);
        fd++;
        Assert.Equal(new FastDecimal64<Four>(expectedAfterIncrease), fd);
    }

    [Fact]
    public void ShouldOverflowInUncheckedEnvironmentWhenIncrementResultIsBiggerThanMaxValue()
    {
        var fd = FastDecimal64<Four>.MaxValue;
        fd++;
        Assert.Equal(new FastDecimal64<Four>(long.MinValue + 0_9999), fd);
    }

    [Fact]
    public void ShouldThrowExceptionInCheckedEnvironmentWhenIncrementResultIsBiggerThanMaxValue()
    {
        var fd = FastDecimal64<Four>.MaxValue;
        Assert.Throws<OverflowException>(() => 
            checked(fd++));
    }
    
    [Theory]
    [InlineData(0_0000, -1_0000)]
    [InlineData(32423_5875, 32422_5875)]
    [InlineData(-32423_5875, -32424_5875)]
    [InlineData(0_5875, -0_4125)]
    public void ShouldIncreaseValueByOneWhenUsingDecrementOperator(int beforeIncrease, int expectedAfterIncrease)
    {
        var fd = new FastDecimal64<Four>(beforeIncrease);
        fd--;
        Assert.Equal(new FastDecimal64<Four>(expectedAfterIncrease), fd);
    }

    [Fact]
    public void ShouldOverflowInUncheckedEnvironmentWhenDecrementResultIsSmallerThanMinValue()
    {
        var fd = FastDecimal64<Four>.MinValue;
        fd--;
        Assert.Equal(new FastDecimal64<Four>(long.MaxValue - 0_9999), fd);
    }

    [Fact]
    public void ShouldThrowExceptionInCheckedEnvironmentWhenDecrementResultIsSmallerThanMinValue()
    {
        var fd = FastDecimal64<Four>.MinValue;
        Assert.Throws<OverflowException>(() => 
            checked(fd--));
    }
    
    [Theory]
    [InlineData(0_0000)]
    [InlineData(-4345_6567)]
    [InlineData(4345_6567)]
    [InlineData(long.MinValue)]
    [InlineData(long.MaxValue)]
    public void PlusOperatorShouldNotChangeValue(long value)
    {
        var fd = new FastDecimal64<Four>(value);
        Assert.Equal(fd, +fd);
    }
    
    [Theory]
    [InlineData(0_0000, 0_0000 )]
    [InlineData(-4345_6567, 4345_6567)]
    [InlineData(4345_6567, -4345_6567)]
    [InlineData(long.MaxValue, -long.MaxValue)]
    public void MinusOperatorShouldNegateValueWhenNegatedValueIsWithinRange(long value, long negatedValue)
    {
        var fd = new FastDecimal64<Four>(value);
        var negatedFd = new FastDecimal64<Four>(negatedValue);
        Assert.Equal(negatedFd, -fd);
    }
    
    [Fact]
    public void NegatingMinValueShouldOverflowInUncheckedEnvironment()
    {
        Assert.Equal(FastDecimal64<Four>.MinValue, -FastDecimal64<Four>.MinValue);
    }
    
    [Fact]
    public void NegatingMinValueShouldThrowExceptionInCheckedEnvironment()
    {
        Assert.Throws<OverflowException>(() => 
            checked(-FastDecimal64<Four>.MinValue));
    }
    
    [Theory]
    [InlineData(0_0000, 0_0000)]
    [InlineData(-4345_6567, 4345_6567)]
    [InlineData(4345_6567, 4345_6567)]
    [InlineData(long.MaxValue, long.MaxValue)]
    public void AbsShouldReturnAbsoluteValueWhenAbsoluteValueIsWithinRange(long value, long absoluteValue)
    {
        var fd = new FastDecimal64<Four>(value);
        var absoluteFd = new FastDecimal64<Four>(absoluteValue);
        Assert.Equal(absoluteFd, FastDecimal64<Four>.Abs(fd));
    }
    
    
    [Fact]
    public void AbsShouldThrowExceptionWhenAbsoluteValueIsOutsideRange()
    {
        Assert.Throws<OverflowException>(() => 
            FastDecimal64<Four>.Abs(FastDecimal64<Four>.MinValue));
    }
    
    [Theory]
    [InlineData(0_0000, true)]
    [InlineData(4_0000, true)]
    [InlineData(123_0000, true)]
    [InlineData(-5_0000, true)]
    [InlineData(-436_0000, true)]
    [InlineData(-435_5000, false)]
    [InlineData(123_2000, false)]
    public void IsIntegerShouldReturnTrueIffValueIsInteger(long value, bool expected)
    {
        var fd = new FastDecimal64<Four>(value);
        Assert.Equal(expected, FastDecimal64<Four>.IsInteger(fd));
    }
    
    [Theory]
    [InlineData(0_0000, true)]
    [InlineData(4_0000, true)]
    [InlineData(123_0000, false)]
    [InlineData(-5_0000, false)]
    [InlineData(-436_0000, true)]
    [InlineData(-435_5000, false)]
    [InlineData(123_2000, false)]
    public void IsEvenIntegerShouldReturnTrueIffValueIsInteger(long value, bool expected)
    {
        var fd = new FastDecimal64<Four>(value);
        Assert.Equal(expected, FastDecimal64<Four>.IsEvenInteger(fd));
    }
    
    [Theory]
    [InlineData(0_0000, false)]
    [InlineData(4_0000, false)]
    [InlineData(123_0000, true)]
    [InlineData(-5_0000, true)]
    [InlineData(-436_0000, false)]
    [InlineData(-435_5000, false)]
    [InlineData(123_2000, false)]
    public void IsOddIntegerShouldReturnTrueIffValueIsInteger(long value, bool expected)
    {
        var fd = new FastDecimal64<Four>(value);
        Assert.Equal(expected, FastDecimal64<Four>.IsOddInteger(fd));
    }
    
    // ShouldConvertToLEssPrecisionWithRounding
    [Theory]
    // Nearest
    [InlineData(4324_4357500, 4324_4358, MidpointRounding.ToEven)]
    [InlineData(4324_4357500, 4324_4358, MidpointRounding.AwayFromZero)]
    [InlineData(4324_4358500, 4324_4358, MidpointRounding.ToEven)]
    [InlineData(4324_4358500, 4324_4359, MidpointRounding.AwayFromZero)]
    [InlineData(-4324_4357500, -4324_4358, MidpointRounding.ToEven)]
    [InlineData(-4324_4357500, -4324_4358, MidpointRounding.AwayFromZero)]
    [InlineData(-4324_4358500, -4324_4358, MidpointRounding.ToEven)]
    [InlineData(-4324_4358500, -4324_4359, MidpointRounding.AwayFromZero)]
    // Directed
    [InlineData(4324_4358999, 4324_4358, MidpointRounding.ToZero)]
    [InlineData(4324_4358999, 4324_4358, MidpointRounding.ToNegativeInfinity)]
    [InlineData(4324_4358999, 4324_4359, MidpointRounding.ToPositiveInfinity)]
    [InlineData(-4324_4358999, -4324_4358, MidpointRounding.ToZero)]
    [InlineData(-4324_4358999, -4324_4359, MidpointRounding.ToNegativeInfinity)]
    [InlineData(-4324_4358999, -4324_4358, MidpointRounding.ToPositiveInfinity)]
    
    public void ShouldChangePrecisionUsingProvidedMidpointRoundingWhenTargetFastDecimalHasLessPrecision(long initial, long expected, MidpointRounding rounding)
    {
        var initialFd = new FastDecimal64<Seven>(initial);
        var expectedFd = new FastDecimal64<Four>(expected);
        Assert.Equal(expectedFd, initialFd.ChangePrecision<Four>(rounding));
    }

    [Fact]
    public void ShouldChangePrecisionWhenTargetFastDecimalHasMorePrecision()
    {
        var fd = new FastDecimal64<Four>(4389_3467);
        var expected = new FastDecimal64<Seven>(4389_3467000);
        Assert.Equal(expected, fd.ChangePrecision<Seven>());
    }

    [Fact]
    public void ChangePrecisionShouldThrowExceptionWhenValueIsOutOfRangeForTargetFastDecimal()
    {
        var fd = new FastDecimal64<Four>(4389_3467);
        Assert.Throws<OverflowException>(() =>
            fd.ChangePrecision<Sixteen>());
    }

    [Theory]
    [InlineData(18, 1_159493067495694854, "1.159493067495694854")]
    [InlineData(18, -1_159493067495694854, "-1.159493067495694854")]
    [InlineData(0, 64564, "64564")]
    [InlineData(0, -64564, "-64564")]
    [InlineData(7, 0_0000000, "0.0000000")]
    [InlineData(7, 54_5475468, "54.5475468")]
    [InlineData(7, -54_5475468, "-54.5475468")]
    public void ShouldCastFastDecimalToDecimal(int fractionalDigits, long fdValue, string expectedDecimalString)
    {
        var expectedDecimal = decimal.Parse(expectedDecimalString, CultureInfo.InvariantCulture);
        CustomizableFractionalDigits.Digits = fractionalDigits;
        var fd = new FastDecimal64<CustomizableFractionalDigits>(fdValue);
        Assert.Equal(expectedDecimal, (decimal) fd);
    }

    [Fact]
    public void ShouldCastDecimalToFastDecimalWithHigherPrecisionWhenValueIsIWithinRange()
    {
        var d = 345.435m;
        var expectedFd = new FastDecimal64<Seven>(345_4350000);
        Assert.Equal(expectedFd, (FastDecimal64<Seven>) d);
    }

    [Theory]
    [InlineData("1.15955", 1_1596)]
    [InlineData("431.15965", 431_1596)]
    public void ShouldCastDecimalToFastDecimalWithLowerPrecisionUsingRoundingToEven(string decimalString, long expectedFdValue)
    {
        var d = decimal.Parse(decimalString, CultureInfo.InvariantCulture);
        var expectedFd = new FastDecimal64<Four>(expectedFdValue);
        Assert.Equal(expectedFd, (FastDecimal64<Four>) d);
    }

    [Fact]
    public void ShouldOverflowInUncheckedEnvironmentWhenCastingDecimalToFastDecimalAndValueIsOutsideRange()
    {
        var d = 432879823329832489.0159m;
        var expectedFd = new FastDecimal64<Four>(-618662402341973_9601);
        Assert.Equal(expectedFd, (FastDecimal64<Four>) d);
    }

    [Fact]
    public void ShouldThrowExceptionInCheckedEnvironmentWhenCastingDecimalToFastDecimalAndValueIsOutsideRange()
    {
        var d = 432879823329832489.0159m;
        Assert.Throws<OverflowException>(() => 
            checked((FastDecimal64<Four>) d));
    }
}