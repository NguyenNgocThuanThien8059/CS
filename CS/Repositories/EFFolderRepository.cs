using CS.Models;
using Microsoft.EntityFrameworkCore;

namespace CS.Repositories
{
    public class EFFolderRepository : IFolderRepository
    {
        private readonly ApplicationDBContext _context;
        public EFFolderRepository(ApplicationDBContext context)
        {
            _context = context;
        }
        public async Task<IEnumerable<Folder>> GetAllAsync()
        {
            return await _context.Folders.Include(f => f.SubFolders).Include(f => f.Files).ToListAsync();
        }
        public async Task<Folder> GetByIdAsync(int id)
        {
            return await _context.Folders.Include(f => f.SubFolders).Include(f => f.Files).FirstOrDefaultAsync(f => f.Id == id);
        }
        public async Task AddAsync(Folder folder)
        {
            _context.Folders.Add(folder);
            await _context.SaveChangesAsync();
        }
        public async Task UpdateAsync(Folder folder)
        {
            _context.Folders.Update(folder);
            await _context.SaveChangesAsync();
        }
        public async Task DeleteAsync(int id)
        {
            var folder = await _context.Folders.Include(f => f.SubFolders).Include(f => f.Files).FirstOrDefaultAsync(f => f.Id == id);

            if (folder != null)
            {
                // Delete subfolders recursively
                foreach (var subfolder in folder.SubFolders)
                {
                    await DeleteAsync(subfolder.Id); // Recursive delete for subfolders
                }

                // Delete files in the folder
                _context.Files.RemoveRange(folder.Files);

                // Remove the folder
                _context.Folders.Remove(folder);

                await _context.SaveChangesAsync();
            }
        }
    }
}
