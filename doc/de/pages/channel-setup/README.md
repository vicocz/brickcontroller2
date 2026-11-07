# Kanaleinrichtung

Auf dieser Seite kannst du einen Servomotor- oder Schrittmotorkanal eines verbundenen Geräts testen und einstellen.

## Möglichkeiten

- Prüfe Gerät, Kanal und Kanaltyp. Im Kanaltest kannst du einen unterstützten **Kanaltyp** auswählen.
- Stelle beim Schrittmotor den **Schrittwinkel** ein und teste die Bewegung mit **Schritt zurück** und **Schritt nach vorn**.
- Teste beim Servomotor Positionen von -100 % bis +100 %. Sofern das Gerät dies unterstützt, kannst du auch den **Servobereich** einstellen.
- Lege den **Basiswinkel** für einen Servo- oder Schrittmotorkanal fest. Bei Servokanälen kannst du ihn auch automatisch kalibrieren. Bei Schrittmotorkanälen dient der Basiswinkel nur zum Testen und wird nicht gespeichert.
- Mit **Zurücksetzen** unter dem **Basiswinkel** bewegst du den Ausgang auf den eingestellten Basiswinkel.

## Symbolleiste

- ![](../../../icons/done_outline.svg) **Änderungen übernehmen** – Speichert die Kanaleinstellungen und schließt die Seite.

## Schaltflächen

- ![](../../../icons/settings_backup_restore.svg) **Zurücksetzen** – Stellt den Standardwert des zugehörigen Schiebereglers wieder her.

## Vorgehensweise

1. Stelle den Winkel mit dem Schieberegler ein. Der Wert ändert sich in 5-Grad-Schritten.
2. Teste den Motor mit den Testschaltflächen. Die Servotestschaltflächen stellen Positionen von -100 %, -50 %, 0 %, +50 % und +100 % ein. Die Schrittmotortestschaltflächen bewegen den Motor mit dem eingestellten Schrittwinkel zurück oder nach vorn.
3. Verwende bei einem Servo **Automatisch kalibrieren**, sofern das Gerät dies unterstützt. Du kannst den **Basiswinkel** auch manuell einstellen.
4. Wähle **Änderungen übernehmen**, um die Einstellungen zu speichern. Bei Schrittmotorkanälen wird nur der **Schrittwinkel** gespeichert; der **Basiswinkel** dient nur zum Testen. Beim Verlassen der Seite wird die Verbindung zum Gerät getrennt.

## Hinweise

- Die Regler für **Schrittwinkel** und **Servobereich** reichen von 0 bis 180 Grad. Der **Basiswinkel** reicht von -180 bis 179 Grad.
- Im Kanaltest werden bei der Auswahl des Kanaltyps nur Typen angeboten, die das Gerät und der Kanal unterstützen.
- Die automatische Kalibrierung ist nur für unterstützte Servokanäle verfügbar.
- Der **Servobereich** lässt sich nur bei Geräten ändern, die dies unterstützen. Bei anderen Geräten, z. B. Technic Move, ist der Regler deaktiviert.
- Wenn du die Seite im Kanaltest über eine Geräteseite öffnest, ist **Änderungen übernehmen** nicht verfügbar und die Testeinstellungen werden nicht gespeichert.

## Siehe auch

- [Steuerungsaktion](../controller-action/README.md)
- [Geräte](../device-list/README.md)
