Console.Title = "IP File Normalizer - v1.0.1";
Console.OutputEncoding = System.Text.Encoding.UTF8;
string exeDir = AppContext.BaseDirectory;
string resultPath = Path.Combine(exeDir, "Results");
if (!Directory.Exists(resultPath))
{
    Directory.CreateDirectory(resultPath);
}
await CreateDirectories(resultPath);
Console.ResetColor();
while (true)
{
    LineRepository repo = new();
    Console.Clear();
    Console.Write("😩 Enter txt SourceFile Path(s) (split by comma or space) [enter * or leave empty to exit]: ");
    var srcInputPath = Console.ReadLine();
    
    List<string> srcPaths = new();
    if (srcInputPath!.Equals("*") || string.IsNullOrEmpty(srcInputPath))
        return;
    else if (srcInputPath.Contains(",") || srcInputPath.Contains("\" \"") || srcInputPath.Contains("' '") || srcInputPath.Contains("' \"") || srcInputPath.Contains("\" '"))
    {
        if (srcInputPath.Contains("\" \""))
        {
            srcInputPath = Regex.Replace(srcInputPath, "\"\\s+\"", "\",\"");
        }
        if (srcInputPath.Contains("' '"))
        {
            srcInputPath = Regex.Replace(srcInputPath, "'\\s+'", "\",\"");
        }
        if (srcInputPath.Contains("' \""))
        {
            srcInputPath = Regex.Replace(srcInputPath, "'\\s+\"", "\",\"");
        }
        if (srcInputPath.Contains("\" '"))
        {
            srcInputPath = Regex.Replace(srcInputPath, "\"\\s+'", "\",\"");
        }
        if (srcInputPath.Contains(","))
        {
            srcInputPath = Regex.Replace(srcInputPath, "\"(\\s|),(\\s|)\"", "\",\"");
        }
        srcInputPath = Regex.Replace(srcInputPath, "[\"']", string.Empty);
        srcPaths.AddRange(srcInputPath.Split(',').Where(s => !string.IsNullOrEmpty(s) && !string.IsNullOrWhiteSpace(s)).Distinct().ToList());
    }
    else
        srcPaths.Add(Regex.Replace(srcInputPath, "[\"']", string.Empty));

    List<string> nfPath = new();
    List<string> etcFile = new();
    foreach (var p in srcPaths)
    {
        if (!File.Exists(p))
        {
            nfPath.Add(p);
        }
        else if (Path.GetExtension(p) != ".txt")
        {
            etcFile.Add(p);
        }
    }
    if (nfPath.Count > 0 || etcFile.Count > 0)
    {
        Console.Clear();
        if (nfPath.Count > 0)
        {
            Console.Write("😐 ");
            Console.ForegroundColor = ConsoleColor.DarkRed;
            Console.WriteLine($"File(s) Not Found at:");
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine($"{string.Join("\r\n", nfPath)}\r\n");
            Console.ResetColor();
        }
        if (etcFile.Count > 0)
        {
            Console.Write("😐 ");
            Console.ForegroundColor = ConsoleColor.DarkRed;
            Console.WriteLine($"Unsupported File(s) Detected at:");
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine($"{string.Join("\r\n", etcFile)}\r\n");
            Console.ResetColor();
        }

        Console.Write("\U0001fae1 ");
        Console.ForegroundColor = ConsoleColor.DarkRed;
        Console.Write("Press * or x to Exit or Any Key to Retry...");
        var choseKey = Console.ReadKey();
        Console.ResetColor();
        if (choseKey.Key == ConsoleKey.Multiply || choseKey.Key == ConsoleKey.X)
            return;
        continue;
    }
    Console.Clear();

    bool isLoaded = false;
    Task loadFiles = Task.Run(async () =>
    {
        byte position = 0;

        while (!isLoaded)
        {
            await PrintWaitingContent("File(s) Loading", position);

            if (position == 7)
            {
                position = 0;
                continue;
            }
            position++;
        }
    });
    uint counter = 0;
    foreach (var p in srcPaths)
    {
        using (var sr = new StreamReader(p))
        {
            string? line;
            while ((line = await sr.ReadLineAsync()) != null)
            {
                repo.Add(
                index: counter,
                @base: line,
                ip: await RegexIP(line),
                port: await RegexPort(line),
                letters: await RegexLetters(line),
                bytes: await RegexBytes(line)
                );

                counter++;
            }

        }
    }
    isLoaded = true;
    Console.Write(new string(' ', Console.WindowWidth));
    Console.WriteLine($"\r✅ {srcPaths.Count} File(s) Loaded Successfully.");

    #region Generate Analyze's Result
    bool isDetected = false;
    Task detectLines = Task.Run(async () =>
    {
        byte position = 0;
        while (!isDetected)
        {
            await PrintWaitingContent("Analyzing", position);

            if (position == 7)
            {
                position = 0;
                continue;
            }
            position++;
        }
    });
    var totalLinesCount = repo.TotalLines;
    var emptyLinesCount = repo.EmptyLines;
    var duplicatedLinesCount = repo.DuplicatedLines;
    var letteredLinesCount = repo.LetteredLines;
    var portedLinesCount = repo.PortedLines;
    var usedPorts = repo.UsedPorts;
    var totalIPsCount = repo.TotalIPs;
    var invalidIPsCount = repo.InvalidIPs;
    var duplicatedIPsCount = repo.DuplicatedIPs;
    var validIPsCount = repo.ValidIPs;
    isDetected = true;
    Console.Write(new string(' ', Console.WindowWidth));
    Console.WriteLine("\r✅ Some Approximate Data Collected Successfully.");
    Console.WriteLine();
    #endregion

    #region Print Collected Data
    PrintCollectedData(totalLinesCount, emptyLinesCount, duplicatedLinesCount, letteredLinesCount, portedLinesCount,
        usedPorts!, totalIPsCount, invalidIPsCount, duplicatedIPsCount, validIPsCount);
    #endregion

    #region Show Menu Options
    Console.WriteLine();
    Console.WriteLine();
    Console.Write("🛠️ ");
    Console.ForegroundColor = ConsoleColor.DarkMagenta;
    Console.WriteLine("Available Options:");
    Console.WriteLine("---------------------");
    Console.ResetColor();
    Console.Write("🧹 ");
    Console.ForegroundColor = ConsoleColor.DarkYellow;
    Console.Write($"‹1› {OptionEnums.NullFix} ");
    Console.ForegroundColor = ConsoleColor.DarkGray;
    Console.WriteLine(" :will delete all null or empty lines");
    Console.ResetColor();
    Console.Write("🔁 ");
    Console.ForegroundColor = ConsoleColor.DarkYellow;
    Console.Write($"‹2› {OptionEnums.DeDup} ");
    Console.ForegroundColor = ConsoleColor.DarkGray;
    Console.WriteLine("   :will delete all duplicated lines & IPs");
    Console.ResetColor();
    Console.Write("🎯 ");
    Console.ForegroundColor = ConsoleColor.DarkYellow;
    Console.Write($"‹3› {OptionEnums.InFix} ");
    Console.ForegroundColor = ConsoleColor.DarkGray;
    Console.WriteLine("   :will delete all invalid ips");
    Console.ResetColor();
    Console.Write("📝 ");
    Console.ForegroundColor = ConsoleColor.DarkYellow;
    Console.Write($"‹4› {OptionEnums.LetOut} ");
    Console.ForegroundColor = ConsoleColor.DarkGray;
    Console.WriteLine("  :will delete all letters, spaces & etc in each line");
    Console.ResetColor();
    Console.Write("🔌 ");
    Console.ForegroundColor = ConsoleColor.DarkYellow;
    Console.Write($"‹5› {OptionEnums.DePort} ");
    Console.ForegroundColor = ConsoleColor.DarkGray;
    Console.WriteLine("  :will delete all ports");
    Console.ResetColor();
    Console.Write("📈 ");
    Console.ForegroundColor = ConsoleColor.DarkYellow;
    Console.Write($"‹6› {OptionEnums.SortUp} ");
    Console.ForegroundColor = ConsoleColor.DarkGray;
    Console.WriteLine("  :will sort ips ascending");
    Console.ResetColor();
    Console.Write("📉 ");
    Console.ForegroundColor = ConsoleColor.DarkYellow;
    Console.Write($"‹7› {OptionEnums.SortDown} ");
    Console.ForegroundColor = ConsoleColor.DarkGray;
    Console.WriteLine(":will sort ips descending");
    Console.ResetColor();
    Console.Write("🔥 ");
    Console.ForegroundColor = ConsoleColor.DarkYellow;
    Console.Write($"‹8› {OptionEnums.Del4} ");
    Console.ForegroundColor = ConsoleColor.DarkGray;
    Console.WriteLine("    :will delete all ipv4 ips");
    Console.ResetColor();
    Console.Write("🔗 ");
    Console.ForegroundColor = ConsoleColor.DarkYellow;
    Console.Write($"‹9› {OptionEnums.Merger} ");
    Console.ForegroundColor = ConsoleColor.DarkGray;
    Console.WriteLine("  :will just merge your input files");
    Console.ResetColor();
    Console.Write("🛸 ");
    Console.ForegroundColor = ConsoleColor.DarkYellow;
    Console.Write($"‹0› {OptionEnums.ElonMod} ");
    Console.ForegroundColor = ConsoleColor.DarkGray;
    Console.WriteLine(" :will process 1-2-3-4-5-6 options");
    Console.ResetColor();
    Console.WriteLine();
    #endregion

    List<OptionEnums> options = new();
    while (options.Count == 0)
    {
        Console.Write("\r🤖 ");
        Console.ForegroundColor = ConsoleColor.DarkMagenta;
        Console.Write("Choose Option(s) Using Option Number (split by comma) [leave empty to return]: ");
        Console.ResetColor();
        var optionsInput = Console.ReadLine();
        optionsInput = Regex.Replace(optionsInput!, @"[\s\t]", string.Empty);

        if (optionsInput!.Equals("*") || string.IsNullOrEmpty(optionsInput.Trim()))
            break;
        else if (Regex.IsMatch(optionsInput.Trim(), @"^\d$") && byte.TryParse(optionsInput.Trim(), out byte parsedOption) && (parsedOption >= 0 && parsedOption <= 9))
        {
            options.Add((OptionEnums)parsedOption);
        }
        else if (Regex.IsMatch(optionsInput.Trim(), @"\d,"))
        {
            var parsedOptions = optionsInput.Trim().Split(",").Distinct().ToList();
            if(!parsedOptions.Any(x=> !byte.TryParse(x.Trim(), out byte opt) || (opt <= 0 || opt >= 9)))
            {
                options.AddRange(parsedOptions.Where(x => byte.TryParse(x.Trim(), out _)).Select(x => (OptionEnums)byte.Parse(x.Trim())).Where(x => (byte)x >= 0 && (byte)x <= 9).ToList());
            }
        }

        if (options.Count == 0)
        {
            var currentPosition = Console.GetCursorPosition();
            Console.SetCursorPosition(0, (currentPosition.Top - 1));
            Console.Write(new string(' ', Console.WindowWidth));
            Console.Write("\r💤 ");
            Console.ForegroundColor = ConsoleColor.DarkRed;
            Console.Write("You Entered Invalid Option Number(s) Bro!!! Press Enter To Retry...");
            Console.ResetColor();

            Console.ReadKey();
            continue;
        }
        break;
    }
    if (options.Count == 0)
        continue;

    Console.Clear();
    List<string> titles = new();
    List<string>? AscendedLines = null;
    List<string>? DescendedLines = null;
    if (options.Any(x => x == OptionEnums.ElonMod))
    {
        titles.Add(OptionEnums.ElonMod.ToString());
        bool nullfixDone = false;
        Task NullFix = Task.Run(async () =>
        {
            byte position = 0;

            while (!nullfixDone)
            {
                await PrintWaitingContent("Empty Lines Deleting", position);

                if (position == 7)
                {
                    position = 0;
                    continue;
                }
                position++;
            }
        });
        await repo.NullFixHandler();
        nullfixDone = true;
        Console.Write(new string(' ', Console.WindowWidth));
        Console.WriteLine($"\r✅ {emptyLinesCount} Empty Line(s) Deleted Successfully.");

        bool deDupDone = false;
        Task DeDup = Task.Run(async () =>
        {
            byte position = 0;

            while (!deDupDone)
            {
                await PrintWaitingContent("Duplicate Lines(s) & IP(s) Deleting", position);

                if (position == 7)
                {
                    position = 0;
                    continue;
                }
                position++;
            }
        });
        await repo.DeDupHandler();
        deDupDone = true;
        Console.Write(new string(' ', Console.WindowWidth));
        Console.WriteLine($"\r✅ {duplicatedLinesCount} Duplicated Line(s) & {duplicatedIPsCount} Duplicated IP(s) Deleted Successfully.");

        bool inFixDone = false;
        Task InFix = Task.Run(async () =>
        {
            byte position = 0;

            while (!inFixDone)
            {
                await PrintWaitingContent("Invalid IP(s) Deleting", position);

                if (position == 7)
                {
                    position = 0;
                    continue;
                }
                position++;
            }
        });
        await repo.InFixHandler();
        inFixDone = true;
        Console.Write(new string(' ', Console.WindowWidth));
        Console.WriteLine($"\r✅ {invalidIPsCount} Invalid IP(s) Deleted Successfully.");

        bool letOutDone = false;
        Task LetOut = Task.Run(async () =>
        {
            byte position = 0;

            while (!letOutDone)
            {
                await PrintWaitingContent("Lines Cleaning Up", position);

                if (position == 7)
                {
                    position = 0;
                    continue;
                }
                position++;
            }
        });
        await repo.LetOutHandler();
        letOutDone = true;
        Console.Write(new string(' ', Console.WindowWidth));
        Console.WriteLine($"\r✅ {letteredLinesCount} Line(s) Cleaned Up Successfully.");

        bool dePortDone = false;
        Task DePort = Task.Run(async () =>
        {
            byte position = 0;

            while (!dePortDone)
            {
                await PrintWaitingContent("Ports Deleting", position);

                if (position == 7)
                {
                    position = 0;
                    continue;
                }
                position++;
            }
        });
        await repo.DePortHandler();
        dePortDone = true;
        Console.Write(new string(' ', Console.WindowWidth));
        Console.WriteLine($"\r✅ {portedLinesCount} Port(s) Deleted Successfully.");

        bool sortUPDone = false;
        Task SortUP = Task.Run(async () =>
        {
            byte position = 0;

            while (!sortUPDone)
            {
                await PrintWaitingContent("Ascending Sorting", position);

                if (position == 7)
                {
                    position = 0;
                    continue;
                }
                position++;
            }
        });
        AscendedLines = new();
        AscendedLines = repo.OrderByAscending;
        sortUPDone = true;
        Console.Write(new string(' ', Console.WindowWidth));
        Console.WriteLine($"\r✅ Ascending Sort Operation Processed Successfully.");
    }
    else if (options.Any(x => x == OptionEnums.Merger))
    {
        titles.Add(OptionEnums.Merger.ToString());
        Console.Write(new string(' ', Console.WindowWidth));
        Console.WriteLine($"\r✅ {srcPaths.Count} File(s) Merged Successfully.");
    }
    else
    {
        bool del4Done = false;
        if (options.Any(x => x == OptionEnums.Del4))
        {
            titles.Add(OptionEnums.Del4.ToString());
            Task Del4 = Task.Run(async () =>
            {
                byte position = 0;

                while (!del4Done)
                {
                    await PrintWaitingContent("All IPv4s Deleting", position);

                    if (position == 7)
                    {
                        position = 0;
                        continue;
                    }
                    position++;
                }
            });

            await repo.Del4Handler();

            del4Done = true;
            Console.Write(new string(' ', Console.WindowWidth));
            Console.WriteLine($"\r✅ {totalIPsCount} IPv4(s) Deleted Successfully.");
        }

        bool nullfixDone = false;
        if (options.Any(x => x == OptionEnums.NullFix))
        {
            titles.Add(OptionEnums.NullFix.ToString());
            Task NullFix = Task.Run(async () =>
            {
                byte position = 0;

                while (!nullfixDone)
                {
                    await PrintWaitingContent("Empty Lines Deleting", position);

                    if (position == 7)
                    {
                        position = 0;
                        continue;
                    }
                    position++;
                }
            });

            await repo.NullFixHandler();

            nullfixDone = true;
            Console.Write(new string(' ', Console.WindowWidth));
            Console.WriteLine($"\r✅ {emptyLinesCount} Empty Line(s) Deleted Successfully.");
        }

        bool deDupDone = false;
        if (options.Any(x => x == OptionEnums.DeDup))
        {
            titles.Add(OptionEnums.DeDup.ToString());
            Task DeDup = Task.Run(async () =>
            {
                byte position = 0;

                while (!deDupDone)
                {
                    await PrintWaitingContent("Duplicate Lines(s) & IP(s) Deleting", position);

                    if (position == 7)
                    {
                        position = 0;
                        continue;
                    }
                    position++;
                }
            });

            await repo.DeDupHandler();

            deDupDone = true;
            Console.Write(new string(' ', Console.WindowWidth));
            Console.WriteLine($"\r✅ {duplicatedLinesCount} Duplicated Line(s) & {duplicatedIPsCount} Duplicated IP(s) Deleted Successfully.");
        }

        bool inFixDone = false;
        if (options.Any(x => x == OptionEnums.InFix))
        {
            titles.Add(OptionEnums.InFix.ToString());
            Task InFix = Task.Run(async () =>
            {
                byte position = 0;

                while (!inFixDone)
                {
                    await PrintWaitingContent("Invalid IP(s) Deleting", position);

                    if (position == 7)
                    {
                        position = 0;
                        continue;
                    }
                    position++;
                }
            });

            await repo.InFixHandler();

            inFixDone = true;
            Console.Write(new string(' ', Console.WindowWidth));
            Console.WriteLine($"\r✅ {invalidIPsCount} Invalid IP(s) Deleted Successfully.");
        }

        bool letOutDone = false;
        if (options.Any(x => x == OptionEnums.LetOut))
        {
            titles.Add(OptionEnums.LetOut.ToString());
            Task LetOut = Task.Run(async () =>
            {
                byte position = 0;

                while (!letOutDone)
                {
                    await PrintWaitingContent("Lines Cleaning Up", position);

                    if (position == 7)
                    {
                        position = 0;
                        continue;
                    }
                    position++;
                }
            });

            await repo.LetOutHandler();

            letOutDone = true;
            Console.Write(new string(' ', Console.WindowWidth));
            Console.WriteLine($"\r✅ {letteredLinesCount} Line(s) Cleaned Up Successfully.");
        }

        bool dePortDone = false;
        if (options.Any(x => x == OptionEnums.DePort))
        {
            titles.Add(OptionEnums.DePort.ToString());
            Task DePort = Task.Run(async () =>
            {
                byte position = 0;

                while (!dePortDone)
                {
                    await PrintWaitingContent("Ports Deleting", position);

                    if (position == 7)
                    {
                        position = 0;
                        continue;
                    }
                    position++;
                }
            });

            await repo.DePortHandler();

            dePortDone = true;
            Console.Write(new string(' ', Console.WindowWidth));
            Console.WriteLine($"\r✅ {portedLinesCount} Port(s) Deleted Successfully.");
        }

        bool sortUpDone = false;
        if (options.Any(x => x == OptionEnums.SortUp))
        {
            titles.Add(OptionEnums.SortUp.ToString());
            Task SortUp = Task.Run(async () =>
            {
                byte position = 0;

                while (!sortUpDone)
                {
                    await PrintWaitingContent("Ascending Sorting", position);

                    if (position == 7)
                    {
                        position = 0;
                        continue;
                    }
                    position++;
                }
            });

            AscendedLines = new();
            AscendedLines = repo.OrderByAscending;

            sortUpDone = true;
            Console.Write(new string(' ', Console.WindowWidth));
            Console.WriteLine($"\r✅ Ascending Sort Operation Processed Successfully.");
        }

        bool sortDownDone = false;
        if (options.Any(x => x == OptionEnums.SortDown))
        {
            titles.Add(OptionEnums.SortDown.ToString());
            Task SortDown = Task.Run(async () =>
            {
                byte position = 0;

                while (!sortDownDone)
                {
                    await PrintWaitingContent("Descending Sorting", position);

                    if (position == 7)
                    {
                        position = 0;
                        continue;
                    }
                    position++;
                }
            });

            DescendedLines = new();
            DescendedLines = repo.OrderByDescending;

            sortDownDone = true;
            Console.Write(new string(' ', Console.WindowWidth));
            Console.WriteLine($"\r✅ Descending Sort Operation Processed Successfully.");
        }
    }

    Console.WriteLine();
    string? targetFolder = await DefineResultFolder(options);
    string destPath = Path.Combine(resultPath, targetFolder!, $"IP File Normalizer_{DateTime.UtcNow.ToString("MM-dd-yyyy_hh-ss-mm")!}_{string.Join('-', titles)}.txt");
    if (AscendedLines == null && DescendedLines == null)
    {
        await using (StreamWriter sw = new(path: destPath, encoding: new UTF8Encoding(), options: new FileStreamOptions() { Access = FileAccess.ReadWrite, Mode = FileMode.Create, Options = FileOptions.Asynchronous }))
        {
            if (AscendedLines == null && DescendedLines == null)
            {
                var lines = repo.GetAllToWrite;
                for (int i = 0; i < lines.Count; i++)
                {
                    if (i < lines.Count - 1)
                    {
                        await sw.WriteLineAsync($"{lines[i]}");
                        continue;
                    }

                    await sw.WriteAsync($"{lines[i]}");
                }
            }
        }
    }
    if (AscendedLines != null)
    {
        await using (StreamWriter sw = new(path: destPath, encoding: new UTF8Encoding(), options: new FileStreamOptions() { Access = FileAccess.ReadWrite, Mode = FileMode.Create, Options = FileOptions.Asynchronous }))
        {
            for (int i = 0; i < AscendedLines.Count; i++)
            {
                if (i < AscendedLines.Count - 1)
                {
                    await sw.WriteLineAsync($"{AscendedLines[i]}");
                    continue;
                }

                await sw.WriteAsync($"{AscendedLines[i]}");
            }
        }
    }
    if (DescendedLines != null)
    {
        await using (StreamWriter sw = new(path: destPath, encoding: new UTF8Encoding(), options: new FileStreamOptions() { Access = FileAccess.ReadWrite, Mode = FileMode.Create, Options = FileOptions.Asynchronous }))
        {
            for (int i = 0; i < DescendedLines.Count; i++)
            {
                if (i < DescendedLines.Count - 1)
                {
                    await sw.WriteLineAsync($"{DescendedLines[i]}");
                    continue;
                }

                await sw.WriteAsync($"{DescendedLines[i]}");
            }
        }
    }

    #region Print Result
    Console.Write("📂 ");
    Console.ForegroundColor = ConsoleColor.DarkGreen;
    Console.WriteLine("Operation Result Saved at:");
    Console.ForegroundColor = ConsoleColor.DarkGray;
    Console.WriteLine($"{destPath}");
    Console.ResetColor();
    Console.Write("📋 ");
    Console.ForegroundColor = ConsoleColor.DarkYellow;
    Console.WriteLine("Copy To Do More...");
    Console.ResetColor();
    Console.WriteLine();
    Console.Write("🛠️ ");
    Console.ForegroundColor = ConsoleColor.DarkCyan;
    Console.Write("Press Any Key To Continue or E for Exit");
    Console.ResetColor();
    
    var endOption = Console.ReadKey();
    if (endOption.Key == ConsoleKey.E)
        return;
    continue;
    #endregion
}











