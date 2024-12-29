namespace CS.Models
{
    public class SharedItemsViewModel
    {
        public IEnumerable<FolderViewModel> SharedFolders { get; set; }
        public IEnumerable<FileViewModel> SharedFiles { get; set; }
    }
}
