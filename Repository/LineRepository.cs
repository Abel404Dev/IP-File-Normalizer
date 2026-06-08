using IP_File_Normalizer.Model;

namespace IP_File_Normalizer.Repository
{
    public class LineRepository
    {
        //fields
        private ConcurrentDictionary<uint, LineModel> _lines = new();

        //indexers
        public string this[uint index]
        {
            get => $"{((_lines[index].IP != string.Empty) ? _lines[index].IP : null)}{((_lines[index].Port != string.Empty) ? $":{_lines[index].IP}" : null)}{((_lines[index].Letters.Count > 0) ? $"{string.Join(string.Empty, _lines[index].Letters)}" : null)}";
        }

        //properties
        public uint TotalLines
        {
            get => (uint)_lines.Count;
        }
        public uint EmptyLines
        {
            get => (uint)_lines.Where(x => string.IsNullOrEmpty(x.Value.Base) || string.IsNullOrWhiteSpace(x.Value.Base)).Count();
        }
        public uint DuplicatedLines
        {
            get => (uint)_lines.GroupBy(x => x.Value.Base).Where(g => g.Count() > 1).Select(g => g.Key).Count();
        }
        public uint LetteredLines
        {
            get => (uint)_lines.Where(x => x.Value.Letters.Count > 0).Count();
        }
        public uint PortedLines
        {
            get => (uint)_lines.Where(x => !string.IsNullOrEmpty(x.Value.Port)).Count();
        }
        public List<string>? UsedPorts
        {
            get => _lines.Where(x => !string.IsNullOrEmpty(x.Value.Base) && !string.IsNullOrEmpty(x.Value.Port)).Select(x => x.Value.Port).Distinct().ToList()!;
        }
        public uint TotalIPs
        {
            get => (uint)_lines.Where(x => !string.IsNullOrEmpty(x.Value.Base) && !string.IsNullOrEmpty(x.Value.IP)).Select(x => x.Value.IP).Count();
        }
        public uint InvalidIPs
        {
            get => (uint)_lines.Where(x => !string.IsNullOrEmpty(x.Value.Base) && !string.IsNullOrEmpty(x.Value.IP)).Select(x => x.Value.IP).Where(x => !IPAddress.TryParse(x, out _)).ToList().Count;
        }
        public uint DuplicatedIPs
        {
            get => (uint)_lines.Where(x => !string.IsNullOrEmpty(x.Value.Base) && !string.IsNullOrEmpty(x.Value.IP)).GroupBy(x => x.Value.IP).Where(g => g.Count() > 1).Select(g => g.Key).Count();
        }
        public uint ValidIPs
        {
            get => (uint)_lines.Where(x => !string.IsNullOrEmpty(x.Value.Base) && !string.IsNullOrEmpty(x.Value.IP)).Select(x => x.Value.IP).Distinct().Where(x => IPAddress.TryParse(x, out _)).ToList().Count;
        }
        public List<string> OrderByDescending
        {
            get
            {
                var result = new List<string>();

                var nonIpLines = _lines.Where(x => !string.IsNullOrEmpty(x.Value.Base) && string.IsNullOrEmpty(x.Value.IP)).ToList();
                foreach (var line in nonIpLines)
                {
                    _lines.TryRemove(line.Key, out _);
                }
                var nonIpsToWrite = nonIpLines.Select(kvp => new LineModel()
                {
                    Base = kvp.Value.Base,
                    IP = null,
                    Port = kvp.Value.Port,
                    Letters = kvp.Value.Letters,
                    Bytes = null
                }).ToList();

                var ipLines = _lines
                    .Select(kvp => new LineModel()
                    {
                        Base = kvp.Value.Base,
                        IP = kvp.Value.IP,
                        Port = kvp.Value.Port,
                        Letters = kvp.Value.Letters,
                        Bytes = kvp.Value.Bytes
                    })
                    .OrderByDescending(x => x.Bytes![0])
                    .ThenByDescending(x => x.Bytes![1])
                    .ThenByDescending(x => x.Bytes![2])
                    .ThenByDescending(x => x.Bytes![3])
                    .ToList();
                foreach (var kvp in ipLines)
                {
                    string? firstLetters = (kvp.Letters.Count > 0 && kvp.Letters.Any(x => x.StartsWith("f:"))) ? kvp.Letters.Single(x => x.StartsWith("f:")).Remove(0, 2) : null;
                    string? endLetters = (kvp.Letters.Count > 0 && kvp.Letters.Any(x => x.StartsWith("e:"))) ? kvp.Letters.Single(x => x.StartsWith("e:")).Remove(0, 2) : null;
                    string? ip = (kvp.IP != string.Empty) ? kvp.IP : null;
                    string? port = (kvp.Port != string.Empty) ? $":{kvp.Port}" : null;

                    result.Add($"{firstLetters}{ip}{port}{endLetters}");
                }

                
                foreach (var kvp in nonIpsToWrite)
                {
                    string? firstLetters = (kvp.Letters.Count > 0 && kvp.Letters.Any(x => x.StartsWith("f:"))) ? kvp.Letters.Single(x => x.StartsWith("f:")).Remove(0, 2) : null;
                    string? endLetters = (kvp.Letters.Count > 0 && kvp.Letters.Any(x => x.StartsWith("e:"))) ? kvp.Letters.Single(x => x.StartsWith("e:")).Remove(0, 2) : null;
                    string? port = (kvp.Port != string.Empty) ? $":{kvp.Port}" : null;

                    result.Add($"{firstLetters}{port}{endLetters}");
                }

                return result;
            }
        }
        public List<string> OrderByAscending
        {
            get
            {
                var result = new List<string>();

                var nonIpLines = _lines.Where(x => !string.IsNullOrEmpty(x.Value.Base) && string.IsNullOrEmpty(x.Value.IP)).ToList();
                foreach (var line in nonIpLines)
                {
                    _lines.TryRemove(line.Key, out _);
                }
                var nonIpsToWrite = nonIpLines.Select(kvp => new LineModel()
                {
                    Base = kvp.Value.Base,
                    IP = null,
                    Port = kvp.Value.Port,
                    Letters = kvp.Value.Letters,
                    Bytes = null
                }).ToList();
                foreach (var kvp in nonIpsToWrite)
                {
                    string? firstLetters = (kvp.Letters.Count > 0 && kvp.Letters.Any(x => x.StartsWith("f:"))) ? kvp.Letters.Single(x => x.StartsWith("f:")).Remove(0, 2) : null;
                    string? endLetters = (kvp.Letters.Count > 0 && kvp.Letters.Any(x => x.StartsWith("e:"))) ? kvp.Letters.Single(x => x.StartsWith("e:")).Remove(0, 2) : null;
                    string? port = (kvp.Port != string.Empty) ? $":{kvp.Port}" : null;

                    result.Add($"{firstLetters}{port}{endLetters}");
                }

                var ipLines = _lines
                    .Select(kvp => new LineModel()
                    {
                        Base = kvp.Value.Base,
                        IP = kvp.Value.IP,
                        Port = kvp.Value.Port,
                        Letters = kvp.Value.Letters,
                        Bytes = kvp.Value.Bytes
                    })
                    .OrderBy(x => x.Bytes![0])
                    .ThenBy(x => x.Bytes![1])
                    .ThenBy(x => x.Bytes![2])
                    .ThenBy(x => x.Bytes![3])
                    .ToList();
                foreach (var kvp in ipLines)
                {
                    string? firstLetters = (kvp.Letters.Count > 0 && kvp.Letters.Any(x => x.StartsWith("f:"))) ? kvp.Letters.Single(x => x.StartsWith("f:")).Remove(0, 2) : null;
                    string? endLetters = (kvp.Letters.Count > 0 && kvp.Letters.Any(x => x.StartsWith("e:"))) ? kvp.Letters.Single(x => x.StartsWith("e:")).Remove(0, 2) : null;
                    string? ip = (kvp.IP != string.Empty) ? kvp.IP : null;
                    string? port = (kvp.Port != string.Empty) ? $":{kvp.Port}" : null;

                    result.Add($"{firstLetters}{ip}{port}{endLetters}");
                }

                return result;
            }
        }
        public List<string> GetAllToWrite
        {
            get
            {
                var list = new List<string>();
                foreach (var kvp in _lines)
                {
                    string? firstLetters = (kvp.Value.Letters.Count > 0 && kvp.Value.Letters.Any(x => x.StartsWith("f:"))) ? kvp.Value.Letters.Single(x => x.StartsWith("f:")).Remove(0, 2) : null;
                    string? endLetters = (kvp.Value.Letters.Count > 0 && kvp.Value.Letters.Any(x => x.StartsWith("e:"))) ? kvp.Value.Letters.Single(x => x.StartsWith("e:")).Remove(0, 2) : null;
                    string? ip = (kvp.Value.IP != string.Empty) ? kvp.Value.IP : null;
                    string? port = (kvp.Value.Port != string.Empty) ? $":{kvp.Value.Port}" : null;

                    list.Add($"{firstLetters}{ip}{port}{endLetters}");
                }
                return list;
            }
        }

