# SBrick Light

SBrick Light ist ein Bluetooth-LE-Controller für LED-Leuchten mit bis zu acht Lichtanschlüssen.

![SBrick Light](../../../../BrickController2/BrickController2/UI/Images/sbricklight_image.png)

## Kanäle

Das Gerät hat 8 Anschlüsse (A - H), jeweils mit einer RGB-Leuchte. Jeder Anschluss lässt sich auf eine von zwei Arten steuern:

- **RGB** – Lege die RGB-Farbe des Anschlusses in den Einstellungen fest. Der Kanal steuert dann dessen Helligkeit (8 Kanäle, einer pro Anschluss).
- **Mikrokanäle** – Steuere die roten, grünen und blauen Farbanteile des Anschlusses getrennt (3 Mikrokanäle je Anschluss, insgesamt 24).

| Modus | Kanäle | Zweck | Ausgabe |
|---|---|---|---|
| RGB | 1 - 8 (einer je Anschluss A - H) | Helligkeit der für den Anschluss festgelegten Farbe | 0 % bis 100 % |
| Mikrokanäle | 24 (R, G, B je Anschluss) | Helligkeit jedes Farbanteils | 0 % bis 100 % |

## Erste Schritte

1. Schalte SBrick Light ein.
2. Öffne die Geräteliste und tippe auf ![](../../../icons/search.svg) **neue Geräte scannen**. Gefundene Geräte werden zur Liste hinzugefügt.
3. Verwende das Gerät in einer Kreation und weise seine Kanäle Steuerungsaktionen zu.

## Einstellungen

Lege in den Geräteeinstellungen unter **Kanalfarben** die Farbe für jeden Anschluss (A - H) fest. Im RGB-Modus wird diese Farbe verwendet. Standardmäßig ist Weiß eingestellt.

## Siehe auch

- [Geräte](../../pages/device-list/README.md)
- [Steuerungsaktion](../../pages/controller-action/README.md)
