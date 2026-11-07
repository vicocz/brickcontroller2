# PFx Brick

PFx Brick (FX Bricks) is a Bluetooth LE brick that drives Power Functions motors and lights, and plays sounds.

![PFx Brick](../../../../BrickController2/BrickController2/UI/Images/pfxbrick_image.png)

## Channels

The device exposes 10 channels:

| Channels | Purpose | Output |
|---|---|---|
| A - B | Power Functions motors | -100 % to +100 % |
| 1 - 8 | Lights | brightness 0 to 100 % |

## Getting started

1. Power on the PFx Brick.
2. Open the device list and press ![](../../../icons/search.svg) **Scan for new devices**. Found devices are added to the list.
3. Use the device in a creation: assign its channels to controller actions.

## Sound

Sound is controlled by macros that can be assigned to controller actions:

- **Play sound** starts a sound file stored on the device.
- **Stop sound** stops a playing sound file.
- **Toggle sound** starts the sound, or stops it if already playing.
- **Set volume** sets the volume to a chosen level.
- **Increase volume** and **Decrease volume** change the volume by a step.

## Light effects

Light macros are channel macros: assign them to a controller action on a light channel (1 - 8). They are ignored on motor channels (A - B).

- **Increase brightness**: increase brightness one step.
- **Decrease brightness**: decrease brightness one step.

Each press of a light effect toggles it on or off:

- **Flash 50 %** / **Flash 50 % (inverted)**: 50 % duty cycle flasher.
- **Strobe** / **Strobe (inverted)**: strobe light flasher.
- **Gyralite** / **Gyralite (inverted)**: fading MARS/Gyralite flasher.
- **Flicker**: random flickering light.
- **Random blink**: random blinking light.
- **Photon torpedo**: photon torpedo effect.
- **Laser pulse**: shooting laser effect.
- **Sci-fi engine glow**: glowing/pulsating engine glow effect.
- **Lighthouse**: rotating lighthouse effect.
- **Broken light**: flickering faulty light effect.
- **Sound modulated**: sound modulated light intensity.

An effect and its inverted variant (opposite phase) can be used on two channels to make them alternate.

## Scripts

Script macros can be assigned to controller actions:

- **Run script** starts a script stored on the device.
- **Stop script** stops a running script.

## Settings

- **Default volume level**: the volume applied after connecting. The default is 50 %.

## Notes

- The device does not connect automatically on first use. Connect it from the device page or by starting a creation that uses it.
- Sound files and scripts must be present on the PFx Brick; the app only triggers them.
- Sound files and scripts are managed by the PFx Brick app, not by this app.

## See also

- [FX Bricks documentation](https://shop.fxbricks.com/pages/documentation)
- [Controller action](../../pages/controller-action/README.md)
