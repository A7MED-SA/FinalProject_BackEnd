using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace backend_project.DTOs.Section;

public class ReorderItemDto
{
    [Required]
    public Guid Id { get; set; }

    [Required]
    [Range(0, int.MaxValue, ErrorMessage = "Position must be a positive number.")]
    public int Position { get; set; }
}

public class ReorderRequestDto
{
    [Required]
    [MinLength(1, ErrorMessage = "At least one item must be provided for reordering.")]
    public List<ReorderItemDto> Items { get; set; } = new();
}
