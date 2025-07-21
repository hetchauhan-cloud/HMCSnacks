using System.ComponentModel.DataAnnotations.Schema;

namespace HMCSnacks.Models
{
    [Table("states")] // maps class to the 'states' table
    public class State
    {
        [Column("state_id")]
        public int Id { get; set; }

        [Column("state_name")]
        public string StateName { get; set; }

        [Column("is_active")]
        public bool IsActive { get; set; }

        [Column("created_date")]
        public DateTime CreatedDate { get; set; }
    }
}
