using System.ComponentModel.DataAnnotations;

namespace MyBudget.Core.Models
{
    public class Group
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Name { get; set; }

        [Required]
        public string Abbr { get; set; }

        [Required]
        public Sense Sense { get; set; }
        
        public List<Category> Categories { get; set; }

        [Required]
        public string OwnerId { get; set; }

        public ApplicationUser Owner { get; set; }
    }
}
