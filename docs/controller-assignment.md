# Per-creation controllers

Each creation now has a **Controller** chooser. Existing creations default to **Any controller**, preserving their existing mappings and ordinary single-creation Play behavior. **None** disables input. A specific assignment overrides the old controller-number restrictions in that creation's mappings; button/axis codes and motor settings remain unchanged.

Choose **Remember this controller** to save an Android descriptor, or **This connection only** to distinguish controllers that Android cannot identify persistently. Connection choices expire on disconnect or app restart; opening another app page does not expire them. Assignments are local settings stored in the creation database and are deliberately excluded from exported/shared creation JSON.

Use **Play assigned creations** on the Creations list to enter selection mode. Assigned creations are initially checked; uncheck the creations you do not want and tap **Play selected**. Only the checked creations start together. Creations set to Any controller or None are excluded. All included creations must have valid mappings and use separate smart bricks; the app refuses a session that shares a brick. Each creation has its own PlayLogic state and SequencePlayer. Group play starts each selected creation with its first profile; the first selected creation in list order is the primary creation. The player screen lists each participant and its connection status; profile selection there changes only the primary creation. Leaving the player screen stops all participants and disconnects their bricks.

## Existing architecture and implementation

Android MainActivity receives OnKeyDown/OnKeyUp/OnGenericMotionEvent and input-device lifecycle notifications. GameControllerService already resolves each native event by KeyEvent.DeviceId or MotionEvent.DeviceId to a separate GamepadController. InputDeviceBase caches changes per controller and publishes InputDeviceEventArgs through the shared InputDeviceManagerService. Discovery runs while input listeners exist.

Previously GamepadController identified events as Controller N using Android ControllerNumber. PlayerPageViewModel subscribed to the global stream and passed every event to a single IPlayLogic.ActiveProfile. ControllerEvent optionally filtered by ControllerId, and ControllerAction described the output device/channel and button/axis behavior. Creation, ControllerProfile, ControllerEvent and ControllerAction are persisted through sqlite-net relations; sharing uses JSON.

The change adds runtime identity metadata without replacing those legacy event identifiers. CreationPlaySession resolves saved assignments against connected devices, dispatches each event only to its owning player, and serializes event/lifecycle handling. Duplicate creation assignments are paused, rather than broadcast. Losing a controller stops that creation's sequences, zeros its mapped outputs, and clears its control state while other participants continue. A uniquely recognized descriptor reconnects automatically. Connection-only assignments require choosing the new connection after reconnect.

## Android identity limitations