        //methods
        public bool Add(uint index, LineModel ln)
        {
            if (_lines.TryAdd(index, ln)) return true;
            else return false;
        }
        public bool Add(uint index, string? @base, string? ip, string? port, List<string> letters, byte[]? bytes)
        {
            LineModel ln = new()
            {
                Base = @base,
                IP = ip,
                Port = port,
                Letters = letters,
                Bytes = bytes
            };
            return Add(index, ln);
        }
        public bool KeyExist(uint i)
        {
            return _lines.ContainsKey(i);
        }

        public async Task NullFixHandler()
        {
            var nulls = _lines.Where(x => x.Value.Base == "" || x.Value.Base == string.Empty).ToList();

            for (int i = 0; i < nulls.Count; i++)
            {
                _lines.TryRemove(nulls[i].Key, out _);
            }
        }
        public async Task DeDupHandler()
        {
            var keys = _lines.Keys.ToArray();

            await DedupByBaseAsync(keys);

            await DedupByIPAsync();
        }
        public async Task InFixHandler()
        {
            var invalidIPsBase = _lines.Where(x => !string.IsNullOrEmpty(x.Value.Base) && !string.IsNullOrEmpty(x.Value.IP)).Where(x => !IPAddress.TryParse(x.Value.IP, out _)).ToList();
            for (int i = 0; i < invalidIPsBase.Count; i++)
            {
                if (_lines[invalidIPsBase[i].Key].Port == string.Empty && _lines[invalidIPsBase[i].Key].Letters.Count == 0)
                    _lines.TryRemove(invalidIPsBase[i].Key, out _);
                else
                {
                    _lines[invalidIPsBase[i].Key].Base!.Replace(_lines[invalidIPsBase[i].Key].IP!, string.Empty);

                    _lines[invalidIPsBase[i].Key].IP = string.Empty;
                    _lines[invalidIPsBase[i].Key].Bytes = null;
                }
            }
        }
        public async Task LetOutHandler()
        {
            var letteredBase = _lines.Where(x => x.Value.Letters.Count > 0).ToList();
            for (int i = 0; i < letteredBase.Count; i++)
            {
                if (_lines[letteredBase[i].Key].Port == string.Empty && _lines[letteredBase[i].Key].IP == string.Empty)
                    _lines.TryRemove(letteredBase[i].Key, out _);

                else
                {
                    foreach (var lt in _lines[letteredBase[i].Key].Letters)
                    {
                        _lines[letteredBase[i].Key].Base!.Replace(lt.Remove(0, 2), string.Empty);
                    }


                    _lines[letteredBase[i].Key].Letters.Clear();
                }
            }
        }
        public async Task DePortHandler()
        {
            var portedBase = _lines.Where(x => !string.IsNullOrEmpty(x.Value.Base) && !string.IsNullOrEmpty(x.Value.Port)).ToList();
            for (int i = 0; i < portedBase.Count; i++)
            {
                if (_lines[portedBase[i].Key].IP == string.Empty && _lines[portedBase[i].Key].Letters.Count == 0)
                    _lines.TryRemove(portedBase[i].Key, out _);

                else
                {
                    _lines[portedBase[i].Key].Base!.Replace(_lines[portedBase[i].Key].Port!, string.Empty);

                    _lines[portedBase[i].Key].Port = string.Empty;
                }
            }
        }
        public async Task Del4Handler()
        {
            var del4Base = _lines.Where(x => !string.IsNullOrEmpty(x.Value.Base) && !string.IsNullOrEmpty(x.Value.IP)).ToList();
            for (int i = 0; i < del4Base.Count; i++)
            {
                if (_lines[del4Base[i].Key].Port == string.Empty && _lines[del4Base[i].Key].Letters.Count == 0)
                    _lines.TryRemove(del4Base[i].Key, out _);
                else
                {
                    _lines[del4Base[i].Key].Base!.Replace(_lines[del4Base[i].Key].IP!, string.Empty);

                    _lines[del4Base[i].Key].IP = string.Empty;
                    _lines[del4Base[i].Key].Bytes = null;
                }
            }
        }

