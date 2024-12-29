namespace CS.Models
{
    public class CreateFolderViewModel
    {
        public string Name { get; set; }
        public int? ParentFolderId { get; set; } // Nullable, because it can be null for root folders
    }
}
