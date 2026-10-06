using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BrickController2.BusinessLogic;
using BrickController2.CreationManagement;
using BrickController2.DeviceManagement;
using BrickController2.PlatformServices.Permission;
using BrickController2.PlatformServices.SharedFileStorage;
using BrickController2.UI.Commands;
using BrickController2.UI.Services.Dialog;
using BrickController2.UI.Services.Navigation;
using BrickController2.UI.Services.Translation;
using BrickController2.UI.ViewModels;
using Microsoft.Maui.ApplicationModel;
using Moq;
using Xunit;

namespace BrickController2.Tests.UI.ViewModels;

public class CreationSelectionTests
{
    private readonly Mock<INavigationService> _navigation = new();
    private readonly Mock<IDialogService> _dialogs = new();
    private readonly Mock<IPlayLogic> _playLogic = new();
    private readonly ObservableCollection<Creation> _creations = new();

    private CreationListPageViewModel MakeViewModel()
    {
        var manager = new Mock<ICreationManager>();
        manager.SetupGet(m => m.Creations).Returns(_creations);
        var translation = new Mock<ITranslationService>();
        translation.Setup(t => t.Translate(It.IsAny<string>())).Returns((string s) => s);
        _playLogic.Setup(p => p.ValidateCreation(It.IsAny<Creation>())).Returns(CreationValidationResult.Ok);
        var permissions = new Mock<IBluetoothPermission>();
        permissions.Setup(p => p.CheckStatusAsync()).ReturnsAsync(PermissionStatus.Granted);
        return new CreationListPageViewModel(_navigation.Object, translation.Object, manager.Object,
            Mock.Of<IDeviceManager>(), _playLogic.Object, _dialogs.Object, Mock.Of<ISharedFileStorageService>(),
            Mock.Of<ICommandFactory<Creation>>(), permissions.Object, Mock.Of<IReadWriteExternalStoragePermission>());
    }

    private static CreationListItemViewModel Item(string? assignment, string brick)
    {
        var c = new Creation { Name = brick, ControllerAssignmentId = assignment };
        var p = new ControllerProfile();
        var e = new ControllerEvent();
        e.ControllerActions.Add(new ControllerAction { DeviceId = brick });
        p.ControllerEvents.Add(e);
        c.ControllerProfiles.Add(p);
        return new CreationListItemViewModel(c, assignment ?? "Any");
    }

    [Fact]
    public void ImportedCreationsAppearImmediatelyAndRemainUncheckedDuringSelection()
    {
        var vm = MakeViewModel();
        var a = Item("controller-a", "A").Creation;
        var b = Item("controller-b", "B").Creation;
        _creations.Add(a);
        _creations.Add(b);
        vm.OnAppearing();
        vm.PlayAssignedCreationsCommand.Execute(null);
        vm.Items.Single(i => i.Creation == b).IsSelected = false;

        var imported = Item("controller-c", "Imported").Creation;
        _creations.Add(imported);

        Assert.Equal(new[] { a, b, imported }, vm.Items.Select(i => i.Creation));
        Assert.True(vm.Items.Single(i => i.Creation == a).IsSelected);
        Assert.False(vm.Items.Single(i => i.Creation == b).IsSelected);
        Assert.False(vm.Items.Single(i => i.Creation == imported).IsSelected);
        Assert.Equal("SelectCreationsToPlay (1)", vm.SelectionHint);
        vm.OnDisappearing();
    }

    [Fact]
    public void DeletionPreservesCheckedSubsetAndPlayDoesNotIncludeUncheckedRobots()
    {
        var vm = MakeViewModel();
        var a = Item("controller-a", "A").Creation;
        var b = Item("controller-b", "B").Creation;
        var c = Item("controller-c", "C").Creation;
        _creations.Add(a);
        _creations.Add(b);
        _creations.Add(c);
        vm.OnAppearing();
        vm.PlayAssignedCreationsCommand.Execute(null);
        vm.Items.Single(i => i.Creation == b).IsSelected = false;

        // The manager removes a creation after the delete command succeeds.
        _creations.Remove(c);
        NavigationParameters? received = null;
        _navigation.Setup(n => n.NavigateToAsync<PlayerPageViewModel>(It.IsAny<NavigationParameters>()))
            .Callback<NavigationParameters>(p => received = p).Returns(Task.CompletedTask);
        vm.PlaySelectedCreationsCommand.Execute(null);

        Assert.NotNull(received);
        Assert.Equal(new[] { a }, received.Get<Creation[]>("creations"));
        _creations.Remove(a);
        Assert.False(vm.PlaySelectedCreationsCommand.CanExecute(null));
        vm.OnDisappearing();
    }

