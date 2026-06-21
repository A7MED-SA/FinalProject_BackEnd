using Athary.Application.DTOs.Communication;
using Athary.Domain.Entities;
using Mapster;

namespace Athary.Application.Mappings;

public class CommunicationMappings : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<Announcement, AnnouncementResponse>()
            .Map(dest => dest.Target, src => src.Target.ToString())
            .Map(dest => dest.CreatedByName, src => src.Creator != null
                ? $"{src.Creator.FirstName} {src.Creator.LastName}" : string.Empty);

        config.NewConfig<Report, ReportResponse>()
            .Map(dest => dest.EntityType, src => src.EntityType.ToString())
            .Map(dest => dest.Reason, src => src.Reason.ToString())
            .Map(dest => dest.Status, src => src.Status.ToString())
            .Map(dest => dest.ReporterName, src => src.Reporter != null
                ? $"{src.Reporter.FirstName} {src.Reporter.LastName}" : string.Empty);

        config.NewConfig<SystemSetting, SettingResponse>();
    }
}
