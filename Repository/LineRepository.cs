namespace IP_File_Normalizer.Repository
{
    public class LineRepository : IDisposable
    {
        //fields
        private ConcurrentDictionary<uint, LineModel>? _lines = new();
        private uint _emptyLines = 0;
        private uint _duplicatedLines = 0;
        private uint _linesWithLetters = 0;
        private uint _linesWithPort = 0;
        private List<string> _usedPorts = new();
        private uint _totalIPs = 0;
        private uint _invalidIPs = 0;
        private uint _duplicatedIPs = 0;
        private uint _validIPs = 0;


        //indexers
        public LineModel? this[uint index]
        {
            get
            {
                if (_lines!.ContainsKey(index))
                    return _lines[index];
                return null;
            }
        }


        //properties
        public uint FreshTotalLines => (uint)_lines!.Count;
        public uint EmptyLines => _emptyLines;
        public uint DuplicatedLines => _duplicatedLines;
        public uint LinesWithLetters => _linesWithLetters;
        public uint LinesWithPort => _linesWithPort;
        public List<string> UsedPorts => _usedPorts.Distinct().ToList();
        public uint TotalIPs => _totalIPs;
        public uint InvalidIPs => _invalidIPs;
        public uint DuplicatedIPs => _duplicatedIPs;
        public uint ValidIPs => _validIPs;


        //methods
        private async Task<bool> RecomputeStatisticsAsync(string? @base, string? ip, string? port, List<string>? letters)
        {
            if (@base == null)
                this._emptyLines++;
            else
            {
                if (letters != null)
                    _linesWithLetters++;
                if (port != null)
                {
                    this._linesWithPort++;
                    this._usedPorts.Add(port);
                }
                if (ip != null)
                    this._totalIPs++;
            }

            return true;
        }
        public async Task<bool> RecomputeHeavyStatisticsAsync()
        {
            _duplicatedLines = (uint)_lines!.Where(x => x.Value.Base != null).GroupBy(x => x.Value.Base).Count(g => g.Count() > 1);
            _duplicatedIPs = (uint)_lines!.Where(x => x.Value.IP != null).GroupBy(x => x.Value.IP).Count(g => g.Count() > 1);
            _invalidIPs = (uint)_lines!.Where(x => x.Value.IP != null).Select(x => x.Value.IP).Distinct().Count(x => x != null && !IPAddress.TryParse(x, out _));
            _validIPs = (uint)_lines!.Where(x => x.Value.IP != null).Select(x => x.Value.IP).Distinct().Count(x => x != null && IPAddress.TryParse(x, out _));
            return true;
        }
        public async Task<bool> AddAsync(uint index, LineModel ln)
        {
            if (_lines!.TryAdd(index, ln)) return true;
            else return false;
        }
        public async Task<bool> AddAsync(uint index, string? @base = null, string? ip = null, string? port = null, List<string>? letters = null, byte[]? bytes = null)
        {
            LineModel ln = new()
            {
                Base = (@base != null) ? @base : null,
                IP = (ip != null) ? ip : null,
                Port = (port != null) ? port : null,
                Letters = (letters != null) ? letters : null,
                Bytes = (bytes != null) ? bytes : null
            };

            await RecomputeStatisticsAsync(@base, ip, port, letters);
            return await AddAsync(index, ln);
        }

        public async Task NullFixHandler()
        {
            var nulls = _lines!.Where(x => x.Value.Base == null).ToList();

            for (int i = 0; i < nulls.Count; i++)
            {
                _lines!.TryRemove(nulls[i].Key, out _);
            }
        }
        public async Task DeDupHandler()
        {
            var keys = _lines!.Keys.ToArray();

            await DedupByBaseAsync(keys);

            await DedupByIPAsync();
        }
        public async Task InFixHandler()
        {
            var invalidIPsBase = _lines!.Where(x => x.Value.Base != null && x.Value.IP != null).Where(x => !IPAddress.TryParse(x.Value.IP, out _)).ToList();
            for (int i = 0; i < invalidIPsBase.Count; i++)
            {
                if (_lines![invalidIPsBase[i].Key].Port == null && _lines![invalidIPsBase[i].Key].Letters == null)
                    _lines!.TryRemove(invalidIPsBase[i].Key, out _);
                else
                {
                    _lines[invalidIPsBase[i].Key].Base!.Replace(_lines[invalidIPsBase[i].Key].IP!, string.Empty);

                    _lines[invalidIPsBase[i].Key].IP = null;
                    _lines[invalidIPsBase[i].Key].Bytes = null;
                }
            }
        }
        public async Task LetOutHandler()
        {
            var letteredBase = _lines!.Where(x => x.Value.Letters != null).ToList();
            for (int i = 0; i < letteredBase.Count; i++)
            {
                if (_lines![letteredBase[i].Key].Port == null && _lines[letteredBase[i].Key].IP == null)
                    _lines!.TryRemove(letteredBase[i].Key, out _);

                else
                {
                    foreach (var lt in _lines[letteredBase[i].Key].Letters!)
                    {
                        _lines[letteredBase[i].Key].Base!.Replace(lt.Remove(0, 2), string.Empty);
                    }


                    _lines[letteredBase[i].Key].Letters = null;
                }
            }
        }
        public async Task DePortHandler()
        {
            var portedBase = _lines!.Where(x => x.Value.Base != null && x.Value.Port != null).ToList();
            for (int i = 0; i < portedBase.Count; i++)
            {
                if (_lines![portedBase[i].Key].IP == null && _lines[portedBase[i].Key].Letters == null)
                    _lines!.TryRemove(portedBase[i].Key, out _);

                else
                {
                    _lines[portedBase[i].Key].Base!.Replace(_lines[portedBase[i].Key].Port!, string.Empty);

                    _lines[portedBase[i].Key].Port = null;
                }
            }
        }
        public async Task Del4Handler()
        {
            var del4Base = _lines!.Where(x => x.Value.Base != null && x.Value.IP != null).ToList();
            for (int i = 0; i < del4Base.Count; i++)
            {
                if (_lines![del4Base[i].Key].Port == null && _lines[del4Base[i].Key].Letters == null)
                    _lines!.TryRemove(del4Base[i].Key, out _);
                else
                {
                    _lines[del4Base[i].Key].Base!.Replace(_lines[del4Base[i].Key].IP!, string.Empty);

                    _lines[del4Base[i].Key].IP = null;
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
                    if (!_lines!.TryGetValue(key, out var item) || item.Base == null)
                        return;

                    var baseValue = item.Base;


                    if (seenBase.TryAdd(baseValue, key))
                        return;


                    if (seenBase[baseValue] != key)
                    {
                        _lines!.TryRemove(key, out _);
                    }
                });
        }
        private async Task DedupByIPAsync()
        {
            var seenIP = new ConcurrentDictionary<string, uint>();

            var keys = _lines!.Keys.ToArray();

            await Parallel.ForEachAsync(keys, new ParallelOptions { MaxDegreeOfParallelism = Environment.ProcessorCount },
                async (key, ct) =>
                {
                    if (!_lines!.TryGetValue(key, out var item) || item.Base == null || item.IP == null)
                        return;

                    var ip = item.IP;

                    if (seenIP.TryAdd(ip, key))
                        return;


                    if (seenIP[ip] == key)
                        return;


                    if (item.Port == null && item.Letters == null)
                    {
                        _lines!.TryRemove(key, out _);
                    }
                    else
                    {

                        if (item.Base != null && item.IP != null)
                        {
                            item.Base = item.Base.Replace(item.IP, string.Empty).Trim();
                        }

                        item.IP = null;
                        item.Bytes = null;
                    }
                });
        }

        public async Task<List<LineModel>?> FilterLinesAsync(Func<LineModel, bool>? condition = null)
        {
            if (_lines == null) return null;
            if (condition == null) return _lines.Values.ToList();
            return _lines.Values.Where(condition).ToList();
        }

        private bool _disposed = false;
        private void Dispose(bool disposing)
        {
            if (_disposed) return;

            if (disposing)
            {
                if (_lines != null)
                    this._lines!.Clear();
                this._lines = null;
            }

            this._disposed = true;
        }
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
