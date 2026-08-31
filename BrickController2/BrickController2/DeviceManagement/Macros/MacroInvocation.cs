namespace BrickController2.DeviceManagement.Macros;

public readonly record struct MacroInvocation(string DescriptorId, int? ChoiceValue, int? Channel);
