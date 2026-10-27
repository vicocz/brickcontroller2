# JIESTAR - SCM 8

**JIESTAR - SCM 8** ist ein Bluetooth-Low-Energy-Modul (BLE), das bis zu acht Motoren ansteuert. Bis zu drei Module können gleichzeitig gesteuert werden.

![JIESTAR - SCM 8](../../../../BrickController2/BrickController2/UI/Images/jiestarscm8_image.png)

## Kanäle

Jedes **JIESTAR - SCM 8**-Modul stellt 8 Kanäle bereit:

| Kanäle | Zweck | Ausgabe |
|---|---|---|
| 1 - 8 | Anschlüsse A, B, C, D, E, F, G, H | -100 % bis +100 % |

## Erste Schritte

**Das Modul wird nicht durch die Suche gefunden.**

1. Öffne die Geräteliste, tippe auf ![](../../../icons/playlist_add.svg) **Geräte manuell hinzufügen**.
2. Aktiviere in der Liste zum manuellen Hinzufügen **JIESTAR - SCM 8 Gerät 1**. Für weitere Module verwendest du **Gerät 2** und **Gerät 3**. Tippe anschließend auf ![](../../../icons/done_outline.svg) **Änderungen übernehmen**.
3. Schalte das **JIESTAR - SCM 8**-Modul ein. Nach dem Einschalten ist **Gerät 1** standardmäßig eingestellt; die LED blinkt einmal pro Zyklus. Ändere die Gerätenummer mit einem kurzen Druck auf die Gerätetaste. Für **Gerät 2** blinkt die LED zweimal pro Zyklus, für **Gerät 3** dreimal.
4. Verwende das Gerät in einer Kreation und weise seine Kanäle Steuerungsaktionen zu.

## Hinweise

- Es werden bis zu drei **JIESTAR - SCM 8**-Module unterstützt.
- Die Ausgabe wird über Bluetooth-Werbenachrichten gesendet. Daher meldet das Modul weder einen Verbindungsstatus noch die Akkuspannung.
- Die Verbindungsnachricht wird erneut gesendet, nachdem alle Kanäle einige Sekunden lang auf null geblieben sind.

## Siehe auch

- [Geräte](../../pages/device-list/README.md)
- [Steuerungsaktion](../../pages/controller-action/README.md)
