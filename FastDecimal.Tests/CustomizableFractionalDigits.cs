using FastDecimal.FractionalDigits;

namespace FastDecimal.Tests;

// Don't use this or something similar outside of testing. Performance will suffer if FractionalDigits is
// not a compile time constant and behavior is undefined if FractionalDigits changes value.
public struct CustomizableFractionalDigits : IFractionalDigits
{
    public static int Digits { get; set; }
    public int FractionalDigits => Digits;
}