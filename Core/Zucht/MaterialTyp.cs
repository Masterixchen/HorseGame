namespace Core;

/// <summary>Zuchtmaterialien verschieben die Wahrscheinlichkeiten bei der Zucht. Sie werden nie mit
/// Geld gekauft (Designgrundsatz 4), sondern kommen aus Wettkämpfen, Hof-Erzeugung und Verkäufen -
/// diese Quellen entstehen erst mit der Wirtschaft in Phase 3.</summary>
public enum MaterialTyp
{
    Kraftfutter,
    Ahnentafel,
    Fremdblut,
    Spezialistenbetreuung,
    SelteneLinie,
    Wagnis
}
