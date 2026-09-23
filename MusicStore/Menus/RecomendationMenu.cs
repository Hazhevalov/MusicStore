using Exam.Models;
using Exam.Services;
using Microsoft.IdentityModel.Tokens;
using System.Numerics;

namespace Exam.Menus;

public class RecomendationMenu
{
    private readonly PlateService _plateService;

    // Initializes the recommendations menu.
    public RecomendationMenu(PlateService plateService)
    {
        _plateService = plateService;
    }

    // Displays recommendation and popularity reports.
    public void Show()
    {
        while (true)
        {
            Console.Clear();

            Console.WriteLine("=== Recommendations ===");
            Console.WriteLine("1. New releases");
            Console.WriteLine("2. Popular records");
            Console.WriteLine("3. Popular artists");
            Console.WriteLine("4. Popular genres");
            Console.WriteLine("0. Back");

            Console.Write("\nChoose an action: ");

            string? choice = Console.ReadLine();

            if (choice == "0")
                return;

            Console.Clear();

            if (choice == "1")
            {
                ShowNewReleases();
                continue;
            }

            if (choice is not ("2" or "3" or "4"))
            {
                Console.WriteLine("Invalid selection.");
                Console.ReadKey();
                continue;
            }

            // Ask the user for the reporting period.
            int period = ReadPeriod();

            if (period == 0)
                continue;

            Console.Clear();

            // Load statistics for the selected report.
            var result = choice switch
            {
                "2" => _plateService.GetPopularPlates(period),
                "3" => _plateService.GetPopularArtists(period),
                "4" => _plateService.GetPopularGenres(period),

                _ => new List<(string Name, int Quantity)>()
            };

            // Print the resulting ranking.
            PrintStatistics(result);
        }
    }

    // Reads the reporting period selected by the user.
    private int ReadPeriod()
    {
        while (true)
        {
            Console.WriteLine("=== SELECT PERIOD ===");
            Console.WriteLine("1. Day");
            Console.WriteLine("2. Week");
            Console.WriteLine("3. Month");
            Console.WriteLine("4. Year");
            Console.WriteLine("0. Back");

            Console.Write("\nSelect a period: ");

            if (int.TryParse(Console.ReadLine(), out int period)
                && period >= 0 && period <= 4)
            {
                return period;
            }

            Console.WriteLine("Invalid period.\n");
        }
    }

    // Prints ranked sales statistics.
    private void PrintStatistics(
        List<(string Name, int Quantity)> statistics)
    {
        if (statistics.Count == 0)
        {
            Console.WriteLine("There were no sales during the selected period.");
        }
        else
        {
            int place = 1;
            Console.WriteLine(new string('-', 67));
            Console.WriteLine(
                $"| {"Rank",5} | {"Name",-40} | {"Units sold",12} |");
            Console.WriteLine(new string('-', 68));

            foreach (var item in statistics)
            {
                string name = item.Name.Length > 40
                    ? item.Name[..39] + "…"
                    : item.Name;

                Console.WriteLine(
                    $"| {place,5} | {name,-40} | {item.Quantity,12} |");

                place++;
            }

            Console.WriteLine(new string('-', 67));
        }

        Console.ReadKey();
    }

    // Displays the newest record releases.
    private void ShowNewReleases()
    {
        var plates = _plateService.GetNewReleases();

        Console.WriteLine("=== NEW RELEASES ===\n");

        if (plates.Count == 0)
        {
            Console.WriteLine("There are no new releases.");
        }
        else
        {
            Console.WriteLine(new string('-', 88));
            Console.WriteLine(
                $"| {"ID",3} | {"Title",-22} | {"Artist",-18} | " +
                $"{"Genre",-12} | {"Year",4} | {"Price",10} |");
            Console.WriteLine(new string('-', 88));

            foreach (var plate in plates)
            {
                string title = plate.Title.Length > 22
                    ? plate.Title[..21] + "…"
                    : plate.Title;
                string artist = plate.Artist.Name.Length > 18
                    ? plate.Artist.Name[..17] + "…"
                    : plate.Artist.Name;
                string genre = plate.Genre.Name.Length > 12
                    ? plate.Genre.Name[..11] + "…"
                    : plate.Genre.Name;

                Console.WriteLine(
                    $"| {plate.Id,3} | {title,-22} | {artist,-18} | " +
                    $"{genre,-12} | {plate.ReleaseYear,4} | " +
                    $"{plate.SalePrise,10:N2} |");
            }

            Console.WriteLine(new string('-', 88));
        }

        Console.ReadKey();
    }
}
