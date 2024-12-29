namespace CS.Models
{
    public class FolderViewModel
    {
        public int? Id { get; set; }
        public string Name { get; set; }
        public DateTime CreatedDate { get; set; }
        public string Size { get; set; }
        public bool IsShared { get; set; }
    }
}
