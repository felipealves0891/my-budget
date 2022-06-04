namespace MyBudget.Core.Models
{
    public interface IModel
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public string OwnerId { get; set; }

    }
}
