using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace _125_BCCK.Models
{
    [Table("Pets")]
    public class Pet
    {
        [Key]
        public int PetId { get; set; }

        public int OwnerId { get; set; }

        [Required]
        [StringLength(100)]
        public string PetName { get; set; }

        [StringLength(50)]
        public string Species { get; set; } // Chó, Mèo...

        [StringLength(100)]
        public string Breed { get; set; }

        public int? Age { get; set; }

        public string Color { get; set; }

        public decimal? Weight { get; set; }

        public bool IsActive { get; set; }

        // Khóa ngoại liên kết với User (Chủ)
        [ForeignKey("OwnerId")]
        public virtual User Owner { get; set; }
    }
}