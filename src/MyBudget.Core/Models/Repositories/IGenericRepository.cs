namespace MyBudget.Core.Models.Repositories
{
    public interface IGenericRepository<T>
        where T : IModel
    {
        IEnumerable<T> GetAll();

        T Get(int id, string ownerId);

        T Get(string name, string ownerId);

        void Add(T model);

        void Update(T model);

        void Delete(int id);
    }
}
