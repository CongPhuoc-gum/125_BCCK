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

        [Required]
        [StringLength(50)]
        public string Species { get; set; } // Chó, Mèo...

        [StringLength(100)]
        public string Breed { get; set; } // Giống loài

        public int? Age { get; set; }

        public decimal? Weight { get; set; }

        [StringLength(10)]
        public string Gender { get; set; }

        [StringLength(50)]
        public string Color { get; set; }

        public string ImageUrl { get; set; }

        public string SpecialNotes { get; set; }

        public bool IsActive { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}