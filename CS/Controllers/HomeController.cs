using System.Diagnostics;
using CS.Models;
using CS.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace CS.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IFolderRepository _folderRepository;
        private readonly IFileRepository _fileRepository;

        public HomeController(ILogger<HomeController> logger, IFolderRepository folderRepository, IFileRepository fileRepository)
        {
            _logger = logger;
            _folderRepository = folderRepository;
            _fileRepository = fileRepository;
        }

        public async Task<IActionResult> Index()
        {
            var folders = await _folderRepository.GetAllAsync();
            var files = await _fileRepository.GetAllAsync();
            var sharedfolders = folders.Where(f => f.IsShared == true);
            var sharedfiles = files.Where(f => f.IsShared == true);
            var folderViewModels = sharedfolders.Select(f => new FolderViewModel
            {
                Name = f.Name,
                CreatedDate = f.CreatedDate,
                Size = FormatSize(GetFolderSize(f)) // Format the folder size
            }).ToList();

            var fileViewModels = sharedfiles.Select(f => new FileViewModel
            {
                Name = f.Name,
                UploadDate = f.UploadDate,
                Type = f.Type,
                Size = FormatSize(f.Size) // Format the file size
            }).ToList();

            var model = new SharedItemsViewModel
            {
                SharedFolders = folderViewModels,
                SharedFiles = fileViewModels
            };


            return View(model);
        }
        public async Task<IActionResult> SharedFolderContents(int folderId)
        {
            var folder = await _folderRepository.GetByIdAsync(folderId);
            if (folder == null || !folder.IsShared)
            {
                return NotFound("Folder not found or is not shared.");
            }

            var subfolders = folder.SubFolders
                .Where(sf => sf.IsShared)
                .Select(sf => new FolderViewModel
                {
                    Name = sf.Name,
                    CreatedDate = sf.CreatedDate,
                    Size = FormatSize(GetFolderSize(sf))
                }).ToList();

            var files = folder.Files
                .Where(f => f.IsShared)
                .Select(f => new FileViewModel
                {
                    Name = f.Name,
                    UploadDate = f.UploadDate,
                    Type = f.Type,
                    Size = FormatSize(f.Size)
                }).ToList();

            var model = new MyFolderViewModel
            {
                RootFolder = folder,
                SubFolders = subfolders,
                Files = files
            };

            return View(model);
        }

        private long GetFolderSize(Folder folder)
        {
            return folder.Files.Sum(f => f.Size) + folder.SubFolders.Sum(f => GetFolderSize(f));
        }

        private string FormatSize(long sizeInBytes)
        {
            if (sizeInBytes < 1024)
                return $"{sizeInBytes} B";
            else if (sizeInBytes < 1024 * 1024)
                return $"{sizeInBytes / 1024.0:0.##} KB";
            else if (sizeInBytes < 1024 * 1024 * 1024)
                return $"{sizeInBytes / (1024.0 * 1024):0.##} MB";
            else
                return $"{sizeInBytes / (1024.0 * 1024 * 1024):0.##} GB";
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
