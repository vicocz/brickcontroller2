# BuWizz 2

BuWizz 2 is a Bluetooth LE battery brick that powers and controls up to four motors.

![BuWizz 2](../../../../BrickController2/BrickController2/UI/Images/buwizz_image.png)

## Channels

The device exposes 4 channels:

| Channels | Purpose | Output |
|---|---|---|
| 1 - 4 | PowerFunctions motors | -100 % to +100 % |

## Getting started

1. Power on the BuWizz 2.
2. Open the device list and press **Scan for new devices**. Found devices are added to the list.
3. Use the device in a creation: assign its channels to controller actions.

## Output level

The output level limits motor power. Four levels are available: **Low**, **Normal** (default), **High**, and **Ludicrous**. Change the default with the **Default output level** setting. While playing, you can change the current level with the **Set output level** macro.

## Notes

- Higher output levels draw more current from the battery and shorten the running time.
- The device reports both battery and motor voltage while connected.
- On some BuWizz 2 units the ports are swapped. The app detects these units and swaps the channels automatically.

## See also

- [Devices](../../pages/device-list/README.md)
- [Controller action](../../pages/controller-action/README.md)
