using ConvertApp;
using DataSources;
using System.Text;
using System.Text.Json;


// Récupération des données sources (albums, artistes)
var allAlbums = ListAlbumsData.ListAlbums;
var allArtists = ListArtistsData.ListArtists;

// Bloc d'entrée pour saisir le chemin du fichier JSON
Console.WriteLine("Entrez le chemin complet du fichier JSON :");
Console.WriteLine("(Vous pouvez aussi glisser-déposer le fichier sur l'exe)\n");
string? jsonPath = Console.ReadLine()?.Trim();
if (string.IsNullOrWhiteSpace(jsonPath))
{
    Console.WriteLine("Aucun chemin saisi.");
    return;
}
if (!File.Exists(jsonPath))
{
    Console.WriteLine($"Erreur : Le fichier '{jsonPath}' n'existe pas.");
    return;
}

// Lecture et parsing du fichier JSON pour initialiser la liste principale
string jsonData = File.ReadAllText(jsonPath);
var jsonObjectArray = ParseJsonToObjects(jsonData);
var currentList = jsonObjectArray;

// Affichage du contenu initial du fichier JSON
DisplayList("Contenu du fichier", jsonObjectArray);

// Boucle principale de menu interactif
string? userChoice;
do
{
    Console.WriteLine("\n--------- Menu Principal ---------");
    Console.WriteLine("1: Trier");
    Console.WriteLine("2: Rechercher");
    Console.WriteLine("3: Exporter en CSV");
    Console.WriteLine("4: Réinitialiser la liste");
    Console.WriteLine("q: Quitter");
    Console.WriteLine("Entrez votre choix :");
    userChoice = Console.ReadLine()?.Trim().ToLower();

    switch (userChoice)
    {
        case "1":
            currentList = HandleSort(currentList);      // Tri de la liste
            break;
        case "2":
            currentList = HandleSearch(currentList);    // Recherche par nom
            break;
        case "3":
            ExportToCsv(currentList);                   // Export CSV
            break;
        case "4":
            currentList = ResetToOriginal(jsonPath);    // Réinitialisation
            break;
        case "q":
            Console.WriteLine("Fermeture du programme...");
            break;
        default:
            Console.WriteLine("Choix invalide. Veuillez réessayer.");
            break;
    }
} while (userChoice != "q");

// --- Fonctions utilitaires ---

// Parsing du JSON : transforme chaque entrée JSON en objet .NET spécifique
static List<DynamicJsonObject> ParseJsonToObjects(string jsonData)
{
    var result = new List<DynamicJsonObject>();
    using JsonDocument document = JsonDocument.Parse(jsonData);

    foreach (var property in document.RootElement.EnumerateObject())
    {
        if (property.Value.ValueKind != JsonValueKind.Array) continue;

        foreach (var item in property.Value.EnumerateArray())
        {
            string? id = null, name = null, extra = null;
            int count = 0;
            foreach (var prop in item.EnumerateObject())
            {
                count++;
                if (count == 1) id = prop.Value.ToString();
                else if (count == 2) name = prop.Value.ToString();
                else extra = prop.Value.ToString();
            }
            result.Add(new DynamicJsonObject(id ?? "", name ?? "", extra ?? ""));
        }
    }
    return result;
}

// Gestion du tri : propose à l'utilisateur de trier la liste par ID, nom ou extra
static List<DynamicJsonObject> HandleSort(List<DynamicJsonObject> data)
{
    Console.WriteLine("\nTrier par ?\n1: Id\n2: Nom\n3: Extra\nq: Annuler");
    string? sortBy = Console.ReadLine()?.Trim().ToLower();
    if (sortBy == "q") return data;

    Console.WriteLine("Ordre ?\n1: Croissant\n2: Décroissant\nq: Annuler");
    string? order = Console.ReadLine()?.Trim().ToLower();
    if (order == "q") return data;

    try
    {
        IEnumerable<DynamicJsonObject> sorted = sortBy switch
        {
            "1" when order == "1" => data.OrderBy(x => int.Parse(x.PropertyId)),
            "1" when order == "2" => data.OrderByDescending(x => int.Parse(x.PropertyId)),
            "2" when order == "1" => data.OrderBy(x => x.PropertyName),
            "2" when order == "2" => data.OrderByDescending(x => x.PropertyName),
            "3" when order == "1" => data.OrderBy(x => x.PropertyExtra),
            "3" when order == "2" => data.OrderByDescending(x => x.PropertyExtra),
            _ => data
        };
        var result = sorted.ToList();
        DisplayList("Résultat du tri", result);
        return result;
    }
    catch (FormatException)
    {
        Console.WriteLine("Erreur : L'ID doit être un nombre valide.");
        return data;
    }
}

