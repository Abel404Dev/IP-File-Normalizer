namespace HelperMethod;

public static class DirHandler
{
    public static async Task CreateResultPath(string resultPath)
    {
        if (!Directory.Exists(resultPath))
        {
            Directory.CreateDirectory(resultPath);
        }
    }
    public static async Task CreateDirectories(string resultPath)
    {
        await CreateResultPath(resultPath);

        List<string> folders = new()
        {
            "1. ElonMod",
            "2. Merger",
            "3. MultiMod",
            "NullFix",
            "DeDup",
            "InFix",
            "LetOut",
            "DePort",
            "SortUp",
            "SortDown",
            "Del4"
        };

        for (int i = 0; i < folders.Count; i++)
        {
            string path = Path.Combine(resultPath, folders[i]);
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
        }
    }
    public static async Task<(List<string> notFoundList, List<string> notTextlist, List<string> emptyFileList, List<string> list)> ValidateFilesAsync(List<string> paths)
    {
        var notFound = new List<string>();
        var notTxt = new List<string>();
        var emptyFile = new List<string>();
        var result = new List<string>();

        foreach (var p in paths)
        {
            var fileInfo = new FileInfo(p);

            if (!fileInfo.Exists)
                notFound.Add(p);
            else if (fileInfo.Extension != ".txt")
                notTxt.Add(p);
            else if (fileInfo.Length == 0)
                emptyFile.Add(p);
            else
                result.Add(p);
        }

        return (notFound, notTxt, emptyFile, result);
    }
    public static async Task<(ConsoleKey? key, List<string>? list)> ValidateFilesHandlerAsync(List<string> paths)
    {
        var validateResult = await ValidateFilesAsync(paths);

        if (validateResult.notFoundList.Count > 0 || validateResult.notTextlist.Count > 0 || validateResult.emptyFileList.Count > 0)
        {
            if (validateResult.emptyFileList.Count > 0)
            {
                AnsiConsole.MarkupLine("😐 [red bold]Error:[/] Empty File(s)");

                for (int i = 0; i < validateResult.emptyFileList.Count; i++)
                {
                    AnsiConsole.Write(
                        new Panel(new Markup($"[grey]{validateResult.emptyFileList[i].EscapeMarkup()}[/]"))
                            .BorderColor(Color.Red)
                            .RoundedBorder()
                            .Header(text: $"✗ File {i + 1}", Justify.Left));
                }

                AnsiConsole.WriteLine();
            }

            if (validateResult.notFoundList.Count > 0)
            {
                AnsiConsole.MarkupLine("😐 [red bold]Error:[/] File(s) Not Found");

                for (int i = 0; i < validateResult.notFoundList.Count; i++)
                {
                    AnsiConsole.Write(
                        new Panel(new Markup($"[grey]{validateResult.notFoundList[i].EscapeMarkup()}[/]"))
                            .BorderColor(Color.Red)
                            .RoundedBorder()
                            .Header(text: $"✗ Path {i + 1}", Justify.Left));
                }

                AnsiConsole.WriteLine();
            }

            if (validateResult.notTextlist.Count > 0)
            {
                AnsiConsole.MarkupLine($"😐 [red bold]Error:[/] Unsupported File(s)");
                for (int i = 0; i < validateResult.notTextlist.Count; i++)
                {
                    AnsiConsole.Write(
                        new Panel(new Markup($"[grey]{validateResult.notTextlist[i].EscapeMarkup()}[/]"))
                            .BorderColor(Color.Red)
                            .RoundedBorder()
                            .Header(text: $"✗ File {i + 1}", Justify.Left));
                }
            }

            AnsiConsole.Markup("\U0001fae1 [OrangeRed1]Press Any Key to Retry or <*> to Exit...[/]");
            return (Console.ReadKey(intercept: false).Key, null);
        }
        else
        {
            return (null, validateResult.list);
        }
    }
}

