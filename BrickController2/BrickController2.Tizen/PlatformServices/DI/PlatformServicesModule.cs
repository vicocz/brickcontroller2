﻿using Autofac;
using BrickController2.Tizen.PlatformServices.BluetoothLE;
using BrickController2.Tizen.PlatformServices.GameController;
using BrickController2.Tizen.PlatformServices.Infrared;
using BrickController2.Tizen.PlatformServices.Localization;
using BrickController2.Tizen.PlatformServices.Permission;
using BrickController2.Tizen.PlatformServices.SharedFileStorage;
using BrickController2.Tizen.PlatformServices.Versioning;
using BrickController2.PlatformServices.BluetoothLE;
using BrickController2.PlatformServices.GameController;
using BrickController2.PlatformServices.Infrared;
using BrickController2.PlatformServices.Localization;
using BrickController2.PlatformServices.Permission;
using BrickController2.PlatformServices.SharedFileStorage;
using BrickController2.PlatformServices.Versioning;

namespace BrickController2.Tizen.PlatformServices.DI;

public class PlatformServicesModule : Module
{
    protected override void Load(ContainerBuilder builder)
    {
        builder.RegisterType<InfraredService>().As<IInfraredService>().SingleInstance();
        builder.RegisterType<GameControllerService>().AsSelf().As<IGameControllerService>().SingleInstance();
        builder.RegisterType<VersionService>().As<IVersionService>().SingleInstance();
        builder.RegisterType<BleService>().As<IBluetoothLEService>().SingleInstance();
        builder.RegisterType<LocalizationService>().As<ILocalizationService>().SingleInstance();
        builder.RegisterType<SharedFileStorageService>().As<ISharedFileStorageService>().SingleInstance();
        builder.RegisterType<ReadWriteExternalStoragePermission>().As<IReadWriteExternalStoragePermission>().InstancePerDependency();
        builder.RegisterType<BluetoothPermission>().As<IBluetoothPermission>().InstancePerDependency();
        builder.RegisterType<CameraPermission>().As<ICameraPermission>().InstancePerDependency();
    }
}