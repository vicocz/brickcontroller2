# BrickController Essentials

Fork of [BrickController 2](https://github.com/imurvai/brickcontroller2) having [additional features](#features-and-fixes) which are not of the original application yet.

Cross platform mobile application for controlling Lego creations using a bluetooth gamepad.


## Supported platforms

- Android 4.3+
- iOS 8+

## Supported receivers

- SBrick - both normal and plus (output only)
- BuWizz 1
- BuWizz 2
- BuWizz 3
- Lego PowerFunctions infrared receiver on Android devices having IR emitter
- Lego Powered-Up hub
- Lego Boost Hub
- Lego Technic Hub
- Circuit Cubes

## Project details

BrickController 2 is a Xamarin.Forms application and can be compiled using Visual Studio 2019 (Professional, Enterprise and Community Editions).

### Features and fixes
The application contains basic features of the released BrickController 2 version [3.2](https://github.com/imurvai/brickcontroller2/tree/6dfe8f2865616bf60b16c4bb4149f7fa5e8d8893) and the following set of improvements and bug fixes:

|  Issue | BrickController 2 status | Description | Verification status |
| :-- | :-- | :-- | :-- |
| imurvai#78 | imurvai#79 | Buwizz v1 device is not able to keep constant channel output for longer period | Manually tested |
| | imurvai#87 | Make BluetoothDevice.Disconnect asynchronous | Manually tested |


## 3rd party libraries used

- Autofac IOC container
- [Plugin.Permissions](https://github.com/jamesmontemagno/PermissionsPlugin)
- [SQLite-Net-Extensions Async](https://bitbucket.org/twincoders/sqlite-net-extensions)

## Author

István Murvai

Vit Nemecky (minor improvements and features)
