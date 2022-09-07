# BrickController Essentials

Cross platform mobile application for controlling Lego creations using a bluetooth gamepad.

Fork of the original application [BrickController 2](https://github.com/imurvai/brickcontroller2). It contains additional features, which are not part of the original application yet. See details [here](#features-and-fixes).


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
Base set of features comes from released BrickController 2 version [3.2](https://github.com/imurvai/brickcontroller2/tree/6dfe8f2865616bf60b16c4bb4149f7fa5e8d8893).

The following features are part of the application:

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
