namespace Core;

/// <summary>Ein einzelnes Pferd - das Herzstück des Spiels. Rasse, Merkmale und Werte stehen bei
/// der Geburt fest und ändern sich nie durch Zufall (Designgrundsatz 3). Nur Training, Ausrüstung
/// und Pflege wirken auf ein lebendes Pferd.</summary>
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

    // Nicht die Uhr begrenzt das Training, sondern die Kondition (siehe Phase-4-Auftrag). Die
    // Warteschlange arbeitet sich in Advance von selbst ab, auch während das Spiel geschlossen
    // ist, und pausiert von selbst, wenn die Kondition für die nächste Einheit nicht mehr reicht.
    public List<Trainingsauftrag> Warteschlange { get; set; } = new();
    public Training? LaufendesTraining { get; set; }

    // Nur bei Stuten belegt, solange eine Zucht läuft. Wird von Spielstand.Advance ausgewertet,
    // weil die Geburt ein neues Pferd zur Stallliste hinzufügt - das kann eine einzelne Pferd-
    // Instanz nicht selbst auslösen.
    public Traechtigkeit? Traechtigkeit { get; set; }

    // Nur bei Marktpferden/Startpferden leer. Für Eigenzucht-Beschränkungen bei Wettkämpfen
    // und spätere Abstammungsanzeigen.
    public Guid? MutterId { get; set; }
    public Guid? VaterId { get; set; }

    // Mehrere gleichzeitig möglich - man darf für mehrere kommende Termine im Voraus anmelden.
    public List<WettkampfAnmeldung> Anmeldungen { get; set; } = new();

    // Nur die Slots mit belegtem Eintrag sind vorhanden. Wert ist die Id der Ausruestung-Instanz,
    // nicht der Definition - dieselbe Definition kann mehrfach besessen werden.
    public Dictionary<AusruestungsSlot, Guid> Ausgeruestet { get; set; } = new();

    // Beschleunigt gegenüber der Echtzeit (siehe Zeitkonstanten.AlterungsFaktor) - nur die
    // Alters*anzeige* und darauf basierende Regeln (Wettkampf-Altersfaktor, Alters-Einschränkungen)
    // sind betroffen, keine anderen Zeitabläufe.
    public int AlterInJahren(DateTime bezugsdatum) =>
        (int)((bezugsdatum - Geburtsdatum).TotalDays / 365.25 * Zeitkonstanten.AlterungsFaktor);

    /// <summary>Solange noch nicht alle Merkmale aufgedeckt sind, eignet sich das Pferd nicht als
    /// Zuchtpartner - eine ehrliche Vorschau braucht bekannte Ausgangswerte.</summary>
    public bool AlleMerkmaleBekannt => AlleMerkmale().All(m => m.Bekannt);

    /// <summary>Training und Wettkampf-Anmeldungen laufen nebenbei mit - nur eine Trächtigkeit
    /// blockiert eine neue Zucht oder Wettkampfteilnahme.</summary>
    public bool IstBeschaeftigt => Traechtigkeit != null;

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

    public void TrainingEinreihen(StatTyp ziel, Intensitaet intensitaet) =>
        Warteschlange.Add(new Trainingsauftrag { Ziel = ziel, Intensitaet = intensitaet });

    public void WarteschlangeLeeren() => Warteschlange.Clear();

    /// <summary>Rechnet den Zeitraum [von, bis) für dieses Pferd durch: Merkmale aufdecken und die
    /// Trainings-Warteschlange abarbeiten, solange Zeit und Kondition reichen. Wird von
    /// Spielstand.Advance in Schritten von höchstens einer Stunde aufgerufen.
    /// zusatzErholungProzent bündelt externe Boni (Hofstufe/Unterbringung, Decken-Ausrüstung), die
    /// dieses Pferd selbst nicht kennt - Spielstand.Advance rechnet sie vorher zusammen.</summary>
    public void Advance(DateTime von, DateTime bis, float zusatzErholungProzent = 0f)
    {
        foreach (var merkmal in AlleMerkmale())
            merkmal.PruefeAufdeckung(bis);

        float traitBonus = ModifikatorSumme(Merkmalsattribut.Erholung);
        float regenProStunde = MathF.Max(1f, BasisKonditionRegenProStunde * (1f + (traitBonus + zusatzErholungProzent) / 100f));

        BearbeiteWarteschlange(von, bis, regenProStunde);
    }

    // Volle Regeneration in ca. vier Stunden ohne weitere Boni (siehe Phase-4-Auftrag).
    private const float BasisKonditionRegenProStunde = 100f / 4f;

    private void BearbeiteWarteschlange(DateTime von, DateTime bis, float regenProStunde)
    {
        var zeitpunkt = von;

        while (true)
        {
            if (LaufendesTraining != null)
            {
                if (LaufendesTraining.Ende > bis) return;
                SchliesseTrainingAb(LaufendesTraining);
                zeitpunkt = LaufendesTraining.Ende;
                LaufendesTraining = null;
                continue;
            }

            if (zeitpunkt >= bis) return;

            if (Warteschlange.Count == 0)
            {
                RegeneriereKondition(zeitpunkt, bis, regenProStunde);
                return;
            }

            float kosten = IntensitaetsRegeln.Konditionskosten(Warteschlange[0].Intensitaet);
            if (Kondition < kosten)
            {
                // Nicht genug Kondition - abwarten, bis genug regeneriert ist oder das Fenster endet.
                double stundenBisBereit = (kosten - Kondition) / regenProStunde;
                var bereitAb = zeitpunkt.AddHours(stundenBisBereit);
                if (bereitAb >= bis)
                {
                    RegeneriereKondition(zeitpunkt, bis, regenProStunde);
                    return;
                }
                RegeneriereKondition(zeitpunkt, bereitAb, regenProStunde);
                zeitpunkt = bereitAb;
                continue;
            }

            var auftrag = Warteschlange[0];
            Warteschlange.RemoveAt(0);
            Kondition -= kosten;
            LaufendesTraining = new Training
            {
                Ziel = auftrag.Ziel,
                Intensitaet = auftrag.Intensitaet,
                Start = zeitpunkt,
                Ende = zeitpunkt + IntensitaetsRegeln.Dauer(auftrag.Intensitaet)
            };
            // Schleife läuft weiter - oben wird sofort geprüft, ob die Einheit noch in dieses
            // Zeitfenster passt oder erst beim nächsten Advance-Aufruf fertig wird.
        }
    }

    private void RegeneriereKondition(DateTime von, DateTime bis, float regenProStunde)
    {
        float stunden = (float)(bis - von).TotalHours;
        float zuwachs = stunden * regenProStunde;
        Kondition = MathF.Min(100f, Kondition + zuwachs);
        Stimmung = MathF.Min(100f, Stimmung + zuwachs * 0.5f);
    }

    private void SchliesseTrainingAb(Training training)
    {
        var stat = Werte.Hole(training.Ziel);
        float luecke = stat.Potenzial - stat.Aktuell;
        if (luecke > 0f)
            stat.Aktuell = MathF.Min(stat.Potenzial, stat.Aktuell + luecke * IntensitaetsRegeln.ErtragsAnteil(training.Intensitaet));
    }
}
