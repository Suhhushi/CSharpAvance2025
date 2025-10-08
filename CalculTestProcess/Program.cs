using System.Diagnostics;

Console.WriteLine("Mesure de performance");

var sw = Stopwatch.StartNew();

// Mode séquentiel
// On effectue le calcul dans une boucle classique, de façon linéaire, avec une seule thread.
double sum = 1;
for (int i = 0; i < 50_000_000; i++)
{
    sum += Math.Sin(i) + Math.Cos(i);
    sum += Math.Sqrt(i);
    sum += Math.Exp(i % 10) + Math.Log(i);
    sum += Math.Pow(i % 100, 3);
    sum *= 1.0000001;
}
// Affichage du temps écoulé pour l'exécution séquentielle
sw.Stop();
Console.WriteLine($"[Séquentiel] Temps : {sw.ElapsedMilliseconds} ms");

// Mode parallèle
// On exécute la même opération, mais cette fois-ci de façon parallèle avec Parallel.For (plusieurs threads).
sw.Restart();
sum = 1;
Parallel.For(0, 50_000_000, (i, state) =>
{
    sum += Math.Sin(i) + Math.Cos(i);
    sum += Math.Sqrt(i);
    sum += Math.Exp(i % 10) + Math.Log(i);
    sum += Math.Pow(i % 100, 3);
    sum *= 1.0000001;
});
// Affichage du temps écoulé pour l'exécution parallèle
sw.Stop();
Console.WriteLine($"[Parallele] Temps : {sw.ElapsedMilliseconds} ms");

// Mode asynchrone
// Lancement du calcul en découpant le travail en tâches asynchrones,
// chaque bloc calcule une partie de la somme et on combine les résultats à la fin.
sw.Restart();
sum = 1;
await RunAsyncComputation();
sw.Stop();
Console.WriteLine($"[Asynchrone] Temps : {sw.ElapsedMilliseconds} ms");

// Fonction qui gère le découpage et l'exécution asynchrone du calcul
static async Task RunAsyncComputation()
{
    int chunkSize = 10_000_000;
    var tasks = new List<Task<double>>();

    for (int start = 0; start < 50_000_000; start += chunkSize)
    {
        int localStart = start;
        tasks.Add(Task.Run(() =>
        {
            double localSum = 1;
            for (int i = localStart; i < localStart + chunkSize && i < 50_000_000; i++)
            {
                localSum += Math.Sin(i) + Math.Cos(i);
                localSum += Math.Sqrt(i);
                localSum += Math.Exp(i % 10) + Math.Log(i);
                localSum += Math.Pow(i % 100, 3);
                localSum *= 1.0000001;
            }
            return localSum;
        }));
    }

    double total = 0;
    var results = await Task.WhenAll(tasks);
    foreach (var res in results)
        total += res;

    _ = total;
}
