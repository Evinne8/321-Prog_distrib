using Transat;

const int iterations = 500;

FaillibleQos0Storage storage1 = new();
FaillibleQos0Storage storage2 = new();

var publishers = new List<NonResilientPublisher>
{
    new(storage1, storage2),
    new(storage1, storage2),
    new(storage1, storage2)
};

// Liste des tâches
var tasks = new List<Task>();

for (var i = 0; i < iterations; i++)
{
    foreach (var publisher in publishers)
    {
        tasks.Add(Task.Run(() => publisher.Send(1)));
    }
}

// Attendre toutes les tâches
Task.WaitAll(tasks.ToArray());

// Compute and show results
int sum1 = storage1.Values.Sum();
int sum2 = storage2.Values.Sum();

Console.WriteLine($"\nTotal expected : {iterations * publishers.Count}");
Console.WriteLine($"Sum storage1: {sum1}");
Console.WriteLine($"Sum storage2: {sum2}");
