# Controller profile

A controller profile maps controller events (buttons, sticks, triggers) to actions on your devices.

## Adding an event

An event is added by using the controller itself, so a game controller (gamepad) or other input device must be connected first.

1. Connect the controller and make sure at least one device is in the device list. If there is none, you are asked to scan for devices first.
2. Press one of the add buttons:
   - **Add a new controller event**: accepts a button or a joystick.
   - **Add a new controller button event**: accepts a button only.
   - **Add a new controller axis event**: accepts a joystick only.
3. When asked, press the button or move the joystick on the controller. The first input detected is used.
4. The action page opens to set up what the event does.

The dialog can be canceled if no input is detected.

## Multiple controllers

By default an event reacts to the chosen button or joystick on any connected controller.

When more than one input device is connected, use **Add a new controller event for specific game controller**. The event is then tied to the controller you used, shown as for example "Controller 1", and other controllers do not trigger it. This lets two controllers control different devices with the same buttons.

The same button or joystick can have separate events for different controllers.

## What you can do

- Add an action for a controller event.
- Edit or delete existing actions.
- Test the profile with the controller tester.

## Tips

- One event can drive several actions on different devices.

## See also

- [Controller action](../controller-action/help.md)
