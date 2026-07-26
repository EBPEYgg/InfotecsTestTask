using InfotecsTestTask.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace InfotecsTestTask.Infrastructure.Persistence.Configurations;

public class ResultsConfiguration : IEntityTypeConfiguration<Results>
{
    public void Configure(EntityTypeBuilder<Results> builder)
    {
        builder.HasKey(results => results.Id);

        builder.Property(results => results.FileName)
               .HasMaxLength(260)
               .IsRequired();

        builder.HasIndex(values => values.FileName).IsUnique();

        builder.HasIndex(values => values.FirstOperationDate);

        builder.HasIndex(values => values.AverageValue);

        builder.HasIndex(values => values.AverageExecutionTime);
    }
}