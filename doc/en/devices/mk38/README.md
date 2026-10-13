# MK 3.8

MK 3.8 is a Mould King Bluetooth Low Energy (BLE) module featuring three integrated motors with LEGO-compatible cross-axle outputs and provides two electrical mini-plug connectors.

![MK 3.8](../../../../BrickController2/BrickController2/UI/Images/mk3_8_image.png)

## Channels

The MK 3.8 module exposes 5 channels:

| Channels | Purpose | Output |
|---|---|---|
| A  | Axle left | -100 % to +100 % |
| B  | Plug 1 | -100 % to +100 % |
| C  | Axle right | -100 % to +100 % |
| D  | Plug 2 | -100 % to +100 % |
| E  | Axle top | left / off / right |

## Getting started

⚠️ **The module is not discovered by scanning!** ⚠️

1. Open the device list and add the device manually.
2. In the manual device list, switch on **MK 3.8 Device**.
3. Power on the MK 3.8 module.
4. Use the device in a creation: assign its channels to controller actions.

## Notes

- Output is sent as Bluetooth advertising, so the module does not report a connection state or battery voltage.
- The connect telegram is sent again after all channels have stayed at zero for a few seconds.

## See also

- [Devices](../../pages/device-list/README.md)
- [Controller action](../../pages/controller-action/README.md)
