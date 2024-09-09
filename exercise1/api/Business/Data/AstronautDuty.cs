using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.Serialization;

namespace StargateAPI.Business.Data
{
    [Table("AstronautDuty")]
    public class AstronautDuty
    {
        public int Id { get; set; }

        public int PersonId { get; set; }

        public string Rank { get; set; } = string.Empty;

        public string DutyTitle { get; set; } = string.Empty;
        public DateTime BackingDutyStartDate { get; set; }

        public DateTime DutyStartDate
        {
            get => BackingDutyStartDate;
            set
            {
                if (value.Kind == DateTimeKind.Unspecified)
                {
                    BackingDutyStartDate = DateTime.SpecifyKind(value, DateTimeKind.Utc);
                }
                else
                {
                    BackingDutyStartDate = value.ToUniversalTime();
                }
            }
        }
        public DateTime? BackingDutyEndDate { get; set; }

        public DateTime? DutyEndDate
        {
            get => BackingDutyEndDate;
            set
            {
                if (value.HasValue)
                {
                    BackingDutyEndDate = value.Value.Kind == DateTimeKind.Unspecified
                        ? DateTime.SpecifyKind(value.Value, DateTimeKind.Utc)
                        : value.Value.ToUniversalTime();
                }
                else
                {
                    BackingDutyEndDate = null;
                }
            }
        }

        public virtual Person Person { get; set; }
    }

    public class AstronautDutyConfiguration : IEntityTypeConfiguration<AstronautDuty>
    {
        public void Configure(EntityTypeBuilder<AstronautDuty> builder)
        {
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Id).ValueGeneratedOnAdd();

            builder.Ignore(x => x.BackingDutyStartDate);
            builder.Ignore(x => x.BackingDutyEndDate);

            builder.Property(x => x.DutyStartDate)
                   .HasColumnType("timestamptz");
            builder.Property(x => x.DutyEndDate)
                   .HasColumnType("timestamptz");
        }
    }
}
