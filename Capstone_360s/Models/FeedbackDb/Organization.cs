using System.ComponentModel;
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

        [Required]
        public string Type { get; set;}
        
        [DisplayName("Google Drive Folder")]
        public string? GDFolderId { get; set; }

        [NotMapped]
        public List<Timeframe> Timeframes { get; set; }

        [NotMapped]
        public List<Metric> Metrics { get; set; }

        [NotMapped]
        public List<Question> Questions { get; set; }

        [NotMapped]
        public List<UserOrganization> Users { get; set; }

        public override string ToString()
        {
            return $"{nameof(Organization)}: Id = {Id}, Name = {Name}, GDFolderId = {GDFolderId}";
        }
    }
}
