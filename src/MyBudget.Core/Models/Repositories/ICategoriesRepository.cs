namespace MyBudget.Core.Models.Repositories
{
    public interface ICategoriesRepository : IGenericRepository<Category>
    {
        Group GetGroupById(int id, string ownerId);
    }
}
