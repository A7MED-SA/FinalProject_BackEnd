using backend_project.DTOs.Profile;
using Microsoft.AspNetCore.Http;

namespace backend_project.Services.Interfaces;

public interface IProfileService
{
    /// <summary>
    /// الحصول على الملف الشخصي الكامل للمستخدم
    /// </summary>
    Task<ProfileDto> GetProfileAsync(Guid userId);

    /// <summary>
    /// تحديث بيانات الملف الشخصي الأساسية
    /// </summary>
    Task<ProfileDto> UpdateProfileAsync(Guid userId, UpdateProfileDto dto);

    /// <summary>
    /// الحصول على الملف الشخصي العام لأي مستخدم
    /// </summary>
    Task<PublicProfileDto> GetPublicProfileAsync(Guid userId);

    /// <summary>
    /// رفع صورة الملف الشخصي
    /// </summary>
    Task<ProfileDto> SetProfileImageAsync(Guid userId, Guid fileId);

    /// <summary>
    /// حذف صورة الملف الشخصي
    /// </summary>
    Task DeleteProfilePictureAsync(Guid userId);

    // --- Phone Management ---

    /// <summary>
    /// إضافة رقم هاتف جديد
    /// </summary>
    Task<PhoneDto> AddPhoneAsync(Guid userId, AddPhoneDto dto);

    /// <summary>
    /// حذف رقم هاتف
    /// </summary>
    Task DeletePhoneAsync(Guid userId, Guid phoneId);

    /// <summary>
    /// تعيين رقم هاتف كافتراضي
    /// </summary>
    Task SetDefaultPhoneAsync(Guid userId, Guid phoneId);

    // --- Address Management ---

    /// <summary>
    /// إضافة عنوان جديد
    /// </summary>
    Task<AddressDto> AddAddressAsync(Guid userId, AddAddressDto dto);

    /// <summary>
    /// تحديث عنوان موجود
    /// </summary>
    Task<AddressDto> UpdateAddressAsync(Guid userId, Guid addressId, UpdateAddressDto dto);

    /// <summary>
    /// حذف عنوان
    /// </summary>
    Task DeleteAddressAsync(Guid userId, Guid addressId);

    /// <summary>
    /// تعيين عنوان كافتراضي
    /// </summary>
    Task SetDefaultAddressAsync(Guid userId, Guid addressId);
}
