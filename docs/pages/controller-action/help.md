# Controller action

A controller action connects a controller event to a device channel or a device macro.

## Main options

- **Device and channel**: the output that the action drives.
- **Invert**: reverses the direction of the output.
- **Max output**: limits the output power. Applies to the normal channel type only.
- **Button type**: how a button behaves, see below.
- **Joy type**, **Joy characteristic**, **Joy dead zone** and **Joy active zone**: how an axis behaves, see below.
- **Macro**: starts a device function, such as playing a sound, instead of driving a channel.

## Button types

A button action sets the output of the channel when the button is pressed or released.

| Button type | Behavior |
|---|---|
| Normal | The output is on while the button is held and off when it is released. |
| Simple toggle | Each press switches the output between on and off. |
| Alternating | Each press switches the output on, in the opposite direction than the previous time. |
| Circular | Each press steps the output through its range and wraps around at the end. |
| Ping pong | A press starts the output, the next press stops it, and the following press starts it in the opposite direction. |
| Stop | Stops the channel and clears the state of any axis driving it. |
| Accelerator | Each press increases the output by the acceleration step of the device, up to the limit. |
| Sequence | Starts or stops the selected sequence on the channel. |
| Macro | Starts a device macro. |

## Axis types

An axis action maps the position of a stick or trigger to the output.

| Joy type | Behavior |
|---|---|
| Normal | The output follows the position of the axis. |
| Train | The output follows the axis when speeding up, but does not drop when the axis is moved back. Slowing down is gradual, and the output stays at zero until the axis returns to the center. |
| Accelerator | Holding the axis at its end position changes the output by the acceleration step of the device, so the output keeps its value when the axis is released. |

Related options:

- **Joy dead zone**: the center range of the axis that is ignored.
- **Joy active zone**: the range of the axis that is used. The full output is reached at the end of this range.
- **Joy characteristic**: the response curve of the axis. Linear is proportional, exponential gives finer control near the center and logarithmic gives more output near the center.

## Channel types

The channel type defines what the output means for the connected motor. It is available for devices and channels that support it.

| Channel type | Behavior |
|---|---|
| Normal | The output is the motor power, from -100 % to +100 %. Limited by **Max output**. |
| Servo | The output is the position of a servo. Set the **Servo range** (0 to 180 degrees) and the **Base angle** (-180 to 179 degrees). |
| Stepper | The output moves a stepper motor in steps. Set the **Stepper angle** (0 to 180 degrees) to define the step size. |

For servo and stepper channel types, a channel setup button appears next to the channel type.

## See also

- [Controller profile](../controller-profile/help.md)
- [Devices](../device-list/help.md)
