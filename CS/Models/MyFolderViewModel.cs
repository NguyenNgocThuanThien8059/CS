namespace CS.Models
{
    public class MyFolderViewModel
    {
        public Folder RootFolder { get; set; }
        public IEnumerable<FolderViewModel> SubFolders { get; set; }
        public IEnumerable<FileViewModel> Files { get; set; }
    }
}
