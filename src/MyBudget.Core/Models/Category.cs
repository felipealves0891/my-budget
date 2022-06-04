using System.ComponentModel.DataAnnotations;

namespace MyBudget.Core.Models
{
    public class Category : IModel
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Name { get; set; }

        [Required]
        public string Abbr { get; set; }

        [Required]
        public Flow Flow { get; set; }

        [Required]
        public int GroupId { get; set; }

        public Group Group { get; set; }

        public List<Cash> Cashes { get; set; }

        [Required]
        public string OwnerId { get; set; }

        public ApplicationUser Owner { get; set; }
    }
}
