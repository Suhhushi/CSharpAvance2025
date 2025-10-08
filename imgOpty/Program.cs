using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using System.Diagnostics;
using System.Text;

// Définition des résolutions cibles pour le redimensionnement
int[] TargetHeights = { 1080, 720, 480 };

// Construction des chemins nécessaires pour le projet et les dossiers d'entrée/sortie
string ProjectRoot = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"..\..\..\")); // Racine projet
string SourceDirectory = Path.Combine(ProjectRoot, "Images"); // Dossier images source
string SequentialOutputDirectory = Path.Combine(ProjectRoot, "ImageSequential"); // Dossier sortie séquentielle
string ParallelOutputDirectory = Path.Combine(ProjectRoot, "ImageParallel"); // Dossier sortie parallèle

// Création des dossiers si besoin (pour éviter les erreurs plus loin)
Directory.CreateDirectory(SourceDirectory);
Directory.CreateDirectory(SequentialOutputDirectory);
Directory.CreateDirectory(ParallelOutputDirectory);

// Bloc d'optimisation séquentielle
// On traite les images une par une, linéairement
var sequentialDuration = OptimizeSequentially();
Console.WriteLine("Optimisation séquentielle terminée.");

// Bloc d'optimisation parallèle
// On traite toutes les images en parallèle (simultanément) avec Parallel.ForEach
var parallelDuration = OptimizeInParallel();
Console.WriteLine("Optimisation parallèle terminée.");

// Bloc de génération du rapport README
// On écrit un résumé des temps d'exécution dans un README.md
GenerateReadme(sequentialDuration, parallelDuration);
Console.WriteLine("Optimisation terminée avec succès !");


// Fonction d'optimisation séquentielle : parcourt chaque fichier image et optimise en mode linéaire
long OptimizeSequentially()
{
    var stopwatch = Stopwatch.StartNew();

    foreach (var filePath in Directory.GetFiles(SourceDirectory))
    {
        if (!IsSupportedImage(filePath)) // Vérifie le format
            continue;

        try
        {
            ProcessAndSaveImage(filePath, SequentialOutputDirectory);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erreur (séquentiel) sur {filePath} : {ex.Message}");
        }
    }

    stopwatch.Stop();
    Console.WriteLine($"[Séquentiel] Durée : {stopwatch.ElapsedMilliseconds} ms");
    return stopwatch.ElapsedMilliseconds;
}

// Fonction d'optimisation parallèle : exploite la parallélisation pour accélérer le workflow
long OptimizeInParallel()
{
    var stopwatch = Stopwatch.StartNew();

    var imageFiles = Directory.GetFiles(SourceDirectory);
    Parallel.ForEach(imageFiles, filePath =>
    {
        if (!IsSupportedImage(filePath)) // Vérifie le format
            return;

        try
        {
            ProcessAndSaveImage(filePath, ParallelOutputDirectory);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erreur (parallèle) sur {filePath} : {ex.Message}");
        }
    });

    stopwatch.Stop();
    Console.WriteLine($"[Parallélisé] Durée : {stopwatch.ElapsedMilliseconds} ms");
    return stopwatch.ElapsedMilliseconds;
}

// Fonction utilitaire (filtre) : vérifie si le format de l'image est pris en charge
bool IsSupportedImage(string filePath)
{
    var extension = Path.GetExtension(filePath).ToLowerInvariant();
    return extension is ".jpg" or ".jpeg" or ".png";
}

// Fonction principale de traitement : redimensionne chaque image selon les tailles cibles et sauvegarde les fichiers
void ProcessAndSaveImage(string filePath, string outputDirectory)
{
    using var image = Image.Load(filePath);

    foreach (var targetHeight in TargetHeights)
    {
        int targetWidth = image.Width * targetHeight / image.Height; // Ratio conservé
        using var resizedImage = image.Clone(ctx => ctx.Resize(targetWidth, targetHeight));

        var fileName = Path.GetFileNameWithoutExtension(filePath);
        var extension = Path.GetExtension(filePath);
        var outputPath = Path.Combine(outputDirectory, $"{fileName}_{targetHeight}p{extension}");

        resizedImage.Save(outputPath);
    }
}

// Fonction de génération du README : crée un résumé des performances dans le dossier racine
void GenerateReadme(long sequentialTimeMs, long parallelTimeMs)
{
    var content = new StringBuilder()
        .AppendLine("# Résultats :")
        .AppendLine()
        .AppendLine($"- Version séquentielle : {sequentialTimeMs} ms")
        .AppendLine($"- Version parallélisée : {parallelTimeMs} ms")
        .ToString();

    var readmePath = Path.Combine(ProjectRoot, "README.md");
    File.WriteAllText(readmePath, content);
    Console.WriteLine($"Fichier README.md généré : {readmePath}");
}
