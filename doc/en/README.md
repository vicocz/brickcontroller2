# BrickController 2 documentation (English)

BrickController 2 is a cross platform app (Android, iOS, Windows) for controlling LEGO and compatible models with a Bluetooth gamepad.

## How the app works

1. **Add devices.** Devices are the receivers built into your model, such as hubs, bricks and modules. Most are found by scanning, some are added manually. Each device has channels, for example motor or light outputs.
2. **Create a creation.** A creation stands for one model and holds everything needed to control it.
3. **Set up controller profiles.** A creation has one or more controller profiles, for example for different gamepads or driving modes.
4. **Assign controller actions.** A controller action connects a gamepad button or axis to a device channel. Settings such as direction, speed and button behavior define how the channel reacts.
5. **Play.** Start the creation, the app connects to its devices and the gamepad controls the model.

Sequences add timed series of outputs, for example light effects, that a controller action can start.

## Main pages

- [Creations](pages/creation-list/README.md): list of your creations, starting point of the app.
- [Devices](pages/device-list/README.md): scan, add, rename and set up devices.
- [Sequences](pages/sequence-list/README.md): create and edit sequences.

Other pages are reached from these and have their own help, available via the **Help** button.

## Devices

See [Supported devices](devices/README.md).
