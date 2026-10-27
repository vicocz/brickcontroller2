# PowerBox - M Battery

**PowerBox - M Battery** ist ein Bluetooth-Low-Energy-Motormodul.

![PowerBox - M Battery](../../../../BrickController2/BrickController2/UI/Images/powerboxmbattery_image.png)

**Achtung:** Diese Geräte sind innerhalb derselben Kreation nicht miteinander kompatibel, da sie identische BLE-Werbenachrichten verwenden.

- [PowerBox - A Series](../powerboxaseries/README.md)
- [PowerBox - M Battery](../powerboxmbattery/README.md)
- [JIESTAR - SCM 4 Gerät 1](../jiestarscm4/README.md)

## Kanäle

Das **PowerBox - M Battery**-Modul stellt 1 Kanal bereit:

| Kanäle | Zweck | Ausgabe |
|---|---|---|
| 1 | Motor A | -100 % bis +100 % |

## Erste Schritte

**Das Modul wird nicht durch die Suche gefunden.**

1. Öffne die Geräteliste, tippe auf ![](../../../icons/playlist_add.svg) **Geräte manuell hinzufügen**.
2. Aktiviere in der Liste zum manuellen Hinzufügen **PowerBox - M Battery Gerät**. Tippe anschließend auf ![](../../../icons/done_outline.svg) **Änderungen übernehmen**.
3. Schalte das **PowerBox - M Battery**-Modul ein.
4. Verwende das Gerät in einer Kreation und weise seinen Kanal einer Steuerungsaktion zu.

## Hinweise

- Die Ausgabe wird über Bluetooth-Werbenachrichten gesendet. Daher meldet das Modul weder einen Verbindungsstatus noch die Akkuspannung.
- Die Verbindungsnachricht wird erneut gesendet, nachdem alle Kanäle einige Sekunden lang auf null geblieben sind.

## Siehe auch

- [Geräte](../../pages/device-list/README.md)
- [Steuerungsaktion](../../pages/controller-action/README.md)
