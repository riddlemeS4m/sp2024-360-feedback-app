using Microsoft.EntityFrameworkCore;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
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

        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        [DisplayName("Release Date")]
        public DateTime? ReleaseDate { get; set; }

        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        [DisplayName("Due Date")]
        public DateTime? DueDate { get; set; }

        [DisplayName("Google Drive Folder")]
        public string? GDFolderId { get; set;}

        public override string ToString()
        {
            return $"{nameof(ProjectRound)}: ProjectId = {ProjectId}, RoundId = {RoundId}, GDFolderId = {GDFolderId}";
        }
    }
}
