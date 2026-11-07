# Mould King - MK 5.0

**Mould King - MK 5.0** ist ein Bluetooth-Low-Energy-Modul (BLE) mit drei integrierten Motoren und LEGO-kompatiblen Kreuzachsen.

![Mould King - MK 5.0](../../../../BrickController2/BrickController2/UI/Images/mk5_image.png)

## Kanäle

Das **Mould King - MK 5.0**-Modul stellt 5 Kanäle bereit:

| Kanäle | Zweck | Ausgabe |
|---|---|---|
| 1 | Motoren A+B | -100 % bis +100 % |
| 2 | Motor T | -100 % bis +100 % |
| 3 | Effekt „Schuss“ | Aus / Ein |
| 4 | Motoren A⇄B+T | -100 % bis +100 % |
| 5 (virtuell) | Motor T sperren: A⇄B | Aus / Ein |

Kanal 5 ist ein virtueller Kanal. Wenn er aktiviert ist, ändert er das Verhalten von Kanal 4: Die Motoren A und B drehen sich dann in entgegengesetzte Richtungen, während Motor T unverändert bleibt.

## Erste Schritte

**Das Modul wird nicht durch die Suche gefunden.**

1. Öffne die Geräteliste, tippe auf ![](../../../icons/playlist_add.svg) **Geräte manuell hinzufügen**.
2. Aktiviere in der Liste zum manuellen Hinzufügen **Mould King - MK 5.0 Gerät**. Tippe anschließend auf ![](../../../icons/done_outline.svg) **Änderungen übernehmen**.
3. Schalte das **Mould King - MK 5.0**-Modul ein.
4. Verwende das Gerät in einer Kreation und weise seine Kanäle Steuerungsaktionen zu.

## Hinweise

- Die Ausgabe wird über Bluetooth-Werbenachrichten gesendet. Daher meldet das Modul weder einen Verbindungsstatus noch die Akkuspannung.
- Die Verbindungsnachricht wird erneut gesendet, nachdem alle Kanäle einige Sekunden lang auf null geblieben sind.

## Siehe auch

- [Geräte](../../pages/device-list/README.md)
- [Steuerungsaktion](../../pages/controller-action/README.md)
