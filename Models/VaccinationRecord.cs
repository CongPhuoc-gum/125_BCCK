using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace _125_BCCK.Models
{
    [Table("VaccinationRecords")]
    public class VaccinationRecord
    {
        [Key]
        public int RecordId { get; set; }

        public int PetId { get; set; }
        public int AppointmentId { get; set; }

        [Required]
        [StringLength(200)]
        public string VaccineName { get; set; }

        public DateTime VaccinationDate { get; set; }
        public DateTime? NextDueDate { get; set; }
        public string Notes { get; set; }

        public int StaffId { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}