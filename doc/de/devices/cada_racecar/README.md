# CaDA Race Car

CaDA Race Car ist ein per Bluetooth LE gesteuertes RC-Auto. Die App steuert es über Bluetooth-LE-Werbenachrichten; es wird keine herkömmliche Verbindung hergestellt.

![CaDA Race Car](../../../../BrickController2/BrickController2/UI/Images/cada_racecar_image.png)

## Kanäle

Das Gerät stellt 4 Kanäle bereit:

| Kanäle | Zweck | Ausgabe |
|---|---|---|
| 1 | Gas | -100 % bis +100 % |
| 2 | Lenkung | -100 % bis +100 % |
| 3 | Frontlichter | Aus / Ein |
| 4 | Rücklichter | Aus / Ein |

Die Lichtkanäle werden eingeschaltet, wenn die Ausgabe in eine der beiden Richtungen über 50 % liegt.

## Erste Schritte

1. Schalte das Auto ein.
2. Öffne die Geräteliste und tippe auf ![](../../../icons/search.svg) **neue Geräte scannen**. Gefundene Geräte werden zur Liste hinzugefügt.
3. Verwende das Gerät in einer Kreation und weise seine Kanäle Steuerungsaktionen zu.

## Siehe auch

- [Geräte](../../pages/device-list/README.md)
- [Steuerungsaktion](../../pages/controller-action/README.md)
