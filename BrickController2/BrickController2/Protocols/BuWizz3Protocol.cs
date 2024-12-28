namespace BrickController2.Protocols;

internal static class BuWizz3Protocol
{
    public const double CurrentLimitStep = 35;

    public static byte[] ActivteShelfMode() => [ 0xA1 ];

    public static byte[] SetCurrentLimits(double v0, double v1, double v2, double v3, double v4, double v5)
    {
        byte l0 = ConvertToRaw(v0);
        byte l1 = ConvertToRaw(v1);
        byte l2 = ConvertToRaw(v2);
        byte l3 = ConvertToRaw(v3);
        byte lA = ConvertToRaw(v4);
        byte lB = ConvertToRaw(v5);
        return [0x38, l0, l1, l2, l3, lA, lB];

        static byte ConvertToRaw(double currentLimit)
            => (byte)(currentLimit / CurrentLimitStep);
    }
}
