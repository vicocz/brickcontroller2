# PfxBrick

PfxBrick (FX Bricks) is a Bluetooth LE brick that drives Power Functions motors and lights, and plays sounds.

## Channels

The device exposes 10 channels:

| Channels | Purpose | Output |
|---|---|---|
| 1 - 2 | Power Functions motors | -100 % to +100 % |
| 3 - 10 | Lights | brightness 0 to 255 |

## Getting started

1. Power on the PfxBrick.
2. Open the device list and scan for devices.
3. Select the PfxBrick from the scan results to add it.
4. Use the device in a creation: assign its channels to controller actions.

## Sound

Sound is controlled by macros that can be assigned to controller actions:

- **Play sound** starts a sound file stored on the device.
- **Stop sound** stops a playing sound file.
- **Toggle sound** starts the sound, or stops it if already playing.
- **Set volume** sets the volume to a chosen level.
- **Increase volume** and **Decrease volume** change the volume by a step.

## Settings

- **Default volume level**: the volume applied after connecting. The default is 50 %.

## Notes

- The device does not connect automatically on first use. Connect it from the device page or by starting a creation that uses it.
- Sound files must be present on the PfxBrick; the app only triggers them.

## See also

- [Controller action](../../pages/controller-action/help.md)
