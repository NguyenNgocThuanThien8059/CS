using Microsoft.AspNetCore.Identity;

namespace CS.Models
{
    public class Folder
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int? ParentFolderId { get; set; }
        public Folder ParentFolder { get; set; }
        public string UserId { get; set; }
        public ApplicationUser User { get; set; }
        public DateTime CreatedDate { get; set; } = DateTime.Now;
        public bool IsShared { get; set; } = false;
        public ICollection<Folder> SubFolders { get; set; }
        public ICollection<File> Files { get; set; }
    }
}
