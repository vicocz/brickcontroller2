# Steuerungsprofil

Ein Steuerungsprofil ordnet Controllerereignisse (Tasten, Sticks und Trigger) Aktionen für deine Geräte zu.

## Ereignis hinzufügen

Ein Ereignis fügst du mit dem Controller hinzu. Daher muss zuerst ein Gamepad oder ein anderes Eingabegerät verbunden sein.

1. Verbinde den Controller und stelle sicher, dass sich mindestens ein Gerät in der Geräteliste befindet. Wenn keines vorhanden ist, wirst du aufgefordert, zuerst nach Geräten zu suchen.
2. Tippe auf eine der Schaltflächen zum Hinzufügen:
   - ![](../../../icons/add.svg) **neues Steuerungsereignis erstellen**: Akzeptiert eine Taste oder einen Joystick.
   - ![](../../../icons/hdr_auto.svg) **neues Controller-Tastenereignis erstellen**: Akzeptiert nur eine Taste.
   - ![](../../../icons/gamepad.svg) **neues Controller-Achsenereignis erstellen**: Akzeptiert nur einen Joystick.
3. Drücke nach Aufforderung eine Taste oder bewege den Joystick am Controller. Die erste erkannte Eingabe wird verwendet.
4. Die Aktionsseite wird geöffnet. Dort legst du fest, was das Ereignis bewirkt.

Wenn keine Eingabe erkannt wird, kannst du den Dialog abbrechen.

## Mehrere Controller

Standardmäßig reagiert ein Ereignis auf die ausgewählte Taste oder den Joystick jedes verbundenen Controllers.

Wenn mehrere Eingabegeräte verbunden sind, verwende ![](../../../icons/playlist_add.svg) **neues Steuerungsereignis für einen bestimmten Controller erstellen**. Das Ereignis wird dann an den verwendeten Controller gebunden, der zum Beispiel als „Controller 1“ angezeigt wird. Andere Controller lösen es nicht aus. So können zwei Controller mit denselben Tasten verschiedene Geräte steuern.

Du kannst dieselbe Taste oder denselben Joystick für verschiedene Controller als separate Ereignisse einrichten.

## Funktionen

- Eine Aktion für ein Controllerereignis hinzufügen.
- Vorhandene Aktionen bearbeiten oder löschen.
- Das Profil mit dem Controllertester überprüfen.

## Symbolleiste

- ![](../../../icons/file_upload.svg) **Steuerungsprofil exportieren** – Exportiere das Profil in eine Datei.
- ![](../../../icons/copy.svg) **Steuerungsprofil in die Zwischenablage kopieren** – Kopiere das Profil.

## Wischaktionen

- ![](../../../icons/cancel.svg) **Löschen** – Lösche ein Ereignis oder eine Aktion (nach rechts wischen).
- ![](../../../icons/add.svg) **Hinzufügen** – Füge einem Ereignis eine Aktion hinzu (nach links wischen).
- ![](../../../icons/settings.svg) **Kanaleinrichtung** – Öffne die Kanaleinrichtung einer Aktion (nach links wischen).

## Weitere Schaltflächen

- ![](../../../icons/play_arrow.svg) **Abspielen** – Starte die Kreation mit diesem Profil.
- ![](../../../icons/warning_amber.svg) **Warnung** – Die Aktion ist ungültig, zum Beispiel weil das Gerät fehlt.

## Tipps

- Ein Ereignis kann mehrere Aktionen an verschiedenen Geräten steuern.

## Siehe auch

- [Steuerungsaktion](../controller-action/README.md)
