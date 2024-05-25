# FastDecimal
FastDecimal is a fast, fixed-point decimal type with strongly typed, configurable precision for .NET.

FastDecimal uses generics to configure the precision, so the precision is part of the type. If you for example want a 64-bit decimal with six fractional digits you will declare it as
`FastDecimal64<Six>`. This allows the JIT compiler to output custom assembly code tailored to this precision.

## Usage

`FastDecimal` comes in a 32-bit and a 64-bit variant, `FastDecimal32<T>` and `FastDecimal64<T>`. `FastDecimal` has the same range as the underlying integer, eg. -9,223,372,036,854,775,808 to 9,223,372,036,854,775,807 for `FastDecimal64` and the generic parameter decides where the decimal point is, so for example `FastDecimal64<Four>` will have four fractional digits and a range from  -922,337,203,685,477.5808 to 922,337,203,685,477.5807.

### Creating instances

The simplest way to initialize a `FastDecimal` is to construct it from a `decimal`:

```
FastDecimal64<Four> price = new (23.54m);
```

`FastDecimal` implements `IParsable` and `ISpanParsable`, so you can create a `FastDecimal` from a string or directly in a deserializer:

```
var price = FastDecimal64<Four>.Parse("23.54", NumberFormatInfo.InvariantInfo);
```

### Calculations

For doing calculations, `FastDecimal` implements `INumber`, so most operations that you would need for a numeric type will work:

```
var price = new FastDecimal64<Four>(23.54m);
var quantity = new FastDecimal64<Four>(5m);
var commission = new FastDecimal64<Four>(0.20m);
var totalCost = price * quantity + commission;
```

### Checked/unchecked operations

`FastDecimal` implements both [checked and unchecked](https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/statements/checked-and-unchecked) operators. By default operators are unchecked, but if you want arithmetic operations to throw an `OverflowException`, when the result would overflow, then you can use a `checked` statement:
```
var value = checked(FastDecimal64<Four>.MaxValue++) // will throw OverflowException
```
This behavior is different from `decimal` which will always throw an `OverflowException` when an operation results in an overflow.

### Rounding

When doing multiplication and division the result might have more fractional digits, than the input numbers, e.g. `2.5 * 0.1 = 0.25`. If the result of a calculation cannot be represented exactly then `FastDecimal` will by default round to the nearest number and if the number is halfway between two numbers it will round to the nearest even number:
```
var a = new FastDecimal64<One>(2.5m);
var b = new FastDecimal64<One>(0.1m);
var c = a * b; // c will be 0.2 because it is the nearest even number
```
This is known as banker's rounding and is also the default behavior for `decimal` and IEEE 754 floating point numbers such as `float` and `double`.

If you want to use a different rounding mode `FastDecimal` provides methods where you can provide the rounding mode as a parameter:

```
var a = new FastDecimal64<One>(2.5m);
var b = new FastDecimal64<One>(0.1m);
var c = FastDecimal64<One>.Multiply(a, b, MidpointRounding.AwayFromZero); // c will be 0.3 because it rounds away from zero
```

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

