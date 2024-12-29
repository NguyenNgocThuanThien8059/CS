using CS.Models;
using Microsoft.EntityFrameworkCore;

namespace CS.Repositories
{
    public class EFFileRepository : IFileRepository
    {
        private readonly ApplicationDBContext _context;
        public EFFileRepository(ApplicationDBContext context)
        {
            _context = context;
        }
        public async Task<IEnumerable<Models.File>> GetAllAsync()
        {
            return await _context.Files.Include(f => f.Folder).ToListAsync();
        }
        public async Task<Models.File> GetByIdAsync(int id)
        {
            return await _context.Files.Include(f => f.Folder).FirstOrDefaultAsync(f => f.Id == id);
        }
        public async Task AddAsync(Models.File file)
        {
            _context.Files.Add(file);
            await _context.SaveChangesAsync();
        }
        public async Task UpdateAsync(Models.File file)
        {
            _context.Files.Update(file);
            await _context.SaveChangesAsync();
        }
        public async Task DeleteAsync(int id)
        {
            var file = await _context.Files.FindAsync(id);
            if (file != null)
            {
                _context.Files.Remove(file);
                await _context.SaveChangesAsync();
            }
        }
    }
}
