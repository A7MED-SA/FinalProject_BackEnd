using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using backend_project.DTOs.Category;
using System.Security.Claims;
using backend_project.Services.Interfaces;

namespace backend_project.Controllers;

[ApiController]
[Route("api/categories")]
public class CategoryController : ControllerBase
{
    private readonly ICategoryService _categoryService;

    public CategoryController(ICategoryService categoryService)
    {
        _categoryService = categoryService;
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GetAll()
    {
        var result = await _categoryService.GetAllCategoriesAsync();
        return Ok(backend_project.DTOs.ApiResponse<System.Collections.Generic.List<CategoryResponseDto>>.SuccessResponse(result));
    }

    [HttpGet("{id}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _categoryService.GetCategoryByIdAsync(id);
        return Ok(backend_project.DTOs.ApiResponse<CategoryResponseDto>.SuccessResponse(result));
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create([FromBody] CreateCategoryDto dto)
    {
        var result = await _categoryService.CreateCategoryAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, backend_project.DTOs.ApiResponse<CategoryResponseDto>.SuccessResponse(result));
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateCategoryDto dto)
    {
        var result = await _categoryService.UpdateCategoryAsync(id, dto);
        return Ok(backend_project.DTOs.ApiResponse<CategoryResponseDto>.SuccessResponse(result));
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(Guid id)
    {
        await _categoryService.DeleteCategoryAsync(id);
        return Ok(backend_project.DTOs.ApiResponse<object>.SuccessResponse(null, "Category deleted successfully"));
    }
    [HttpPut("{categoryId}/image")]
[Authorize(Roles = "Admin")] // ⚠️ تأكد من الصلاحيات
public async Task<IActionResult> SetCategoryImage(
    Guid categoryId, 
    [FromBody] SetCategoryImageRequest request)
{
    try
    {
        var userId = GetUserId();
        var result = await _categoryService.SetCategoryImageAsync(
            categoryId, 
            request.FileId,
            userId);
        
        return Ok(result);
    }
    catch (KeyNotFoundException ex)
    {
        return NotFound(new { message = ex.Message });
    }
    catch (UnauthorizedAccessException ex)
    {
        return Unauthorized(new { message = ex.Message });
    }
    catch (InvalidOperationException ex)
    {
        return BadRequest(new { message = ex.Message });
    }
}
    private Guid GetUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? throw new UnauthorizedAccessException("User not authenticated");
        return Guid.Parse(userIdClaim);
    }
}
