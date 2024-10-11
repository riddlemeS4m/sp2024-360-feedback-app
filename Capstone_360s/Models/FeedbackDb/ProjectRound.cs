using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;

namespace Capstone_360s.Models.FeedbackDb
{
    [Table("projectrounds")]
    [PrimaryKey(nameof(ProjectId), nameof(RoundId))]
    public class ProjectRound
    {
        public Guid ProjectId { get; set; }

        [ForeignKey(nameof(ProjectId))]
        public Project Project { get; set; }

        public int RoundId { get; set;}

        [ForeignKey(nameof(RoundId))]
        public Round Round { get; set;}

        public string? GDFolderId { get; set;}

        public override string ToString()
        {
            return $"{nameof(ProjectRound)}: ProjectId = {ProjectId}, RoundId = {RoundId}, GDFolderId = {GDFolderId}";
        }
    }
}
