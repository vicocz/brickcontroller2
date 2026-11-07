# BuWizz 2

BuWizz 2 ist ein Bluetooth-LE-Akkustein, der bis zu vier Motoren mit Strom versorgt und steuert.

![BuWizz 2](../../../../BrickController2/BrickController2/UI/Images/buwizz_image.png)

## Kanäle

Das Gerät stellt 4 Kanäle bereit:

| Kanäle | Zweck | Ausgabe |
|---|---|---|
| 1 - 4 | Power-Functions-Motoren | -100 % bis +100 % |

## Erste Schritte

1. Schalte BuWizz 2 ein.
2. Öffne die Geräteliste und tippe auf ![](../../../icons/search.svg) **neue Geräte scannen**. Gefundene Geräte werden zur Liste hinzugefügt.
3. Verwende das Gerät in einer Kreation und weise seine Kanäle Steuerungsaktionen zu.

## Ausgabestufe

Die Ausgabestufe begrenzt die Motorleistung. Es gibt vier Stufen: **Niedrig**, **Normal** (Standard), **Hoch** und **Ludicrous**. Mit der Einstellung **Standard-Ausgabestufe** legst du den Standardwert fest. Während der Steuerung kannst du die aktuelle Stufe mit dem Makro **Ausgabestufe einstellen** ändern.

## Hinweise

- Höhere Ausgabestufen benötigen mehr Akkustrom und verkürzen die Laufzeit.
- Während der Verbindung meldet das Gerät sowohl Akku- als auch Motorspannung.
- Bei einigen BuWizz-2-Geräten sind die Anschlüsse vertauscht. Die App erkennt diese Geräte und tauscht die Kanäle automatisch.

## Siehe auch

- [Geräte](../../pages/device-list/README.md)
- [Steuerungsaktion](../../pages/controller-action/README.md)
