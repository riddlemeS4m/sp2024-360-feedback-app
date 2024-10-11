using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Capstone_360s.Models.FeedbackDb
{
    [Table("metrics")]
    public class Metric
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; }

        [Required]
        [StringLength(255)]
        public string Description { get; set; }

        [Required]
        public int MinValue { get; set; }

        [Required]
        public int MaxValue { get; set; }

        public int? Weight { get; set; }

        public Guid OrganizationId { get; set; }

        [ForeignKey(nameof(OrganizationId))]
        public Organization Organization { get; set; }

        [DefaultValue(false)]
        public bool IsDeleted { get; set; }

        [NotMapped]
        public List<Feedback> Feedback { get; set; }

        [NotMapped]
        public List<MetricResponse> MetricResponses { get; set; }

        public override string ToString()
        {
            return $"{nameof(Metric)}: Id = {Id}, Name = {Name}, Description = {Description}, MinValue = {MinValue}, MaxValue = {MaxValue}, Weight = {Weight}, OrganizationId = {OrganizationId}, IsDeleted = {IsDeleted}";
        }
        
    }
}
