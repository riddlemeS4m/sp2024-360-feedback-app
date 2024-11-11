using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Capstone_360s.Models.FeedbackDb
{
    [Table("users")]
    public class User
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        [StringLength(100)]
        public string FirstName { get; set; }

        [Required]
        [StringLength(100)]
        public string LastName { get; set; }

        [Required]
        [StringLength(100)]
        public string Email { get; set; }

        public Guid? MicrosoftId { get; set;  }

        [Required]
        public Guid OrganizationId { get; set; }

        [ForeignKey(nameof(OrganizationId))]
        public Organization Organization { get; set; }

        [DefaultValue(false)]
        public bool IsPOC { get; set;  }

        public override string ToString()
        {
            return $"{nameof(User)}: Id = {Id}, FirstName = {FirstName}, LastName = {LastName}, Email = {Email}, MicrosoftId = {MicrosoftId}, IsPOC = {IsPOC}";
        }

    }
}
