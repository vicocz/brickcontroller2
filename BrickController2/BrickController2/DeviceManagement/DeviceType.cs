using BrickController2.DeviceManagement.Vendors;

namespace BrickController2.DeviceManagement
{
    public enum DeviceType
    {
        Unknown,
        SBrick,
        BuWizz,
        BuWizz2,
        Infrared,
        Boost,
        PoweredUp,
        TechnicHub,
        DuploTrainHub,
        BuWizz3,
        CircuitCubes,
        WeDo2,
        TechnicMove,
        [DeviceVendor(DeviceVendor.MouldKing)]
        MK4,
        [DeviceVendor(DeviceVendor.MouldKing)]
        MK6,
        [DeviceVendor(DeviceVendor.MouldKing)]
        MK_DIY,
        CaDA_RaceCar,
        PfxBrick,
    }
}
