using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using BrickController2.BusinessLogic;
using BrickController2.CreationManagement;
using BrickController2.CreationManagement.Sharing;
using BrickController2.InputDeviceManagement;
using BrickController2.PlatformServices.InputDevice;
using BrickController2.PlatformServices.InputDeviceService;
using BrickController2.PlatformServices.SharedFileStorage;
using BrickController2.UI.Commands;
using BrickController2.UI.Services.Dialog;
using BrickController2.UI.Services.Navigation;
using BrickController2.UI.Services.Translation;
using BrickController2.UI.ViewModels;
using Moq;
using Xunit;

namespace BrickController2.Tests.UI.ViewModels;

public class ControllerAssignmentIdentityTests
{
    private sealed class UnsupportedInput : InputDeviceBase<object>
    {
        // Mapping labels can be reused after a disconnect or app restart.
        public UnsupportedInput(IInputDeviceEventServiceInternal events) : base(events, new object())
        {
            InputDeviceId = "Controller 1";
            Name = "Unsupported gamepad";
        }
    }

    [Fact]
    public void UnsupportedProviderIsOmittedFromChooserButStillPublishesLegacyInput()
    {
        var inputs = new InputDeviceManagerService();
        inputs.InputDeviceEvent += (_, _) => { }; // Keep the device available while opening the chooser.
        var input = new UnsupportedInput(inputs);
        inputs.AddInputDevice(input);
        Assert.Empty(input.RuntimeId);
        Assert.Null(CreationPlaySession.Resolve("Controller 1", [input]));

        var dialogs = new Mock<IDialogService>();
        string[]? choices = null;
        dialogs.Setup(d => d.ShowSelectionDialogAsync(It.IsAny<IEnumerable<string>>(), "ControllerAssignment", "Cancel",
                It.IsAny<CancellationToken>()))
            .Callback<IEnumerable<string>, string, string, CancellationToken>((items, _, _, _) => choices = items.ToArray())
            .ReturnsAsync(new SelectionDialogResult<string>(false, ""));
        var translation = new Mock<ITranslationService>();
        translation.Setup(t => t.Translate(It.IsAny<string>())).Returns((string key) => key);
        var manager = new Mock<ICreationManager>();
        manager.SetupGet(m => m.Creations).Returns(new ObservableCollection<Creation>());
        var vm = new CreationPageViewModel(Mock.Of<INavigationService>(), translation.Object, manager.Object,
            dialogs.Object, Mock.Of<ISharedFileStorageService>(), Mock.Of<IPlayLogic>(), inputs,
            Mock.Of<ISharingManager<ControllerProfile>>(), Mock.Of<ICommandFactory<Creation>>(),
            new NavigationParameters(("creation", new Creation())));

        vm.AssignControllerCommand.Execute(null);

        Assert.Equal(new[] { "AnyController", "NoController" }, choices);
        InputDeviceEventArgs? received = null;
        inputs.InputDeviceEvent += (_, e) => received = e;
        input.RaiseEvent(new Dictionary<(InputDeviceEventType, string), float> { [(InputDeviceEventType.Button, "A")] = 1 });
        Assert.NotNull(received);
        Assert.Equal("Controller 1", received.InputDeviceId);
        Assert.Empty(received.RuntimeId);
    }
}
