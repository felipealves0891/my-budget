using System.ComponentModel.DataAnnotations;

namespace MyBudget.Core.Models
{
    public class Cash : IModel
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Name { get; set; }

        [Required]
        public decimal Value { get; set; }

        [Required]
        public DateTime CreatedAt { get; set; }

        [Required]
        public int CategoryId { get; set; }

        public Category Category { get; set; }

        [Required]
        public string OwnerId { get; set; }

        public ApplicationUser Owner { get; set; }
    }
}
