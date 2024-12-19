using Microsoft.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Capstone_360s.Models.FeedbackDb
{
    [Table("userorganizations")]
    [PrimaryKey(nameof(UserId), nameof(OrganizationId))]
    public class UserOrganization
    {
        public Guid UserId { get; set; }

        [ForeignKey(nameof(UserId))]
        public User User { get; set; }

        public Guid OrganizationId { get; set; }

        [ForeignKey(nameof(OrganizationId))]
        public Organization Organization { get; set; }

        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime? AddedDate { get; set; }

        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime? RemovedDate { get; set; }

        public override string ToString()
        {
            return $"{nameof(UserOrganization)}: UserId = {UserId}, OrganizationId = {OrganizationId}";
        }
    }
}
