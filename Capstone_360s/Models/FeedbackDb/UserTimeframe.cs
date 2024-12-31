using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Capstone_360s.Models.FeedbackDb
{
    [Table("usertimeframes")]
    [PrimaryKey(nameof(UserId), nameof(TimeframeId))]
    public class UserTimeframe
    {
        [DisplayName("Student")]
        public Guid UserId { get; set; }

        [ForeignKey(nameof(UserId))]
        public User User { get; set; }

        [DisplayName("Semester")]
        public int TimeframeId { get; set; }

        [ForeignKey(nameof(TimeframeId))]
        public Timeframe Timeframe { get; set; }

        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        [DisplayName("Added Date")]
        public DateTime? AddedDate { get; set; }

        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        [DisplayName("Removed Date")]
        public DateTime? RemovedDate { get; set; }

        public override string ToString()
        {
            return $"{nameof(UserOrganization)}: UserId = {UserId}, TimeframeId = {TimeframeId}";
        }
    }
}