public static class UIHandler
{
    public static async Task CleanConsoleManuallyAsync((int left, int top) position)
    {
        while (Console.GetCursorPosition().Top >= position.top)
        {
            Console.SetCursorPosition(0, Console.GetCursorPosition().Top);
            Console.Write(new string(' ', Console.WindowWidth));
            Console.SetCursorPosition(0, Console.GetCursorPosition().Top - 1);
        }
    }
    public static async Task<uint> ComputeTotalLinesAsync(List<string> paths)
    {
        if (paths.Count == 0) return 0;

        return await Task.Run(() =>
        {
            uint combinedTotalLines = 0;

            const long chunkSize = 1024 * 1024 * 512;

            foreach (var path in paths)
            {
                FileInfo fileInfo = new FileInfo(path);
                long fileSize = fileInfo.Length;

                if (fileSize == 0) continue;

                long offset = 0;

                using var mmf = MemoryMappedFile.CreateFromFile(path, FileMode.Open, null, 0, MemoryMappedFileAccess.Read);

                while (offset < fileSize)
                {
                    long sizeToMap = Math.Min(chunkSize, fileSize - offset);

                    using var accessor = mmf.CreateViewAccessor(offset, sizeToMap, MemoryMappedFileAccess.Read);

                    unsafe
                    {
                        byte* ptr = null;
                        accessor.SafeMemoryMappedViewHandle.AcquirePointer(ref ptr);
                        try
                        {
                            byte* start = ptr + accessor.PointerOffset;
                            byte* end = start + sizeToMap;

                            while (start < end)
                            {
                                if (*start == (byte)'\n')
                                {
                                    combinedTotalLines++;
                                }
                                start++;
                            }
                        }
                        finally
                        {
                            accessor.SafeMemoryMappedViewHandle.ReleasePointer();
                        }
                    }

                    offset += sizeToMap;
                }
            }

            return combinedTotalLines;
        });
    }
    public static async Task<uint> ComputeTotalLinesWithSpinnerAsync(List<string> paths)
    {
        uint result = 0;
        await AnsiConsole.Status()
            .Spinner(Spinner.Known.Mindblown)
            .StartAsync("[grey]Please Wait...[/]", async ctx =>
            {
                result = await ComputeTotalLinesAsync(paths);
            });

        return result;
    }
    public static async Task LoadFilesWithProgressAsync(LineRepository repo, List<string> paths, uint totalLines)
    {
        string? elapsedTime = null;

        Console.SetCursorPosition(0, Console.GetCursorPosition().Top - 1);
        await AnsiConsole.Progress()
            .Columns(new SpinnerColumn() { Style = new Style() { Foreground = Color.OrangeRed1, Decoration = Decoration.Bold } }, new TaskDescriptionColumn(), new ProgressBarColumn(), new PercentageColumn(), new ElapsedTimeColumn())
            .StartAsync(async ctx =>
            {
                var task = ctx.AddTask("[grey]Loading Files:[/]", maxValue: totalLines);

                long counter = 0;
                const int updateInterval = 10;
                foreach (var path in paths)
                {
                    using var sr = new StreamReader(path);
                    string? line;
                    while ((line = await sr.ReadLineAsync()) != null)
                    {
                        var _ip = await Utility.RegexIP(line);
                        var _port = await Utility.RegexPort(line);
                        var _letters = await Utility.RegexLetters(line);
                        var _bytes = await Utility.RegexBytes(line);

                        await repo.AddAsync(
                                  index: (uint)counter,
                                  @base: (!string.IsNullOrEmpty(line) && !string.IsNullOrWhiteSpace(line)) ? line : null,
                                  ip: (_ip != null) ? _ip : null,
                                  port: (_port != null) ? _port : null,
                                  letters: (_letters != null && _letters.Count > 0) ? _letters : null,
                                  bytes: (_bytes != null) ? _bytes : null);

                        counter++;

                        if (counter % updateInterval == 0)
                        {
                            task.Value = counter;
                        }
                    }
                }

                elapsedTime = task.ElapsedTime?.ToString(@"hh\:mm\:ss");
            });

        Console.SetCursorPosition(0, Console.GetCursorPosition().Top - 2);
        Console.Write(new string(' ', Console.WindowWidth));
        Console.SetCursorPosition(0, Console.GetCursorPosition().Top);
        AnsiConsole.MarkupLine($"✅ All {paths.Count:N0} File(s) Loaded Successfully. [SpringGreen3][[{elapsedTime}]][/] ");
    }
    public static async Task AnalyzeWithSpinnerAsync(LineRepository repo)
    {
        await AnsiConsole.Status()
            .Spinner(Spinner.Known.Toggle10)
            .SpinnerStyle(Style.Parse("OrangeRed1 Bold"))
            .StartAsync("[grey]Analyzing...[/]", async ctx =>
            {
                await repo.RecomputeHeavyStatisticsAsync();
            });

        AnsiConsole.MarkupLine("✅ Some [Bold Underline]Approximate[/] Statistics Collected Successfully.");
    }
    public static async Task PrintAnalyzeResultAsync(LineRepository repo, uint totalLines)
    {
        Console.WriteLine();
        AnsiConsole.MarkupLine($"☠️ [Cyan]Total Lines:[/] {totalLines:N0}");
        AnsiConsole.MarkupLine($"🤣 [Cyan]Empty Lines:[/] {repo.EmptyLines:N0}");
        AnsiConsole.MarkupLine($"💩 [Cyan]Duplicate Lines:[/] {repo.DuplicatedLines:N0}");
        AnsiConsole.MarkupLine($"👎 [Cyan]Lines That Include Everything Except The Correct IP Syntax:[/] {repo.LinesWithLetters:N0}");
        AnsiConsole.MarkupLine($"👙 [Cyan]Lines That Include Port:[/] {repo.LinesWithPort:N0}");
        if (repo.LinesWithPort > 0)
            AnsiConsole.MarkupLine($"📜 [Cyan]Used Ports:[/] {string.Join(" - ", repo.UsedPorts)}");
        AnsiConsole.MarkupLine($"🥵 [Cyan]Total IPs:[/] {repo.TotalIPs:N0}");
        AnsiConsole.MarkupLine($"🍌 [Cyan]Duplicate IPs:[/] {repo.DuplicatedIPs:N0}");
        AnsiConsole.MarkupLine($"👏 [Cyan]Invalid IPs:[/] {repo.InvalidIPs:N0}");
        AnsiConsole.MarkupLine($"💎 [Cyan]Valid IPs:[/] {repo.ValidIPs:N0}");
        if (repo.ValidIPs > 0)
            AnsiConsole.MarkupLine("☢️ [red]maybe needs some cleanup to reach![/]");
    }

