using Athary.Application.DTOs.Courses;
using Athary.Domain.Entities;
using Mapster;

namespace Athary.Application.Mappings;

public class CourseMappings : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<Course, CourseSummaryDto>()
            .Map(dest => dest.CategoryName, src => src.Category != null ? src.Category.Name : null);

        config.NewConfig<Section, SectionDto>();
    }
}
