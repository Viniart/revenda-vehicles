using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Revenda.Vehicles.Domain.Entities;
using Revenda.Vehicles.Domain.ValueObjects;

namespace Revenda.Vehicles.Infrastructure.Persistence.Configurations;

internal sealed class SaleConfiguration : IEntityTypeConfiguration<Sale>
{
    public void Configure(EntityTypeBuilder<Sale> builder)
    {
        builder.ToTable("sales");

        builder.HasKey(sale => sale.Id);

        builder.Property(sale => sale.Id).HasColumnName("id").ValueGeneratedNever();
        builder.Property(sale => sale.VehicleId).HasColumnName("vehicle_id").IsRequired();
        builder.Property(sale => sale.BuyerId).HasColumnName("buyer_id").IsRequired();

        builder.Property(sale => sale.Price)
            .HasColumnName("price")
            .HasColumnType("numeric(12,2)")
            .HasConversion(price => price.Amount, amount => Money.Create(amount))
            .IsRequired();

        builder.Property(sale => sale.PaymentCode)
            .HasColumnName("payment_code")
            .HasMaxLength(40)
            .IsRequired();

        builder.Property(sale => sale.Status)
            .HasColumnName("status")
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(sale => sale.CreatedAt).HasColumnName("created_at").IsRequired();
        builder.Property(sale => sale.CompletedAt).HasColumnName("completed_at");

        builder.HasIndex(sale => sale.PaymentCode).IsUnique().HasDatabaseName("ix_sales_payment_code");
        builder.HasIndex(sale => sale.BuyerId).HasDatabaseName("ix_sales_buyer");

        builder.HasOne<Vehicle>()
            .WithMany()
            .HasForeignKey(sale => sale.VehicleId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
