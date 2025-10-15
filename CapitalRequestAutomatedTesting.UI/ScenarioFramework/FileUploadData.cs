public class FileUploadData
{
    public string FileName { get; set; }
    public long FileSize { get; set; }
    public string ContentType { get; set; }
    public string TempFilePath { get; set; } // Path to the temporarily saved file
}