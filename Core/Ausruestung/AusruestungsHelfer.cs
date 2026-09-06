namespace Core;

/// <summary>Summiert die Ausrüstungsboni eines Pferdes für ein Attribut. Eigenständig statt Teil
/// von Pferd, weil dafür der komplette Ausrüstungsbestand (Spielstand) gebraucht wird - ein
/// einzelnes Pferd kennt nur, welche Ids in welchem Slot stecken. Braucht bewusst keine
/// Inhaltsdatenbank: Ausruestung trägt ihre Effekte seit dem Kauf selbst (siehe Ausruestung.cs),
/// damit auch Spielstand.Advance(DateTime) ohne Content-Zugriff auskommt.</summary>
public static class AusruestungsHelfer
{
    public static float ModifikatorSumme(Pferd pferd, IEnumerable<Ausruestung> bestand, Merkmalsattribut attribut)
    {
        float summe = 0f;
        foreach (var id in pferd.Ausgeruestet.Values)
        {
            var ausruestung = bestand.FirstOrDefault(a => a.Id == id);
            if (ausruestung == null) continue;

            foreach (var effekt in ausruestung.Effekte)
                if (effekt.Attribut == attribut)
                    summe += effekt.Wert;
        }
        return summe;
    }
}
