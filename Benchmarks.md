# Benchmarks
These benchmarks were created to test the performance of `FastDecimal`, so I have left out some benchmarks that are unfair to `decimal`. E.g. adding two floating-point decimals with different precision will be slower than adding two floating-point decimals with the same precision, but since fixed-point decimals will have the same precision, this benchmark will not tell me anything about the performance of `FastDecimal`.

To test how `FastDecimal` will perform on *your* workload compared to `decimal` I recommend creating your own benchmark on a realistic workload, rather than extrapolating from these benchmarks.

## Addition / subtraction

Addition and subtraction is very fast for fixed-point decimals. It basically is the same speed as integer addition and subtraction.

|                     Method |      Mean |     Error |    StdDev |
|--------------------------- |----------:|----------:|----------:|
|        FastDecimalAddition | 0.0102 ns | 0.0071 ns | 0.0067 ns |
| FastDecimalCheckedAddition | 0.0000 ns | 0.0000 ns | 0.0000 ns |
|            DecimalAddition | 5.6585 ns | 0.0340 ns | 0.0318 ns |
As expected addition is extremely fast for FastDecimal.

|                        Method |      Mean |     Error |    StdDev |
|------------------------------ |----------:|----------:|----------:|
|        FastDecimalSubtraction | 0.0070 ns | 0.0147 ns | 0.0123 ns |
| FastDecimalCheckedSubtraction | 0.0016 ns | 0.0029 ns | 0.0026 ns |
|            DecimalSubtraction | 5.6538 ns | 0.0579 ns | 0.0541 ns |
And same for subtraction.



## Multiplication
In theory, floating-point decimals should have an advantage over fixed-point decimals when doing multiplications, since fixed-point decimals always have to  do expensive divisions, while floating-point decimals can just change the precision if the value is in range. However, as can be seen from the benchmarks `FastDecimal64` is a bit faster than `decimal` in all cases, at least on my machine when running in .NET 8.

|                                  Method |      Mean |     Error |    StdDev |
|---------------------------------------- |----------:|----------:|----------:|
|          FastDecimalMultiplicationSmall |  2.408 ns | 0.0194 ns | 0.0181 ns |
|   FastDecimalCheckedMultiplicationSmall |  2.406 ns | 0.0095 ns | 0.0084 ns |
|              DecimalMultiplicationSmall |  5.501 ns | 0.0186 ns | 0.0174 ns |
First multiplication benchmark is with small values, where intermediate result fits in 64-bit, so `FastDecimal64` will do 64-bit division.

|                                  Method |      Mean |     Error |    StdDev |
|---------------------------------------- |----------:|----------:|----------:|
|            FastDecimalMultiplicationBig |  9.065 ns | 0.0368 ns | 0.0326 ns |
|     FastDecimalCheckedMultiplicationBig |  9.190 ns | 0.0504 ns | 0.0447 ns |
|                DecimalMultiplicationBig |  9.429 ns | 0.0419 ns | 0.0350 ns |
Second multiplication benchmark is with larger values, where intermediate result fits in 96-bit, so `FastDecimal64` will do 128-bit division.


|                                  Method |      Mean |     Error |    StdDev |
|---------------------------------------- |----------:|----------:|----------:|
|        FastDecimalMultiplicationVeryBig |  8.730 ns | 0.0476 ns | 0.0446 ns |
| FastDecimalCheckedMultiplicationVeryBig |  9.264 ns | 0.0939 ns | 0.0832 ns |
|            DecimalMultiplicationVeryBig | 21.025 ns | 0.1964 ns | 0.1741 ns |
Last multiplication benchmark is with even larger values, where intermediate result fits in 128-bit, so `FastDecimal64` will do 128-bit division, but `decimal` will also have to do division to fit the value.

## Division
For the division benchmark, I have made sure that the results do not have more fractional digits than can be stored in the `FastDecimal`, since the benchmark would otherwise be unfair to the `decimal`. In these benchmarks I have seperated the `FastDecimal` abd the `decimal` benchmarks, since each type has different best/worst cases.

|                                             Method |      Mean |     Error |    StdDev |
|--------------------------------------------------- |----------:|----------:|----------:|
|         FastDecimalDivision64BitIntermediateResult |  8.656 ns | 0.0403 ns | 0.0358 ns |
|  FastDecimalCheckedDivision64BitIntermediateResult |  6.550 ns | 0.0331 ns | 0.0310 ns |
|        FastDecimalDivision128BitIntermediateResult | 22.422 ns | 0.0560 ns | 0.0524 ns |
| FastDecimalCheckedDivision128BitIntermediateResult | 23.732 ns | 0.0609 ns | 0.0569 ns |
The two interesting cases for `FastDecimal` division is whether the intermediate result can fit in 64 bits, or if 128 bits are needed. Notice that both cases are faster than the `decimal` benchmarks.

|                                             Method |      Mean |     Error |    StdDev |
|--------------------------------------------------- |----------:|----------:|----------:|
|                        DecimalDivision32BitBy32Bit | 31.966 ns | 0.1190 ns | 0.1055 ns |
|                        DecimalDivision64BitBy32Bit | 29.052 ns | 0.2400 ns | 0.1874 ns |
|                        DecimalDivision64BitBy64Bit | 49.006 ns | 0.3396 ns | 0.2836 ns |
`decimal` can use a faster algorithm if the divisor is 32-bit.
