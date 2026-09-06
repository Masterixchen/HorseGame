namespace Core;

/// <summary>Ein einzelnes Pferd - das Herzstück des Spiels. Rasse, Merkmale und Werte stehen bei
/// der Geburt fest und ändern sich nie durch Zufall (Designgrundsatz 3). Nur Training, Ausrüstung
/// und Pflege wirken auf ein lebendes Pferd - hier ist erstmal nur Training umgesetzt.</summary>
public class Pferd
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = "";
    public Geschlecht Geschlecht { get; set; }
    public string RasseId { get; set; } = "";
    public string Farbe { get; set; } = "";
    public DateTime Geburtsdatum { get; set; }
    public Seltenheit Seltenheit { get; set; }
    public int Blutlinienstufe { get; set; }

    public MerkmalsInstanz ImplizitesMerkmal { get; set; } = new();
    public List<MerkmalsInstanz> Praefixe { get; set; } = new();
    public List<MerkmalsInstanz> Suffixe { get; set; } = new();

    public StatBlock Werte { get; set; } = new();
    public float Kondition { get; set; } = 100f;
    public float Stimmung { get; set; } = 100f;

    public Training? AktivesTraining { get; set; }

    // Nur bei Stuten belegt, solange eine Zucht läuft. Wird von Spielstand.Advance ausgewertet,
    // weil die Geburt ein neues Pferd zur Stallliste hinzufügt - das kann eine einzelne Pferd-
    // Instanz nicht selbst auslösen.
    public Traechtigkeit? Traechtigkeit { get; set; }

    public int AlterInJahren(DateTime bezugsdatum) => (int)((bezugsdatum - Geburtsdatum).TotalDays / 365.25);

    /// <summary>Solange noch nicht alle Merkmale aufgedeckt sind, eignet sich das Pferd nicht als
    /// Zuchtpartner - eine ehrliche Vorschau braucht bekannte Ausgangswerte.</summary>
    public bool AlleMerkmaleBekannt => AlleMerkmale().All(m => m.Bekannt);

    /// <summary>Alle Merkmale zusammen - praktisch für Anzeige und für die Modifikator-Summe.</summary>
    public IEnumerable<MerkmalsInstanz> AlleMerkmale()
    {
        yield return ImplizitesMerkmal;
        foreach (var praefix in Praefixe) yield return praefix;
        foreach (var suffix in Suffixe) yield return suffix;
    }

    /// <summary>Summiert alle gewürfelten Effekte zu einem Attribut auf (in Prozent). Attribute wie
    /// Erholung oder Trainingsgeschwindigkeit haben keinen eigenen Stat-Wert, sondern wirken
    /// ausschließlich über diese Summe.</summary>
    public float ModifikatorSumme(Merkmalsattribut attribut)
    {
        float summe = 0f;
        foreach (var merkmal in AlleMerkmale())
            foreach (var effekt in merkmal.Effekte)
                if (effekt.Attribut == attribut)
                    summe += effekt.Wert;
        return summe;
    }

    public void StarteTraining(StatTyp ziel, DateTime jetzt, GameRandom zufall)
    {
        if (AktivesTraining != null)
            throw new InvalidOperationException("Es läuft bereits ein Training für dieses Pferd.");

        // Trainingsgeschwindigkeit aus Suffixen verkürzt die Dauer, mindestens aber eine Stunde.
        float geschwindigkeitsBonus = ModifikatorSumme(Merkmalsattribut.Trainingsgeschwindigkeit) / 100f;
        float basisStunden = zufall.NaechsterBereich(4f, 8f);
        float stunden = MathF.Max(1f, basisStunden / (1f + geschwindigkeitsBonus));

        AktivesTraining = new Training { Ziel = ziel, Start = jetzt, Ende = jetzt.AddHours(stunden) };
    }

    /// <summary>Rechnet den Zeitraum [von, bis) für dieses Pferd durch: fälliges Training
    /// abschließen, sonst Kondition und Stimmung fortschreiben. Wird von Spielstand.Advance in
    /// Schritten von höchstens einer Stunde aufgerufen, damit sich nichts überholt.</summary>
    public void Advance(DateTime von, DateTime bis)
    {
        foreach (var merkmal in AlleMerkmale())
            merkmal.PruefeAufdeckung(bis);

        bool trainierteWaehrendSchritt = AktivesTraining != null;

        if (AktivesTraining != null && AktivesTraining.Ende <= bis)
        {
            SchliesseTrainingAb(AktivesTraining);
            AktivesTraining = null;
        }

        if (trainierteWaehrendSchritt) return;

        // Erholung ohne Training: Kondition und Stimmung steigen langsam Richtung 100,
        // beschleunigt durch das Suffix-Attribut "Erholung". Feinere Pflege-Mechanik folgt in Phase 3.
        float erholungsBonus = 1f + ModifikatorSumme(Merkmalsattribut.Erholung) / 100f;
        float zuwachs = (float)(bis - von).TotalHours * 2f * erholungsBonus;
        Kondition = MathF.Min(100f, Kondition + zuwachs);
        Stimmung = MathF.Min(100f, Stimmung + zuwachs * 0.5f);
    }

    private void SchliesseTrainingAb(Training training)
    {
        var stat = Werte.Hole(training.Ziel);
        float luecke = stat.Potenzial - stat.Aktuell;
        if (luecke > 0f)
        {
            // Abnehmender Ertrag: je näher am Potenzial, desto kleiner der Schritt.
            stat.Aktuell = MathF.Min(stat.Potenzial, stat.Aktuell + luecke * 0.15f);
        }

        Kondition = MathF.Max(0f, Kondition - 8f);
    }
}
