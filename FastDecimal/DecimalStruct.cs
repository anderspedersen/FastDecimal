using System.Runtime.InteropServices;

namespace FastDecimal;

[StructLayout(LayoutKind.Sequential)]
internal struct DecimalStruct
{
    private const int ScaleShift = 16;
    
    private int _flags;
    private uint _high;
    private ulong _low;

    public uint High => _high;
    public ulong Low => _low;
    public byte Scale => (byte)(_flags >> ScaleShift);
    public bool Negative => _flags < 0;
}