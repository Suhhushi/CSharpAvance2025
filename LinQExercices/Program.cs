using DataSources;
using System.Xml.Linq;

// Données
var allAlbums = ListAlbumsData.ListAlbums;
var allArtists = ListArtistsData.ListArtists;

// Ex 1 : Afficher tous les albums
Console.WriteLine("Tous les albums :");
allAlbums
    .Select((album, index) => $"Album N°{index + 1} : {album.Title}")
    .ToList()
    .ForEach(Console.WriteLine);

// Ex 2 : Filtrer les albums par titre
Console.Write("Entrez une chaîne pour filtrer les albums : ");
string content = Console.ReadLine() ?? "";
var filteredAlbums = allAlbums
    .Where(a => a.Title.Contains(content, StringComparison.OrdinalIgnoreCase))
    .ToList();

Console.WriteLine("Albums filtrés :");
filteredAlbums
    .Select((album, index) => $"Album N°{index + 1} : {album.Title}")
    .ToList()
    .ForEach(Console.WriteLine);

// Ex 3 : Trier ascendant / descendant
var ascendingAlbums = filteredAlbums
    .OrderBy(a => a.Title)
    .ThenBy(a => a.AlbumId)
    .ToList();

Console.WriteLine("Albums triés (ascendant) :");
ascendingAlbums.ForEach(a => Console.WriteLine($"Album N°{a.AlbumId} : {a.Title}"));

var descendingAlbums = filteredAlbums
    .OrderByDescending(a => a.Title)
    .ThenByDescending(a => a.AlbumId)
    .ToList();

Console.WriteLine("Albums triés (descendant) :");
descendingAlbums.ForEach(a => Console.WriteLine($"Album N°{a.AlbumId} : {a.Title}"));

// Ex 4 : Groupé par artiste (ID)
Console.Write("Filtrer et grouper par artiste ID : ");
content = Console.ReadLine() ?? "";

var groupedByArtistId = allAlbums
    .Where(a => a.Title.Contains(content, StringComparison.OrdinalIgnoreCase))
    .GroupBy(a => a.ArtistId)
    .OrderBy(g => g.Key);

foreach (var group in groupedByArtistId)
{
    Console.WriteLine($"Artiste ID {group.Key}:");
    foreach (var album in group)
        Console.WriteLine($"  - {album.Title}");
}

// Ex 5 : Groupé par nom d’artiste (join)
Console.Write("Filtrer et grouper par nom d’artiste : ");
content = Console.ReadLine() ?? "";

var groupedByArtistName = allAlbums
    .Where(a => a.Title.Contains(content, StringComparison.OrdinalIgnoreCase))
    .Join(allArtists, a => a.ArtistId, ar => ar.ArtistId, (a, ar) => new { a, ar.Name })
    .GroupBy(x => x.Name)
    .OrderBy(g => g.Key);

foreach (var group in groupedByArtistName)
{
    Console.WriteLine($"Artiste : {group.Key}");
    foreach (var album in group)
        Console.WriteLine($"  - Album N°{album.a.AlbumId} : {album.a.Title}");
}

// Ex 6 : Pagination simple
var albumsDisplay = allAlbums
    .OrderBy(a => a.AlbumId)
    .Select(a => $"   Album N°{a.AlbumId} : {a.Title}")
    .ToList();

int pageSize = 20;
int totalPages = (int)Math.Ceiling((double)albumsDisplay.Count / pageSize);

Console.WriteLine($"Total d'albums : {albumsDisplay.Count}, Pages totales : {totalPages}");
Console.Write("Entrez le numéro de page (1-based) : ");
if (int.TryParse(Console.ReadLine(), out int page) && page >= 1 && page <= totalPages)
{
    var pageAlbums = albumsDisplay.Skip((page - 1) * pageSize).Take(pageSize).ToList();
    Console.WriteLine($"Page {page} :");
    pageAlbums.ForEach(Console.WriteLine);
}
else
{
    Console.WriteLine("Numéro de page invalide.");
}

// Ex 7 : Lire un fichier texte et filtrer
Console.Write("Filtrer les lignes du fichier Albums.txt : ");
content = Console.ReadLine() ?? "";

string filePath = Path.Combine(Directory.GetCurrentDirectory(), "Text", "Albums.txt");
if (File.Exists(filePath))
{
    File.ReadAllLines(filePath)
        .Where(line => string.IsNullOrEmpty(content) || line.Contains(content, StringComparison.OrdinalIgnoreCase))
        .OrderBy(line => line)
        .ToList()
        .ForEach(Console.WriteLine);
}
else
{
    Console.WriteLine($"Fichier non trouvé : {filePath}");
}

// Ex XML : transformer les albums en XML
var xml = new XElement("Albums",
    allAlbums.Select(album =>
        new XElement("Album",
            new XElement("Id", album.AlbumId),
            new XElement("Titre", album.Title)
        )
    )
);

Console.WriteLine("Albums en XML :");
Console.WriteLine(xml);