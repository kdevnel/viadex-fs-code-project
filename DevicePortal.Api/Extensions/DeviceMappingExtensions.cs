using DevicePortal.Api.DTOs;
using DevicePortal.Api.Models;
using DevicePortal.Api.Services.Interfaces;

namespace DevicePortal.Api.Extensions;

public static class DeviceMappingExtensions
{
    public static Device ToEntity( this DeviceCreateDto dto )
    {
        return new Device {
            Name = dto.Name.Trim(),
            Model = dto.Model.Trim(),
            MonthlyPrice = dto.MonthlyPrice,
            Status = dto.Status
            // PurchaseDate will be set by the service
        };
    }

    public static DeviceResponseDto ToDto( this Device entity )
    {
        return new DeviceResponseDto {
            Id = entity.Id,
            Name = entity.Name,
            Model = entity.Model,
            MonthlyPrice = entity.MonthlyPrice,
            PurchaseDate = entity.PurchaseDate,
            Status = entity.Status
        };
    }

    public static List<DeviceResponseDto> ToDto( this List<Device> entities )
    {
        return entities.Select( e => e.ToDto() ).ToList();
    }

    public static DevicePagedResponseDto ToPagedDto( this DevicePagedResult result )
    {
        return new DevicePagedResponseDto {
            Total = result.Total,
            Items = result.Items.ToDto()
        };
    }

    public static DeviceStatusDistributionDto ToDto( this DeviceStatusDistributionResult result )
    {
        return new DeviceStatusDistributionDto {
            Distribution = result.Distribution.ToDictionary(
                kvp => kvp.Key.ToString(),
                kvp => kvp.Value
            )
        };
    }
}