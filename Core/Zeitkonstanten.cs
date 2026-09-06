namespace Core;

/// <summary>Zentrale Zeit-Stellschrauben, die mehrere Systeme betreffen. Einzeln liegende
/// Zeitangaben (Trainingsdauer, Trächtigkeit, ...) stehen dagegen direkt bei der Klasse, die sie
/// verwendet - hier nur, was wirklich geteilt gebraucht wird.</summary>
public static class Zeitkonstanten
{
    // Ein Pferd soll rund vier bis sechs Wochen Echtzeit lang konkurrenzfähig sein (siehe
    // Altersfaktor in WettkampfRechner), danach bleibt es als Elterntier wertvoll. Bei diesem
    // Faktor erreicht ein Pferd nach ca. 5 Wochen Echtzeit ein Spielalter von ~18 Jahren, wo die
    // Wettkampfform spürbar nachlässt. Nur die Alters*anzeige* wird beschleunigt, keine anderen
    // Zeitabläufe - Training, Trächtigkeit usw. haben ihre eigenen, direkt angegebenen Dauern.
    public const float AlterungsFaktor = 180f;
}
