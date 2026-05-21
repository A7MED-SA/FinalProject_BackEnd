using System;
using System.Collections.Generic;
using backend_project.DTOs.ContentProgress;

namespace backend_project.DTOs.Enrollment;

public class EnrollmentDetailDto : EnrollmentResponseDto
{
    public IEnumerable<ContentProgressDto> Progresses { get; set; } = new List<ContentProgressDto>();
}
