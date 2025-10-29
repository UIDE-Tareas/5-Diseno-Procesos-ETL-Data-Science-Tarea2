using Spectre.Console;
using Tarea2.Ejercicio2.ConsoleApp.Model;
using Tarea2.Ejercicio2.ConsoleApp.Services;

Console.OutputEncoding = System.Text.Encoding.UTF8;
AnsiConsole.MarkupLine("[yellow]Cargando productos desde FakeStore API...[/]");
var service = new FakeStoreService();
var products = await service.GetProductsAsync();
AnsiConsole.MarkupLine($"[green]Se cargaron {products.Count} productos correctamente.[/]\n");

while (true)
{
    var option = AnsiConsole.Prompt(
        new SelectionPrompt<string>()
            .Title("[bold cyan]Seleccione una opción:[/]")
            .PageSize(10)
            .AddChoices(new[]
            {
                "1 Ver todos los productos",
                "2 Productos más baratos",
                "3️ Productos más caros",
                "4 Promedio de precios", 
                "5️ Productos más vendidos",
                "6️ Productos mejor valorados",
                "🔍  Buscar producto por nombre",
                "❌ Salir"
            }));

    switch (option)
    {
        case var o when o.Contains("todos"):
            ShowTable(products, "Todos los productos");
            break;

        case var o when o.Contains("baratos"):
            ShowTable(products.OrderBy(p => p.Price).Take(10).ToList(), "Top 10 más baratos");
            break;

        case var o when o.Contains("caros"):
            ShowTable(products.OrderByDescending(p => p.Price).Take(10).ToList(), "Top 10 más caros");
            break;

        case var o when o.Contains("Promedio"):
        case var o2 when o2.Contains("promedio"):
            decimal avg = products.Average(p => p.Price);
            AnsiConsole.MarkupLine($"\n[bold yellow]El precio promedio es:[/] [green]${avg:F2}[/]\n");
            AnsiConsole.MarkupLine("\n[grey](Presione cualquier tecla para volver al menú...)[/]");
            Console.ReadKey(true);
            AnsiConsole.Clear();
            break;

        case var o when o.Contains("vendidos"):
            ShowTable(products.OrderByDescending(p => p.Rating.Count).Take(10).ToList(), "Top 10 más vendidos");
            break;

        case var o when o.Contains("valorados"):
            ShowTable(products.OrderByDescending(p => p.Rating.Rate).Take(10).ToList(), "Top 10 mejor valorados");
            break;

        case var o when o.Contains("Salir"):
            AnsiConsole.MarkupLine("[bold red]Saliendo...[/]");
            return;
        case var o when o.Contains("Buscar") || o.Contains("buscar"):
            SearchProduct(products);
            break;
    }
}

static void ShowTable(List<Product> products, string title)
{
    AnsiConsole.Clear();
    AnsiConsole.MarkupLine($"[bold underline yellow]{title}[/]\n");

    var table = new Table()
        .Border(TableBorder.Rounded)
        .AddColumn("[cyan]ID[/]")
        .AddColumn("[green]Título[/]")
        .AddColumn("[yellow]Precio[/]")
        .AddColumn("[magenta]Categoría[/]")
        .AddColumn("[blue]Rating[/]")
        .AddColumn("[red]Ventas[/]");

    foreach (var p in products)
    {
        table.AddRow(
            p.Id.ToString(),
            Truncate(p.Title, 40),
            $"${p.Price:F2}",
            p.Category,
            p.Rating.Rate.ToString("F1"),
            p.Rating.Count.ToString()
        );
    }

    AnsiConsole.Write(table);
    AnsiConsole.MarkupLine("\n[grey](Presione cualquier tecla para volver al menú...)[/]");
    Console.ReadKey(true);
    AnsiConsole.Clear();
}

static string Truncate(string value, int maxLength)
    => value.Length <= maxLength ? value : value.Substring(0, maxLength - 3) + "...";


static void SearchProduct(List<Product> products)
{
    AnsiConsole.Clear();
    var term = AnsiConsole.Ask<string>("[yellow]Ingrese parte del nombre del producto:[/]");

    var found = products
        .Where(p => p.Title.Contains(term, StringComparison.OrdinalIgnoreCase))
        .ToList();

    if (found.Count == 0)
    {
        AnsiConsole.MarkupLine($"[red]No se encontraron productos que coincidan con:[/] [bold]{term}[/]");
    }
    else
    {
        ShowTable(found, $"Resultados de búsqueda: '{term}' ({found.Count} encontrados)");
    }
}