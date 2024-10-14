using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Capstone_360s.Models.FeedbackDb
{
    [Table("organizations")]
    public class Organization
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; }

        [NotMapped]
        public List<Timeframe> Timeframes { get; set; }

        [NotMapped]
        public List<User> Users { get; set; }

        public string? GDFolderId { get; set; }

        [NotMapped]
        public List<Metric> Metrics { get; set; }

        [NotMapped]
        public List<Question> Questions { get; set; }

        public override string ToString()
        {
            return $"{nameof(Organization)}: Id = {Id}, Name = {Name}, GDFolderId = {GDFolderId}";
        }
    }
}
