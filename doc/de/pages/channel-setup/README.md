# Kanaleinrichtung

Auf dieser Seite kannst du einen Servomotor- oder Schrittmotorkanal eines verbundenen Geräts testen und einstellen.

## Möglichkeiten

- Prüfe Gerät, Kanal und Kanaltyp. Im Kanaltest kannst du einen unterstützten **Kanaltyp** auswählen.
- Stelle beim Schrittmotor den **Schrittwinkel** ein und teste die Bewegung mit **Schritt zurück** und **Schritt nach vorn**.
- Stelle beim Servomotor den **Servobereich** ein und teste Positionen von -100 % bis +100 %.
- Lege den **Basiswinkel** für einen Servo- oder Schrittmotorkanal fest. Bei Servokanälen kannst du ihn auch automatisch kalibrieren.
- Mit **Zurücksetzen** unter dem **Basiswinkel** bewegst du den Ausgang auf den eingestellten Basiswinkel.

## Symbolleiste

- ![](../../../icons/done_outline.svg) **Änderungen übernehmen** – Speichert die Kanaleinstellungen und schließt die Seite.

## Schaltflächen

- ![](../../../icons/settings_backup_restore.svg) **Zurücksetzen** – Stellt den Standardwert des zugehörigen Schiebereglers wieder her.

## Vorgehensweise

1. Stelle den Winkel mit dem Schieberegler ein. Der Wert ändert sich in 5-Grad-Schritten.
2. Teste den Motor mit den Testschaltflächen. Die Servotestschaltflächen stellen Positionen von -100 %, -50 %, 0 %, +50 % und +100 % ein. Die Schrittmotortestschaltflächen bewegen den Motor mit dem eingestellten Schrittwinkel zurück oder nach vorn.
3. Verwende bei einem Servo **Automatisch kalibrieren**, sofern das Gerät dies unterstützt. Du kannst den **Basiswinkel** auch manuell einstellen.
4. Wähle **Änderungen übernehmen**, um die Einstellungen zu speichern. Beim Verlassen der Seite wird die Verbindung zum Gerät getrennt.

## Hinweise

- Die Regler für **Schrittwinkel** und **Servobereich** reichen von 0 bis 180 Grad. Der **Basiswinkel** reicht von -180 bis 179 Grad.
- Im Kanaltest werden bei der Auswahl des Kanaltyps nur Typen angeboten, die das Gerät und der Kanal unterstützen.
- Die automatische Kalibrierung ist nur für unterstützte Servokanäle verfügbar.
- Wenn du die Seite im Kanaltest über eine Geräteseite öffnest, ist **Änderungen übernehmen** nicht verfügbar und die Testeinstellungen werden nicht gespeichert.

## Siehe auch

- [Steuerungsaktion](../controller-action/README.md)
- [Geräte](../device-list/README.md)
