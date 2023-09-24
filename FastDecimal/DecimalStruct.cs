using System.Runtime.InteropServices;

namespace FastDecimal;

[StructLayout(LayoutKind.Explicit)]
internal struct DecimalStruct
{
    private const int ScaleShift = 16;
    
    [FieldOffset(0)]
    private int _flags;
    
    // Used for FastDecimal64
    [FieldOffset(4)]
    private uint _high32;
    [FieldOffset(8)]
    private ulong _low64;
    
    // Used for FastDecimal32
    [FieldOffset(4)]
    private ulong _high64;
    [FieldOffset(12)]
    private uint _low32;

    public uint High32 => _high32;
    public ulong Low64 => _low64;
    public ulong High64 => _high64;
    public uint Low32 => _low32;
    public byte Scale => (byte)(_flags >> ScaleShift);
    public bool Negative => _flags < 0;
}