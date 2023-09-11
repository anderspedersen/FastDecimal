using BenchmarkDotNet.Running;
using FastDecimal.Benchmarks;

BenchmarkRunner.Run(new[]
{
    typeof(AdditionBenchmark),
    typeof(DivisionBenchmark),
    typeof(MultiplicationBenchmark),
    typeof(SubtractionBenchmark)
});