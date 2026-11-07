# BuWizz 3

BuWizz 3 (BuWizz 3.0 Pro) ist ein Bluetooth-LE-Akkustein mit vier Powered-Up-Anschlüssen und zwei Power-Functions-Anschlüssen.

![BuWizz 3](../../../../BrickController2/BrickController2/UI/Images/buwizz3_image.png)

## Kanäle

Das Gerät stellt 6 Kanäle bereit:

| Kanäle | Zweck | Ausgabe |
|---|---|---|
| 1 - 4 | Powered-Up-Motoren | -100 % bis +100 %, Servo oder Schrittmotor |
| A, B | Power-Functions-Motoren | -100 % bis +100 % |

## Erste Schritte

1. Schalte BuWizz 3 ein.
2. Öffne die Geräteliste und tippe auf ![](../../../icons/search.svg) **neue Geräte scannen**. Gefundene Geräte werden zur Liste hinzugefügt.
3. Verwende das Gerät in einer Kreation und weise seine Kanäle Steuerungsaktionen zu.

## Strombegrenzung

Jeder Anschluss hat eine eigene Strombegrenzung, die du in den Geräteeinstellungen ändern kannst. Die Standardwerte entsprechen denen der offiziellen BuWizz-App:

| Anschlüsse | Standardgrenzwert |
|---|---|
| 1 - 4 (Powered Up) | 1050 mA |
| A, B (Power Functions) | 2100 mA |

## Lagerungsmodus

Der Lagerungsmodus trennt den Akku vom Gerät, zum Beispiel zur Aufbewahrung oder zum Transport. Du kannst ihn bei bestehender Verbindung über die Geräteseite aktivieren und musst die Aktion bestätigen. Um das Gerät anschließend wieder einzuschalten, musst du üblicherweise das Ladegerät anschließen.

## Hinweise

- Die Ausgabearten Servo und Schrittmotor sind nur an den Powered-Up-Anschlüssen 1 - 4 verfügbar.
- Während der Verbindung wird die Akkuspannung auf der Geräteseite angezeigt.

## Siehe auch

- [Geräte](../../pages/device-list/README.md)
- [Steuerungsaktion](../../pages/controller-action/README.md)
