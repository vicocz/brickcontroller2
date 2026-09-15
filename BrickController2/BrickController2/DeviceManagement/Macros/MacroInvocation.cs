namespace BrickController2.DeviceManagement.Macros;

public readonly record struct MacroInvocation(string DescriptorId, MacroChoiceValue ChoiceValue, int? Channel);
