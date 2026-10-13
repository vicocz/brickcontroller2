# MK 4.0

MK 4.0 is a Mould King Bluetooth Low Energy (BLE) module that drives up to four motors. Up to three modules can be controlled at the same time.

![MK 4.0](../../../../BrickController2/BrickController2/UI/Images/mk4_image.png)

## Channels

Each module exposes 4 channels:

| Channels | Purpose | Output |
|---|---|---|
| A - D | Motors | -100 % to +100 % |

## Getting started

⚠️ **The module is not discovered by scanning!** ⚠️

1. Open the device list and add the device manually.
2. In the manual device list, switch on **MK 4.0 Device 1**.
Use **Device 2** and **Device 3** for additional modules.
3. Power on the MK 4.0 module.
**Device 1** is the default instance number after power on, the Led is cyclic blinking once.
Change the instance number by short-pressing the button on the device. The Led is cyclic blinking twice for **Device 2** and three times for **Device 3**.
4. Use the device in a creation: assign its channels to controller actions.

## Notes

- Up to three MK 4.0 modules are supported. All of them share one Bluetooth advertising stream.
- Output is sent as Bluetooth advertising, so the module does not report a connection state or battery voltage.
- The connect telegram is sent again after all channels have stayed at zero for a few seconds.

## See also

- [Devices](../../pages/device-list/README.md)
- [Controller action](../../pages/controller-action/README.md)
