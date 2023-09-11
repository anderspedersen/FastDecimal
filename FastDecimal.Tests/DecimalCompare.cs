using FastDecimal.FractionalDigits;

namespace FastDecimal.Tests;

public class DecimalCompare
{
    [Fact]
    public void TestChangePrecision()
    {
        var rand = new Random();
        for (int i = 0; i < 100_000_000; i++)
        {

            var fastDec = new FastDecimal64<FourFractionalDigits>(rand.NextInt64(long.MinValue, long.MaxValue));
            var dec = (decimal) fastDec;

            var rounding = (MidpointRounding) rand.Next(0, 4);

            var roundedFastDec = fastDec.ChangePrecision<TwoFractionalDigits>(rounding);
            var roundedDec = decimal.Round(dec, 2, rounding);

            Assert.Equal(roundedDec, (decimal) roundedFastDec);
        }
    }


    [Fact]
    public void TestMultiply()
    {
        var rand = new Random();
        for (int i = 0; i < 100_000_000; i++)
        {

            var fastDec1 = new FastDecimal64<FourFractionalDigits>(rand.NextInt64(long.MinValue, long.MaxValue)/100000000);
            var fastDec2 = new FastDecimal64<FourFractionalDigits>(rand.NextInt64(long.MinValue, long.MaxValue)/100000000);
            var dec1 = (decimal) fastDec1;
            var dec2 = (decimal) fastDec2;

            var rounding = (MidpointRounding) rand.Next(0, 4);

            var fastProduct = FastDecimal64<FourFractionalDigits>.Multiply(fastDec1,fastDec2, rounding);
            var product = decimal.Round(dec1 * dec2, 4, rounding);

            Assert.Equal(product, (decimal) fastProduct);
        }
    }

    [Fact]
    public void TestDivision()
    {
        var rand = new Random();
        for (int i = 0; i < 100_000_000; i++)
        {

            var fastDec1 = new FastDecimal64<FourFractionalDigits>(rand.NextInt64(long.MinValue/10000, long.MaxValue/10000));
            FastDecimal64<FourFractionalDigits> fastDec2;
            do
            {
                fastDec2 = new FastDecimal64<FourFractionalDigits>(rand.NextInt64(-10000, 10000));
            } while (fastDec2 == new FastDecimal64<FourFractionalDigits>(0));

            var dec1 = (decimal) fastDec1;
            var dec2 = (decimal) fastDec2;

            var fastProduct = fastDec1 / fastDec2;
            var product = decimal.Round(dec1 / dec2, 4, MidpointRounding.ToEven);
            Assert.Equal(product, (decimal) fastProduct);
        }
    }

    [Fact]
    public void TestDivision128()
    {
        var rand = new Random();
        for (int i = 0; i < 100_000_000; i++)
        {

            var fastDec1 = new FastDecimal64<FourFractionalDigits>(rand.NextInt64(long.MinValue, long.MaxValue));
            FastDecimal64<FourFractionalDigits> fastDec2;
            do
            {
                fastDec2 = new FastDecimal64<FourFractionalDigits>(rand.NextInt64(-10000, 10000));
            } while (fastDec2 < new FastDecimal64<FourFractionalDigits>(10000) &&
                     fastDec2 > new FastDecimal64<FourFractionalDigits>(-10000));

            var dec1 = (decimal) fastDec1;
            var dec2 = (decimal) fastDec2;

            var fastProduct = fastDec1 / fastDec2;
            var product = decimal.Round(dec1 / dec2, 4, MidpointRounding.ToEven);
            Assert.Equal(product, (decimal) fastProduct);
        }
    }
}