namespace Core;

public partial class Spielstand
{
    private const int AnzahlMarktpferde = 5;

    /// <summary>Erneuert das Marktangebot, falls der wöchentliche Termin erreicht ist. Braucht die
    /// Inhaltsdatenbank (Rassen für neue Pferde) und läuft deshalb bewusst nicht in
    /// Advance(DateTime), sondern wird von der Oberfläche beim Öffnen des Marktes angestoßen.</summary>
    public void AktualisiereMarktFallsFaellig(Inhaltsdatenbank inhalte, DateTime jetzt)
    {
        if (jetzt < NaechsteMarktAktualisierung) return;

        var rassen = inhalte.AlleRassen().ToList();
        MarktPferde.Clear();
        for (int i = 0; i < AnzahlMarktpferde; i++)
            MarktPferde.Add(PferdeGenerator.Erzeuge(Zufall, inhalte, Zufall.Waehle(rassen), jetzt));

        NaechsteMarktAktualisierung = jetzt.AddDays(7);
    }

    public void KaufeMarktpferd(Pferd pferd)
    {
        if (!MarktPferde.Remove(pferd))
            throw new InvalidOperationException("Dieses Pferd steht nicht (mehr) im Markt.");

        int preis = Preisrechner.Kaufpreis(pferd);
        if (Guthaben < preis)
        {
            MarktPferde.Add(pferd);
            throw new InvalidOperationException("Nicht genug Guthaben.");
        }

        Guthaben -= preis;
        Pferde.Add(pferd);
    }

    /// <summary>Verkauft ein eigenes Pferd. Bringt Geld und - als Nebenprodukt - manchmal ein
    /// Material (siehe Projektanweisung: "Materialien ... als Nebenprodukt von Verkäufen").</summary>
    public MaterialTyp? VerkaufePferd(Pferd pferd)
    {
        if (pferd.IstBeschaeftigt)
            throw new InvalidOperationException("Dieses Pferd ist gerade anderweitig beschäftigt.");
        if (!Pferde.Remove(pferd))
            throw new InvalidOperationException("Dieses Pferd gehört nicht (mehr) zum eigenen Bestand.");

        Guthaben += Preisrechner.Verkaufspreis(pferd);

        foreach (var slot in pferd.Ausgeruestet.Values.ToList())
            foreach (var ausruestung in Ausruestungen)
                if (ausruestung.Id == slot)
                    ausruestung.AngelegtBeiPferdId = null;

        MaterialTyp? gewonnenesMaterial = null;
        if (Zufall.NaechsterBool(0.25))
        {
            gewonnenesMaterial = Zufall.Waehle(Enum.GetValues<MaterialTyp>());
            Materialbestand[gewonnenesMaterial.Value] = Materialbestand.GetValueOrDefault(gewonnenesMaterial.Value) + 1;
        }
        return gewonnenesMaterial;
    }

    public void KaufeAusruestung(AusruestungsDefinition definition)
    {
        if (Guthaben < definition.Preis)
            throw new InvalidOperationException("Nicht genug Guthaben.");

        Guthaben -= definition.Preis;
        Ausruestungen.Add(new Ausruestung
        {
            DefinitionId = definition.Id,
            Effekte = definition.Effekte.Select(e => new AusruestungsEffekt { Attribut = e.Attribut, Wert = e.Wert }).ToList()
        });
    }

    public void RuesteAus(Pferd pferd, Ausruestung ausruestung, AusruestungsSlot slot)
    {
        if (ausruestung.AngelegtBeiPferdId != null)
            throw new InvalidOperationException("Dieses Ausrüstungsstück ist bereits vergeben.");

        RuesteAb(pferd, slot);
        pferd.Ausgeruestet[slot] = ausruestung.Id;
        ausruestung.AngelegtBeiPferdId = pferd.Id;
    }

    public void RuesteAb(Pferd pferd, AusruestungsSlot slot)
    {
        if (!pferd.Ausgeruestet.TryGetValue(slot, out var bisherigeId)) return;

        pferd.Ausgeruestet.Remove(slot);
        var bisherige = Ausruestungen.FirstOrDefault(a => a.Id == bisherigeId);
        if (bisherige != null) bisherige.AngelegtBeiPferdId = null;
    }
}