//methods
async Task<string?> RegexIP(string? txt)
{
    if (string.IsNullOrEmpty(txt) || string.IsNullOrWhiteSpace(txt)) return txt;

    string ipPattern = @"(\d+\.\d+\.\d+\.\d+)";
    return Regex.Match(txt.Trim(), ipPattern).Groups[1].Value;
}
async Task<string?> RegexPort(string? txt)
{
    if (string.IsNullOrEmpty(txt) || string.IsNullOrWhiteSpace(txt)) return txt;

    string ipPattern = @"\d+\.\d+\.\d+\.\d+:(\d+)";
    return Regex.Match(txt.Trim(), ipPattern).Groups[1].Value;
}
async Task<List<string>> RegexLetters(string? txt)
{
    if (string.IsNullOrEmpty(txt) || string.IsNullOrWhiteSpace(txt)) return new List<string>();

    string ipWithLetterPattern = @"^\d+\.\d+\.\d+\.\d+(:\d+|)$";
    if (Regex.Match(txt, ipWithLetterPattern).Success) return new List<string>();

    List<string> result = new();
    var hasIP = await RegexIP(txt);
    if (hasIP != "")
    {
        var letters = Regex.Match(txt, @"(.+|)\d+\.\d+\.\d+\.\d+(:\d+|)(.+|)");
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
async Task<byte[]?> RegexBytes(string? txt)
{
    if (string.IsNullOrEmpty(txt) || string.IsNullOrWhiteSpace(txt)) return null;

    var hasIP = await RegexIP(txt);
    if (hasIP == "") return null;

    if (!IPAddress.TryParse(hasIP, out var ip)) return null;

    return ip.GetAddressBytes();
}

async Task CreateDirectories(string resultPath)
{
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
async Task<string> DefineResultFolder(List<OptionEnums> options)
{
    if (options.Any(x => x == OptionEnums.ElonMod)) return "1. ElonMod";
    else if (options.Any(y => y == OptionEnums.Merger)) return "2. Merger";
    else if (options.Count > 1) return "3. MultiMod";
    else
    {
        switch (options[0])
        {
            case OptionEnums.NullFix: return "NullFix";
            case OptionEnums.DeDup: return "DeDup";
            case OptionEnums.InFix: return "InFix";
            case OptionEnums.LetOut: return "LetOut";
            case OptionEnums.DePort: return "DePort";
            case OptionEnums.SortUp: return "SortUp";
            case OptionEnums.SortDown: return "SortDown";
            case OptionEnums.Del4: return "Del4";
            default: return null!;
        }
    }
}
async Task PrintWaitingContent(string text, byte position)
{
    switch (position)
    {
        case 0:
            {
                Console.ForegroundColor = ConsoleColor.DarkYellow;
                Console.Write("| ");
                Console.ForegroundColor = ConsoleColor.DarkGray;
                Console.Write($"{text}...\r");
                Console.ResetColor();

                break;
            }

        case 1:
            {
                Console.ForegroundColor = ConsoleColor.DarkYellow;
                Console.Write("/ ");
                Console.ForegroundColor = ConsoleColor.DarkGray;
                Console.Write($"{text}...\r");
                Console.ResetColor();

                break;
            }

        case 2:
            {
                Console.ForegroundColor = ConsoleColor.DarkYellow;
                Console.Write("— ");
                Console.ForegroundColor = ConsoleColor.DarkGray;
                Console.Write($"{text}...\r");
                Console.ResetColor();

                break;
            }

        case 3:
            {
                Console.ForegroundColor = ConsoleColor.DarkYellow;
                Console.Write("\\ ");
                Console.ForegroundColor = ConsoleColor.DarkGray;
                Console.Write($"{text}...\r");
                Console.ResetColor();

                break;
            }

        case 4:
            {
                Console.ForegroundColor = ConsoleColor.DarkYellow;
                Console.Write("| ");
                Console.ForegroundColor = ConsoleColor.DarkGray;
                Console.Write($"{text}...\r");
                Console.ResetColor();

                break;
            }

        case 5:
            {
                Console.ForegroundColor = ConsoleColor.DarkYellow;
                Console.Write("/ ");
                Console.ForegroundColor = ConsoleColor.DarkGray;
                Console.Write($"{text}...\r");
                Console.ResetColor();

                break;
            }

        case 6:
            {
                Console.ForegroundColor = ConsoleColor.DarkYellow;
                Console.Write("— ");
                Console.ForegroundColor = ConsoleColor.DarkGray;
                Console.Write($"{text}...\r");
                Console.ResetColor();

                break;
            }

        case 7:
            {
                Console.ForegroundColor = ConsoleColor.DarkYellow;
                Console.Write("\\ ");
                Console.ForegroundColor = ConsoleColor.DarkGray;
                Console.Write($"{text}...\r");
                Console.ResetColor();

                break;
            }
    }

    await Task.Delay(TimeSpan.FromMilliseconds(100));
}
void PrintCollectedData(uint totalLines, uint emptyLinesCount, uint dupLinesCount, uint linesWithLetters, uint linesWithPort, List<string> ports,
    uint totalIPsCount, uint invalidIPsCount, uint dupIPsCount, uint totalIPv4Count)
{
    Console.Write("☠️ ");
    Console.ForegroundColor = ConsoleColor.DarkCyan;
    Console.Write("Total Lines: ");
    Console.ResetColor();
    Console.WriteLine($"{totalLines.ToString("N0")}");
    Console.Write("\U0001f923 ");
    Console.ForegroundColor = ConsoleColor.DarkCyan;
    Console.Write("Empty Lines: ");
    Console.ResetColor();
    Console.WriteLine($"{emptyLinesCount.ToString("N0")}");
    Console.Write("💩 ");
    Console.ForegroundColor = ConsoleColor.DarkCyan;
    Console.Write("Duplicate Lines: ");
    Console.ResetColor();
    Console.WriteLine($"{dupLinesCount.ToString("N0")}");
    Console.Write("👎 ");
    Console.ForegroundColor = ConsoleColor.DarkCyan;
    Console.Write("Lines Containing Letters, Space, False IP Syntax, etc: ");
    Console.ResetColor();
    Console.WriteLine($"{linesWithLetters.ToString("N0")}");
    Console.Write("👙 ");
    Console.ForegroundColor = ConsoleColor.DarkCyan;
    Console.Write("Lines Containing Port: ");
    Console.ResetColor();
    Console.WriteLine($"{linesWithPort.ToString("N0")}");
    Console.Write("📜 ");
    Console.ForegroundColor = ConsoleColor.DarkCyan;
    Console.Write("Used Ports: ");
    Console.ResetColor();
    Console.WriteLine($"{((ports.Count > 0) ? string.Join(" - ", ports) : "Null")}");
    Console.Write("🥵 ");
    Console.ForegroundColor = ConsoleColor.DarkCyan;
    Console.Write("Total IPs: ");
    Console.ResetColor();
    Console.WriteLine($"{totalIPsCount.ToString("N0")}");
    Console.Write("👏 ");
    Console.ForegroundColor = ConsoleColor.DarkCyan;
    Console.Write("Invalid IPs: ");
    Console.ResetColor();
    Console.WriteLine($"{invalidIPsCount.ToString("N0")}");
    Console.Write("🍌 ");
    Console.ForegroundColor = ConsoleColor.DarkCyan;
    Console.Write("Duplicate IPs: ");
    Console.ResetColor();
    Console.WriteLine($"{dupIPsCount.ToString("N0")}");
    Console.Write("💎 ");
    Console.ForegroundColor = ConsoleColor.DarkCyan;
    Console.Write("Available Usable IPv4s: ");
    Console.ResetColor();
    Console.Write($"{totalIPv4Count.ToString("N0")} ");
    if (totalIPv4Count > 0)
    {
        Console.Write("\n☢️ ");
        Console.ForegroundColor = ConsoleColor.DarkRed;
        Console.WriteLine("maybe needs some cleanup to reach!");
        Console.ResetColor();
    }
}