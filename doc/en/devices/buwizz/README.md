# BuWizz

BuWizz (1.0) is a Bluetooth LE battery brick that powers and controls up to four motors.

![BuWizz](../../../../BrickController2/BrickController2/UI/Images/buwizz_image.png)

## Channels

The device exposes 4 channels:

| Channels | Purpose | Output |
|---|---|---|
| 1 - 4 | PowerFunctions motors | -100 % to +100 % |

## Getting started

1. Power on the BuWizz.
2. Open the device list and press **Scan for new devices**. Found devices are added to the list.
3. Use the device in a creation: assign its channels to controller actions.

## Output level

The output level limits motor power. The available levels are **Low**, **Normal** (default) and **High**. The persisted default is **Normal** and can be changed using **Output level** on the device page.

## Notes

- Higher output levels draw more current from the battery and shorten the running time.
- The battery voltage is not shown on the device page.

## See also

- [Devices](../../pages/device-list/README.md)
- [Controller action](../../pages/controller-action/README.md)
