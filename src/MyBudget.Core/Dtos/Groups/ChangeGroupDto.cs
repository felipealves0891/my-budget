namespace MyBudget.Core.Dtos.Groups
{

    public class ChangeGroupDto
    {
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public string Abbr { get; set; } = string.Empty;
    }
}
