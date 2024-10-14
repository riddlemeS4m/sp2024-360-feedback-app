using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Capstone_360s.Models.FeedbackDb
{
    [Table("rounds")]
    public class Round
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(10)]
        public string Name { get; set; }

        [NotMapped]
        public List<Project> Projects { get; set; }

        [NotMapped]
        public List<ProjectRound> ProjectRounds { get; set; }

        public override string ToString()
        {
            return $"{nameof(Round)}: Id = {Id}, Name = {Name}";
        }

    }
}
