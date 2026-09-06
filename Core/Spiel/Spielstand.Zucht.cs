namespace Core;

public partial class Spielstand
{
    /// <summary>Prüft alle Voraussetzungen, bucht die Materialien ab und setzt die Stute trächtig.
    /// Das Fohlen selbst wird sofort von ZuchtRechner gewürfelt - diese Methode kümmert sich nur um
    /// den Rahmen (Validierung, Materialbestand, Zuweisung).</summary>
    public void StarteZucht(Inhaltsdatenbank inhalte, Pferd mutter, Pferd vater, ZuchtEinsatz einsatz, DateTime jetzt)
    {
        if (mutter.Geschlecht != Geschlecht.Stute || vater.Geschlecht != Geschlecht.Hengst)
            throw new InvalidOperationException("Es braucht eine Stute und einen Hengst.");
        if (mutter.Id == vater.Id)
            throw new InvalidOperationException("Ein Pferd kann nicht mit sich selbst gezüchtet werden.");
        if (mutter.IstBeschaeftigt)
            throw new InvalidOperationException("Diese Stute ist gerade anderweitig beschäftigt.");
        if (!mutter.AlleMerkmaleBekannt || !vater.AlleMerkmaleBekannt)
            throw new InvalidOperationException("Beide Elterntiere müssen vollständig aufgedeckt sein.");
        if (!einsatz.IstGueltig())
            throw new InvalidOperationException("Wagnis lässt sich nicht mit Garantie-Materialien kombinieren.");

        int laufendeTraechtigkeiten = Pferde.Count(p => p.Traechtigkeit != null);
        int deckstationLimit = GebaeudeRegeln.MaxGleichzeitigeTraechtigkeiten(GebaeudeStufe(Gebaeude.Deckstation));
        if (laufendeTraechtigkeiten >= deckstationLimit)
            throw new InvalidOperationException($"Die Deckstation lässt nur {deckstationLimit} gleichzeitige Trächtigkeit(en) zu - erst abwarten oder ausbauen.");

        foreach (var material in einsatz.GenutzteMaterialien())
            if (Materialbestand.GetValueOrDefault(material) < 1)
                throw new InvalidOperationException($"Nicht genug {material} vorhanden.");

        // Eine gut ausgebaute Deckstation macht die Zucht günstiger: jedes verbrauchte Material
        // hat eine Chance, direkt zurückerstattet zu werden.
        float rueckerstattungsChance = GebaeudeRegeln.MaterialRueckerstattungsChance(GebaeudeStufe(Gebaeude.Deckstation));
        foreach (var material in einsatz.GenutzteMaterialien())
        {
            Materialbestand[material] -= 1;
            if (Zufall.NaechsterBool(rueckerstattungsChance))
                Materialbestand[material] += 1;
        }

        mutter.Traechtigkeit = ZuchtRechner.StarteZucht(Zufall, inhalte, mutter, vater, einsatz, Gebaeudestufen, jetzt);
    }
}
