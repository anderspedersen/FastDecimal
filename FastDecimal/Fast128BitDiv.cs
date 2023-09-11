using System.Runtime.CompilerServices;

namespace FastDecimal;

// The algorithm and constants used for division are based on the paper 'Division by invariant integers using multiplication' (1994)
internal static class Fast128BitDiv
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static (UInt128 q, UInt128 rem) DecDivRem128By128(ulong nHigh, ulong nLow, int digits)
    {
        var mHigh = GetMHigh(digits);
        var mLow = GetMLow(digits);
        var sh1 = GetSh1(digits);
        var sh2 = GetSh2(digits);

        var n = new UInt128(nHigh, nLow);

        var nShifted = n >> sh1;

        var upper1 = new UInt128(0UL, Math.BigMul(mHigh, nShifted.Lower, out var low1));
        var upper2 = new UInt128(0UL,Math.BigMul(nShifted.Upper, mLow, out var low2));
        var u3h = Math.BigMul(nShifted.Upper, mHigh, out var u3l);
        var upper3 = new UInt128(u3h, u3l);

        var to = Math.BigMul(nShifted.Lower, mLow, out var ulow);

        var upper = upper1 + upper2 + upper3;

        var t1 = new UInt128(0UL, low1);
        var t2 = new UInt128(0UL, low2);
        var t3 = new UInt128(0UL, to);
        var t4 = (t1 + t2 + t3) >> 64;

        var t = upper + t4;
        
        var q = t >> sh2;
        var rem = n - q * new UInt128(GetDivisorHigh(digits), GetDivisorLow(digits));

        return (q, rem);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ulong GetDivisorLow(int digits)
    {
        ReadOnlySpan<ulong> divisors = new ulong[]
        {
            0x1,
            0xa,
            0x64,
            0x3e8,
            0x2710,
            0x186a0,
            0xf4240,
            0x989680,
            0x5f5e100,
            0x3b9aca00,
            0x2540be400,
            0x174876e800,
            0xe8d4a51000,
            0x9184e72a000,
            0x5af3107a4000,
            0x38d7ea4c68000,
            0x2386f26fc10000,
            0x16345785d8a0000,
            0xde0b6b3a7640000,
            0x8ac7230489e80000,
            0x6bc75e2d63100000,
            0x35c9adc5dea00000,
            0x19e0c9bab2400000,
            0x2c7e14af6800000,
            0x1bcecceda1000000,
            0x161401484a000000,
            0xdcc80cd2e4000000,
            0x9fd0803ce8000000,
            0x3e25026110000000,
            0x6d7217caa0000000,
            0x4674edea40000000,
            0xc0914b2680000000,
            0x85acef8100000000,
            0x38c15b0a00000000,
            0x378d8e6400000000,
            0x2b878fe800000000,
            0xb34b9f1000000000,
            0xf436a000000000,
            0x98a224000000000,

        };

        return divisors[digits];
    }
    
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ulong GetDivisorHigh(int digits)
    {
        ReadOnlySpan<ulong> divisors = new ulong[]
        {
            0x0,
            0x0,
            0x0,
            0x0,
            0x0,
            0x0,
            0x0,
            0x0,
            0x0,
            0x0,
            0x0,
            0x0,
            0x0,
            0x0,
            0x0,
            0x0,
            0x0,
            0x0,
            0x0,
            0x0,
            0x5,
            0x36,
            0x21e,
            0x152d,
            0xd3c2,
            0x84595,
            0x52b7d2,
            0x33b2e3c,
            0x204fce5e,
            0x1431e0fae,
            0xc9f2c9cd0,
            0x7e37be2022,
            0x4ee2d6d415b,
            0x314dc6448d93,
            0x1ed09bead87c0,
            0x13426172c74d82,
            0xc097ce7bc90715,
            0x785ee10d5da46d9,
            0x4b3b4ca85a86c47a,
        };

        return divisors[digits];
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static int GetSh2(int digits)
    {
        ReadOnlySpan<int> sh2 = new int[]
        {
            0,
            3,
            4,
            4,
            13,
            14,
            15,
            23,
            11,
            29,
            31,
            36,
            37,
            42,
            46,
            20,
            51,
            56,
            23,
            62,
            24,
            28,
            73,
            74,
            79,
            83,
            85,
            88,
            93,
            96,
            39,
            41,
            41,
            109,
            112,
            116,
            118,
            122,
            125,
        };

        return sh2[digits];
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static int GetSh1(int digits)
    {
        ReadOnlySpan<int> sh1 = new int[]
        {
            0,
            0,
            0,
            3,
            0,
            0,
            0,
            0,
            8,
            0,
            0,
            0,
            0,
            0,
            0,
            15,
            0,
            0,
            18,
            0,
            20,
            21,
            0,
            0,
            0,
            0,
            0,
            0,
            0,
            0,
            30,
            31,
            32,
        };
        return sh1[digits];
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static ulong GetMLow(int digits)
    {
        ReadOnlySpan<ulong> mLow = new ulong[]
        {
            0,
            0xCCCCCCCCCCCCCCCDUL,
            0x8F5C28F5C28F5C29UL,
            0xD916872B020C49BBUL,
            0xD3C36113404EA4A9UL,
            0xC3F3E0370CDC8755UL,
            0x5A63F9A49C2C1B11UL,
            0x3D32907604691B4DUL,
            0xF9FB841A566D74F9UL,
            0x31680A88F8953031UL,
            0xAD5CD10396A21347UL,
            0xF78F69A51539D749UL,
            0xFE4FE1EDD10B9175UL,
            0x9432D2F9035837DDUL,
            0x538484C19EF38C95UL,
            0xB3643E74DC052FD9UL,
            0xD30BAF9A1E626A6DUL,
            0x9BEFEB9FAD487C3UL,
            0x5741CEBFCC8B9891UL,
            0x9598F4F1E8361973UL,
            0x446BAA23D2EC729BUL,
            0xA7BEED3F6FC16EBDUL,
            0x5324C68B12DD6339UL,
            0xDD6DC14F03C5E0A5UL,
            0xC4926A9672793543UL,
            0x3A83DDBD83F52205UL,
            0x4A9B257F019540CFUL,
            0x3BAF513267AA9A3FUL,
            0x8BCA9D6E188853FDUL,
            0x96EE45813A04331UL,
            0x7853F0C684960DE7UL,
            0x2D0FF3D203AB3E53UL,
            0xA29CCA5D33EF0C77UL,
            0xECB1AD8AEACDD58FUL,
            0xBD5AF13BEF0B113FUL,
            0x955E4EC64B44E865UL,
            0x6EF285E8EAE85CF5UL,
            0x7E50D64177DA2E55UL,
            0xCB73DE9AC6482511UL,
        };

        return mLow[digits];
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static ulong GetMHigh(int digits)
    {
        ReadOnlySpan<ulong> mHigh = new ulong[]
        {
            0,
            0xCCCCCCCCCCCCCCCCUL,
            0x28F5C28F5C28F5C2UL,
            0x20C49BA5E353F7CEUL,
            0xD1B71758E219652BUL,
            0x29F16B11C6D1E108UL,
            0x08637BD05AF6C69BUL,
            0xD6BF94D5E57A42BCUL,
            0x15798EE2308C39DUL,
            0x89705F4136B4A597UL,
            0x36F9BFB3AF7B756FUL,
            0xAFEBFF0BCB24AAFEUL,
            0x232F33025BD42232UL,
            0x709709A125DA0709UL,
            0xB424DC35095CD80FUL,
            0x24075F3DCEAC2UL,
            0x39A5652FB1137856UL,
            0xB877AA3236A4B449UL,
            0x24E4BBA3A487UL,
            0x760F253EDB4AB0D2UL,
            0x2F394219248UL,
            0x971DA05074DUL,
            0xF1C90080BAF72CB1UL,
            0x305B66802564A289UL,
            0x9ABE14CD44753B52UL,
            0xF79687AED3EEC551UL,
            0x63090312BB2C4EEDUL,
            0x4F3A68DBC8F03F24UL,
            0xFD87B5F28300CA0DUL,
            0xCAD2F7F5359A3B3EUL,
            0x289097FDDUL,
            0x2073ACCB1UL,
            0x67D88F56UL,
            0xA6274BBDD0FADD61UL,
            0x84EC3C97DA624AB4UL,
            0xD4AD2DBFC3D07787UL,
            0x5512124CB4B9C969UL,
            0x881CEA14545C7575UL,
            0x6CE3EE76A9E3912AUL,
        };

        return mHigh[digits];
    }
}