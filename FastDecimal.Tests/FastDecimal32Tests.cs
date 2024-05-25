using System.Globalization;
using FastDecimal.FractionalDigits;

namespace FastDecimal.Tests;

public class FastDecimal32Tests
{
    [Fact]
    public void ShouldAddTwoDecimals()
    {
        var result = new FastDecimal32<Four>(34_2945) +
                     new FastDecimal32<Four>(24534_2945);
        
        Assert.Equal(new FastDecimal32<Four>(24568_5890), result);
    }

    [Fact]
    public void ShouldOverflowInUncheckedEnvironmentWhenAddResultIsTooBig()
    {
        var result = new FastDecimal32<Four>(178957_3302) +
                     new FastDecimal32<Four>(144389_9843);
        
        Assert.Equal(new FastDecimal32<Four>(-106149_4151), result);
    }
    

    [Fact]
    public void ShouldThrowExceptionInCheckedEnvironmentWhenAddResultIsTooBig()
    {
        Assert.Throws<OverflowException>(() => 
            checked(new FastDecimal32<Four>(178957_3302) +
                     new FastDecimal32<Four>(144389_9843)));
    }
    
    [Fact]
    public void ShouldSubtractTwoDecimals()
    {
        var result = new FastDecimal32<Four>(34_2945) -
                     new FastDecimal32<Four>(24534_2945);
        
        Assert.Equal(new FastDecimal32<Four>(-24500_0000), result);
    }

    [Fact]
    public void ShouldOverflowInUncheckedEnvironmentWhenSubtractResultIsTooBig()
    {
        var result = new FastDecimal32<Four>(-78957_3302) -
                     new FastDecimal32<Four>(144389_9843);
        
        Assert.Equal(new FastDecimal32<Four>(206149_4151), result);
    }

    [Fact]
    public void ShouldThrowExceptionInCheckedEnvironmentWhenSubtractResultIsTooBig()
    {
        Assert.Throws<OverflowException>(() => 
            checked(new FastDecimal32<Four>(-78957_3302) -
                    new FastDecimal32<Four>(144389_9843)));
    }

    [Fact]
    public void ShouldMultiplyWhenIntermediateResultIsLessThan32Bit()
    {
        // To multiply fixed point decimals with four fractional digits, we calculate
        // (a*b)/1000. Point of this test is that the intermediate result (a*b) fits in
        // 32 bits
        var result = new FastDecimal32<Four>(3_4959) *
                     new FastDecimal32<Four>(3_4546);
        
        Assert.Equal(new FastDecimal32<Four>(12_0769), result);
    }

    [Fact]
    public void ShouldMultiplyWhenIntermediateResultIsLargerThan32Bit()
    {
        // To multiply fixed point decimals with four fractional digits, we calculate
        // (a*b)/1000. Point of this test is that the intermediate result (a*b) does not
        // fit in 32 bits (but the final result does, after division, does)
        
        var result = new FastDecimal32<Four>(83_4959) *
                     new FastDecimal32<Four>(23_4546);
        
        Assert.Equal(new FastDecimal32<Four>(1958_3629), result);
    }
    
    [Fact]
    public void ShouldOverflowInUncheckedEnvironmentWhenMultiplyResultIsLargerThan32Bit()
    {
        var result = new FastDecimal32<Four>(142984_4538) *
                     new FastDecimal32<Four>(154543_2198);
        
        Assert.Equal(new FastDecimal32<Four>(100630_4059), result);
    }
    
