# SBrick

SBrick is a Bluetooth LE brick that powers and controls up to four motors.

![SBrick](../../../../BrickController2/BrickController2/UI/Images/sbrick_image.png)

## Channels

The device exposes 4 channels:

| Channels | Purpose | Output |
|---|---|---|
| 1 - 4 | Ports A, B, C, D | -100 % to +100 % |

## Getting started

1. Power on the SBrick.
2. Open the device list and press **Scan for new devices**. Found devices are added to the list.
3. Use the device in a creation: assign its channels to controller actions.

## Notes

- The ports are LEGO Power Functions compatible.
- The output changes in steps (the acceleration step is 1/7).
- SBrick Plus is supported as well, but only its output ports.
- The battery voltage is shown on the device page.

## See also

- [Devices](../../pages/device-list/README.md)
- [Controller action](../../pages/controller-action/README.md)
