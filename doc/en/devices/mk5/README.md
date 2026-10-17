# Mould King - MK 5.0

**Mould King - MK 5.0** is a Bluetooth Low Energy (BLE) module featuring three integrated motors with LEGO-compatible cross-axle outputs.

![Mould King - MK 5.0](../../../../BrickController2/BrickController2/UI/Images/mk5_image.png)

## Channels

The **Mould King - MK 5.0** module exposes 5 channels:

| Channels | Purpose | Output |
|---|---|---|
| 1 | Motors A+B | -100 % to +100 % |
| 2 | Motor T | -100 % to +100 % |
| 3 | Effect Shot | OFF / ON |
| 4 | Motors A⇄B+T | -100 % to +100 % |
| 5 Ⓥ | Lock Motor T: A⇄B | OFF / ON |

Channel 5 is a Ⓥirtual channel. When enabled, it modifies the behavior of Channel 4 so that Motors A and B rotate in opposite directions while Motor T remains unchanged.

## Getting started

⚠️ **The module is not discovered by scanning!** ⚠️

1. Open the device list to add the device manually.
2. In the manual device list, switch on **Mould King - MK 5.0 Device**.
3. Power on the **Mould King - MK 5.0** module.
4. Use the device in a creation: assign its channels to controller actions.

## Notes

- Output is sent as Bluetooth advertising, so the module does not report a connection state or battery voltage.
- The connect telegram is sent again after all channels have stayed at zero for a few seconds.

## See also

- [Devices](../../pages/device-list/README.md)
- [Controller action](../../pages/controller-action/README.md)
