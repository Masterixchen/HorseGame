namespace Core;

/// <summary>Die Anmeldung zu einem konkreten, künftigen Termin einer Wettkampfklasse. Man meldet
/// vorher an und sieht das Ergebnis beim nächsten Reinschauen - Anwesenheit zum Zeitpunkt selbst
/// ist nicht nötig (siehe Projektanweisung). Die Klasse wird bei der Anmeldung eingefroren
/// (Regeln ändern sich ja ohnehin nicht zur Laufzeit), damit Spielstand.Advance(DateTime) beim
/// Auswerten ohne Zugriff auf die Inhaltsdatenbank auskommt - Werte, Kondition und Stimmung des
/// Pferdes bleiben dabei live, nur die Wettkampfregeln selbst sind eingefroren.</summary>
public class WettkampfAnmeldung
{
    public DateTime Zeitpunkt { get; set; }
    public Wettkampfklasse Klasse { get; set; } = null!;
}
