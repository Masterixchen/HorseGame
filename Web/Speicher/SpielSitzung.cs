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

    // Damit das Layout Guthaben/Ansehen live zeigt, auch wenn die Änderung auf einer Unterseite
    // passiert ist - ohne dieses Ereignis müsste man manuell neu laden, um es zu sehen.
    public event Action? ZustandGeaendert;

    public SpielSitzung(IJSRuntime js, Inhaltsdatenbank inhalte)
    {
        _js = js;
        _inhalte = inhalte;
    }

    public async Task LadenOderErzeugenAsync()
    {
        var json = await _js.InvokeAsync<string?>("localStorage.getItem", SchluesselName);

        Zustand = string.IsNullOrWhiteSpace(json)
            ? SpielstandFabrik.NeuesSpiel(_inhalte, DateTime.UtcNow)
            : SpielstandService.Laden(json);

        // Zeit seit dem letzten Besuch nachrechnen - auch direkt nach dem Neuanlegen, das ist billig.
        Zustand.Advance(DateTime.UtcNow);
        Zustand.AktualisiereMarktFallsFaellig(_inhalte, DateTime.UtcNow);
        Geladen = true;
        await SpeichernAsync();
    }

    public async Task SpeichernAsync()
    {
        var json = SpielstandService.Speichern(Zustand);
        await _js.InvokeVoidAsync("localStorage.setItem", SchluesselName, json);
        ZustandGeaendert?.Invoke();
    }
}
