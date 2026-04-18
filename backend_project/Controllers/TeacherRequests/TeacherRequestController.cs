using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using FluentValidation;
using System.Security.Claims;
using backend_project.DTOs.TeacherRequests.Requests;
using backend_project.DTOs.TeacherRequests.Responses;
using backend_project.Services.TeacherRequests;
using backend_project.Validators.TeacherRequests;

namespace backend_project.Controllers.TeacherRequests;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class TeacherRequestController : ControllerBase
{
    private readonly ITeacherRequestService _service;
    private readonly SubmitTeacherRequestValidator _submitValidator;
    private readonly IValidator<ProcessTeacherRequestDto> _processValidator;
    private readonly IValidator<UpdateTeacherRequestDto> _updateValidator;
    private readonly IValidator<AddDocumentToRequestDto> _addDocumentValidator;

    public TeacherRequestController(
        ITeacherRequestService service,
        SubmitTeacherRequestValidator submitValidator,
        IValidator<ProcessTeacherRequestDto> processValidator,
        IValidator<UpdateTeacherRequestDto> updateValidator,
        IValidator<AddDocumentToRequestDto> addDocumentValidator)
    {
        _service = service;
        _submitValidator = submitValidator;
        _processValidator = processValidator;
        _updateValidator = updateValidator;
        _addDocumentValidator = addDocumentValidator;
    }

    /// <summary>
    /// التحقق إذا كان يمكن تقديم طلب جديد
    /// </summary>
    [HttpGet("can-submit")]
    public async Task<IActionResult> CanSubmitRequest()
    {
    var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        var canSubmit = await _service.CanSubmitRequestAsync(userId);
        return Ok(new { canSubmit });
    }

    /// <summary>
    /// تقديم طلب جديد ليصبح معلم
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> SubmitRequest([FromBody] SubmitTeacherRequestDto dto)
    {
    var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        // التحقق من صحة البيانات
        var validationResult = await _submitValidator.ValidateAsync(dto);
        if (!validationResult.IsValid)
            return BadRequest(validationResult.Errors);

        try
        {
            var request = await _service.SubmitRequestAsync(userId, dto);
            return CreatedAtAction(nameof(GetMyRequestById), new { requestId = request.Id }, request);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// تحديث طلب (إذا كان مازال معلق)
    /// </summary>
    [HttpPut("{requestId}")]
    public async Task<IActionResult> UpdateRequest(
        Guid requestId, 
        [FromBody] UpdateTeacherRequestDto dto)
    {
    var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        // التحقق من صحة البيانات
        var validationResult = await _updateValidator.ValidateAsync(dto);
        if (!validationResult.IsValid)
            return BadRequest(validationResult.Errors);

        try
        {
            var request = await _service.UpdateRequestAsync(requestId, userId, dto);
            return Ok(request);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    /// <summary>
    /// إضافة مستند لطلب موجود
    /// </summary>
    [HttpPost("{requestId}/documents")]
    public async Task<IActionResult> AddDocument(
        Guid requestId, 
        [FromBody] AddDocumentToRequestDto dto)
    {
    var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        // التحقق من صحة البيانات
        var validationResult = await _addDocumentValidator.ValidateAsync(dto);
        if (!validationResult.IsValid)
            return BadRequest(validationResult.Errors);

        try
        {
            var success = await _service.AddDocumentAsync(requestId, userId, dto);
            if (!success)
                return NotFound(new { message = "الطلب غير موجود أو لا يمكن تعديله" });

            return Ok(new { message = "تم إضافة المستند بنجاح" });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// عرض جميع طلباتي
    /// </summary>
    [HttpGet("my-requests")]
    public async Task<IActionResult> GetMyRequests()
    {
    var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        var requests = await _service.GetMyRequestsAsync(userId);
        return Ok(requests);
    }

    /// <summary>
    /// عرض تفاصيل طلب معين (خاصة بي)
    /// </summary>
    [HttpGet("my-requests/{requestId}")]
    public async Task<IActionResult> GetMyRequestById(Guid requestId)
    {
    var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        try
        {
            var request = await _service.GetMyRequestByIdAsync(userId, requestId);
            return Ok(request);
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new { message = "الطلب غير موجود" });
        }
    }

    /// <summary>
    /// إلغاء طلب (إذا كان مازال معلق)
    /// </summary>
    [HttpDelete("{requestId}/cancel")]
    public async Task<IActionResult> CancelRequest(Guid requestId)
    {
    var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        var success = await _service.CancelRequestAsync(userId, requestId);
        if (!success)
            return BadRequest(new { message = "لا يمكن إلغاء هذا الطلب" });

        return Ok(new { message = "تم إلغاء الطلب بنجاح" });
    }

    /// <summary>
    /// عرض جميع الطلبات المعلقة (للأدمن فقط)
    /// </summary>
    [HttpGet("pending")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetPendingRequests()
    {
        var requests = await _service.GetPendingRequestsAsync();
        return Ok(requests);
    }

    /// <summary>
    /// عرض تفاصيل طلب معين (للأدمن)
    /// </summary>
    [HttpGet("{requestId}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetRequestById(Guid requestId)
    {
        try
        {
            var request = await _service.GetRequestByIdAsync(requestId);
            return Ok(request);
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new { message = "الطلب غير موجود" });
        }
    }

    /// <summary>
    /// معالجة طلب (موافقة/رفض) - للأدمن فقط
    /// </summary>
    [HttpPut("{requestId}/process")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> ProcessRequest(
        Guid requestId, 
        [FromBody] ProcessTeacherRequestDto dto)
    {
        // التحقق من صحة البيانات
        var validationResult = await _processValidator.ValidateAsync(dto);
        if (!validationResult.IsValid)
            return BadRequest(validationResult.Errors);

        var adminId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(adminId))
            return Unauthorized();

        try
        {
            var request = await _service.ProcessRequestAsync(requestId, adminId, dto);
            return Ok(request);
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new { message = "الطلب غير موجود" });
        }
    }

    /// <summary>
    /// حذف طلب (للأدمن فقط)
    /// </summary>
    [HttpDelete("{requestId}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteRequest(Guid requestId)
    {
        var success = await _service.DeleteRequestAsync(requestId);
        if (!success)
            return NotFound(new { message = "الطلب غير موجود" });

        return Ok(new { message = "تم حذف الطلب بنجاح" });
    }
}