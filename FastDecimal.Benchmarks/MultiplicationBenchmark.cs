using BenchmarkDotNet.Attributes;
using FastDecimal.FractionalDigits;

namespace FastDecimal.Benchmarks;

[DisassemblyDiagnoser(maxDepth: 10)]
public class MultiplicationBenchmark
{
    private readonly FastDecimal64<Four> _fastDecimalSmall = new (443_4242);
    private readonly FastDecimal64<Four> _fastDecimalBig = new (1223434_5456);
    private readonly FastDecimal64<Twelve> _fastDecimalVeryBig = new (954_095490905490);
    private readonly decimal _decimalSmall = (decimal) new FastDecimal64<Four>(443_4242);
    private readonly decimal _decimalBig = (decimal) new FastDecimal64<Four>(1223434_5456);
    private readonly decimal _decimalVeryBig = (decimal) new FastDecimal64<Twelve>(954_095490905490);
    private readonly decimal _decimalMax = 10m/3m;
    

    [Benchmark]
    public FastDecimal64<Four> FastDecimalMultiplicationSmall()
    {
        var fd = _fastDecimalSmall;
        return fd * fd;
    }
    
    [Benchmark]
    public FastDecimal64<Four> FastDecimalCheckedMultiplicationSmall()
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
    public FastDecimal64<Four> FastDecimalMultiplicationBig()
    {
        var fd = _fastDecimalBig;
        return fd * fd;
    }
    
    [Benchmark]
    public FastDecimal64<Four> FastDecimalCheckedMultiplicationBig()
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
    public FastDecimal64<Twelve> FastDecimalMultiplicationVeryBig()
    {
        var fd = _fastDecimalVeryBig;
        return fd * fd;
    }
    
    [Benchmark]
    public FastDecimal64<Twelve> FastDecimalCheckedMultiplicationVeryBig()
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