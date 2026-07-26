using InfotecsTestTask.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace InfotecsTestTask.Infrastructure.Persistence.Configurations;

public class ValuesConfiguration : IEntityTypeConfiguration<Values>
{
    public void Configure(EntityTypeBuilder<Values> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(values => values.FileName)
            .HasMaxLength(260).IsRequired();

        builder.HasIndex(values => values.FileName);

        builder.HasIndex(values => new { values.FileName, values.Date });
    }
}