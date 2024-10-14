using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Capstone_360s.Models.FeedbackDb
{
    [Table("questions")]
    public class Question
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(255)]
        public string Q { get; set; }

        [Required]
        [StringLength(255)]
        public string Example { get; set; }
        public string? OriginalQuestionId { get; set; }

        public Guid OrganizationId { get; set; }

        [ForeignKey(nameof(OrganizationId))]
        public Organization Organization { get; set; }

        [DefaultValue(false)]
        public bool IsDeleted { get; set; }

        [NotMapped]
        public List<Feedback> Feedback { get; set; }

        [NotMapped]
        public List<QuestionResponse> QuestionResponses { get; set; }

        public override string ToString()
        {
            return $"{nameof(Question)}: Id = {Id}, Q = {Q}, Example = {Example}, OrganizationId = {OrganizationId}, IsDeleted = {IsDeleted}";
        }
    }
}
