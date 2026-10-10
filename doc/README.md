# BrickController 2 documentation

BrickController 2 lets you control LEGO and compatible creations with a gamepad, using Bluetooth (and infrared on some Android devices). This documentation explains the app and the supported devices.

## Basic principles

- **Devices** are the receivers in your model (hubs, bricks, modules). Add them by scanning or, for some devices, manually. Each device exposes channels, e.g. motor or light outputs.
- **Creations** represent your models. A creation groups everything needed to control one model.
- **Controller profiles** belong to a creation. A creation can have several profiles, e.g. for different driving modes or gamepads.
- **Controller actions** map a gamepad button or axis to a device channel, with settings such as direction, speed or button behavior.
- **Sequences** are timed series of channel outputs that can be played by a controller action.
- The **Input device tester** shows the events of your connected gamepads, so you can check that a controller works and find the event code of a button or axis. See [Input device tester](en/pages/input-device-tester/README.md).

Typical flow: add devices, create a creation, add a controller profile, assign gamepad buttons and axes to device channels, then play the creation.

## Available documentation

| Language | Index | Content |
|---|---|---|
| English | [en](en/README.md) | Device and page topics |
| Deutsch | [de](de/README.md) | Not translated yet, English is shown |
| Magyar | [hu](hu/README.md) | Not translated yet, English is shown |

The same help is available inside the app via the **Help** button on pages and devices.

## Contributing

See [Writing documentation](HOWTO.md) for structure, conventions and how to add or translate topics.
