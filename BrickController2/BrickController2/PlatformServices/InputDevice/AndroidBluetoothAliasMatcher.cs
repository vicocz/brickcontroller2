using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace BrickController2.PlatformServices.InputDevice;

/// <summary>Best-effort name lookup using exact AOSP descriptor matches, never name/order guesses.</summary>
public static class AndroidBluetoothAliasMatcher
{
    // AOSP EventHub.generateDescriptor hashes ":vvvv:pppp:uniqueId:Bluetooth-address".
    // This is an implementation detail, so OEMs with a different format simply retain the input name.
    // https://android.googlesource.com/platform/frameworks/native/+/refs/heads/main/services/inputflinger/reader/EventHub.cpp
    public static string? FindAlias(string? descriptor, int vendor, int product, string? inputName,
        IEnumerable<(string Address, string? Alias)> pairedDevices)
    {
        if (string.IsNullOrWhiteSpace(descriptor)) return null;
        var matches = pairedDevices.Where(d => !string.IsNullOrWhiteSpace(d.Alias) &&
            Regex.IsMatch(d.Address, "^[0-9a-fA-F]{2}(:[0-9a-fA-F]{2}){5}$") &&
            Matches(descriptor, vendor, product, inputName, d.Address)).ToArray();
        return matches.Length == 1 ? matches[0].Alias : null;
    }

    private static bool Matches(string descriptor, int vendor, int product, string? name, string address)
    {
        foreach (var uniqueId in new[] { address.ToLowerInvariant(), address.ToUpperInvariant() }.Distinct())
        {
            // Modern AOSP can add a nonce to distinguish HID collections with the same unique ID.
            // Bound the optional probes; unsupported descriptors fall back to the native input name.
            for (var nonce = 0; nonce < 32; nonce++)
            {
                var raw = FormattableString.Invariant($":{vendor:x4}:{product:x4}:uniqueId:{uniqueId}");
                if (nonce != 0) raw += "nonce:" + nonce.ToString("x4", CultureInfo.InvariantCulture);
                if (vendor == 0 && product == 0 && !string.IsNullOrEmpty(name)) raw += "name:" + name;
                var hash = Convert.ToHexString(SHA1.HashData(Encoding.UTF8.GetBytes(raw)));
                if (string.Equals(hash, descriptor, StringComparison.OrdinalIgnoreCase)) return true;
            }
        }
        return false;
    }
}
