using IdentityServer4SignalR.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IdentityServer4SignalR.Data.Configurations
{
    public class RoomConfiguration : IEntityTypeConfiguration<Room>
    {
        public void Configure(EntityTypeBuilder<Room> builder)
        {
            builder.ToTable("Rooms");
            builder.Property(x => x.Name).IsRequired().HasMaxLength(500);
            builder.HasOne(s => s.Admin)
                    .WithMany(m => m.Rooms).IsRequired();
        }
    }
}