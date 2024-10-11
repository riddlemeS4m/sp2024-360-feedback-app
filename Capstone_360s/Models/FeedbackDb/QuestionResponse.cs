using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Capstone_360s.Models.FeedbackDb
{
    [Table("questionresponses")]
    [PrimaryKey(nameof(QuestionId), nameof(FeedbackId))]
    public class QuestionResponse
    {
        public int QuestionId { get; set; }

        [ForeignKey(nameof(QuestionId))]
        public Question Question { get; set; }

        public Guid FeedbackId { get; set; }

        [ForeignKey(nameof(FeedbackId))]
        public Feedback Feedback { get; set; }

        [Required]
        public string Response { get; set; }

        public override string ToString()
        {
            return $"{nameof(QuestionResponse)}: QuestionId = {QuestionId}, Response = {Response}";
        }
    }
}
