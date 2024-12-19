using Microsoft.CodeAnalysis;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Capstone_360s.Models.FeedbackDb
{
    [Table("pdffiles")]
    public class FeedbackPdf
    {
        [Key]
        public Guid Id { get; set; }

        public Guid UserId { get; set; }

        [ForeignKey(nameof(UserId))]
        public User User { get; set; }

        public string FileName { get; set; }

        public string ParentGDFolderId { get; set; }
        public string GDFileId { get; set; }

        public Guid ProjectId { get; set; }

        [ForeignKey(nameof(ProjectId))]
        public Project Project { get; set; }

        public int RoundId { get; set; }

        [ForeignKey(nameof(RoundId))]
        public Round Round { get; set; }

        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd hh:mm tt}", ApplyFormatInEditMode = true)]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        [NotMapped]
        public byte[] Data { get; set; }

        [NotMapped]
        public List<Feedback> SurveyResponses { get; set; }

        public override string ToString()
        {
            return $"{nameof(FeedbackPdf)}: Id = {Id}, FileName = {FileName}, ParentGDFolderId = {ParentGDFolderId}, GDFileId = {GDFileId}, ProjectId = {ProjectId}, RoundId = {RoundId}";
        }
    }
}