[Android InputDevice documentation](https://developer.android.com/reference/android/view/InputDevice) specifies that getId() can change after reconnect, reconfiguration or reboot. getDescriptor() (API 16) is intended to survive these changes, but two indistinguishable devices or multiple HID collections can have the same descriptor. Vendor/product IDs identify a model, not a physical unit; ControllerNumber is an allocation, not persistent identity. The existing minimum Android API is 23 and is unchanged.

Android native IDs locate live events only. Each detected connection gets a random application connection token; getId() is never saved as persistent identity. A nonempty descriptor is saved only when unique among connected inputs at selection time. If a descriptor collision appears during play, that remembered assignment stays paused for the rest of the session, even after one controller disappears, because the remaining physical controller cannot be identified safely. Exit play and choose connection-only assignments for those controllers. On a later app run, Android cannot tell the app whether an indistinguishable single controller is the original unit. No vendor-specific workaround can guarantee identification in that case.

The chooser lists each connected input once, followed by a separate remembered/connection-only choice, and rechecks the connection before saving. Long names wrap rather than sharing a fixed first-row height. On Android 11/API 30 and later, the app reads Bluetooth aliases using the existing Bluetooth permissions. It matches them to gamepad descriptors by reproducing the standard AOSP descriptor hash from vendor/product IDs and paired Bluetooth addresses (including bounded HID-collection nonce probes). This is a best-effort display-name lookup, not a new identity mechanism. Names or pairing order are never used to guess physical identity. Unsupported OEM formats, redacted addresses, missing aliases, ambiguity, and denied permissions keep the original input name. No hidden Android APIs are used. Reopen the chooser and reselect an existing assignment to update its saved label after renaming a controller in Bluetooth settings. Reopen it after connecting another controller. New text uses the neutral English resources; existing language resources fall back to English for these entries. Firmware macros already started on a smart brick have device-specific cancellation behavior; disconnect handling stops application sequences and sends zero output to mapped channels, but does not add a new firmware macro cancellation API.

## Modified files

Paths below are relative to BrickController2/.

| Files | Purpose |
| --- | --- |
| BrickController2.Android/PlatformServices/GameController/GameControllerService.cs | Keep per-connection tokens across discovery cycles; invalidate them on removal/reconfiguration; avoid duplicate registrations. |
| BrickController2.Android/PlatformServices/GameController/GamepadController.cs | Expose descriptor and connection identity while preserving legacy Controller N identifiers. |
| BrickController2/PlatformServices/InputDevice/IInputDevice.cs | Add optional persistent and runtime identity with compatible defaults for other platforms/providers. |
| BrickController2/PlatformServices/InputDevice/InputDeviceBase.cs | Include runtime source identity in input events, including reset events. |
| BrickController2/PlatformServices/InputDevice/InputDeviceEventArgs.cs | Carry runtime identity while preserving existing constructors and mapping identifiers. |
| BrickController2/CreationManagement/Creation.cs | Store assignment ID/name locally; existing rows retain Any controller. |
| BrickController2/CreationManagement/ICreationManager.cs and CreationManager.cs | Persist chooser changes through the existing repository, rolling back failed writes. |
| BrickController2/BusinessLogic/CreationPlaySession.cs | Resolve assignments, route simultaneous input, isolate players, handle disconnects and ambiguity. |
| BrickController2/BusinessLogic/IPlayLogic.cs and PlayLogic.cs | Support creation-level controller filtering and safely reset outputs/state on stop. |
| BrickController2/BusinessLogic/DI/BusinessLogicModule.cs | Register the session with the existing dependency container. |
| BrickController2/UI/ViewModels/CreationPageViewModel.cs | Short controller chooser, separate assignment-mode selection, and assignment validation. |
| BrickController2/UI/ViewModels/PlayerPageViewModel.cs | Connect all session bricks, forward lifecycle to the session, expose status and switch the primary profile independently. |
| BrickController2/UI/Pages/CreationPage.xaml and PlayerPage.xaml | Controller assignment and participant status using existing controls. |
| BrickController2/Resources/TranslationResources.resx | Add neutral UI strings. |
| BrickController2.Tests/BusinessLogic/CreationPlaySessionTests.cs | Cover routing, actual motor output isolation, disconnect/reconnect, ambiguity, None, and legacy Any behavior. |
| BrickController2.Tests/CreationManagement/ControllerAssignmentPersistenceTests.cs | Cover actual SQLite schema migration/reload, sharing exclusion, and write rollback. |
| BrickController2/UI/Pages/CreationListPage.xaml and UI/ViewModels/CreationListPageViewModel.cs | Select a subset of assigned creations from the list before launching simultaneous play. |
| BrickController2/UI/ViewModels/CreationListItemViewModel.cs | Keep checkbox selection transient and separate from persisted creations. |
| BrickController2/UI/Controls/Dialogs.xaml | Measure each selection row and wrap long controller names. |
| BrickController2/PlatformServices/InputDevice/AndroidBluetoothAliasMatcher.cs | Match public Bluetooth aliases by exact AOSP descriptor hash; fall back without guessing. |
| BrickController2.Tests/PlatformServices/InputDevice/AndroidBluetoothAliasMatcherTests.cs and UI/ViewModels/CreationSelectionTests.cs | Test nickname identity/fallback and selected-only play, cancellation, and invalid/conflicting selections. |
| ../docs/controller-assignment.md | Architecture, behavior, identity limitations and hardware verification instructions. |

## Validation and manual hardware test

Automated tests cover interleaved joystick/button events through real PlayLogic instances, independent output values, unknown controllers, disconnect/reconnect, duplicate assignments/descriptors, None, legacy Any, and SQLite migration/persistence. They do not replace Android Bluetooth hardware tests.

Verified locally:

- `dotnet test BrickController2/BrickController2.Tests/BrickController2.Tests.csproj --no-restore --verbosity minimal`: 671 passed, 0 failed, 0 skipped.
- `dotnet build BrickController2/BrickController2.Android/BrickController2.Android.csproj --no-restore --configuration Debug --verbosity minimal`: succeeded with 0 warnings and 0 errors.
- `git diff --check`: passed.

Bluetooth pairing, physical controller descriptors and actual motor behavior still require the hardware checks below.

1. Pair one controller. Leave one existing creation on Any controller, tap its normal Play action, and check its existing axes/buttons and profile selection.
2. Pair two gamepads with Android. Create Robot A and Robot B using separate smart bricks. Map the same buttons/axes to their respective motors. Choose each physical controller in its creation's Controller field; use Remember when available. For identical names, verify the Controller N number in the existing input-device tester.
3. On the Creations list, tap Play assigned creations. Leave only Robot A and Robot B checked, then tap Play selected. Confirm both smart bricks connect and both participants appear ready. Move A's stick and press its buttons while B is idle, then reverse the test. Hold different joystick values on both and press buttons concurrently; only the intended motors should respond.
4. Connect a third unassigned controller and use its controls. Neither assigned robot should move. Also test a creation set to None with the normal Play action.
5. While A is driving, power off controller A. A's mapped outputs should stop; B must continue responding. Reconnect A: a unique remembered descriptor should resume its assignment, while a connection-only choice requires exiting play and reassigning the new connection.
6. Exit play, restart the app, and verify remembered choices remain. Test connecting controllers in reverse order: remembered assignments must follow descriptors, regardless of Controller N. Connection-only choices must remain inactive after restart until reassigned.
7. If Android reports duplicate descriptors, choose This connection only for both controllers and repeat simultaneous play. Test a collision appearing after a remembered assignment: that creation should pause, and must not resume merely because one indistinguishable controller disconnects.
8. Leave the player screen and verify both robots stop and their smart bricks disconnect. Confirm ordinary creation/profile editing, save/reload and export/import retain mappings; imported creations should default to Any controller.

9. Rename paired gamepads in Android Bluetooth settings (for example GamePad White and GamePad Black). Reopen the app controller chooser and check that an exact supported descriptor match shows the nickname; otherwise the original name should remain readable and wrapped. Reselect to update an older saved label. On the Creations list, uncheck one creation and verify only the checked creation connects and receives input. Cancel selection and confirm tapping a row opens its details normally.

The updated installable test APK is generated at BrickController2/BrickController2.Android/bin/ControllerSelection/com.scn.BrickController2-Signed.apk. Install it over the earlier test build to retain local creations and assignments; do not uninstall the earlier test build just to update it.
