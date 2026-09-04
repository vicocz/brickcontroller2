using BrickController2.DeviceManagement.Macros;
using BrickController2.UI.Services.Translation;

namespace BrickController2.UI.ViewModels;

public class MacroItemViewModel
{
    private readonly ITranslationService _translationService;

    public MacroItemViewModel(MacroDescriptor descriptor, ITranslationService translationService)
    {
        Descriptor = descriptor;
        _translationService = translationService;
    }

    public MacroDescriptor Descriptor { get; }
    public string DisplayName => _translationService.Translate(Descriptor.NameKey);
}
