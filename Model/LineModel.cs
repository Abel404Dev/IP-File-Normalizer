namespace IP_File_Normalizer.Model
{
    public class LineModel
    {
        public string? Base { get; set; }
        public string? IP { get; set; }
        public string? Port { get; set; }
        public List<string> Letters { get; set; } = new List<string>();
        public byte[]? Bytes { get; set; }
    }
}
