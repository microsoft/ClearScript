using System;

internal static class EnumUtils
{
    public static unsafe bool HasFlagNonAlloc<T>(this T x, T y) where T : unmanaged, Enum
    {
        switch (sizeof(T))
        {
            case sizeof(byte):
                return (*(byte*) &x & *(byte*) &y) != 0;

            case sizeof(short):
                return (*(short*) &x & *(short*) &y) != 0;

            case sizeof(int):
                return (*(int*) &x & *(int*) &y) != 0;

            case sizeof(long):
                return (*(long*) &x & *(long*) &y) != 0L;

            default:
                return false;
        }
    }
}