    public static async Task NullFixHandlerWithSpinnerAsync(LineRepository repo, uint emptyLinesCount)
    {
        await AnsiConsole.Status()
            .Spinner(Spinner.Known.Dots)
            .SpinnerStyle(Style.Parse("OrangeRed1 Bold"))
            .StartAsync("[grey]Removing Empty Lines...[/]", async ctx =>
            {
                await repo.NullFixHandler();
            });

        AnsiConsole.MarkupLine($"✅ All {emptyLinesCount} Empty Line(s) Deleted Successfully.");
    }
    public static async Task DeDupHandlerWithSpinnerAsync(LineRepository repo, uint duplicatedLinesCount, uint duplicatedIPsCount)
    {
        await AnsiConsole.Status()
            .Spinner(Spinner.Known.Dots)
            .SpinnerStyle(Style.Parse("OrangeRed1 Bold"))
            .StartAsync("[grey]Removing Duplicates...[/]", async ctx =>
            {
                await repo.DeDupHandler();
            });

        AnsiConsole.MarkupLine($"✅ All {duplicatedLinesCount} Duplicated Line(s) & {duplicatedIPsCount} Duplicated IP(s) Deleted Successfully.");
    }
    public static async Task InFixHandlerWithSpinnerAsync(LineRepository repo, uint invalidIPsCount)
    {
        await AnsiConsole.Status()
            .Spinner(Spinner.Known.Dots)
            .SpinnerStyle(Style.Parse("OrangeRed1 Bold"))
            .StartAsync("[grey]Removing Invalid IPs...[/]", async ctx =>
            {
                await repo.InFixHandler();
            });

        AnsiConsole.MarkupLine($"✅ All {invalidIPsCount} Invalid IP(s) Deleted Successfully.");
    }
    public static async Task LetOutHandlerWithSpinnerAsync(LineRepository repo, uint linesWithLetters)
    {
        await AnsiConsole.Status()
            .Spinner(Spinner.Known.Dots)
            .SpinnerStyle(Style.Parse("OrangeRed1 Bold"))
            .StartAsync("[grey]Cleaning Unwanted Text &...[/]", async ctx =>
            {
                await repo.LetOutHandler();
            });

        AnsiConsole.MarkupLine($"✅ All {linesWithLetters} Line(s) Cleaned Up Successfully.");
    }
    public static async Task DePortHandlerWithSpinnerAsync(LineRepository repo, uint linesWithPort)
    {
        await AnsiConsole.Status()
            .Spinner(Spinner.Known.Dots)
            .SpinnerStyle(Style.Parse("OrangeRed1 Bold"))
            .StartAsync("[grey]Removing Ports...[/]", async ctx =>
            {
                await repo.DePortHandler();
            });

        AnsiConsole.MarkupLine($"✅ All {linesWithPort} Port(s) Deleted Successfully.");
    }
    public static async Task Del4HandlerWithSpinnerAsync(LineRepository repo, uint totalIPsCount)
    {
        await AnsiConsole.Status()
            .Spinner(Spinner.Known.Dots)
            .SpinnerStyle(Style.Parse("OrangeRed1 Bold"))
            .StartAsync("[grey]Removing All IPv4s...[/]", async ctx =>
            {
                await repo.Del4Handler();
            });

        AnsiConsole.MarkupLine($"✅ All {totalIPsCount} IPv4(s) Deleted Successfully.");
    }

