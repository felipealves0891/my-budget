namespace MyBudget.Core.Dtos.Categories
{
    public class ChangeCategoryDto
    {
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public string Abbr { get; set; } = string.Empty;
    }
}
