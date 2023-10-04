# FastDecimal
FastDecimal is a fast, fixed-point decimal type with strongly typed, configurable precision for .NET.

FastDecimal uses generics to configure the precision, so the precision is part of the type. If you for example want a 64-bit decimal with six fractional digits you will declare it as
`FastDecimal64<SixFractionalDigits>`. This allows the JIT compiler to output custom assembly code tailored to this precision.

## Performance

The main reason to use `FastDecimal` over the built-in `decimal` type is for performance since `decimal` strictly can represent more values than `FastDecimal64`. But if you don't need the range offered by `decimal`, then `FastDecimal` can be a lot faster!

The arithmetic operations in FastDecimal are written to be highly performant, and in general, `FastDecimal` should be faster than `decimal`. To get an idea of the performance difference between `FastDecimal` and `decimal` you can take a look at [the benchmarks](Benchmarks.md).

`FastDecimal64` and `FastDecimal32` are approximately equally fast (on 64-bit systems) when doing the same calculations, but `FastDecimal32` takes up less memory, so it might be a good choice if you are allocating a lot of objects with decimal values, e.g. if you are getting thousands of price updates per second.

And casting from `FastDecimal32` to `FastDecimal64` is extremely fast, so you can easily use `FastDecimal32` for storage and `FastDecimal64` for doing calculations.

## Comparison with decimal

|                       | decimal                                    | FastDecimal64                                          | FastDecimal32                  |
|-----------------------|--------------------------------------------|--------------------------------------------------------|--------------------------------|
| Integer range         | -79,228,162,514,264,337,593,543,950,335 - 79,228,162,514,264,337,593,543,950,335 | -9,223,372,036,854,775,808 - 9,223,372,036,854,775,807 | -2,147,483,648 - 2,147,483,647 |
| Scaling factor        | 0 - 28 | 0 - 19 | 0 - 9 |
| Size                  | 128 bit | 64 bit | 32 bit |
| Checked/unchecked     | Checked | Both | Both |
| Default rounding mode | ToEven | ToEven | ToEven |

