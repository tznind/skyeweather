
using System.Text;
using System.Text.RegularExpressions;
using Spectre.Console;



var p = new SelectionPrompt<Option>();
p.AddChoice(Option.BuildCsv);
p.AddChoice(Option.FetchData);

var chosen = p.Show(AnsiConsole.Console);


if (chosen == Option.FetchData)
{
    FetchData().Wait();
}
else
{
    BuildCsv();
}


 static async Task FetchData()
{

    // Not available 
    // https://www.skyeweather.co.uk/data/historical/daily-30-03-2025.html

    var failed = new FileInfo("failed");
    if (!failed.Exists)
    {
        failed.Create();
    }

    // var dt = DateTime.Now.Date;
    var dt = new DateTime(2024, 03, 31);

    // For past 1 day
    while (true)
    {
        dt = dt.AddDays(-1);

        var day = dt.ToString("dd-MM-yyyy");
        var fetch = "https://www.skyeweather.co.uk/data/historical/daily-" + day + ".html";

        var htc = new HttpClient();

        Console.WriteLine("Fetching:" + fetch);

        try
        {
            var result = await htc.GetStringAsync(fetch);
            File.WriteAllText(day, result);
        }
        catch (HttpRequestException ex)
        {
            Console.WriteLine("Failed:" + day + "(" + ex.StatusCode + ")");


            File.AppendAllText(failed.FullName, day + Environment.NewLine);
        }
    }
}

 static void BuildCsv()
 {
     var csv = new FileInfo("out.csv");
     if (!csv.Exists)
     {
        csv.Create();
     }

     var sb = new StringBuilder();

     var d = new DirectoryInfo(Environment.CurrentDirectory);
     var find = new Regex(@"^\d\d-\d\d-\d\d\d\d$");




    Interesting[] interesting = new[]
    {
    new Interesting
    {
        Pattern = new Regex(@"(?<=<html[^>]*>\s*)\s*([^<]+?), ([^<]+?),"),
        GroupCount = 2,
        Titles = new[]
        {
            "Location name",
            "Region name"
        }
    },
    new Interesting
    {
        Pattern = new Regex(@"Lat:\s*([NS])(\d{1,2}):(\d{1,2}):(\d{1,2})\s*\(([-+]?\d+\.\d+)\)"),
        GroupCount = 5,
        Titles = new[]
        {
            "Latitude hemisphere",
            "Latitude degrees",
            "Latitude minutes",
            "Latitude seconds",
            "Latitude decimal degrees"
        }
    },
    new Interesting
    {
        Pattern = new Regex(@"Lon:\s*([EW])(\d{1,2}):(\d{1,2}):(\d{1,2})\s*\(([-+]?\d+\.\d+)\)"),
        GroupCount = 5,
        Titles = new[]
        {
            "Longitude hemisphere",
            "Longitude degrees",
            "Longitude minutes",
            "Longitude seconds",
            "Longitude decimal degrees"
        }
    },
    new Interesting
    {
        Pattern = new Regex(@"Maximum Pressure:\s*([\d.]+) hPa @ ([\d/]+ \d{2}:\d{2})"),
        GroupCount = 2,
        Titles = new[]
        {
            "Maximum pressure hPa",
            "Maximum pressure time"
        }
    },
    new Interesting
    {
        Pattern = new Regex(@"Minimum Pressure:\s*([\d.]+) hPa @ ([\d/]+ \d{2}:\d{2})"),
        GroupCount = 2,
        Titles = new[]
        {
            "Minimum pressure hPa",
            "Minimum pressure time"
        }
    },
    new Interesting
    {
        Pattern = new Regex(@"Average Pressure:\s*([\d.]+) hPa"),
        GroupCount = 1,
        Titles = new[]
        {
            "Average pressure hPa"
        }
    },
    new Interesting
    {
        Pattern = new Regex(@"Average Windspeed:\s*([\d.]+) m/s \(([\d.]+) mph\)"),
        GroupCount = 2,
        Titles = new[]
        {
            "Average windspeed mps",
            "Average windspeed mph"
        }
    },
    new Interesting
    {
        Pattern = new Regex(@"Maximum Gust:\s*([\d.]+) m/s \(([\d.]+) mph\) @ ([\d/]+ \d{2}:\d{2})"),
        GroupCount = 3,
        Titles = new[]
        {
            "Maximum gust mps",
            "Maximum gust mph",
            "Maximum gust time"
        }
    },
    new Interesting
    {
        Pattern = new Regex(@"Average Wind Direction:\s*([A-Z]+)\s*-\s*(\d{1,3})"),
        GroupCount = 2,
        Titles = new[]
        {
            "Average wind direction compass",
            "Average wind direction degrees"
        }
    },
    new Interesting
    {
        Pattern = new Regex(@"Maximum Temperature:\s*([\d.]+) .*? @ ([\d/]+ \d{2}:\d{2})"),
        GroupCount = 2,
        Titles = new[]
        {
            "Maximum temperature Celsius",
            "Maximum temperature time"
        }
    },
    new Interesting
    {
        Pattern = new Regex(@"Minimum Temperature:\s*([\d.]+) .*? @ ([\d/]+ \d{2}:\d{2})"),
        GroupCount = 2,
        Titles = new[]
        {
            "Minimum temperature Celsius",
            "Minimum temperature time"
        }
    },
    new Interesting
    {
        Pattern = new Regex(@"Average Temperature:\s*([\d.]+)"),
        GroupCount = 1,
        Titles = new[]
        {
            "Average temperature Celsius"
        }
    },
    new Interesting
    {
        Pattern = new Regex(@"Maximum Humidity:\s*([\d.]+) % @ ([\d/]+ \d{2}:\d{2})"),
        GroupCount = 2,
        Titles = new[]
        {
            "Maximum humidity percent",
            "Maximum humidity time"
        }
    },
    new Interesting
    {
        Pattern = new Regex(@"Minimum Humidity:\s*([\d.]+) % @ ([\d/]+ \d{2}:\d{2})"),
        GroupCount = 2,
        Titles = new[]
        {
            "Minimum humidity percent",
            "Minimum humidity time"
        }
    },
    new Interesting
    {
        Pattern = new Regex(@"Average Humidity:\s*([\d.]+) %"),
        GroupCount = 1,
        Titles = new[]
        {
            "Average humidity percent"
        }
    },
    new Interesting
    {
        Pattern = new Regex(@"Rainfall:\s*([\d.]+) mm"),
        GroupCount = 1,
        Titles = new[]
        {
            "Daily rainfall millimetres"
        }
    },
    new Interesting
    {
        Pattern = new Regex(@"Month Rain:\s*([\d.]+) mm"),
        GroupCount = 1,
        Titles = new[]
        {
            "Monthly rainfall millimetres"
        }
    }
};


    sb.Append("Date,");

    foreach (var i in interesting)
    {
        foreach (var t in i.Titles)
        {

            sb.Append(t + ",");
        }
    }

    sb.AppendLine();
    

    foreach (var f in d.GetFiles())
    {
         if (find.IsMatch(f.Name))
         {
             var content = File.ReadAllText(f.FullName);


            sb.Append(f.Name +",");
            foreach (Interesting interest in interesting)
            {
                var m = interest.Pattern.Match(content);

                for (int i = 0; i < interest.Titles.Length; i++)
                {
                    sb.Append(m.Groups[i + 1] + ",");
                }
            }

            sb.AppendLine();
         }
    }

    File.WriteAllText(csv.FullName,sb.ToString());
 }

enum Option
{
    FetchData,
    BuildCsv
}
class Interesting
{
    public Regex Pattern;
    public int GroupCount;

    public string[] Titles;
}