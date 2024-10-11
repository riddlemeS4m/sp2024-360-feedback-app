using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Capstone_360s.Models.FeedbackDb
{
    [Table("teams")]
    public class Team
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; }

        public Guid? ManagerId { get; set; }

        [ForeignKey(nameof(ManagerId))]
        public User? Manager { get; set; }

        [Required]
        public int NoOfMembers { get; set; }

        [NotMapped]
        public List<User> Members { get; set; }

        public override string ToString()
        {
            return $"{nameof(Team)}: Id = {Id}, Name = {Name}, ManagerId = {ManagerId}, NoOfMembers = {NoOfMembers}";
        }
    }
}
