namespace CS.Models
{
    public class File
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Type { get; set; }
        public DateTime UploadDate { get; set; } = DateTime.Now;
        public long Size { get; set; }
        public int FolderId { get; set; }
        public Folder Folder { get; set; }
        public bool IsShared { get; set; } = false;
    }
}