// Gestion de la recherche : filtre sur nom ou extra
static List<DynamicJsonObject> HandleSearch(List<DynamicJsonObject> data)
{
    Console.WriteLine("\nEntrez une chaîne à rechercher (Nom ou Extra) ou 'q' pour quitter :");
    string? search = Console.ReadLine()?.Trim();
    if (search == "q" || string.IsNullOrEmpty(search)) return data;

    var filtered = data
        .Where(x => x.PropertyName.Contains(search, StringComparison.OrdinalIgnoreCase)
                 || x.PropertyExtra.Contains(search, StringComparison.OrdinalIgnoreCase))
        .ToList();

    DisplayList($"Résultats pour '{search}'", filtered.Any() ? filtered : new List<DynamicJsonObject>());
    return filtered;
}

// Réinitialisation de la liste : recharge le fichier JSON d'origine
static List<DynamicJsonObject> ResetToOriginal(string jsonPath)
{
    try
    {
        string jsonData = File.ReadAllText(jsonPath);
        var originalList = ParseJsonToObjects(jsonData);
        DisplayList("Liste réinitialisée", originalList);
        return originalList;
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Erreur lors de la réinitialisation : {ex.Message}");
        return new List<DynamicJsonObject>();
    }
}

// Export CSV incluant les 3 champs
// Export CSV avec choix des champs
static void ExportToCsv(List<DynamicJsonObject> data)
{
    Console.WriteLine("\nChoisissez les champs à exporter (séparés par des virgules) :");
    Console.WriteLine("1: Id");
    Console.WriteLine("2: Name");
    Console.WriteLine("3: Extra");
    Console.WriteLine("Exemple : 1,3 pour exporter Id et Extra uniquement");
    string? selectedFieldsInput = Console.ReadLine()?.Trim();

    if (string.IsNullOrWhiteSpace(selectedFieldsInput))
    {
        Console.WriteLine("Aucun champ sélectionné, opération annulée.");
        return;
    }

    var selectedIndexes = selectedFieldsInput
        .Split(',', StringSplitOptions.RemoveEmptyEntries)
        .Select(s => s.Trim())
        .ToHashSet();

    var headers = new List<string>();
    if (selectedIndexes.Contains("1")) headers.Add("Id");
    if (selectedIndexes.Contains("2")) headers.Add("Name");
    if (selectedIndexes.Contains("3")) headers.Add("Extra");

    if (!headers.Any())
    {
        Console.WriteLine("Aucun champ valide sélectionné, opération annulée.");
        return;
    }

    Console.WriteLine("\nEntrez le chemin complet pour le fichier CSV (si vide, dossier 'Export' du projet sera utilisé) :");
    string? csvPath = Console.ReadLine()?.Trim();

    if (string.IsNullOrWhiteSpace(csvPath))
    {
        string projectRoot = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"..\..\..\"));
        csvPath = Path.Combine(projectRoot, "Export");
        csvPath = Path.Combine(csvPath, $"export_{DateTime.Now:yyyyMMdd_HHmmss}.csv");
    }
    else
    {
        string dir = Path.GetDirectoryName(csvPath)!;
        if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
    }

    try
    {
        var csvLines = new List<string> { string.Join(";", headers) };

        foreach (var item in data)
        {
            var values = new List<string>();
            if (selectedIndexes.Contains("1")) values.Add($"\"{item.PropertyId.Replace("\"", "\"\"")}\"");
            if (selectedIndexes.Contains("2")) values.Add($"\"{item.PropertyName.Replace("\"", "\"\"")}\"");
            if (selectedIndexes.Contains("3")) values.Add($"\"{item.PropertyExtra.Replace("\"", "\"\"")}\"");

            csvLines.Add(string.Join(";", values));
        }

        File.WriteAllText(csvPath, string.Join("\r\n", csvLines), Encoding.UTF8);
        Console.WriteLine($"Export réussi vers '{csvPath}' !");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Erreur lors de l'export : {ex.Message}");
    }
}

// Affichage de la liste
static void DisplayList(string title, IEnumerable<DynamicJsonObject> list)
{
    Console.WriteLine($"\n--- {title} ---");
    if (!list.Any())
    {
        Console.WriteLine("Aucune donnée à afficher.");
        return;
    }

    int index = 1;
    foreach (var obj in list)
    {
        Console.WriteLine($"{obj}");
    }
    Console.WriteLine($"Total: {list.Count()} éléments");
}
