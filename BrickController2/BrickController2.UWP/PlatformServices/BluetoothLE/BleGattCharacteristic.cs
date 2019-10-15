﻿using System;
using System.Threading.Tasks;
using BrickController2.PlatformServices.BluetoothLE;
using BrickController2.Windows.Extensions;
using Windows.Devices.Bluetooth.GenericAttributeProfile;

namespace BrickController2.Windows.PlatformServices.BluetoothLE
{
    internal class BleGattCharacteristic : IGattCharacteristic
    {
        private readonly GattCharacteristic _gattCharacteristic;

        private bool isNotifySet;
        private Action<Guid, byte[]> _valueChangedCallback;

        public BleGattCharacteristic(GattCharacteristic bluetoothGattCharacteristic)
        {
            _gattCharacteristic = bluetoothGattCharacteristic;
            Uuid = bluetoothGattCharacteristic.Uuid;
        }

        public Guid Uuid { get; }

        public bool CanNotify => _gattCharacteristic != null &&
            _gattCharacteristic.CharacteristicProperties.HasFlag(GattCharacteristicProperties.Notify);

        public async Task<GattCommunicationStatus> WriteNoResponseAsync(byte[] data)
        {
            System.Diagnostics.Debug.WriteLine($"Writing [{_gattCharacteristic.AttributeHandle:X}] - {BitConverter.ToString(data)}");

            var buffer = data.ToBuffer();

            return await _gattCharacteristic
                .WriteValueAsync(buffer, GattWriteOption.WriteWithoutResponse)
                .AsTask();
        }

        public async Task<GattWriteResult> WriteWithResponseAsync(byte[] data)
        {
            System.Diagnostics.Debug.WriteLine($"Writing [{_gattCharacteristic.AttributeHandle:X}] - {BitConverter.ToString(data)}");

            var buffer = data.ToBuffer();

            return await _gattCharacteristic
                .WriteValueWithResultAsync(buffer, GattWriteOption.WriteWithResponse)
                .AsTask();
        }

        internal async Task<bool> EnableNotificationAsync(Action<Guid, byte[]> callback)
        {
            // setup callback before writing client char. so as no event is skipped
            _gattCharacteristic.ValueChanged += _gattCharacteristic_ValueChanged;
            _valueChangedCallback = callback;

            var result = await ApplyClientCharacteristicConfigurationDescriptorAsync(GattClientCharacteristicConfigurationDescriptorValue.Notify, isNotifySet)
                .ConfigureAwait(false);

            isNotifySet = result;
            return result;
        }

        internal async Task<bool> DisableNotificationAsync()
        {
            var result = await ApplyClientCharacteristicConfigurationDescriptorAsync(GattClientCharacteristicConfigurationDescriptorValue.None, isNotifySet);

            _valueChangedCallback = null;
            _gattCharacteristic.ValueChanged -= _gattCharacteristic_ValueChanged;

            isNotifySet = result;
            return result;
        }

        private void _gattCharacteristic_ValueChanged(GattCharacteristic sender, GattValueChangedEventArgs args)
        {
            if (_valueChangedCallback != null)
            {
                var eventData = args.CharacteristicValue.ToByteArray();

                _valueChangedCallback.Invoke(Uuid, eventData);
            }
        }

        /// <summary>
        /// Sets the notify / indicate / characteristic
        /// </summary>
        /// <returns>If application was successfull (or has been already applied)</returns>
        private async Task<bool> ApplyClientCharacteristicConfigurationDescriptorAsync(GattClientCharacteristicConfigurationDescriptorValue value, bool currentFlagValue)
        {
            bool targetFlagValue = value == GattClientCharacteristicConfigurationDescriptorValue.None ? false : true;

            if (currentFlagValue == targetFlagValue)
            {
                // already applied
                return true;
            }

            try
            {
                // write ClientCharacteristicConfigurationDescriptor in order to get notifications
                // it's recieved in ValueChanged event handler than
                var result = await _gattCharacteristic.WriteClientCharacteristicConfigurationDescriptorWithResultAsync(value);
                if (result.Status == GattCommunicationStatus.Success)
                {
                    return true;
                }
            }
            catch (UnauthorizedAccessException)
            {
                //TODO report
            }
            catch (Exception)
            {
                //TODO report
            }

            return false;
        }
    }
}