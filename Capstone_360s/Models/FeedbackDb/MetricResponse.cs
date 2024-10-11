using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Capstone_360s.Models.FeedbackDb
{
    [Table("metricresponses")]
    [PrimaryKey(nameof(MetricId), nameof(FeedbackId))]
    public class MetricResponse
    {
        public int MetricId { get; set; }

        [ForeignKey(nameof(MetricId))]

        public Metric Metric { get; set; }

        public Guid FeedbackId { get; set; }

        [ForeignKey(nameof(FeedbackId))]

        public Feedback Feedback { get; set; }

        [Required]
        public int Response { get; set; }

        public override string ToString()
        {
            return $"{nameof(MetricResponse)}: MetricId = {MetricId}, Response = {Response}";
        }
    }
}
