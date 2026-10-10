# BuWizz 3

BuWizz 3 (BuWizz 3.0 Pro) is a Bluetooth LE battery brick with four Powered Up ports and two PowerFunctions ports.

![BuWizz 3](../../../../BrickController2/BrickController2/UI/Images/buwizz3_image.png)

## Channels

The device exposes 6 channels:

| Channels | Purpose | Output |
|---|---|---|
| 1 - 4 | Powered Up motors | -100 % to +100 %, servo or stepper |
| A, B | PowerFunctions motors | -100 % to +100 % |

## Getting started

1. Power on the BuWizz 3.
2. Open the device list and press **Scan for new devices**. Found devices are added to the list.
3. Use the device in a creation: assign its channels to controller actions.

## Current limit

Each port has its own current limit, which can be changed in the device settings. Defaults (same as the official BuWizz app):

| Ports | Default limit |
|---|---|
| 1 - 4 (Powered Up) | 1050 mA |
| A, B (PowerFunctions) | 2100 mA |

## Shelf mode

Shelf mode disconnects the battery from the device, for example for storage or transport. It can be activated from the device page while connected, after confirmation. To wake the device up afterwards, usually connect the charger.

## Notes

- Servo and stepper output types are available on the Powered Up ports (1 - 4) only.
- The battery voltage is shown on the device page while connected.

## See also

- [Devices](../../pages/device-list/README.md)
- [Controller action](../../pages/controller-action/README.md)