    [Fact]
    public void ShouldThrowExceptionInCheckedEnvironmentWhenMultiplyResultIsLargerThan64Bit()
    {
        Assert.Throws<OverflowException>(() => 
            checked(new FastDecimal32<Four>(142984_4538) *
                    new FastDecimal32<Four>(154543_2198)));
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
    public void ShouldRoundUsingTheMidpointRoundingParameter(MidpointRounding midpointRounding, int roundingNumber, int expectedNumber)
    {
        var result = FastDecimal32<Four>.Multiply(
            new FastDecimal32<Four>(roundingNumber),
            new FastDecimal32<Four>(0_0001),
            midpointRounding);
        
        Assert.Equal(new FastDecimal32<Four>(expectedNumber), result);
    }
    
    [Fact]
    public void ShouldDivideWhenIntermediateResultIsLessThan32Bit()
    {
        // To multiply fixed point decimals with four fractional digits, we calculate
        // (a*1000)/b. Point of this test is that the intermediate result a*1000 fits in
        // 32 bits
        var result = new FastDecimal32<Four>(4_7000) /
                     new FastDecimal32<Four>(2_0000);
        
        Assert.Equal(new FastDecimal32<Four>(2_3500), result);
    }
    
    
    [Fact]
    public void ShouldDivideWhenIntermediateResultIsLargerThan32Bit()
    {
        // To multiply fixed point decimals with four fractional digits, we calculate
        // (a*1000)/b. Point of this test is that the intermediate result a*1000 does not
        // fit in 32 bits (but the final result does, after division, does)
        var result = new FastDecimal32<Four>(47000_0000) /
                     new FastDecimal32<Four>(2_0000);
        
        Assert.Equal(new FastDecimal32<Four>(23500_0000), result);
    }
    
    [Fact]
    public void ShouldOverflowInUncheckedEnvironmentWhenDivideResultIsLargerThan32Bit()
    {
        var result = new FastDecimal32<Four>(47000_0000) /
                     new FastDecimal32<Four>(0_0001);
        
        Assert.Equal(new FastDecimal32<Four>(130577_8176), result);
    }
    
    [Fact]
    public void ShouldThrowExceptionInCheckedEnvironmentWhenDivideResultIsLargerThan64Bit()
    {
        Assert.Throws<OverflowException>(() => 
            checked(new FastDecimal32<Four>(47000_0000) /
                    new FastDecimal32<Four>(0_0001)));
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
    public void ShouldRoundUsingTheMidpointRoundingParameterWhenDividing(MidpointRounding midpointRounding, int roundingNumber, int expectedNumber)
    {
        var result = FastDecimal32<Four>.Divide(
            new FastDecimal32<Four>(roundingNumber),
            new FastDecimal32<Four>(10000_0000),
            midpointRounding);
        
        Assert.Equal(new FastDecimal32<Four>(expectedNumber), result);
    }

    [Theory]
    [InlineData(0_0000, 1_0000)]
    [InlineData(32423_5875, 32424_5875)]
    [InlineData(-32423_5875, -32422_5875)]
    [InlineData(-0_5875, 0_4125)]
    public void ShouldIncreaseValueByOneWhenUsingIncrementOperator(int beforeIncrease, int expectedAfterIncrease)
    {
        var fd = new FastDecimal32<Four>(beforeIncrease);
        fd++;
        Assert.Equal(new FastDecimal32<Four>(expectedAfterIncrease), fd);
    }

    [Fact]
    public void ShouldOverflowInUncheckedEnvironmentWhenIncrementResultIsBiggerThanMaxValue()
    {
        var fd = FastDecimal32<Four>.MaxValue;
        fd++;
        Assert.Equal(new FastDecimal32<Four>(int.MinValue + 0_9999), fd);
    }

    [Fact]
    public void ShouldThrowExceptionInCheckedEnvironmentWhenIncrementResultIsBiggerThanMaxValue()
    {
        var fd = FastDecimal32<Four>.MaxValue;
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
        var fd = new FastDecimal32<Four>(beforeIncrease);
        fd--;
        Assert.Equal(new FastDecimal32<Four>(expectedAfterIncrease), fd);
    }

    [Fact]
    public void ShouldOverflowInUncheckedEnvironmentWhenDecrementResultIsSmallerThanMinValue()
    {
        var fd = FastDecimal32<Four>.MinValue;
        fd--;
        Assert.Equal(new FastDecimal32<Four>(int.MaxValue - 0_9999), fd);
    }

    [Fact]
    public void ShouldThrowExceptionInCheckedEnvironmentWhenDecrementResultIsSmallerThanMinValue()
    {
        var fd = FastDecimal32<Four>.MinValue;
        Assert.Throws<OverflowException>(() => 
            checked(fd--));
    }
    
    [Theory]
    [InlineData(0_0000)]
    [InlineData(-4345_6567)]
    [InlineData(4345_6567)]
    [InlineData(int.MinValue)]
    [InlineData(int.MaxValue)]
    public void PlusOperatorShouldNotChangeValue(int value)
    {
        var fd = new FastDecimal32<Four>(value);
        Assert.Equal(fd, +fd);
    }
    
    [Theory]
    [InlineData(0_0000, 0_0000 )]
    [InlineData(-4345_6567, 4345_6567)]
    [InlineData(4345_6567, -4345_6567)]
    [InlineData(int.MaxValue, -int.MaxValue)]
    public void MinusOperatorShouldNegateValueWhenNegatedValueIsWithinRange(int value, int negatedValue)
    {
        var fd = new FastDecimal32<Four>(value);
        var negatedFd = new FastDecimal32<Four>(negatedValue);
        Assert.Equal(negatedFd, -fd);
    }
    
    [Fact]
    public void NegatingMinValueShouldOverflowInUncheckedEnvironment()
    {
        Assert.Equal(FastDecimal32<Four>.MinValue, -FastDecimal32<Four>.MinValue);
    }
    
    [Fact]
    public void NegatingMinValueShouldThrowExceptionInCheckedEnvironment()
    {
        Assert.Throws<OverflowException>(() => 
            checked(-FastDecimal32<Four>.MinValue));
    }
    
    [Theory]
    [InlineData(0_0000, 0_0000)]
    [InlineData(-4345_6567, 4345_6567)]
    [InlineData(4345_6567, 4345_6567)]
    [InlineData(int.MaxValue, int.MaxValue)]
    public void AbsShouldReturnAbsoluteValueWhenAbsoluteValueIsWithinRange(int value, int absoluteValue)
    {
        var fd = new FastDecimal32<Four>(value);
        var absoluteFd = new FastDecimal32<Four>(absoluteValue);
        Assert.Equal(absoluteFd, FastDecimal32<Four>.Abs(fd));
    }
    
    
    [Fact]
    public void AbsShouldThrowExceptionWhenAbsoluteValueIsOutsideRange()
    {
        Assert.Throws<OverflowException>(() => 
            FastDecimal32<Four>.Abs(FastDecimal32<Four>.MinValue));
    }
    
    [Theory]
    [InlineData(0_0000, true)]
    [InlineData(4_0000, true)]
    [InlineData(123_0000, true)]
    [InlineData(-5_0000, true)]
    [InlineData(-436_0000, true)]
    [InlineData(-435_5000, false)]
    [InlineData(123_2000, false)]
    public void IsIntegerShouldReturnTrueIffValueIsInteger(int value, bool expected)
    {
        var fd = new FastDecimal32<Four>(value);
        Assert.Equal(expected, FastDecimal32<Four>.IsInteger(fd));
    }
    
    [Theory]
    [InlineData(0_0000, true)]
    [InlineData(4_0000, true)]
    [InlineData(123_0000, false)]
    [InlineData(-5_0000, false)]
    [InlineData(-436_0000, true)]
    [InlineData(-435_5000, false)]
    [InlineData(123_2000, false)]
    public void IsEvenIntegerShouldReturnTrueIffValueIsInteger(int value, bool expected)
    {
        var fd = new FastDecimal32<Four>(value);
        Assert.Equal(expected, FastDecimal32<Four>.IsEvenInteger(fd));
    }
    
    [Theory]
    [InlineData(0_0000, false)]
    [InlineData(4_0000, false)]
    [InlineData(123_0000, true)]
    [InlineData(-5_0000, true)]
    [InlineData(-436_0000, false)]
    [InlineData(-435_5000, false)]
    [InlineData(123_2000, false)]
    public void IsOddIntegerShouldReturnTrueIffValueIsInteger(int value, bool expected)
    {
        var fd = new FastDecimal32<Four>(value);
        Assert.Equal(expected, FastDecimal32<Four>.IsOddInteger(fd));
    }
    
    // ShouldConvertToLEssPrecisionWithRounding
    [Theory]
    // Nearest
    [InlineData(24_4357500, 24_4358, MidpointRounding.ToEven)]
    [InlineData(24_4357500, 24_4358, MidpointRounding.AwayFromZero)]
    [InlineData(24_4358500, 24_4358, MidpointRounding.ToEven)]
    [InlineData(24_4358500, 24_4359, MidpointRounding.AwayFromZero)]
    [InlineData(-24_4357500, -24_4358, MidpointRounding.ToEven)]
    [InlineData(-24_4357500, -24_4358, MidpointRounding.AwayFromZero)]
    [InlineData(-24_4358500, -24_4358, MidpointRounding.ToEven)]
    [InlineData(-24_4358500, -24_4359, MidpointRounding.AwayFromZero)]
    // Directed
    [InlineData(24_4358999, 24_4358, MidpointRounding.ToZero)]
    [InlineData(24_4358999, 24_4358, MidpointRounding.ToNegativeInfinity)]
    [InlineData(24_4358999, 24_4359, MidpointRounding.ToPositiveInfinity)]
    [InlineData(-24_4358999, -24_4358, MidpointRounding.ToZero)]
    [InlineData(-24_4358999, -24_4359, MidpointRounding.ToNegativeInfinity)]
    [InlineData(-24_4358999, -24_4358, MidpointRounding.ToPositiveInfinity)]
    
    public void ShouldChangePrecisionUsingProvidedMidpointRoundingWhenTargetFastDecimalHasLessPrecision(int initial, int expected, MidpointRounding rounding)
    {
        var initialFd = new FastDecimal32<Seven>(initial);
        var expectedFd = new FastDecimal32<Four>(expected);
        Assert.Equal(expectedFd, initialFd.ChangePrecision<Four>(rounding));
    }

    [Fact]
    public void ShouldChangePrecisionWhenTargetFastDecimalHasMorePrecision()
    {
        var fd = new FastDecimal32<Four>(89_3467);
        var expected = new FastDecimal32<Seven>(89_3467000);
        Assert.Equal(expected, fd.ChangePrecision<Seven>());
    }

    [Fact]
    public void ChangePrecisionShouldThrowExceptionWhenValueIsOutOfRangeForTargetFastDecimal()
    {
        var fd = new FastDecimal32<Four>(4389_3467);
        Assert.Throws<OverflowException>(() =>
            fd.ChangePrecision<Ten>());
    }

    [Theory]
    [InlineData(0, 64564, "64564")]
    [InlineData(0, -64564, "-64564")]
    [InlineData(7, 0_0000000, "0.0000000")]
    [InlineData(7, 54_5475468, "54.5475468")]
    [InlineData(7, -54_5475468, "-54.5475468")]
    public void ShouldCastFastDecimalToDecimal(int fractionalDigits, int fdValue, string expectedDecimalString)
    {
        var expectedDecimal = decimal.Parse(expectedDecimalString, CultureInfo.InvariantCulture);
        CustomizableFractionalDigits.Digits = fractionalDigits;
        var fd = new FastDecimal32<CustomizableFractionalDigits>(fdValue);
        Assert.Equal(expectedDecimal, (decimal) fd);
    }

    [Fact]
    public void ShouldCastDecimalToFastDecimalWithHigherPrecisionWhenValueIsIWithinRange()
    {
        var d = 345.435m;
        var expectedFd = new FastDecimal32<Five>(345_43500);
        Assert.Equal(expectedFd, (FastDecimal32<Five>) d);
    }

    [Theory]
    [InlineData("1.15955", 1_1596)]
    [InlineData("431.15965", 431_1596)]
    public void ShouldCastDecimalToFastDecimalWithLowerPrecisionUsingRoundingToEven(string decimalString, int expectedFdValue)
    {
        var d = decimal.Parse(decimalString, CultureInfo.InvariantCulture);
        var expectedFd = new FastDecimal32<Four>(expectedFdValue);
        Assert.Equal(expectedFd, (FastDecimal32<Four>) d);
    }

    [Fact]
    public void ShouldOverflowInUncheckedEnvironmentWhenCastingDecimalToFastDecimalAndValueIsOutsideRange()
    {
        var d = 432879823329832489.0159m;
        var expectedFd = new FastDecimal32<Four>(-179233_5313);
        Assert.Equal(expectedFd, (FastDecimal32<Four>) d);
    }

    [Fact]
    public void ShouldThrowExceptionInCheckedEnvironmentWhenCastingDecimalToFastDecimalAndValueIsOutsideRange()
    {
        var d = 432879823329832489.0159m;
        Assert.Throws<OverflowException>(() => 
            checked((FastDecimal32<Four>) d));
    }
}