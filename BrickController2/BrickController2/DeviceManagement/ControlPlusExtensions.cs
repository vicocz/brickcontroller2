using System;
using System.Collections.Generic;

namespace BrickController2.DeviceManagement
{
    public static class ControlPlusExtensions
    {

        /// <summary>
        /// Constructs message array by adding proper header from the source command bytes
        /// </summary>
        /// <param name="command">Source command</param>
        /// <param name="hubId">Optional hub ID</param>
        /// <returns>Allocated message of the required size, it may be modified</returns>
        public static byte[] ToMessageTemplate(this byte[] command, byte hubId = 0)
        {
            if (command.Length > 253)
                throw new ArgumentException("Byte array of the command is too long", nameof(command));

            var length = 2 + command.Length;

            var targetArray = new byte[length];

            targetArray[0] = (byte)length;
            targetArray[1] = hubId;

            Array.Copy(command, 0, targetArray, 2, command.Length);

            return targetArray;
        }

        /// <summary>
        /// Constructs message array by adding proper header from the source command bytes
        /// </summary>
        /// <param name="command">Source command</param>
        /// <param name="hubId">Optional hub ID</param>
        /// <returns></returns>
        public static IReadOnlyList<byte> ToMessage(this byte[] command, byte hubId = 0)
        {
            return command.ToMessageTemplate(hubId);
        }
    }
}