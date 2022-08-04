using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Registration.Models
{
    [Table("userinfos")]
    public partial class UserInfo
    {
        public UserInfo()
        {
        }

        [Key]
        [Column("id")]
        public int Id { get; set; }
        [Required]
        [Column("username")]
        public string Username { get; set; } = null!;
        [Required]
        [Column("password_hash")]
        public string PasswordHash { get; set; } = null!;

        [InverseProperty("User")]
        public virtual Record? Record { get; set; }
    }
}
