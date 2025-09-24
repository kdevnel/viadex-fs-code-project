using DevicePortal.Api.DTOs;
using DevicePortal.Api.Models;

namespace DevicePortal.Api.Extensions;

public static class ShipmentMappingExtensions
{
    public static Shipment ToEntity( this ShipmentCreateDto dto )
    {
        return new Shipment {
            TrackingNumber = dto.TrackingNumber,
            CustomerName = dto.CustomerName,
            EstimatedDelivery = dto.EstimatedDelivery,
            Destination = dto.Destination,
            Status = ShipmentStatus.Processing,
            CreatedAt = DateTime.UtcNow
        };
    }

    public static ShipmentResponseDto ToDto( this Shipment shipment )
    {
        return new ShipmentResponseDto {
            Id = shipment.Id,
            TrackingNumber = shipment.TrackingNumber,
            CustomerName = shipment.CustomerName,
            Status = ( int ) shipment.Status,
            StatusName = shipment.Status.ToString(),
            EstimatedDelivery = shipment.EstimatedDelivery,
            ActualDelivery = shipment.ActualDelivery,
            Destination = shipment.Destination,
            CreatedAt = shipment.CreatedAt
        };
    }

    public static ShipmentStatusDistributionDto ToStatusDistributionDto( this IEnumerable<Shipment> shipments )
    {
        var distribution = new ShipmentStatusDistributionDto();

        foreach ( var shipment in shipments )
        {
            switch ( shipment.Status )
            {
                case ShipmentStatus.Processing:
                    distribution.Processing++;
                    break;
                case ShipmentStatus.InTransit:
                    distribution.InTransit++;
                    break;
                case ShipmentStatus.Delivered:
                    distribution.Delivered++;
                    break;
                case ShipmentStatus.Delayed:
                    distribution.Delayed++;
                    break;
            }
        }

        return distribution;
    }
}