    [Fact]
    public void CollectionSubscriptionOnlyUpdatesVisiblePageAndResumesWithoutDuplicates()
    {
        var vm = MakeViewModel();
        vm.OnAppearing();
        _creations.Add(Item(null, "A").Creation);
        Assert.Single(vm.Items);
        vm.OnDisappearing();
        _creations.Add(Item(null, "B").Creation);
        Assert.Single(vm.Items);
        vm.OnAppearing();
        Assert.Equal(2, vm.Items.Count);
        vm.OnAppearing();
        var resets = 0;
        vm.Items.CollectionChanged += (_, e) =>
        {
            if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Reset) resets++;
        };
        _creations.Add(Item(null, "C").Creation);
        Assert.Equal(3, vm.Items.Count);
        Assert.Equal(1, resets);
        vm.OnDisappearing();
    }

    [Fact]
    public void SelectionModeOnlyEnablesAssignedCreationsAndCancelClearsSelection()
    {
        var vm = MakeViewModel();
        var assigned = Item("controller-a", "A");
        var any = Item(null, "B");
        var none = Item("none", "C");
        vm.Items.Add(assigned);
        vm.Items.Add(any);
        vm.Items.Add(none);
        vm.PlayAssignedCreationsCommand.Execute(null);
        Assert.True(vm.IsSelectingCreations);
        Assert.True(assigned.IsSelected);
        Assert.False(any.IsSelected);
        Assert.False(none.IsSelected);
        any.IsSelected = true;
        Assert.False(any.IsSelected);
        vm.CancelSelectionCommand.Execute(null);
        Assert.True(vm.IsBrowsingCreations);
        Assert.False(assigned.IsSelected);
        Assert.False(vm.PlaySelectedCreationsCommand.CanExecute(null));
    }

    [Fact]
    public void PlayNavigatesWithOnlyCheckedCreations()
    {
        var vm = MakeViewModel();
        var a = Item("controller-a", "A");
        var b = Item("controller-b", "B");
        vm.Items.Add(a);
        vm.Items.Add(b);
        vm.PlayAssignedCreationsCommand.Execute(null);
        // Tapping a row in selection mode toggles it instead of opening its details.
        vm.CreationTappedCommand.Execute(b);
        NavigationParameters? received = null;
        _navigation.Setup(n => n.NavigateToAsync<PlayerPageViewModel>(It.IsAny<NavigationParameters>()))
            .Callback<NavigationParameters>(p => received = p).Returns(Task.CompletedTask);
        vm.PlaySelectedCreationsCommand.Execute(null);
        Assert.NotNull(received);
        Assert.Equal(new[] { a.Creation }, received.Get<Creation[]>("creations"));
        Assert.Same(a.Creation, received.Get<Creation>("creation"));
        _navigation.Verify(n => n.NavigateToAsync<CreationPageViewModel>(It.IsAny<NavigationParameters>()), Times.Never);
    }

    [Fact]
    public void ConflictingSmartBricksBlockSelectedSession()
    {
        var vm = MakeViewModel();
        vm.Items.Add(Item("controller-a", "shared-brick"));
        vm.Items.Add(Item("controller-b", "shared-brick"));
        vm.PlayAssignedCreationsCommand.Execute(null);
        vm.PlaySelectedCreationsCommand.Execute(null);
        _dialogs.Verify(d => d.ShowMessageBoxAsync("Warning", "AssignedCreationsInvalid", "Ok", It.IsAny<CancellationToken>()), Times.Once);
        _navigation.Verify(n => n.NavigateToAsync<PlayerPageViewModel>(It.IsAny<NavigationParameters>()), Times.Never);
    }

    [Fact]
    public void InvalidSelectedMappingsBlockSession()
    {
        var vm = MakeViewModel();
        var item = Item("controller-a", "A");
        vm.Items.Add(item);
        _playLogic.Setup(p => p.ValidateCreation(item.Creation)).Returns(CreationValidationResult.MissingDevice);
        vm.PlayAssignedCreationsCommand.Execute(null);
        vm.PlaySelectedCreationsCommand.Execute(null);
        _dialogs.Verify(d => d.ShowMessageBoxAsync("Warning", "AssignedCreationsInvalid", "Ok", It.IsAny<CancellationToken>()), Times.Once);
        _navigation.Verify(n => n.NavigateToAsync<PlayerPageViewModel>(It.IsAny<NavigationParameters>()), Times.Never);
    }
}
