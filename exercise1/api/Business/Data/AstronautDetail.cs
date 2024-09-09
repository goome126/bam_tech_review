using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.Serialization;

namespace StargateAPI.Business.Data
{
    [Table("AstronautDetail")]
    public class AstronautDetail
    {
        public int Id { get; set; }

        public int PersonId { get; set; }

        public string CurrentRank { get; set; } = string.Empty;

        public string CurrentDutyTitle { get; set; } = string.Empty;
        public DateTime BackingCareerStartDate { get; set; }
        public DateTime? BackingCareerEndDate { get; set; }

        public DateTime CareerStartDate
        {
            get => BackingCareerStartDate;  // Always return the UTC value
            set
            {
                // Convert to UTC when setting the value, handling Unspecified DateTimeKind
                if (value.Kind == DateTimeKind.Unspecified)
                {
                    BackingCareerStartDate = DateTime.SpecifyKind(value, DateTimeKind.Utc);
                }
                else
                {
                    BackingCareerStartDate = value.ToUniversalTime();
                }
            }
        }

        public DateTime? CareerEndDate
        {
            get => BackingCareerEndDate;  // Always return the UTC value
            set
            {
                if (value.HasValue)
                {
                    BackingCareerEndDate = value.Value.Kind == DateTimeKind.Unspecified
                        ? DateTime.SpecifyKind(value.Value, DateTimeKind.Utc)
                        : value.Value.ToUniversalTime();
                }
                else
                {
                    BackingCareerEndDate = null;
                }
            }
        }

        public virtual Person Person { get; set; }
    }

    public class AstronautDetailConfiguration : IEntityTypeConfiguration<AstronautDetail>
    {
        public void Configure(EntityTypeBuilder<AstronautDetail> builder)
        {
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Id).ValueGeneratedOnAdd();

            // Ignore private backing fields
            builder.Ignore(x => x.BackingCareerStartDate);
            builder.Ignore(x => x.BackingCareerEndDate);

            builder.Property(x => x.CareerStartDate)
                   .HasColumnType("timestamptz"); 
            builder.Property(x => x.CareerEndDate)
                   .HasColumnType("timestamptz"); 
        }
    }
}
