# Channel setup

Use this page to test and adjust a servo or stepper motor channel on a connected device.

## What you can do

- Check the device, channel and channel type. In channel test mode, select a supported **Channel type**.
- For a stepper motor, set the **Stepper angle**, then test the movement with **Step back** and **Step forward**.
- For a servo motor, test positions from -100 % to +100 %. If the device supports it, you can also adjust the **Servo range**.
- Set the **Base angle** for a servo or stepper channel. For servo channels, you can also calibrate it automatically. For stepper channels, the base angle is used for testing only and is not saved.
- Use **Reset** below **Base angle** to move the output to the selected base angle.

## How to

1. Adjust the angle slider. The value changes in 5-degree steps.
2. Use the test buttons to check the motor. Servo test buttons set positions at -100 %, -50 %, 0 %, +50 % and +100 %. Stepper test buttons move the motor backward or forward by the selected step angle.
3. For a servo, use **Auto calibrate** if the device supports it. You can also adjust the **Base angle** slider manually.
4. Select **Apply changes** to keep your settings. For stepper channels, only the **Stepper angle** is saved; the **Base angle** is used for testing only. Leaving the page disconnects the device.

## Notes

- The **Stepper angle** and **Servo range** sliders range from 0 to 180 degrees. The **Base angle** slider ranges from -180 to 179 degrees.
- In channel test mode, the channel type selector only offers types supported by the device and channel.
- Automatic calibration is available only for supported servo outputs.
- The **Servo range** can only be changed on devices that support it. On other devices, such as Technic Move, the slider is disabled.
- When you open this page in channel test mode from a device page, **Apply changes** is unavailable and your test settings are not saved.

## Toolbar

- ![](../../../icons/done_outline.svg) **Apply changes** - save the channel settings and close the page.

## Floating buttons

- ![](../../../icons/settings_backup_restore.svg) **Reset** - restore the default value for the associated angle slider.

## See also

- [Controller action](../controller-action/README.md)
- [Devices](../device-list/README.md)
