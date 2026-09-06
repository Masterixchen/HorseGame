using Core;
using Sim;

var inhalte = new Inhaltsdatenbank();
var zufall = new GameRandom(1);

Verteilungsbericht.Drucken(zufall, inhalte);

Console.WriteLine();
Console.WriteLine(new string('-', 60));
Console.WriteLine();

Zuchtbericht.Drucken(zufall, inhalte);

Console.WriteLine();
Console.WriteLine(new string('-', 60));
Console.WriteLine();

Wirtschaftsbericht.Drucken(inhalte);

Console.WriteLine();
Console.WriteLine(new string('-', 60));
Console.WriteLine();

Bilddateien.Drucken(inhalte);
