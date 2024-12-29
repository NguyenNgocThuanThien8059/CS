namespace CS.Repositories
{
    public interface IFileRepository
    {
        Task<IEnumerable<Models.File>> GetAllAsync();
        Task<Models.File> GetByIdAsync(int id);
        Task AddAsync(Models.File file);
        Task UpdateAsync(Models.File file);
        Task DeleteAsync(int id);
    }
}
