using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Capstone_360s.Models.FeedbackDb
{
    [Table("feedback")]
    public class Feedback
    {
        [Key]
        public Guid Id { get; set;}

        [Required]
        public Guid ReviewerId { get; set;}

        [ForeignKey(nameof(ReviewerId))]

        public User Reviewer { get; set;}

        [Required]
        public Guid RevieweeId { get; set;}

        [ForeignKey(nameof(RevieweeId))]

        public User Reviewee { get; set;}

        [Required]
        public Guid ProjectId { get; set;}

        [ForeignKey(nameof(ProjectId))]

        public Project Project { get; set;}

        [Required]
        public int RoundId { get; set;}

        [ForeignKey(nameof(RoundId))]

        public Round Round { get; set;}

        public int TimeframeId { get; set; }

        [ForeignKey(nameof(TimeframeId))]
        public Timeframe Timeframe { get; set;}

        public Guid? FeedbackPdfId { get; set;}

        [ForeignKey(nameof(FeedbackPdfId))]
        public FeedbackPdf? FeedbackPdf { get; set;}

        [DataType(DataType.Time)]
        [DisplayFormat(DataFormatString = "{0:hh:mm tt}", ApplyFormatInEditMode = true)]
        public DateTime? StartTime { get; set;}

        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:hh:mm tt}", ApplyFormatInEditMode = true)]
        public DateTime? EndTime { get; set;}
        public int? DurationSeconds { get; set;}

        // public string? GDFileId { get; set; }
        public string? OriginalResponseId { get; set; }

        [NotMapped]
        public List<Metric> Metrics { get; set;}

        [NotMapped]
        public List<MetricResponse> MetricResponses { get; set;}

        [NotMapped]
        public List<Question> Questions { get; set;}

        [NotMapped]
        public List<QuestionResponse> QuestionResponses { get; set;}

        public override string ToString()
        {
            return $"{nameof(Feedback)}: Id = {Id}, ReviewerId = {ReviewerId}, RevieweeId = {RevieweeId}, ProjectId = {ProjectId}, RoundId = {RoundId}, TimeframeId = {TimeframeId}, StartTime = {StartTime}, EndTime = {EndTime}, DurationSeconds = {DurationSeconds}";
        }
    }
}
