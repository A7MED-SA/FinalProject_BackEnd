using System;
using System.Collections.Generic;
using backend_project.Models;

namespace backend_project.DTOs.Quiz;

public class CreateQuestionDto
{
    public string QuestionText { get; set; } = string.Empty;
    public QuestionType Type { get; set; } = QuestionType.MultipleChoice;
    public int Points { get; set; } = 1;
    public string? Explanation { get; set; }
    public int OrderIndex { get; set; }
    
    public List<CreateOptionDto> Options { get; set; } = new List<CreateOptionDto>();
}

public class CreateOptionDto
{
    public string OptionText { get; set; } = string.Empty;
    public bool IsCorrect { get; set; } = false;
    public int OrderIndex { get; set; }
}

public class QuestionResponseDto
{
    public Guid Id { get; set; }
    public string QuestionText { get; set; } = string.Empty;
    public QuestionType Type { get; set; }
    public int Points { get; set; }
    public string? Explanation { get; set; }
    public int OrderIndex { get; set; }
    
    public IEnumerable<OptionResponseDto> Options { get; set; } = new List<OptionResponseDto>();
}

public class OptionResponseDto
{
    public Guid Id { get; set; }
    public string OptionText { get; set; } = string.Empty;
    // IsCorrect might be omitted depending on the context (e.g. taking the quiz vs instructor view)
    public bool? IsCorrect { get; set; } 
    public int OrderIndex { get; set; }
}
