using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Registration.Models
{
    [Table("records")]
    public partial class Record
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }
        [Column("user_id")]
        public int UserId { get; set; }
        [Column("code")]
        public string? Code { get; set; }
        [Required]
        [Column("name")]
        public string Name { get; set; } = null!;
        [Required]
        [Column("cpf")]
        public string Cpf { get; set; } = null!;
        [Column("address")]
        public string? Address { get; set; }
        [Column("phone")]
        public string? Phone { get; set; }

        [ForeignKey("UserId")]
        [InverseProperty("Record")]
        public virtual UserInfo User { get; set; } = null!;
    }
}
