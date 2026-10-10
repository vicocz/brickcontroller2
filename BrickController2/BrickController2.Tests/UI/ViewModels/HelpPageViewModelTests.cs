using BrickController2.UI.Services.Help;
using BrickController2.UI.Services.Navigation;
using BrickController2.UI.Services.Translation;
using BrickController2.UI.ViewModels;
using FluentAssertions;
using Moq;
using System.Globalization;
using System.Threading.Tasks;
using Xunit;

namespace BrickController2.Tests.UI.ViewModels;

public class HelpPageViewModelTests
{
    private static readonly HelpTopic TopicA = new("pages/a");
    private static readonly HelpTopic TopicB = new("pages/b");
    private static readonly HelpTopic TopicC = new("pages/c");

    private readonly Mock<INavigationService> _navigationServiceMock = new();
    private readonly Mock<ITranslationService> _translationServiceMock = new();
    private readonly Mock<IHelpService> _helpServiceMock = new();

    private readonly HelpPageViewModel _viewModel;

    public HelpPageViewModelTests()
    {
        _translationServiceMock.Setup(t => t.Translate(It.IsAny<string>())).Returns("Help");
        _helpServiceMock.Setup(h => h.HasHelp(It.IsAny<HelpTopic>())).Returns(true);
        _helpServiceMock
            .Setup(h => h.GetHelpMarkdownAsync(It.IsAny<HelpTopic>(), It.IsAny<CultureInfo?>()))
            .Returns((HelpTopic topic, CultureInfo? _) => Task.FromResult<string?>($"# {topic.Path}"));
        _helpServiceMock
            .Setup(h => h.ResolveLink(It.IsAny<HelpTopic>(), It.IsAny<string>()))
            .Returns((HelpTopic?)null);

        _viewModel = new HelpPageViewModel(
            _navigationServiceMock.Object,
            _translationServiceMock.Object,
            _helpServiceMock.Object,
            new NavigationParameters(("topic", TopicA)));
        _viewModel.OnAppearing();
    }

    [Fact]
    public void OnAppearing_LoadsInitialTopic()
    {
        _viewModel.Title.Should().Be("pages/a");
        _viewModel.Source.Should().NotBeNull();
    }

    [Fact]
    public void FollowingLink_StaysOnSamePage_AndRendersNewTopic()
    {
        var handled = _viewModel.TryHandleNavigation("help://pages/b");

        handled.Should().BeTrue();
        _viewModel.Title.Should().Be("pages/b");
        _navigationServiceMock.Verify(n => n.NavigateToAsync<HelpPageViewModel>(It.IsAny<NavigationParameters>()), Times.Never);
    }

    [Fact]
    public void LinkToMissingTopic_IsIgnored()
    {
        _helpServiceMock.Setup(h => h.HasHelp(TopicB)).Returns(false);

        _viewModel.TryHandleNavigation("help://pages/b").Should().BeTrue();

        _viewModel.Title.Should().Be("pages/a");
    }

    [Fact]
    public void GoBack_ReturnsToPreviousTopic_AndForwardRestoresIt()
    {
        _viewModel.TryHandleNavigation("help://pages/b");

        _viewModel.GoBackCommand.Execute(null);
        _viewModel.Title.Should().Be("pages/a");
        _viewModel.GoForwardCommand.CanExecute(null).Should().BeTrue();

        _viewModel.GoForwardCommand.Execute(null);
        _viewModel.Title.Should().Be("pages/b");
        _viewModel.GoForwardCommand.CanExecute(null).Should().BeFalse();
    }

    [Fact]
    public void GoBack_OnFirstTopic_IsDisabled_AndDoesNotLeaveHelp()
    {
        _viewModel.GoBackCommand.CanExecute(null).Should().BeFalse();

        _viewModel.GoBackCommand.Execute(null);

        _navigationServiceMock.Verify(n => n.NavigateBackAsync(), Times.Never);
        _viewModel.Title.Should().Be("pages/a");
    }

    [Fact]
    public void OpeningTopicAfterBack_DiscardsForwardHistory()
    {
        _viewModel.TryHandleNavigation("help://pages/b");
        _viewModel.GoBackCommand.Execute(null);

        _viewModel.TryHandleNavigation("help://pages/c");

        _viewModel.Title.Should().Be("pages/c");
        _viewModel.GoForwardCommand.CanExecute(null).Should().BeFalse();

        _viewModel.GoBackCommand.Execute(null);
        _viewModel.Title.Should().Be("pages/a");
    }

    [Fact]
    public void LinkToCurrentTopic_DoesNotAddHistoryEntry()
    {
        _viewModel.TryHandleNavigation("help://pages/a");

        _viewModel.GoForwardCommand.CanExecute(null).Should().BeFalse();
        _viewModel.GoBackCommand.CanExecute(null).Should().BeFalse();
    }

    [Fact]
    public void UnknownUrl_IsNotHandled()
    {
        _viewModel.TryHandleNavigation("about:blank").Should().BeFalse();
    }
}
