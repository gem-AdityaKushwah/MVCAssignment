using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;

namespace MVCAssignment1.Models
{
    public class MVCAssignmentDBContext : DbContext
    {
        public MVCAssignmentDBContext() : base("name=MVCAssignmentDBContext")
        {
        }

        public DbSet<UserDetail> UserDetails { get; set; }
        public DbSet<PasswordDetails> PasswordDetails { get; set; }
    }
}