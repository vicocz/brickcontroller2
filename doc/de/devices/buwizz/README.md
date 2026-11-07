# BuWizz

BuWizz (1.0) ist ein Bluetooth-LE-Akkustein, der bis zu vier Motoren mit Strom versorgt und steuert.

![BuWizz](../../../../BrickController2/BrickController2/UI/Images/buwizz_image.png)

## Kanäle

Das Gerät stellt 4 Kanäle bereit:

| Kanäle | Zweck | Ausgabe |
|---|---|---|
| 1 - 4 | Power-Functions-Motoren | -100 % bis +100 % |

## Erste Schritte

1. Schalte BuWizz ein.
2. Öffne die Geräteliste und tippe auf ![](../../../icons/search.svg) **neue Geräte scannen**. Gefundene Geräte werden zur Liste hinzugefügt.
3. Verwende das Gerät in einer Kreation und weise seine Kanäle Steuerungsaktionen zu.

## Ausgabestufe

Die Ausgabestufe begrenzt die Motorleistung. Es gibt drei Stufen: **Niedrig**, **Normal** (Standard) und **Hoch**. Mit der Einstellung **Standard-Ausgabestufe** legst du den Standardwert fest.

## Hinweise

- Höhere Ausgabestufen benötigen mehr Akkustrom und verkürzen die Laufzeit.
- Die Akkuspannung wird auf der Geräteseite nicht angezeigt.

## Siehe auch

- [Geräte](../../pages/device-list/README.md)
- [Steuerungsaktion](../../pages/controller-action/README.md)
