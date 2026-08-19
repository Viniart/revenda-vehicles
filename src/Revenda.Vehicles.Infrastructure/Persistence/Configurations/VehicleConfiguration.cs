using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Revenda.Vehicles.Domain.Entities;
using Revenda.Vehicles.Domain.ValueObjects;

namespace Revenda.Vehicles.Infrastructure.Persistence.Configurations;

internal sealed class VehicleConfiguration : IEntityTypeConfiguration<Vehicle>
{
    public void Configure(EntityTypeBuilder<Vehicle> builder)
    {
        builder.ToTable("vehicles");

        builder.HasKey(vehicle => vehicle.Id);

        builder.Property(vehicle => vehicle.Id).HasColumnName("id").ValueGeneratedNever();

        builder.Property(vehicle => vehicle.Brand)
            .HasColumnName("brand")
            .HasMaxLength(Vehicle.MaxTextLength)
            .IsRequired();

        builder.Property(vehicle => vehicle.Model)
            .HasColumnName("model")
            .HasMaxLength(Vehicle.MaxTextLength)
            .IsRequired();

        builder.Property(vehicle => vehicle.Year).HasColumnName("year").IsRequired();

        builder.Property(vehicle => vehicle.Color)
            .HasColumnName("color")
            .HasMaxLength(Vehicle.MaxTextLength)
            .IsRequired();

        builder.Property(vehicle => vehicle.Price)
            .HasColumnName("price")
            .HasColumnType("numeric(12,2)")
            .HasConversion(price => price.Amount, amount => Money.Create(amount))
            .IsRequired();

        builder.Property(vehicle => vehicle.LicensePlate)
            .HasColumnName("license_plate")
            .HasMaxLength(7)
            .HasConversion(plate => plate.Value, value => LicensePlate.Create(value))
            .IsRequired();

        builder.Property(vehicle => vehicle.Status)
            .HasColumnName("status")
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(vehicle => vehicle.CreatedAt).HasColumnName("created_at").IsRequired();
        builder.Property(vehicle => vehicle.UpdatedAt).HasColumnName("updated_at").IsRequired();

        // Dois compradores podem tentar reservar o mesmo veiculo ao mesmo tempo. O xmin do
        // PostgreSQL entra como token de concorrencia em propriedade sombra, para que a
        // segunda gravacao falhe em vez de sobrescrever a primeira, sem sujar a entidade.
        builder.Property<uint>("xmin").IsRowVersion();

        builder.HasIndex(vehicle => vehicle.LicensePlate)
            .IsUnique()
            .HasDatabaseName("ix_vehicles_license_plate");

        builder.HasIndex(vehicle => new { vehicle.Status, vehicle.Price })
            .HasDatabaseName("ix_vehicles_status_price");
    }
}
