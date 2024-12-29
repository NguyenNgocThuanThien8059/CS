namespace CS.Models
{
    public class FileViewModel
    {
        public int? Id { get; set; }
        public string Name { get; set; }
        public DateTime UploadDate { get; set; }
        public string Type { get; set; }
        public string Size { get; set; }
        public bool IsShared { get; set; }
    }
}