    public static bool GetOptions(List<OptionEnums> godMod, out List<OptionEnums> list)
    {
        var prompt = new MultiSelectionPrompt<OptionEnums>()
            .Title("🛠️ [Magenta]Available Options\r\n-----------------------[/]")
            .InstructionsText("💦 [grey]press [blue]<space>[/] to toggle, then [green]<enter>[/] to confirm and start.[/]\r\n⚠️ [OrangeRed1 Italic]leave empty/unchecked & press <enter> to cancel.[/]")
            .NotRequired()
            .UseConverter(opt => Utility.GetOptionDisplay(opt)!);
        prompt.AddChoiceGroup(OptionEnums.ElonMod, godMod);
        prompt.AddChoice(OptionEnums.SortDown);
        prompt.AddChoice(OptionEnums.Merger);
        prompt.AddChoice(OptionEnums.Del4);
        list = AnsiConsole.Prompt(prompt);
        if (list.Count == 0)
            return false;
        return true;
    }
}

public static class ResultHandler
{
    public static async Task<string> WriteDefaultResultAsync(LineRepository repo, string path)
    {
        string? elapsedTime = null;
        var tL = repo.FreshTotalLines;

        Console.SetCursorPosition(0, Console.GetCursorPosition().Top - 1);
        await AnsiConsole.Progress()
            .Columns(new SpinnerColumn() { Style = new Style() { Foreground = Color.OrangeRed1, Decoration = Decoration.Bold } }, new TaskDescriptionColumn(), new ProgressBarColumn(), new PercentageColumn(), new ElapsedTimeColumn())
            .StartAsync(async ctx =>
            {
                var task = ctx.AddTask("\t[grey]Writing The Result File:[/]", maxValue: tL);

                long counter = 0;
                await using (StreamWriter sw = new(path: path, encoding: new UTF8Encoding(), options: new FileStreamOptions() { Access = FileAccess.ReadWrite, Mode = FileMode.Create, Options = FileOptions.Asynchronous, BufferSize = 1024 * 1024 }))
                {
                    byte i = 0;
                    var lines = await repo.FilterLinesAsync();
                    foreach (var kvp in lines!)
                    {
                        string? firstLetters = null;
                        string? endLetters = null;
                        if (kvp.Letters != null)
                        {
                            foreach (var item in kvp.Letters)
                            {
                                if (item.StartsWith("f:"))
                                    firstLetters = item[2..];
                                else if (item.StartsWith("e:"))
                                    endLetters = item[2..];
                            }
                        }
                        string? ip = (kvp.IP != null) ? kvp.IP : null;
                        string? port = (kvp.Port != null) ? $":{kvp.Port}" : null;
                        if (i == 0)
                        {
                            await sw.WriteAsync($"{firstLetters}{ip}{port}{endLetters}");
                            i++;
                            Utility.UpdateProgress(ref counter, ref task);
                            continue;
                        }
                        await sw.WriteAsync($"\r\n{firstLetters}{ip}{port}{endLetters}");
                        Utility.UpdateProgress(ref counter, ref task);
                    }
                    lines.Clear();
                    lines.TrimExcess();
                    lines = null;
                }

                elapsedTime = task.ElapsedTime?.ToString(@"hh\:mm\:ss");
            });

        repo.Dispose();
        Console.SetCursorPosition(0, Console.GetCursorPosition().Top - 2);
        Console.Write(new string(' ', Console.WindowWidth));
        Console.SetCursorPosition(0, Console.GetCursorPosition().Top - 1);

        return elapsedTime!;
    }
    public static async Task<string> WriteAscendingResultAsync(LineRepository repo, string path)
    {
        string? elapsedTime = null;
        var tL = repo.FreshTotalLines;

        Console.SetCursorPosition(0, Console.GetCursorPosition().Top - 1);
        await AnsiConsole.Progress()
            .Columns(new SpinnerColumn() { Style = new Style() { Foreground = Color.OrangeRed1, Decoration = Decoration.Bold } }, new TaskDescriptionColumn(), new ProgressBarColumn(), new PercentageColumn(), new ElapsedTimeColumn())
            .StartAsync(async ctx =>
            {
                var task = ctx.AddTask("\t[grey]Writing The Result File:[/]", maxValue: tL);

                long counter = 0;
                await using (StreamWriter sw = new(path: path, encoding: new UTF8Encoding(), options: new FileStreamOptions() { Access = FileAccess.ReadWrite, Mode = FileMode.Create, Options = FileOptions.Asynchronous, BufferSize = 1024 * 1024 }))
                {
                    byte i = 0;
                    var nonIpLines = await repo.FilterLinesAsync(x => x.Base != null && x.IP == null);
                    if (nonIpLines != null)
                    {
                        foreach (var kvp in nonIpLines)
                        {
                            string? firstLetters = null;
                            string? endLetters = null;
                            if (kvp.Letters != null)
                            {
                                foreach (var item in kvp.Letters)
                                {
                                    if (item.StartsWith("f:"))
                                        firstLetters = item[2..];
                                    else if (item.StartsWith("e:"))
                                        endLetters = item[2..];
                                }
                            }
                            string? port = (kvp.Port != null) ? $":{kvp.Port}" : null;

                            if (i == 0)
                            {
                                await sw.WriteAsync($"{firstLetters}{port}{endLetters}");
                                i++;
                                Utility.UpdateProgress(ref counter, ref task);
                                continue;
                            }
                            await sw.WriteAsync($"\r\n{firstLetters}{port}{endLetters}");
                            Utility.UpdateProgress(ref counter, ref task);
                        }
                        nonIpLines.Clear();
                        nonIpLines.TrimExcess();
                        nonIpLines = null;
                    }

                    var ipLines = await repo.FilterLinesAsync(x => x.Bytes != null);
                    if (ipLines != null)
                    {
                        ipLines = ipLines.OrderBy(x => x.Bytes![0])
                            .ThenBy(x => x.Bytes![1])
                            .ThenBy(x => x.Bytes![2])
                            .ThenBy(x => x.Bytes![3])
                            .ToList();

                        foreach (var kvp in ipLines)
                        {
                            string? firstLetters = null;
                            string? endLetters = null;
                            if (kvp.Letters != null)
                            {
                                foreach (var item in kvp.Letters)
                                {
                                    if (item.StartsWith("f:"))
                                        firstLetters = item[2..];
                                    else if (item.StartsWith("e:"))
                                        endLetters = item[2..];
                                }
                            }
                            string? ip = (kvp.IP != null) ? kvp.IP : null;
                            string? port = (kvp.Port != null) ? $":{kvp.Port}" : null;

                            if (i == 0)
                            {
                                await sw.WriteAsync($"{firstLetters}{ip}{port}{endLetters}");
                                i++;
                                Utility.UpdateProgress(ref counter, ref task);
                                continue;
                            }
                            await sw.WriteAsync($"\r\n{firstLetters}{ip}{port}{endLetters}");
                            Utility.UpdateProgress(ref counter, ref task);
                        }
                        ipLines.Clear();
                        ipLines.TrimExcess();
                        ipLines = null;
                    }
                }

                elapsedTime = task.ElapsedTime?.ToString(@"hh\:mm\:ss");
            });

        repo.Dispose();
        Console.SetCursorPosition(0, Console.GetCursorPosition().Top - 2);
        Console.Write(new string(' ', Console.WindowWidth));
        Console.SetCursorPosition(0, Console.GetCursorPosition().Top - 1);
        AnsiConsole.MarkupLine($"✅ Ascending Sort Operation Processed Successfully.");

        return elapsedTime!;
    }
    public static async Task<string> WriteDescendingResultAsync(LineRepository repo, string path)
    {
        string? elapsedTime = null;
        var tL = repo.FreshTotalLines;

        Console.SetCursorPosition(0, Console.GetCursorPosition().Top - 1);
        await AnsiConsole.Progress()
            .Columns(new SpinnerColumn() { Style = new Style() { Foreground = Color.OrangeRed1, Decoration = Decoration.Bold } }, new TaskDescriptionColumn(), new ProgressBarColumn(), new PercentageColumn(), new ElapsedTimeColumn())
            .StartAsync(async ctx =>
            {
                var task = ctx.AddTask("\t[grey]Writing The Result File:[/]", maxValue: tL);

                long counter = 0;
                await using (StreamWriter sw = new(path: path, encoding: new UTF8Encoding(), options: new FileStreamOptions() { Access = FileAccess.ReadWrite, Mode = FileMode.Create, Options = FileOptions.Asynchronous, BufferSize = 1024 * 1024 }))
                {
                    byte i = 0;
                    var ipLines = await repo.FilterLinesAsync(x => x.Bytes != null);
                    if (ipLines != null)
                    {
                        ipLines = ipLines.OrderByDescending(x => x.Bytes![0])
                            .ThenByDescending(x => x.Bytes![1])
                            .ThenByDescending(x => x.Bytes![2])
                            .ThenByDescending(x => x.Bytes![3])
                            .ToList();
                        foreach (var kvp in ipLines)
                        {
                            string? firstLetters = null;
                            string? endLetters = null;
                            if (kvp.Letters != null)
                            {
                                foreach (var item in kvp.Letters)
                                {
                                    if (item.StartsWith("f:"))
                                        firstLetters = item[2..];
                                    else if (item.StartsWith("e:"))
                                        endLetters = item[2..];
                                }
                            }
                            string? ip = (kvp.IP != null) ? kvp.IP : null;
                            string? port = (kvp.Port != null) ? $":{kvp.Port}" : null;

                            if (i == 0)
                            {
                                await sw.WriteAsync($"{firstLetters}{ip}{port}{endLetters}");
                                i++;
                                Utility.UpdateProgress(ref counter, ref task);
                                continue;
                            }
                            await sw.WriteAsync($"\r\n{firstLetters}{ip}{port}{endLetters}");
                            Utility.UpdateProgress(ref counter, ref task);
                        }
                        ipLines.Clear();
                        ipLines.TrimExcess();
                        ipLines = null;
                    }

                    var nonIpLines = await repo.FilterLinesAsync(x => x.Base != null && x.IP == null);
                    if (nonIpLines != null)
                    {
                        foreach (var kvp in nonIpLines)
                        {
                            string? firstLetters = null;
                            string? endLetters = null;
                            if (kvp.Letters != null)
                            {
                                foreach (var item in kvp.Letters)
                                {
                                    if (item.StartsWith("f:"))
                                        firstLetters = item[2..];
                                    else if (item.StartsWith("e:"))
                                        endLetters = item[2..];
                                }
                            }
                            string? port = (kvp.Port != null) ? $":{kvp.Port}" : null;

                            if (i == 0)
                            {
                                await sw.WriteAsync($"{firstLetters}{port}{endLetters}");
                                i++;
                                Utility.UpdateProgress(ref counter, ref task);
                                continue;
                            }
                            await sw.WriteAsync($"\r\n{firstLetters}{port}{endLetters}");
                            Utility.UpdateProgress(ref counter, ref task);
                        }
                        nonIpLines.Clear();
                        nonIpLines.TrimExcess();
                        nonIpLines = null;
                    }
                }

                elapsedTime = task.ElapsedTime?.ToString(@"hh\:mm\:ss");
            });

        repo.Dispose();
        Console.SetCursorPosition(0, Console.GetCursorPosition().Top - 2);
        Console.Write(new string(' ', Console.WindowWidth));
        Console.SetCursorPosition(0, Console.GetCursorPosition().Top - 1);
        AnsiConsole.MarkupLine($"✅ Descending Sort Operation Processed Successfully.");

        return elapsedTime!;
    }
}

