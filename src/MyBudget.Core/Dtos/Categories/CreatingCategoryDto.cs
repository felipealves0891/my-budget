namespace MyBudget.Core.Dtos.Categories
{
    public class CreatingCategoryDto
    {
        public string Name { get; set; } = string.Empty;

        public string Abbr { get; set; } = string.Empty;

        public string Flow { get; set; } = string.Empty;

        public int GroupId { get; set; }
    }
}
