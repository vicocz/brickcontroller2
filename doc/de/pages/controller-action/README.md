# Steuerungsaktion

Eine Steuerungsaktion verbindet ein Controllerereignis mit einem Gerätekanal oder einem Gerätemakro.

## Wichtige Optionen

- **Gerät und Kanal**: Der Ausgang, den die Aktion steuert.
- **Umkehren**: Kehrt die Ausgaberichtung um.
- **Max Leistung**: Begrenzt die Ausgangsleistung. Gilt nur für den normalen Kanaltyp.
- **Tastenart**: Legt fest, wie sich eine Taste verhält (siehe unten).
- **Joy-Typ**, **Joy-Charakteristik**, **Joy-Totzone** und **Joy-Aktivzone**: Legen fest, wie sich eine Achse verhält (siehe unten).
- **Makro**: Startet eine Gerätefunktion, zum Beispiel die Wiedergabe eines Tons, anstatt einen Kanal anzusteuern.

## Tastenarten

Eine Tastenaktion setzt die Kanalausgabe, wenn die Taste gedrückt oder losgelassen wird.

| Tastenart | Verhalten |
|---|---|
| Normal | Die Ausgabe ist aktiv, solange du die Taste gedrückt hältst, und wird beim Loslassen deaktiviert. |
| Einfacher Umschalter | Jeder Tastendruck schaltet die Ausgabe ein oder aus. |
| Abwechselnd | Jeder Tastendruck schaltet die Ausgabe ein, beim nächsten Mal in die entgegengesetzte Richtung. |
| Kreisförmig | Jeder Tastendruck schaltet die Ausgabe zum nächsten Wert weiter und beginnt nach dem letzten Wert wieder von vorn. |
| Pingpong | Ein Tastendruck startet die Ausgabe, der nächste stoppt sie und der darauffolgende startet sie in der entgegengesetzten Richtung. |
| Stoppen | Stoppt den Kanal und löscht den Zustand aller Achsen, die ihn ansteuern. |
| Beschleuniger | Jeder Tastendruck erhöht die Ausgabe um den Beschleunigungsschritt des Geräts, bis zum Grenzwert. |
| Sequenz | Startet oder stoppt die ausgewählte Sequenz auf dem Kanal. |
| Makro | Startet ein Gerätemakro. |

## Achsentypen

Eine Achsenaktion ordnet die Position eines Sticks oder Triggers der Ausgabe zu.

| Joy-Typ | Verhalten |
|---|---|
| Normal | Die Ausgabe folgt der Position der Achse. |
| Zug | Beim Beschleunigen folgt die Ausgabe der Achse, nimmt beim Zurückbewegen aber nicht ab. Das Abbremsen erfolgt schrittweise; die Ausgabe bleibt auf null, bis die Achse in die Mittelstellung zurückkehrt. |
| Beschleuniger | Hältst du die Achse an ihrem Endpunkt, ändert sich die Ausgabe jeweils um den Beschleunigungsschritt des Geräts. Beim Loslassen behält die Ausgabe ihren Wert. |

Weitere Optionen:

- **Joy-Totzone**: Der mittlere Achsenbereich, der ignoriert wird.
- **Joy-Aktivzone**: Der verwendete Achsenbereich. Am Ende dieses Bereichs wird die volle Ausgabe erreicht.
- **Joy-Charakteristik**: Die Reaktionskurve der Achse. Linear bedeutet eine proportionale Reaktion. Exponentiell ermöglicht eine feinere Steuerung nahe der Mitte; logarithmisch bewirkt eine stärkere Reaktion nahe der Mitte.

## Kanaltypen

Der Kanaltyp legt fest, was die Ausgabe für den angeschlossenen Motor bedeutet. Er ist für Geräte und Kanäle verfügbar, die ihn unterstützen.

| Kanaltyp | Verhalten |
|---|---|
| Normal | Die Ausgabe entspricht der Motorleistung von -100 % bis +100 %. Sie wird durch **Max Leistung** begrenzt. |
| Servo | Die Ausgabe bestimmt die Position eines Servos. Lege den **Servobereich** (0 bis 180 Grad) und den **Grundwinkel** (-180 bis 179 Grad) fest. |
| Schrittmotor | Die Ausgabe bewegt einen Schrittmotor schrittweise. Lege mit dem **Schrittwinkel** (0 bis 180 Grad) die Schrittgröße fest. |

Bei den Kanaltypen Servo und Schrittmotor wird neben dem Kanaltyp eine Schaltfläche zum Einrichten des Kanals angezeigt.

## Schaltflächen und Symbolleiste

- ![](../../../icons/done_outline.svg) **Änderungen übernehmen** – Speichere die Aktion und schließe die Seite.
- ![](../../../icons/contactless.svg) **Gerätedetails anzeigen** – Öffne das ausgewählte Gerät.
- ![](../../../icons/settings.svg) **Kanaleinrichtung** / **Sequenz bearbeiten** – Öffne die Kanaleinrichtung oder bearbeite die ausgewählte Sequenz.
- ![](../../../icons/cached.svg) **Makros neu laden** – Lade die Makros des Geräts neu.

## Siehe auch

- [Steuerungsprofil](../controller-profile/README.md)
- [Geräte](../device-list/README.md)
