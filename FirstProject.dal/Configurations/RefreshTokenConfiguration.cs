using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FirstProject.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FirstProject.Data.Configurations
{
    public class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
    {
        public void Configure(EntityTypeBuilder<RefreshToken> builder)
        {
            builder.HasKey(rt => rt.Id);

            builder.Property(rt => rt.Token)
                   .IsRequired()
                   .HasMaxLength(256);

            builder.Property(rt => rt.UserId)
                   .IsRequired();

            builder.Property(rt => rt.Expires)
                   .IsRequired();

            builder.Property(rt => rt.IsUsed)
                   .HasDefaultValue(false);

            builder.Property(rt => rt.IsRevoked)
                   .HasDefaultValue(false);

            // Optional: Foreign key
            // builder.HasOne<ApplicationUser>()
            //        .WithMany()
            //        .HasForeignKey(rt => rt.UserId);
        }
    }
}
