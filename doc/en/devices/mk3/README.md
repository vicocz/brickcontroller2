# MK 3.0

MK 3.0 is a Mould King Bluetooth LE module that provides connections for two LEGO cross axles.

![MK 3.0](../../../../BrickController2/BrickController2/UI/Images/mk3_image.png)

## Channels

The MK 3.0 module exposes 4 channels:

| Channels | Purpose | Output |
|---|---|---|
| A  | Axle left | -100 % to +100 % |
| B  | Axle right | -100 % to +100 % |
| C  | internal Function 1 | OFF / ON |
| D  | internal Function 2 | OFF / ON |

## Getting started

⚠️ **The module is not discovered by scanning!** ⚠️

1. Open the device list and add the device manually.
2. In the manual device list, switch on **MK 3.0 Device**.
3. Power on the MK 3.0 module.
4. Use the device in a creation: assign its channels to controller actions.

## Notes

- Output is sent as Bluetooth advertising, so the module does not report a connection state or battery voltage.
- The connect telegram is sent again after all channels have stayed at zero for a few seconds.

## See also

- [Devices](../../pages/device-list/README.md)
- [Controller action](../../pages/controller-action/README.md)
