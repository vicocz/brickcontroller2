# Sequenzeditor

Eine Sequenz ist eine wiederverwendbare Folge von Steuerungspunkten. Beim Abspielen durchläuft die Ausgabe nacheinander diese Punkte. So kannst du Bewegungsabläufe automatisieren, zum Beispiel ein Tor öffnen oder ein Muster abfahren.

## Sequenzeigenschaften

- **Name**: Tippe oben auf den Namen, um die Sequenz umzubenennen.
- **Schleife**: Wenn diese Option aktiviert ist, beginnt die Sequenz nach dem letzten Steuerungspunkt wieder beim ersten.
- **Interpolieren**: Wenn diese Option aktiviert ist, ändert sich der Wert gleichmäßig zwischen den Steuerungspunkten. Andernfalls springt er direkt zum Wert des nächsten Punkts.

## Steuerungspunkte

Jeder Steuerungspunkt hat folgende Eigenschaften:

- **Wert (%)**: Der Ausgabewert von -100 bis 100 in Schritten von 5. Negative Werte steuern den Kanal in die entgegengesetzte Richtung.
- **Dauer (ms)**: Legt fest, wie lange dieser Steuerungspunkt gilt. Tippe auf die Dauer, um sie zu ändern.

Tippe auf die schwebende Schaltfläche ![](../../../icons/add.svg) (**neuen Kontrollpunkt erstellen**), um einen Kontrollpunkt hinzuzufügen. Tippe in einer Zeile auf ![](../../../icons/clear.svg), um den Punkt zu löschen.

## Symbolleiste

- ![](../../../icons/share.svg) **Sequenz als Text teilen** – Teile die Sequenz als Text.
- ![](../../../icons/upload_file.svg) **Sequenz als Datei teilen** – Teile die Sequenz als Datei.
- ![](../../../icons/copy.svg) **Sequenz in die Zwischenablage kopieren** (Überlaufmenü) – Kopiere die Sequenz in die Zwischenablage.
- ![](../../../icons/file_upload.svg) **Sequenz exportieren** (Überlaufmenü) – Exportiere die Sequenz in eine Datei.
- ![](../../../icons/done_outline.svg) **Änderungen übernehmen** – Speichere die Sequenz und schließe den Editor.
- ![](../../../icons/help_outline.svg) **Hilfe** (Überlaufmenü) – Öffne diese Hilfeseite.
