using BenchmarkDotNet.Attributes;
using FastDecimal.FractionalDigits;

namespace FastDecimal.Benchmarks;

[DisassemblyDiagnoser(maxDepth: 10)]
public class DivisionBenchmark
{
    private readonly FastDecimal64<FourFractionalDigits> _fastDecimalSmallDividend = new (443_4242);
    private readonly FastDecimal64<FourFractionalDigits> _fastDecimalBigDividend = new (4454343665463_4242);
    private readonly FastDecimal64<FourFractionalDigits> _fastDecimalDivisor = new (3_4242);
    
    private readonly FastDecimal64<FourFractionalDigits> _fastDecimalOverflowDividend = new (443544587909298_4242);
    private readonly FastDecimal64<FourFractionalDigits> _fastDecimalOverflowDivisor = new (0_0001);
    
    private readonly decimal _decimal32BitDividend = 443.4242m;
    private readonly decimal _decimal32BitDivisor = 2.5m;
    private readonly decimal _decimal64BitDividend = 4454343368527000.0000m;
    private readonly decimal _decimal64BitDivisor = 450000.0000m;
     
    [Benchmark]
    public FastDecimal64<FourFractionalDigits> FastDecimalDivision64BitIntermediateResult()
    {
        return _fastDecimalSmallDividend / _fastDecimalDivisor;
    }
    
    [Benchmark]
    public FastDecimal64<FourFractionalDigits> FastDecimalCheckedDivision64BitIntermediateResult()
    {
        return checked(_fastDecimalSmallDividend / _fastDecimalDivisor);
    }
    
    [Benchmark]
    public FastDecimal64<FourFractionalDigits> FastDecimalDivision128BitIntermediateResult()
    {
        return _fastDecimalBigDividend / _fastDecimalDivisor;
    }
    
    [Benchmark]
    public FastDecimal64<FourFractionalDigits> FastDecimalCheckedDivision128BitIntermediateResult()
    {
        return checked(_fastDecimalBigDividend / _fastDecimalDivisor);
    }
    
    [Benchmark]
    public FastDecimal64<FourFractionalDigits> FastDecimalDivisionOverflow()
    {
        return _fastDecimalOverflowDividend / _fastDecimalOverflowDivisor;
    }
    
    [Benchmark]
    public decimal DecimalDivision32BitBy32Bit()
    {
        return _decimal32BitDividend / _decimal32BitDivisor;
    }
    
    [Benchmark]
    public decimal DecimalDivision64BitBy32Bit()
    {
        return _decimal64BitDividend / _decimal32BitDivisor;
    }
    
    [Benchmark]
    public decimal DecimalDivision64BitBy64Bit()
    {
        return _decimal64BitDividend / _decimal64BitDivisor;
    }
}