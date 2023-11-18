# Benchmarks
These benchmarks were created to test the performance of `FastDecimal`, so I have left out some benchmarks that are unfair to `decimal`. E.g. adding two floating-point decimals with different precision will be slower than adding two floating-point decimals with the same precision, but since fixed-point decimals will have the same precision, this benchmark will not tell me anything about the performance of `FastDecimal`.

To test how `FastDecimal` will perform on *your* workload compared to `decimal` I recommend creating your own benchmark on a realistic workload, rather than extrapolating from these benchmarks.

## Addition / subtraction

Addition and subtraction is very fast for fixed-point decimals. It basically is the same speed as integer addition and subtraction.

|                     Method |      Mean |     Error |    StdDev |
|--------------------------- |----------:|----------:|----------:|
|        FastDecimalAddition | 0.0030 ns | 0.0048 ns | 0.0045 ns |
| FastDecimalCheckedAddition | 0.0025 ns | 0.0044 ns | 0.0039 ns |
|            DecimalAddition | 5.6897 ns | 0.0031 ns | 0.0024 ns |

As expected addition is extremely fast for FastDecimal.

|                        Method |      Mean |     Error |    StdDev |
|------------------------------ |----------:|----------:|----------:|
|        FastDecimalSubtraction | 0.0005 ns | 0.0020 ns | 0.0016 ns |
| FastDecimalCheckedSubtraction | 0.0002 ns | 0.0004 ns | 0.0003 ns |
|            DecimalSubtraction | 5.7040 ns | 0.0339 ns | 0.0317 ns |

And same for subtraction.



## Multiplication
In theory, floating-point decimals should have an advantage over fixed-point decimals when doing multiplications, since fixed-point decimals always have to  do expensive divisions, while floating-point decimals can just change the precision if the value is in range. However, as can be seen from the benchmarks `FastDecimal64` is a bit faster than `decimal` in all cases, at least on my machine when running in .NET 8.

|                                  Method |      Mean |     Error |    StdDev |
|---------------------------------------- |----------:|----------:|----------:|
|          FastDecimalMultiplicationSmall |  2.231 ns | 0.0067 ns | 0.0056 ns |
|   FastDecimalCheckedMultiplicationSmall |  2.805 ns | 0.0043 ns | 0.0038 ns |
|              DecimalMultiplicationSmall |  5.516 ns | 0.0149 ns | 0.0116 ns |

First multiplication benchmark is with small values, where intermediate result fits in 64-bit, so `FastDecimal64` will do 64-bit division.

|                                  Method |      Mean |     Error |    StdDev |
|---------------------------------------- |----------:|----------:|----------:|
|            FastDecimalMultiplicationBig |  6.973 ns | 0.0072 ns | 0.0060 ns |
|     FastDecimalCheckedMultiplicationBig |  7.592 ns | 0.0044 ns | 0.0037 ns |
|                DecimalMultiplicationBig |  9.513 ns | 0.0341 ns | 0.0319 ns |

Second multiplication benchmark is with larger values, where intermediate result fits in 96-bit, so `FastDecimal64` will do 128-bit division.


|                                  Method |      Mean |     Error |    StdDev |
|---------------------------------------- |----------:|----------:|----------:|
|        FastDecimalMultiplicationVeryBig |  7.082 ns | 0.0060 ns | 0.0050 ns |
| FastDecimalCheckedMultiplicationVeryBig |  7.800 ns | 0.0154 ns | 0.0136 ns |
|            DecimalMultiplicationVeryBig | 21.047 ns | 0.0173 ns | 0.0135 ns |

Last multiplication benchmark is with even larger values, where intermediate result fits in 128-bit, so `FastDecimal64` will do 128-bit division, but `decimal` will also have to do division to fit the value.

## Division
For the division benchmark, I have made sure that the results do not have more fractional digits than can be stored in the `FastDecimal`, since the benchmark would otherwise be unfair to the `decimal`. In these benchmarks I have seperated the `FastDecimal` abd the `decimal` benchmarks, since each type has different best/worst cases.

|                                             Method |      Mean |     Error |    StdDev |
|--------------------------------------------------- |----------:|----------:|----------:|
|         FastDecimalDivision64BitIntermediateResult |  8.054 ns | 0.0286 ns | 0.0267 ns |
|  FastDecimalCheckedDivision64BitIntermediateResult |  6.694 ns | 0.0040 ns | 0.0036 ns |
|        FastDecimalDivision128BitIntermediateResult | 22.753 ns | 0.0143 ns | 0.0119 ns |
| FastDecimalCheckedDivision128BitIntermediateResult | 23.842 ns | 0.0147 ns | 0.0123 ns |

The two interesting cases for `FastDecimal` division is whether the intermediate result can fit in 64 bits, or if 128 bits are needed. Notice that both cases are faster than the `decimal` benchmarks.

|                                             Method |      Mean |     Error |    StdDev |
|--------------------------------------------------- |----------:|----------:|----------:|
|                        DecimalDivision32BitBy32Bit | 31.920 ns | 0.0243 ns | 0.0203 ns |
|                        DecimalDivision64BitBy32Bit | 29.088 ns | 0.1233 ns | 0.1154 ns |
|                        DecimalDivision64BitBy64Bit | 48.096 ns | 0.0677 ns | 0.0565 ns |

`decimal` can use a faster algorithm if the divisor is 32-bit.
