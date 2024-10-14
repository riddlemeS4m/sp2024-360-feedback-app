using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Capstone_360s.Models.FeedbackDb
{
    [Table("teammembers")]
    [PrimaryKey(nameof(ProjectId), nameof(UserId))]
    public class TeamMember
    {
        public Guid ProjectId { get; set; }

        [ForeignKey(nameof(ProjectId))]
        public Project Project { get; set; }

        public Guid UserId { get; set; }

        [ForeignKey(nameof(UserId))]
        public User User { get; set; }

        public override string ToString()
        {
            return $"{nameof(TeamMember)}: ProjectId = {ProjectId}, UserId = {UserId}";
        }
    }
}
