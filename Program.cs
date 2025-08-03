
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

     sb.AppendLine("Date");

     foreach (var f in d.GetFiles())
     {
         if (find.IsMatch(f.Name))
         {
             var content = File.ReadAllText(f.FullName);

             sb.AppendLine(f.Name);
         }
     }

     File.WriteAllText(csv.FullName,sb.ToString());


 }

enum Option
{
    FetchData,
    BuildCsv
}