        private async Task DedupByBaseAsync(uint[] keys)
        {
            var seenBase = new ConcurrentDictionary<string, uint>();

            await Parallel.ForEachAsync(keys, new ParallelOptions { MaxDegreeOfParallelism = Environment.ProcessorCount },
                async (key, ct) =>
                {
                    if (!_lines.TryGetValue(key, out var item) || string.IsNullOrEmpty(item.Base))
                        return;

                    var baseValue = item.Base;


                    if (seenBase.TryAdd(baseValue, key))
                        return;


                    if (seenBase[baseValue] != key)
                    {
                        _lines.TryRemove(key, out _);
                    }
                });
        }
        private async Task DedupByIPAsync()
        {
            var seenIP = new ConcurrentDictionary<string, uint>();

            var keys = _lines.Keys.ToArray();

            await Parallel.ForEachAsync(keys, new ParallelOptions { MaxDegreeOfParallelism = Environment.ProcessorCount },
                async (key, ct) =>
                {
                    if (!_lines.TryGetValue(key, out var item))
                        return;

                    if (string.IsNullOrEmpty(item.Base) || string.IsNullOrEmpty(item.IP))
                        return;

                    var ip = item.IP;

                    if (seenIP.TryAdd(ip, key))
                        return;


                    if (seenIP[ip] == key)
                        return;


                    if (string.IsNullOrEmpty(item.Port) && (item.Letters?.Count ?? 0) == 0)
                    {
                        _lines.TryRemove(key, out _);
                    }
                    else
                    {

                        if (!string.IsNullOrEmpty(item.Base) && !string.IsNullOrEmpty(item.IP))
                        {
                            item.Base = item.Base.Replace(item.IP, string.Empty).Trim();
                        }

                        item.IP = string.Empty;
                        item.Bytes = null;
                    }
                });
        }
    }
}
