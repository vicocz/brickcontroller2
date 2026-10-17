# Mould King - MK 4.0

**Mould King - MK 4.0** is a Bluetooth Low Energy (BLE) module that drives up to four motors. Up to three modules can be controlled at the same time.

![Mould King - MK 4.0](../../../../BrickController2/BrickController2/UI/Images/mk4_image.png)

## Channels

Each **Mould King - MK 4.0** module exposes 4 channels:

| Channels | Purpose | Output |
|---|---|---|
| 1 - 4 | Ports A, B, C, D | -100 % to +100 % |

## Getting started

⚠️ **The module is not discovered by scanning!** ⚠️

1. Open the device list to add the device manually.
2. In the manual device list, switch on **Mould King - MK 4.0 Device 1**.
Use **Device 2** and **Device 3** for additional modules.
3. Power on the **Mould King MK 4.0** module.
**Device 1** is the default instance number after power on, the LED blinks once per cycle.
Change the instance number by short-pressing the button on the device. The LED blinks twice per cycle for **Device 2** and three times per cycle for **Device 3**.
4. Use the device in a creation: assign its channels to controller actions.

## Notes

- Up to three **Mould King - MK 4.0** modules are supported. All of them share one Bluetooth advertising stream.
- Output is sent as Bluetooth advertising, so the module does not report a connection state or battery voltage.
- The connect telegram is sent again after all channels have stayed at zero for a few seconds.

## See also

- [Devices](../../pages/device-list/README.md)
- [Controller action](../../pages/controller-action/README.md)
