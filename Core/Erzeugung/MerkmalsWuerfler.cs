namespace Core;

/// <summary>Würfelt eine konkrete MerkmalsInstanz aus einer Definition. Gemeinsam genutzt von
/// PferdeGenerator (Marktpferde) und ZuchtRechner (Zuchtfohlen), damit beide exakt dieselbe Regel
/// anwenden, wie eine Stufe und ihr Wert innerhalb der Bandbreite gewürfelt werden.</summary>
public static class MerkmalsWuerfler
{
    /// <summary>Würfelt zusätzlich, welche Stufe (bis zu einem Deckel) getroffen wird - der übliche
    /// Fall bei einem komplett neuen Merkmal.</summary>
    public static MerkmalsInstanz Wuerfle(GameRandom zufall, MerkmalsDefinition definition, int maxStufe)
    {
        maxStufe = Math.Clamp(maxStufe, 1, definition.Stufen.Count);
        var moegliche = definition.Stufen.Where(s => s.Stufe <= maxStufe).ToList();
        int stufe = moegliche.Count > 0 ? zufall.Waehle(moegliche).Stufe : 1;
        return WuerfleAufStufe(zufall, definition, stufe);
    }

    /// <summary>Würfelt nur die Werte innerhalb einer bereits feststehenden Stufe - für die Zucht,
    /// wo die Stufe aus dem geerbten Elternmerkmal folgt statt neu ausgelost zu werden.</summary>
    public static MerkmalsInstanz WuerfleAufStufe(GameRandom zufall, MerkmalsDefinition definition, int stufe)
    {
        var gewaehlteStufe = definition.Stufen.FirstOrDefault(s => s.Stufe == stufe) ?? definition.Stufen[^1];
        var instanz = new MerkmalsInstanz { DefinitionId = definition.Id, Stufe = gewaehlteStufe.Stufe };
        foreach (var effekt in gewaehlteStufe.Effekte)
        {
            instanz.Effekte.Add(new GewuerfelterEffekt
            {
                Attribut = effekt.Attribut,
                Min = effekt.Min,
                Max = effekt.Max,
                Wert = zufall.NaechsterBereich(effekt.Min, effekt.Max)
            });
        }
        return instanz;
    }
}
