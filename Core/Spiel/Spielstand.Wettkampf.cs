namespace Core;

public partial class Spielstand
{
    private const int MaxErgebnisHistorie = 50;

    /// <summary>Meldet ein Pferd zum nächsten Termin einer Wettkampfklasse an. Die Klasse wird
    /// dabei eingefroren (siehe WettkampfAnmeldung) - Advance(DateTime) braucht später keine
    /// Inhaltsdatenbank mehr, um die Anmeldung auszuwerten.</summary>
    public void MeldeAn(Wettkampfklasse klasse, Pferd pferd, DateTime jetzt)
    {
        if (pferd.IstBeschaeftigt)
            throw new InvalidOperationException("Dieses Pferd ist gerade anderweitig beschäftigt.");
        if (!pferd.AlleMerkmaleBekannt)
            throw new InvalidOperationException("Dieses Pferd ist noch nicht vollständig aufgedeckt.");
        if (!WettkampfRechner.DarfTeilnehmen(klasse, pferd, Ansehen, jetzt))
            throw new InvalidOperationException("Die Voraussetzungen für diese Wettkampfklasse sind nicht erfüllt.");

        pferd.Anmeldung = new WettkampfAnmeldung
        {
            Klasse = klasse,
            Zeitpunkt = WettkampfRechner.NaechsterZeitpunkt(klasse, jetzt)
        };
    }

    private void LoeseWettkampfAus(Pferd pferd, WettkampfAnmeldung anmeldung, DateTime zeitpunkt)
    {
        var ergebnis = WettkampfRechner.Werte(Zufall, pferd, anmeldung.Klasse, Ausruestungen, zeitpunkt);

        Guthaben += ergebnis.Preisgeld;
        Ansehen += ergebnis.AnsehenGewonnen;
        if (ergebnis.GewonnenesMaterial is MaterialTyp material)
            Materialbestand[material] = Materialbestand.GetValueOrDefault(material) + 1;

        WettkampfErgebnisse.Insert(0, ergebnis);
        if (WettkampfErgebnisse.Count > MaxErgebnisHistorie)
            WettkampfErgebnisse.RemoveRange(MaxErgebnisHistorie, WettkampfErgebnisse.Count - MaxErgebnisHistorie);
    }
}
