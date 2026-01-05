using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

public interface SizedBuffer { }

[StructLayout(LayoutKind.Explicit)]
public unsafe struct Size32 : SizedBuffer
{
    [FieldOffset(0)] public fixed byte _buffer[32];
}

[StructLayout(LayoutKind.Explicit)]
public unsafe struct Size64 : SizedBuffer
{
    [FieldOffset(0)] public fixed byte _buffer[64];
}

[StructLayout(LayoutKind.Explicit)]
public unsafe struct Size128 : SizedBuffer
{
    [FieldOffset(0)] public fixed byte _buffer[128];
}


[StructLayout(LayoutKind.Sequential)]
public unsafe struct ArrayInline<T, TSize>
    where T : unmanaged
    where TSize : unmanaged, SizedBuffer
{
    private TSize inlinebuffer;

    private static readonly int _length = sizeof(TSize) / sizeof(T);

    public readonly int Length => _length;

    public T this[int index]
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get
        {
            if((uint)index >= (uint)_length)
                throw new IndexOutOfRangeException();

            fixed(TSize* p = &inlinebuffer)
            {
                return ((T*)p)[index];
            }
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        set
        {
            if((uint)index >= (uint)_length)
                throw new IndexOutOfRangeException();

            fixed(TSize* p = &inlinebuffer)
            {
                ((T*)p)[index] = value;
            }
        }
    }
}
