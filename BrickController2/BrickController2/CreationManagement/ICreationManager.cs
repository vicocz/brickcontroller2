﻿using BrickController2.PlatformServices.GameController;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace BrickController2.CreationManagement
{
    public interface ICreationManager
    {
        ObservableCollection<Creation> Creations { get; }

        Task LoadCreationsAsync();

        Task<bool> IsCreationNameAvailableAsync(string creationName);
        Task<Creation> AddCreationAsync(string creationName);
        Task DeleteCreationAsync(Creation creation);
        Task RenameCreationAsync(Creation creation, string newName);

        Task<Creation> ImportCreationAsync(Creation importCreation);

        Task<bool> IsControllerProfileNameAvailableAsync(Creation creation, string controllerProfileName);
        Task<ControllerProfile> AddControllerProfileAsync(Creation creation, string controllerProfileName);
        Task DeleteControllerProfileAsync(ControllerProfile controllerProfile);
        Task RenameControllerProfileAsync(ControllerProfile controllerProfile, string newName);
        Task<ControllerProfile> CopyControllerProfileAsync(ControllerProfile controllerProfile, string copiedProfileName);

        Task<ControllerEvent> AddOrGetControllerEventAsync(ControllerProfile controllerProfile, GameControllerEventType eventType, string eventCode);
        Task DeleteControllerEventAsync(ControllerEvent controllerEvent);

        Task<ControllerAction> AddOrUpdateControllerActionAsync(
            ControllerEvent controllerEvent,
            string deviceId,
            int channel,
            bool isInvert,
            ControllerButtonType buttonType,
            ControllerAxisType axisType,
            ControllerAxisCharacteristic axisCharacteristic,
            int maxOutputPercent,
            int axisDeadZonePercent,
            ChannelOutputType channelOutputType,
            int maxServoAngle,
            int servoBaseAngle,
            int stepperAngle);
        Task DeleteControllerActionAsync(ControllerAction controllerAction);
        Task UpdateControllerActionAsync(
            ControllerAction controllerAction,
            string deviceId,
            int channel,
            bool isInvert,
            ControllerButtonType buttonType,
            ControllerAxisType axisType,
            ControllerAxisCharacteristic axisCharacteristic,
            int maxOutputPercent,
            int axisDeadZonePercent,
            ChannelOutputType channelOutputType,
            int maxServoAngle,
            int servoBaseAngle,
            int stepperAngle);
    }
}
