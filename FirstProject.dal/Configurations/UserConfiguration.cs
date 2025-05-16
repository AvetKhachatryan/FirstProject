using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using FirstProject.Data.Entities;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FirstProject.Data.Configurations
{
    public class UserConfiguration:IEntityTypeConfiguration<User>
    {
        public void Configure(EntityTypeBuilder<User> builder)
        {
            builder.Property(n  => n.Username)
                .HasMaxLength(50)
                .IsUnicode(false);
            builder.Property(p => p.Password)
                .HasMaxLength(100)
                .IsUnicode(false);
            builder.Property(e  => e.Email)
                .HasMaxLength(254)
                .IsUnicode(false);
        }
    }
}
