using Core;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Web;
using Web.Speicher;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// Inhaltsdatenbank und Spielstand leben genau einmal pro Sitzung - Blazor WebAssembly läuft
// ohnehin nur für eine Person im Browser-Tab, ein Singleton entspricht also der Realität.
builder.Services.AddSingleton<Inhaltsdatenbank>();
builder.Services.AddSingleton<SpielSitzung>();

await builder.Build().RunAsync();
