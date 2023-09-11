using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using FastDecimal.FractionalDigits;

namespace FastDecimal.Benchmarks;

[DisassemblyDiagnoser(maxDepth: 10)]
[SimpleJob(RuntimeMoniker.Net80)]
[SimpleJob(RuntimeMoniker.Net70)]
public class AdditionBenchmark
{
    private readonly FastDecimal64<FourFractionalDigits> _fd1 = new FastDecimal64<FourFractionalDigits>(234_8945);
    private readonly FastDecimal64<FourFractionalDigits> _fd2 = new FastDecimal64<FourFractionalDigits>(3289_9832);
    
    private readonly decimal _d1 = (decimal) new FastDecimal64<FourFractionalDigits>(234_8945);
    private readonly decimal _d2 = (decimal) new FastDecimal64<FourFractionalDigits>(3289_9832);
    
    
    
    [Benchmark(Baseline = true)]
    public FastDecimal64<FourFractionalDigits> FastDecimalAddition()
    {
        return _fd1 + _fd2;
    }
    
    [Benchmark]
    public FastDecimal64<FourFractionalDigits> FastDecimalCheckedAddition()
    {
        return checked(_fd1 + _fd2);
    }
    
    [Benchmark]
    public decimal DecimalAddition()
    {
        return _d1 + _d2;
    }
}