# Copilot instructions for BrickController2

BrickController2 is a cross-platform .NET MAUI app (Android, iOS, Windows) that controls LEGO and compatible
Bluetooth/IR devices (SBrick, BuWizz, PoweredUp, Technic Hub, MK4, CaDA, PFx Brick, Circuit Cubes, ...) with
game controllers or on-screen controls.

## Solution layout (`BrickController2.slnx`)

- `BrickController2/BrickController2` – shared MAUI library (`net10.0`): all business logic, UI (XAML + view models), DI modules.
- `BrickController2/BrickController2.Android`, `.iOS`, `.WinUI` – platform heads; implement platform services (Bluetooth LE, input devices, etc.).
- `BrickController2/BrickController2.Tools` – helper tooling shared with tests.
- `BrickController2/BrickController2.Tests` – unit tests (`net10.0`).
- `doc/{lang}/{devices|pages}/{topic}/README.md` – in-app help documents, embedded into the shared project (see `doc/README.md`, template in `doc/_template`).
- Package versions are managed centrally (no `Version` attribute on `PackageReference`).

## Key technologies

- .NET 10, .NET MAUI (compiled XAML bindings enabled – always set `x:DataType`).
- Autofac for DI (modules under `*/DI/*Module.cs`), `Microsoft.Extensions.Logging`.
- SQLite (`sqlite-net-pcl`, `SQLiteNetExtensions`) for persistence, Newtonsoft.Json for serialization.
- Markdig for rendering help markdown, ZXing.Net.Maui for QR sharing.
- Do not use Xamarin.Forms APIs; use MAUI equivalents.

## Architecture

### Dependency injection
- Each area registers itself in an Autofac `Module` (e.g. `DeviceManagement/DI/DeviceManagementModule.cs`, `UI/DI/UiModule.cs`, `Database/DI/DatabaseModule.cs`).
- Platform-specific implementations are registered in the platform project's `PlatformServices/DI/PlatformServicesModule.cs`.
- Interfaces for platform services live in the shared project under `PlatformServices/*` (e.g. `IBluetoothLEService`, `IBluetoothLEDevice`, `IInputDevice`).

### Devices
- Devices derive from `Device` or one of its specialized bases: `BluetoothDevice`, `BluetoothAdvertisingDevice`, `BluetoothMacroBasedDevice`, `Lego/WirelessProtocolBasedDevice`.
- Every device type has a value in `DeviceManagement/DeviceType.cs`; devices are created via the `DeviceFactory` delegate.
- Devices are grouped by vendor. A vendor is an Autofac module deriving from `Vendor<TVendor>` (`DeviceManagement/Vendors/Vendor.cs`) and registers its devices, input device services and device manager in `Register(VendorBuilder<TVendor>)`, e.g.:
  ```csharp
  builder.ContainerBuilder.RegisterDevice<PoweredUpDevice>(DeviceType.PoweredUp);
  builder.RegisterDevice<RemoteControl>().WithImage("remotecontrol_image_small.png");
  builder.RegisterDeviceManager<LegoDeviceManager>();
  ```
- Adding a new device typically requires:
  1. New `DeviceType` value.
  2. Device class (+ device manager / scan logic if new vendor).
  3. Registration in the vendor module.
  4. Device image in `UI/Images` (embedded resource) registered via `DeviceImageRegistry` / `WithImage`.
  5. Help page `doc/en/devices/{topic}/README.md`, linked from `doc/en/devices/README.md`.
  6. Vendor registration tests in `BrickController2.Tests/DeviceManagement/DI`.

### UI (MVVM)
- Pages are XAML under `UI/Pages`, view models under `UI/ViewModels` deriving from `PageViewModelBase`.
- Naming: `XxxPage` ↔ `XxxPageViewModel`. Navigate via `INavigationService.NavigateToAsync<TViewModel>(NavigationParameters)`.
- Use `SafeCommand` for `ICommand` properties; property changes go through `NotifyPropertyChangedSource`.
- Use `DisappearingToken` for work that must stop when the page disappears.
- Help topic is derived from the view model name (`CreationListPageViewModel` → `pages/creation-list`); override `GetHelpTopic()` if needed.
- Value converters live in `UI/Converters` and are named `*Converter`.

### Localization
- All user-visible strings come from `Resources/TranslationResources.resx` (with `.de`, `.hu`, ... variants).
- XAML: `{extensions:Translate Key}`; C#: `Translate("Key")` from `PageViewModelBase` / `ITranslationService`.
- When adding a key, add it to the neutral `.resx` (and translations when known).

## Coding conventions

- Nullable reference types enabled; avoid `!` unless justified.
- Prefer file-scoped namespaces for new files; namespace mirrors folder path (`BrickController2.DeviceManagement.Lego`).
- Private fields `_camelCase`, `readonly` where possible; async methods end with `Async` and accept `CancellationToken` when cancellable.
- Prefer `internal` for implementation types; `InternalsVisibleTo` is set for tests and Moq.
- Use collection expressions (`[]`), pattern matching and expression-bodied members where it improves readability.
- Keep comments sparse; XML doc `<summary>` on public/base types.
- Match the style of the surrounding file; do not reformat unrelated code.

## Testing

- xUnit v3 + FluentAssertions + Moq.
- Test classes mirror the production namespace under `BrickController2.Tests` and are named `{Type}Tests`.
- Test method naming: `Method_Scenario_ExpectedResult` (e.g. `RegisterDevice_PoweredUpDevice_ReturnedDevice`).
- Use `// Arrange`, `// Act`, `// Assert` sections; mock dependencies with `Mock.Of<T>()` / `new Mock<T>()`.
- Run tests before finishing changes; build the whole solution to catch platform-head breaks.

## Documentation

- Help pages are markdown in `doc/en/...`, embedded with logical name `doc/{lang}/{devices|pages}/{topic}/README.md`.
- Follow `doc/_template` when adding a page; keep images next to the README and reference them relatively.
