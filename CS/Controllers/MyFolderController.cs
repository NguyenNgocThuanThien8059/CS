using CS.Models;
using CS.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace CS.Controllers
{
    public class MyFolderController : Controller
    {
        private readonly IFolderRepository _folderRepository;
        private readonly IFileRepository _fileRepository;
        private readonly UserManager<ApplicationUser> _userManager;

        public MyFolderController(IFolderRepository folderRepository, IFileRepository fileRepository, UserManager<ApplicationUser> userManager)
        {
            _folderRepository = folderRepository;
            _fileRepository = fileRepository;
            _userManager = userManager;
        }

        // GET: Index - Display user folders and files, and allow navigating through subfolders
        [Authorize]
        public async Task<IActionResult> Index(int? folderId)
        {
            var user = await _userManager.GetUserAsync(User);
            var folders = await _folderRepository.GetAllAsync();
            var files = await _fileRepository.GetAllAsync();

            var userFolders = folders.Where(f => f.UserId == user.Id).ToList();
            var userFiles = files.Where(f => f.Folder.UserId == user.Id).ToList();

            var currentFolder = folderId.HasValue ? userFolders.FirstOrDefault(f => f.Id == folderId.Value) : null;
            if (currentFolder == null && userFolders.Any())
            {
                currentFolder = userFolders.FirstOrDefault(f => f.ParentFolderId == null); // Root folder
            }

            var subFolders = currentFolder != null ? userFolders.Where(f => f.ParentFolderId == currentFolder.Id).ToList() : userFolders.Where(f => f.ParentFolderId != null).ToList();
            var folderFiles = currentFolder != null ? userFiles.Where(f => f.FolderId == currentFolder.Id).ToList() : userFiles.Where(f => f.FolderId == null).ToList();

            var folderViewModels = subFolders.Select(f => new FolderViewModel
            {
                Id = f.Id,
                Name = f.Name,
                CreatedDate = f.CreatedDate,
                Size = FormatSize(GetFolderSize(f))
            }).ToList();

            var fileViewModels = folderFiles.Select(f => new FileViewModel
            {
                Name = f.Name,
                UploadDate = f.UploadDate,
                Type = f.Type,
                Size = FormatSize(f.Size)
            }).ToList();

            var model = new MyFolderViewModel
            {
                RootFolder = currentFolder,
                SubFolders = folderViewModels,
                Files = fileViewModels
            };

            return View(model);
        }

        // GET: Create Folder
        [Authorize]
        public async Task<IActionResult> Create(int? parentFolderId)
        {
            return View(new CreateFolderViewModel { ParentFolderId = parentFolderId });
        }

        // POST: Create Folder
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Create(CreateFolderViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.GetUserAsync(User);

                var folder = new Folder
                {
                    Name = model.Name,
                    CreatedDate = DateTime.Now,
                    UserId = user.Id,
                    ParentFolderId = model.ParentFolderId
                };

                await _folderRepository.AddAsync(folder);

                if (model.ParentFolderId.HasValue)
                    return RedirectToAction("Index", new { folderId = model.ParentFolderId });
                else
                    return RedirectToAction("Index");
            }

            return View(model);
        }
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> UploadFile(int folderId)
        {
            // Get the current user
            var user = await _userManager.GetUserAsync(User);

            // Get the folder where the file should be uploaded
            var folder = await _folderRepository.GetByIdAsync(folderId);

            // Check if the folder exists and belongs to the current user
            if (folder == null || folder.UserId != user.Id)
            {
                TempData["Error"] = "You do not have permission to upload to this folder.";
                return RedirectToAction("Index", new { folderId });
            }

            // Just pass the folderId to the view, no need to load subfolders or files here
            return View(folderId);
        }
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> UploadFile(int folderId, IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                TempData["Error"] = "No file selected.";
                return RedirectToAction("UploadFile", new { folderId });
            }

            // Get the current user
            var user = await _userManager.GetUserAsync(User);

            // Get the folder where the file should be uploaded
            var folder = await _folderRepository.GetByIdAsync(folderId);

            // Check if the folder exists and belongs to the current user
            if (folder == null || folder.UserId != user.Id)
            {
                TempData["Error"] = "You do not have permission to upload to this folder.";
                return RedirectToAction("Index", new { folderId });
            }

            // Save the file (you can save it in a physical directory or database as per your requirement)
            var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", file.FileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            // Save file metadata to the database if necessary (e.g., File name, path, size)
            var newFile = new Models.File
            {
                Name = file.FileName,
                Size = file.Length,
                UploadDate = DateTime.Now,
                FolderId = folderId,
                Type = file.ContentType
            };

            await _fileRepository.AddAsync(newFile);

            TempData["Success"] = "File uploaded successfully!";
            return RedirectToAction("Index", new { folderId });
        }
        // GET: Edit Folder
        [Authorize]
        public async Task<IActionResult> EditFolder(int folderId)
        {
            var folder = await _folderRepository.GetByIdAsync(folderId);
            if (folder == null || folder.UserId != (await _userManager.GetUserAsync(User)).Id)
            {
                TempData["Error"] = "You do not have permission to edit this folder.";
                return RedirectToAction("Index");
            }

            return View(new EditFolderViewModel { Id = folder.Id, Name = folder.Name });
        }

        // POST: Edit 
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> EditFolder(EditFolderViewModel model)
        {
            if (ModelState.IsValid)
            {
                var folder = await _folderRepository.GetByIdAsync(model.Id);
                if (folder == null || folder.UserId != (await _userManager.GetUserAsync(User)).Id)
                {
                    TempData["Error"] = "You do not have permission to edit this folder.";
                    return RedirectToAction("Index");
                }

                folder.Name = model.Name;
                await _folderRepository.UpdateAsync(folder);

                TempData["Success"] = "Folder updated successfully!";
                return RedirectToAction("Index", new { folderId = folder.ParentFolderId });
            }

            return View(model);
        }

        // GET: Delete Folder
        [Authorize]
        public async Task<IActionResult> DeleteFolder(int folderId)
        {
            var folder = await _folderRepository.GetByIdAsync(folderId);
            if (folder == null || folder.UserId != (await _userManager.GetUserAsync(User)).Id)
            {
                TempData["Error"] = "You do not have permission to delete this folder.";
                return RedirectToAction("Index");
            }

            return View(folder);
        }

        // POST: Delete Folder
        [HttpPost, ActionName("DeleteFolder")]
        [Authorize]
        public async Task<IActionResult> DeleteConfirmed(int folderId)
        {
            var folder = await _folderRepository.GetByIdAsync(folderId);
            if (folder == null || folder.UserId != (await _userManager.GetUserAsync(User)).Id)
            {
                TempData["Error"] = "You do not have permission to delete this folder.";
                return RedirectToAction("Index");
            }

            await _folderRepository.DeleteAsync(folderId);

            TempData["Success"] = "Folder deleted successfully!";
            return RedirectToAction("Index");
        }

        // POST: Share Folder
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ShareFolder(int folderId)
        {
            var folder = await _folderRepository.GetByIdAsync(folderId);
            if (folder == null || folder.UserId != User.FindFirstValue(ClaimTypes.NameIdentifier))
            {
                return NotFound(); // Ensure the folder exists and belongs to the logged-in user
            }

            folder.IsShared = !folder.IsShared; // Toggle the IsShared property

            await _folderRepository.UpdateAsync(folder);
            TempData["SuccessMessage"] = folder.IsShared
                ? "Folder shared successfully!"
                : "Folder sharing revoked successfully!";

            return RedirectToAction("Index", new { folderId = folder.ParentFolderId ?? 0 });
        }

        // Private helper to update sharing status recursively
        private async Task UpdateFolderSharingStatus(Folder folder)
        {
            // Update the sharing status of the folder itself
            folder.IsShared = true;
            await _folderRepository.UpdateAsync(folder);

            // Update sharing status for all files (if any)
            foreach (var file in folder.Files ?? Enumerable.Empty<Models.File>())
            {
                file.IsShared = true;
                await _fileRepository.UpdateAsync(file);
            }

            // Recursively update sharing status for all subfolders (if any)
            foreach (var subFolder in folder.SubFolders ?? Enumerable.Empty<Folder>())
            {
                await UpdateFolderSharingStatus(subFolder);
            }
        }
        // GET: Edit File
        [Authorize]
        public async Task<IActionResult> EditFile(int fileId)
        {
            var file = await _fileRepository.GetByIdAsync(fileId);
            if (file == null || file.Folder.UserId != (await _userManager.GetUserAsync(User)).Id)
            {
                TempData["Error"] = "You do not have permission to edit this file.";
                return RedirectToAction("Index");
            }

            var model = new EditFileViewModel
            {
                Id = file.Id,
                Name = file.Name
            };

            return View(model);
        }
        // POST: Edit File
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> EditFile(EditFileViewModel model)
        {
            if (ModelState.IsValid)
            {
                var file = await _fileRepository.GetByIdAsync(model.Id);
                if (file == null || file.Folder.UserId != (await _userManager.GetUserAsync(User)).Id)
                {
                    TempData["Error"] = "You do not have permission to edit this file.";
                    return RedirectToAction("Index");
                }

                file.Name = model.Name;
                await _fileRepository.UpdateAsync(file);

                TempData["Success"] = "File updated successfully!";
                return RedirectToAction("Index", new { folderId = file.FolderId });
            }

            return View(model);
        }
        // GET: Delete File
        [Authorize]
        public async Task<IActionResult> DeleteFile(int fileId)
        {
            var file = await _fileRepository.GetByIdAsync(fileId);
            if (file == null || file.Folder.UserId != (await _userManager.GetUserAsync(User)).Id)
            {
                TempData["Error"] = "You do not have permission to delete this file.";
                return RedirectToAction("Index");
            }

            return View(file);
        }
        // POST: Delete File
        [HttpPost, ActionName("DeleteFile")]
        [Authorize]
        public async Task<IActionResult> DeleteFileConfirmed(int fileId)
        {
            var file = await _fileRepository.GetByIdAsync(fileId);
            if (file == null || file.Folder.UserId != (await _userManager.GetUserAsync(User)).Id)
            {
                TempData["Error"] = "You do not have permission to delete this file.";
                return RedirectToAction("Index");
            }

            await _fileRepository.DeleteAsync(fileId);

            TempData["Success"] = "File deleted successfully!";
            return RedirectToAction("Index", new { folderId = file.FolderId });
        }
        [Authorize]
        [HttpPost]
        public async Task<IActionResult> ShareFile(int fileId)
        {
            var file = await _fileRepository.GetByIdAsync(fileId);
            if (file == null)
            {
                return NotFound("File not found.");
            }

            file.IsShared = true;
            await _fileRepository.UpdateAsync(file);

            TempData["Success"] = $"File '{file.Name}' has been shared successfully.";
            return RedirectToAction(nameof(Index), new { folderId = file.FolderId });
        }
        // Method to calculate folder size (including files and subfolders)
        private long GetFolderSize(Folder folder)
        {
            return folder.Files.Sum(f => f.Size) + folder.SubFolders.Sum(f => GetFolderSize(f));
        }

        // Method to format size into a readable format
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
    }
}
