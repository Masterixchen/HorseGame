using Core;
using Microsoft.JSInterop;

namespace Web.Speicher;

/// <summary>Hält den Spielstand während der Sitzung im Speicher und kümmert sich um Laden/Speichern
/// im Browser-Speicher (localStorage). Reine Browser-Anbindung - die eigentliche JSON-Umwandlung
/// übernimmt Core.SpielstandService, damit Core selbst nichts von localStorage wissen muss.</summary>
public class SpielSitzung
{
    private const string SchluesselName = "pferdeguet-spielstand";

    private readonly IJSRuntime _js;
    private readonly Inhaltsdatenbank _inhalte;

    public Spielstand Zustand { get; private set; } = new();
    public bool Geladen { get; private set; }

    public SpielSitzung(IJSRuntime js, Inhaltsdatenbank inhalte)
    {
        _js = js;
        _inhalte = inhalte;
    }

    public async Task LadenOderErzeugenAsync()
    {
        var json = await _js.InvokeAsync<string?>("localStorage.getItem", SchluesselName);

        Zustand = string.IsNullOrWhiteSpace(json)
            ? ErzeugeStartbestand()
            : SpielstandService.Laden(json);

        // Zeit seit dem letzten Besuch nachrechnen - auch direkt nach dem Neuanlegen, das ist billig.
        Zustand.Advance(DateTime.UtcNow);
        Geladen = true;
        await SpeichernAsync();
    }

    public async Task SpeichernAsync()
    {
        var json = SpielstandService.Speichern(Zustand);
        await _js.InvokeVoidAsync("localStorage.setItem", SchluesselName, json);
    }

    private Spielstand ErzeugeStartbestand()
    {
        var zufall = new GameRandom((ulong)DateTime.UtcNow.Ticks);
        var stand = new Spielstand { ZuletztAktualisiert = DateTime.UtcNow, Zufall = zufall };
        var rassen = _inhalte.AlleRassen().ToList();

        // Sechs Startpferde, quer über die Rassen gestreut - Ausgangspunkt für Phase 1.
        for (int i = 0; i < 6; i++)
        {
            var rasse = zufall.Waehle(rassen);
            stand.Pferde.Add(PferdeGenerator.Erzeuge(zufall, _inhalte, rasse, DateTime.UtcNow));
        }

        // Solange es noch keine Wirtschaft gibt (Phase 3: Wettkämpfe, Hof-Erzeugung, Verkäufe),
        // startet jedes Spiel mit einer kleinen Grundausstattung, damit sich die Zucht ausprobieren lässt.
        stand.Materialbestand[MaterialTyp.Kraftfutter] = 3;
        stand.Materialbestand[MaterialTyp.Ahnentafel] = 2;
        stand.Materialbestand[MaterialTyp.Fremdblut] = 2;
        stand.Materialbestand[MaterialTyp.Spezialistenbetreuung] = 2;
        stand.Materialbestand[MaterialTyp.SelteneLinie] = 1;
        stand.Materialbestand[MaterialTyp.Wagnis] = 2;

        return stand;
    }
}