public static class Utility
{
    public static async Task<(bool result, List<string>? list)> ParseInputPathsAsync(string? input)
    {
        var list = new List<string>();

        if (string.IsNullOrEmpty(input) || string.IsNullOrWhiteSpace(input) || input == "*")
            return (false, null);

        var regexedInput = Regex.Split(input, @"([""']\s*,*\s*[""'])|,\s*,", RegexOptions.Compiled);
        list = regexedInput.Where(p => !string.IsNullOrEmpty(p) && !string.IsNullOrWhiteSpace(p)).Select(p => p.Trim('\"', '\''))
            .Where(p => !string.IsNullOrEmpty(p) && !string.IsNullOrWhiteSpace(p))
            .Distinct()
            .ToList();

        return (true, list);
    }
    public static string GetOptionDisplay(OptionEnums opt) => opt switch
    {
        OptionEnums.NullFix => "🧹 NullFix  [grey]:will delete all null or empty lines[/]",
        OptionEnums.DeDup => "🔁 DeDup    [grey]:will delete all duplicated lines & ips[/]",
        OptionEnums.InFix => "🎯 InFix    [grey]:will delete all invalid ips[/]",
        OptionEnums.LetOut => "📝 LetOut   [grey]:will delete all letters, spaces & etc in each line[/]",
        OptionEnums.DePort => "🔌 DePort   [grey]:will delete all ports[/]",
        OptionEnums.SortUp => "📈 SortUp   [grey]:will sort ips ascending[/]",
        OptionEnums.SortDown => "📉 SortDown [grey]:will sort ips descending[/]",
        OptionEnums.Del4 => "🔥 Del4     [grey]:will delete all ipv4 ips[/]",
        OptionEnums.Merger => "🔗 Merger   [grey]:will just merge your input files into single file[/]",
        OptionEnums.ElonMod => "🚀 ElonMod  [grey]:will process NullFix..SortUp operations[/]",
        _ => opt.ToString()
    };
    public static string GetShortOptionDisplay(OptionEnums opt) => opt switch
    {
        OptionEnums.NullFix => "🧹 NullFix",
        OptionEnums.DeDup => "🔁 DeDup",
        OptionEnums.InFix => "🎯 InFix",
        OptionEnums.LetOut => "📝 LetOut",
        OptionEnums.DePort => "🔌 DePort",
        OptionEnums.SortUp => "📈 SortUp",
        OptionEnums.SortDown => "📉 SortDown",
        OptionEnums.Del4 => "🔥 Del4",
        OptionEnums.Merger => "🔗 Merger",
        OptionEnums.ElonMod => "🚀 ElonMod",
        _ => opt.ToString()
    };
    public static async Task<string?> RegexIP(string? txt)
    {
        if (string.IsNullOrEmpty(txt) || string.IsNullOrWhiteSpace(txt)) return null;

        string ipPattern = @"(\d+\.\d+\.\d+\.\d+)";
        var result = Regex.Match(txt.Trim(), ipPattern, RegexOptions.Compiled);

        if (result.Groups[1].Value != "")
            return result.Groups[1].Value;
        return null;
    }
    public static async Task<string?> RegexPort(string? txt)
    {
        if (string.IsNullOrEmpty(txt) || string.IsNullOrWhiteSpace(txt)) return null;

        string ipPattern = @"\d+\.\d+\.\d+\.\d+:(\d+)";
        var result = Regex.Match(txt.Trim(), ipPattern, RegexOptions.Compiled);

        if (result.Groups[1].Value != "")
            return result.Groups[1].Value;
        return null;
    }
    public static async Task<List<string>?> RegexLetters(string? txt)
    {
        if (string.IsNullOrEmpty(txt) || string.IsNullOrWhiteSpace(txt)) return null;

        string ipWithLetterPattern = @"^\d+\.\d+\.\d+\.\d+(:\d+|)$";
        if (Regex.IsMatch(txt, ipWithLetterPattern, RegexOptions.Compiled)) return null;

        List<string> result = new();
        var hasIP = await RegexIP(txt);
        if (hasIP != null)
        {
            var letters = Regex.Match(txt, @"(.+|)\d+\.\d+\.\d+\.\d+(:\d+|)(.+|)", RegexOptions.Compiled);
            if (letters.Success)
            {
                if (letters.Groups[1].Value != "")
                    result.Add($"f:{letters.Groups[1].Value}");
                if (letters.Groups[3].Value != "")
                    result.Add($"e:{letters.Groups[3].Value}");
            }
        }
        else result.Add($"f:{txt}");

        return result;
    }
    public static async Task<byte[]?> RegexBytes(string? txt)
    {
        if (string.IsNullOrEmpty(txt) || string.IsNullOrWhiteSpace(txt)) return null;

        var hasIP = await RegexIP(txt);
        if (hasIP == null) return null;

        if (!IPAddress.TryParse(hasIP, out var ip)) return null;

        return ip.GetAddressBytes();
    }
    public static async Task<string> DefineResultFolder(List<OptionEnums> options, List<OptionEnums> godMod)
    {
        if (options.SequenceEqual(godMod)) return "1. ElonMod";
        else if (options.Any(y => y == OptionEnums.Merger)) return "2. Merger";
        else if (options.Count > 1) return "3. MultiMod";
        else
            return options[0] switch
            {
                OptionEnums.NullFix => "NullFix",
                OptionEnums.DeDup => "DeDup",
                OptionEnums.InFix => "InFix",
                OptionEnums.LetOut => "LetOut",
                OptionEnums.DePort => "DePort",
                OptionEnums.SortUp => "SortUp",
                OptionEnums.SortDown => "SortDown",
                OptionEnums.Del4 => "Del4",
                _ => options[0].ToString()
            };
    }
    public static void UpdateProgress(ref long counter, ref ProgressTask task)
    {
        counter++;
        if (counter % 10 == 0)
        {
            task.Value = counter;
        }
    }
    public static List<string> SelectedOptionsContext(List<OptionEnums> godMod, List<OptionEnums> selectedOptions, out string? warn, out int length)
    {
        List<string> displayOptions = new();
        warn = null;
        length = 0;
        if (godMod.All(o => selectedOptions.Contains(o)))
        {
            displayOptions.Add($"{Utility.GetShortOptionDisplay(OptionEnums.ElonMod)} [grey]Which Contains:[/]");

            foreach (var opt in selectedOptions)
            {
                if (godMod.Contains(opt))
                    displayOptions.Add($"  →{Utility.GetShortOptionDisplay(opt)}");
                else
                {
                    displayOptions.Add($"[grey strikethrough]{Utility.GetShortOptionDisplay(opt)}[/]");
                    if (warn == null)
                        warn = "⚠️ [OrangeRed1 Italic]when ElonMod is selected, the other options will be ignored.[/]";
                }
            }
        }
        else if (selectedOptions.Contains(OptionEnums.Merger))
        {
            length = Length(selectedOptions);

            displayOptions.Add($"{Utility.GetShortOptionDisplay(OptionEnums.Merger)}");

            if (selectedOptions.Count > 1)
            {
                foreach (var opt in selectedOptions)
                {
                    displayOptions.Add($"[grey strikethrough]{Utility.GetShortOptionDisplay(opt)}[/]");
                }

                if (warn == null)
                    warn = "⚠️ [OrangeRed1 Italic]when Merger is selected, the other options will be ignored.[/]";
            }
        }
        else
        {
            length = Length(selectedOptions);

            foreach (var opt in selectedOptions)
            {
                displayOptions.Add($"{Utility.GetShortOptionDisplay(opt)}");
            }

            if (selectedOptions.Contains(OptionEnums.SortUp) && selectedOptions.Contains(OptionEnums.SortDown))
                warn = $"⚠️ [OrangeRed1 Italic]when both SortUp & SortDown are selected, just SortUp will be processed.[/]";
        }

        return displayOptions;

        int Length(List<OptionEnums> optionEnumsList)
        {
            int i;
            var displayLength = optionEnumsList.MaxBy(x => x.ToString().Length).ToString().Length;
            i = 17 - displayLength;
            return i;
        }
    }
}