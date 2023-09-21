using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using FastDecimal.FractionalDigits;

namespace FastDecimal.Benchmarks;

[DisassemblyDiagnoser(maxDepth: 10)]
[SimpleJob(RuntimeMoniker.Net80)]
[SimpleJob(RuntimeMoniker.Net70)]
public class MultiplicationBenchmark
{
    private readonly FastDecimal64<FourFractionalDigits> _fastDecimalSmall = new (443_4242);
    private readonly FastDecimal64<FourFractionalDigits> _fastDecimalBig = new (1223434_5456);
    private readonly FastDecimal64<TwelveFractionalDigits> _fastDecimalVeryBig = new (954_095490905490);
    private readonly decimal _decimalSmall = (decimal) new FastDecimal64<FourFractionalDigits>(443_4242);
    private readonly decimal _decimalBig = (decimal) new FastDecimal64<FourFractionalDigits>(1223434_5456);
    private readonly decimal _decimalVeryBig = (decimal) new FastDecimal64<TwelveFractionalDigits>(954_095490905490);
    private readonly decimal _decimalMax = 10m/3m;
    

    [Benchmark]
    public FastDecimal64<FourFractionalDigits> FastDecimalMultiplicationSmall()
    {
        var fd = _fastDecimalSmall;
        return fd * fd;
    }
    
    [Benchmark]
    public FastDecimal64<FourFractionalDigits> FastDecimalCheckedMultiplicationSmall()
    {
        var fd = _fastDecimalSmall;
        return checked(fd * fd);
    }
    
    [Benchmark]
    public decimal DecimalMultiplicationSmall()
    {
        var d = _decimalSmall;
        return d * d;
    }
    
    [Benchmark]
    public FastDecimal64<FourFractionalDigits> FastDecimalMultiplicationBig()
    {
        var fd = _fastDecimalBig;
        return fd * fd;
    }
    
    [Benchmark]
    public FastDecimal64<FourFractionalDigits> FastDecimalCheckedMultiplicationBig()
    {
        var fd = _fastDecimalBig;
        return checked(fd * fd);
    }
    
    [Benchmark]
    public decimal DecimalMultiplicationBig()
    {
        var d = _decimalBig;
        return d * d;
    }
    
    [Benchmark]
    public FastDecimal64<TwelveFractionalDigits> FastDecimalMultiplicationVeryBig()
    {
        var fd = _fastDecimalVeryBig;
        return fd * fd;
    }
    
    [Benchmark]
    public FastDecimal64<TwelveFractionalDigits> FastDecimalCheckedMultiplicationVeryBig()
    {
        var fd = _fastDecimalVeryBig;
        return checked(fd * fd);
    }
    
    [Benchmark]
    public decimal DecimalMultiplicationVeryBig()
    {
        var d = _decimalVeryBig;
        return d * d;
    }
    
    [Benchmark]
    public decimal DecimalMultiplicationMax()
    {
        var d = _decimalMax;
        return d * d;
    }
}