Console.Title = "IP File Normalizer - v2.0.0";
Console.OutputEncoding = Encoding.UTF8;

string resultPath = Path.Combine(AppContext.BaseDirectory, "Results");
await DirHandler.CreateDirectories(resultPath);

while (true)
{
    Console.Clear();
    using (var repo = new LineRepository())
    {
        AnsiConsole.Write(new FigletText("IP File Normalizer").LayoutMode(FigletLayoutMode.Fitted).Color(Color.Gold1));
        AnsiConsole.Write(new Panel(new Markup("🦩 Version: 2.0.0      ☣️ Developer: Abel      🌎 Github: [SkyBlue3 link]https://google.com[/]"))
                .BorderColor(Color.Yellow)
                .RoundedBorder()
                .Header(text: "🛠️ [bold]App Info[/]", Justify.Left));
        AnsiConsole.WriteLine();

        Console.CursorVisible = true;
        Console.Write("😩 Enter txt SourceFile Path(s) (split by comma or space) [enter * or leave empty to exit]:");
        var currentPosition = Console.GetCursorPosition();
        string? inputPaths = Console.ReadLine();
        await UIHandler.CleanConsoleManuallyAsync(currentPosition);
        Console.WriteLine();
        Console.CursorVisible = false;

        var srcPaths = await Utility.ParseInputPathsAsync(inputPaths);
        if (!srcPaths.result)
            return;

        var validPaths = await DirHandler.ValidateFilesHandlerAsync(srcPaths.list!);
        if (validPaths.key == ConsoleKey.Multiply)
            return;
        else if (validPaths.key != null && validPaths.key != ConsoleKey.Multiply)
            continue;

        uint totalLines = await UIHandler.ComputeTotalLinesWithSpinnerAsync(validPaths.list!);
        await UIHandler.LoadFilesWithProgressAsync(repo, validPaths.list!, totalLines);

        await UIHandler.AnalyzeWithSpinnerAsync(repo);
        if (repo.TotalIPs == 0)
        {
            Console.CursorVisible = false;
            await UIHandler.CleanConsoleManuallyAsync(currentPosition);

            AnsiConsole.MarkupLine("\r\n😐 [red bold]Error:[/] Not Any IP Was Detected!");
            AnsiConsole.Markup("\U0001fae1 [OrangeRed1]Press Any Key to Retry or <*> to Exit...[/]");
            if (Console.ReadKey().Key == ConsoleKey.Multiply)
                return;
            continue;
        }

        await UIHandler.PrintAnalyzeResultAsync(repo, totalLines);

        Console.WriteLine();
        Console.WriteLine();
        var GODMod = new List<OptionEnums>() { OptionEnums.NullFix, OptionEnums.DeDup, OptionEnums.InFix, OptionEnums.LetOut, OptionEnums.DePort, OptionEnums.SortUp };
        if (!UIHandler.GetOptions(GODMod, out var selectedOptions)) continue;

        var displayOptions = Utility.SelectedOptionsContext(GODMod, selectedOptions, out var warnText, out var length);
        AnsiConsole.Write(new Panel(new Markup(text:$"{string.Join('\n', displayOptions)}"))
            .BorderColor(Color.Magenta)
            .RoundedBorder()
            .Header(text: "🛠️ [bold]Selected Options[/]")
            .PadRight(length));
        if (warnText != null)
            AnsiConsole.MarkupLine(warnText);
        var confirmed = AnsiConsole.Confirm("🤝 Do You Want To Continue?", true);
        if (!confirmed)
            continue;


        Console.Clear();
        List<string> titles = new();
        bool ascendNeeded = false;
        bool descendNeeded = false;
        if (selectedOptions.SequenceEqual(GODMod))
        {
            titles.Add(Enum.GetName(OptionEnums.ElonMod)!);
            await UIHandler.NullFixHandlerWithSpinnerAsync(repo, repo.EmptyLines);
            await UIHandler.DeDupHandlerWithSpinnerAsync(repo, repo.DuplicatedLines, repo.DuplicatedIPs);
            await UIHandler.InFixHandlerWithSpinnerAsync(repo, repo.InvalidIPs);
            await UIHandler.LetOutHandlerWithSpinnerAsync(repo, repo.LinesWithLetters);
            await UIHandler.DePortHandlerWithSpinnerAsync(repo, repo.LinesWithPort);
            ascendNeeded = true;
        }
        else if (selectedOptions.Any(x => x == OptionEnums.Merger))
        {
            titles.Add(Enum.GetName(OptionEnums.Merger)!);
            AnsiConsole.MarkupLine($"✅ All {validPaths.list!.Count} Input Files Merged Successfully.");
        }
        else
        {
            if (selectedOptions.Any(x => x == OptionEnums.Del4))
            {
                titles.Add(Enum.GetName(OptionEnums.Del4)!);
                await UIHandler.Del4HandlerWithSpinnerAsync(repo, repo.TotalIPs);
            }

            if (selectedOptions.Any(x => x == OptionEnums.NullFix))
            {
                titles.Add(Enum.GetName(OptionEnums.NullFix)!);
                await UIHandler.NullFixHandlerWithSpinnerAsync(repo, repo.EmptyLines);
            }

            if (selectedOptions.Any(x => x == OptionEnums.DeDup))
            {
                titles.Add(Enum.GetName(OptionEnums.DeDup)!);
                await UIHandler.DeDupHandlerWithSpinnerAsync(repo, repo.DuplicatedLines,
                    repo.DuplicatedIPs);
            }

            if (selectedOptions.Any(x => x == OptionEnums.InFix))
            {
                titles.Add(Enum.GetName(OptionEnums.InFix)!);
                await UIHandler.InFixHandlerWithSpinnerAsync(repo, repo.InvalidIPs);
            }

            if (selectedOptions.Any(x => x == OptionEnums.LetOut))
            {
                titles.Add(Enum.GetName(OptionEnums.LetOut)!);
                await UIHandler.LetOutHandlerWithSpinnerAsync(repo, repo.LinesWithLetters);
            }

            if (selectedOptions.Any(x => x == OptionEnums.DePort))
            {
                titles.Add(Enum.GetName(OptionEnums.DePort)!);
                await UIHandler.DePortHandlerWithSpinnerAsync(repo, repo.LinesWithPort);
            }

            if (selectedOptions.Any(x => x == OptionEnums.SortUp))
            {
                titles.Add(Enum.GetName(OptionEnums.SortUp)!);
                ascendNeeded = true;
            }

            if (selectedOptions.Any(x => x == OptionEnums.SortDown))
            {
                titles.Add(Enum.GetName(OptionEnums.SortDown)!);
                descendNeeded = true;
            }
        }

        Console.WriteLine();
        string targetFolder = await Utility.DefineResultFolder(selectedOptions, GODMod);
        string destPath = Path.Combine(resultPath, targetFolder!, $"IP File Normalizer_{DateTime.UtcNow.ToString("MM-dd-yyyy_hh-ss-mm")!}_{string.Join('-', titles)}.txt");
        string? elapsedTime = null;

        if (ascendNeeded == false && descendNeeded == false)
            elapsedTime = await ResultHandler.WriteDefaultResultAsync(repo, destPath);
        else if (ascendNeeded)
            elapsedTime = await ResultHandler.WriteAscendingResultAsync(repo, destPath);
        else
            elapsedTime = await ResultHandler.WriteDescendingResultAsync(repo, destPath);

        Console.CursorVisible = false;
        AnsiConsole.MarkupLine($"\r\n\n📂 [SpringGreen3]Operation Result Saved at[[{elapsedTime}]]:[/]");
        AnsiConsole.MarkupLine($"[grey]{destPath}[/]");
        AnsiConsole.MarkupLine($"📋 [DeepSkyBlue3]Copy To Do More...[/]");
        Console.WriteLine();
        AnsiConsole.Markup("🛠️ [LightPink3]Press Any Key To Continue or <E> for Exit[/]");
        var endOption = Console.ReadKey();
        if (endOption.Key == ConsoleKey.E)
            return;
        continue;
    }
}