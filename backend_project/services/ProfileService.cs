using backend_project.Configuration;
using backend_project.Data;
using backend_project.DTOs.Profile;
using backend_project.Models;
using backend_project.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace backend_project.Services;

public class ProfileService : IProfileService
{
    private readonly ApplicationDbContext _context;
    private readonly IObjectStorage _objectStorage;
    private readonly MinioSettings _minioSettings;
    private readonly ILogger<ProfileService> _logger;

    private static readonly string[] AllowedImageExtensions = { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
    private const long MaxFileSize = 5 * 1024 * 1024; // 5 MB

    public ProfileService(
        ApplicationDbContext context,
        IObjectStorage objectStorage,
        IOptions<MinioSettings> minioSettings,
        ILogger<ProfileService> logger)
    {
        _context = context;
        _objectStorage = objectStorage;
        _minioSettings = minioSettings.Value;
        _logger = logger;
    }

    public async Task<ProfileDto> GetProfileAsync(Guid userId)
    {
        var user = await _context.Users
            .Include(u => u.UserPhones)
            .Include(u => u.Addresses.Where(a => a.DeletedAt == null))
            .Include(u => u.ProfileImageFile)
            .FirstOrDefaultAsync(u => u.Id == userId && u.DeletedAt == null)
            ?? throw new KeyNotFoundException("User not found");

        return MapToProfileDto(user);
    }

    public async Task<PublicProfileDto> GetPublicProfileAsync(Guid userId)
    {
        var user = await _context.Users
            .Include(u => u.ProfileImageFile)
            .FirstOrDefaultAsync(u => u.Id == userId && u.DeletedAt == null)
            ?? throw new KeyNotFoundException("User not found");

        return new PublicProfileDto
        {
            Id = user.Id,
            Name = user.Name,
            Bio = user.Bio,
            Nationality = user.Nationality,
            ProfileImageUrl = user.ProfileImageFile != null
                ? _objectStorage.GetPublicUrl(
                    user.ProfileImageFile.Bucket,
                    user.ProfileImageFile.FilePath)
                : null,
            CreatedAt = user.CreatedAt
        };
    }


    public async Task<ProfileDto> UpdateProfileAsync(Guid userId, UpdateProfileDto dto)
    {
        var user = await _context.Users
            .Include(u => u.UserPhones)
            .Include(u => u.Addresses.Where(a => a.DeletedAt == null))
            .Include(u => u.ProfileImageFile)
            .FirstOrDefaultAsync(u => u.Id == userId && u.DeletedAt == null)
            ?? throw new KeyNotFoundException("User not found");

        // Update only provided fields
        if (dto.Name != null)
            user.Name = dto.Name;

        if (dto.Bio != null)
            user.Bio = dto.Bio;

        if (dto.Gender.HasValue)
            user.Gender = dto.Gender;

        if (dto.DateOfBirth.HasValue)
            user.DateOfBirth = dto.DateOfBirth;

        if (dto.Nationality != null)
            user.Nationality = dto.Nationality;

        user.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        _logger.LogInformation("Profile updated for user {UserId}", userId);

        return MapToProfileDto(user);
    }

    public async Task<ProfileDto> SetProfileImageAsync(Guid userId, Guid fileId)
    {
        var user = await _context.Users
            .Include(u => u.ProfileImageFile)
            .FirstOrDefaultAsync(u => u.Id == userId && u.DeletedAt == null)
            ?? throw new KeyNotFoundException("User not found");

        var file = await _context.Files.FirstOrDefaultAsync(f =>
            f.Id == fileId &&
            f.FileType == StoredFileType.Image &&
            f.Status == FileStatus.Ready &&
            f.DeletedAt == null)
            ?? throw new InvalidOperationException("Invalid image file");

        if (file.UploadedBy != userId)
            throw new UnauthorizedAccessException();


        if (user.ProfileImageFileId.HasValue &&
            user.ProfileImageFileId != file.Id)
        {
            var oldFile = await _context.Files
                .FirstOrDefaultAsync(f =>
                    f.Id == user.ProfileImageFileId &&
                    f.DeletedAt == null);

            if (oldFile != null)
            {
                oldFile.DeletedAt = DateTime.UtcNow;
                oldFile.Status = FileStatus.Deleted;
            }
        }

        file.Visibility = FileVisibility.Public;

        user.ProfileImageFileId = file.Id;
        user.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return MapToProfileDto(user);
    }


   public async Task DeleteProfilePictureAsync(Guid userId)
{
    var user = await _context.Users
        .Include(u => u.ProfileImageFile)
        .FirstOrDefaultAsync(u => u.Id == userId && u.DeletedAt == null)
        ?? throw new KeyNotFoundException("User not found");

    if (user.ProfileImageFile == null)
        throw new InvalidOperationException("User has no profile picture");

    var file = user.ProfileImageFile;

    // Soft delete file
    file.DeletedAt = DateTime.UtcNow;
    file.Status = FileStatus.Deleted;

    // Unlink from user
    user.ProfileImageFileId = null;
    user.UpdatedAt = DateTime.UtcNow;

    await _context.SaveChangesAsync();

    _logger.LogInformation(
        "Profile picture soft-deleted for user {UserId}, file {FileId}",
        userId, file.Id
    );
}

    // --- Phone Management ---

    public async Task<PhoneDto> AddPhoneAsync(Guid userId, AddPhoneDto dto)
    {
        var user = await _context.Users
            .Include(u => u.UserPhones)
            .FirstOrDefaultAsync(u => u.Id == userId && u.DeletedAt == null)
            ?? throw new KeyNotFoundException("User not found");

        // Check for duplicate phone
        if (user.UserPhones.Any(p => p.PhoneNumber == dto.PhoneNumber))
            throw new InvalidOperationException("Phone number already exists");

        // If setting as default, unset other defaults
        if (dto.IsDefault)
        {
            foreach (var phone in user.UserPhones)
                phone.IsDefault = false;
        }

        var userPhone = new UserPhone
        {
            UserId = userId,
            PhoneNumber = dto.PhoneNumber,
            Type = dto.Type,
            IsDefault = dto.IsDefault || !user.UserPhones.Any(),
            IsVerified = false,
            CreatedAt = DateTime.UtcNow
        };

        user.UserPhones.Add(userPhone);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Phone added for user {UserId}: {PhoneNumber}", userId, dto.PhoneNumber);

        return MapToPhoneDto(userPhone);
    }

    public async Task DeletePhoneAsync(Guid userId, Guid phoneId)
    {
        var phone = await _context.UserPhones
            .FirstOrDefaultAsync(p => p.Id == phoneId && p.UserId == userId)
            ?? throw new KeyNotFoundException("Phone not found");

        if (phone.IsDefault)
        {
            var anotherPhone = await _context.UserPhones
                .Where(p => p.UserId == userId && p.Id != phoneId)
                .OrderBy(p => p.CreatedAt)
                .FirstOrDefaultAsync();

            if (anotherPhone != null)
                anotherPhone.IsDefault = true;
        }

        _context.UserPhones.Remove(phone);

        await _context.SaveChangesAsync();

        _logger.LogInformation("Phone deleted for user {UserId}: {PhoneId}", userId, phoneId);
    }

    public async Task SetDefaultPhoneAsync(Guid userId, Guid phoneId)
    {
        var phones = await _context.UserPhones
            .Where(p => p.UserId == userId)
            .ToListAsync();

        var targetPhone = phones.FirstOrDefault(p => p.Id == phoneId)
            ?? throw new KeyNotFoundException("Phone not found");

        foreach (var phone in phones)
            phone.IsDefault = phone.Id == phoneId;

        await _context.SaveChangesAsync();

        _logger.LogInformation("Default phone set for user {UserId}: {PhoneId}", userId, phoneId);
    }

    // --- Address Management ---

    public async Task<AddressDto> AddAddressAsync(Guid userId, AddAddressDto dto)
    {
        var user = await _context.Users
            .Include(u => u.Addresses.Where(a => a.DeletedAt == null))
            .FirstOrDefaultAsync(u => u.Id == userId && u.DeletedAt == null)
            ?? throw new KeyNotFoundException("User not found");

        // If setting as default, unset other defaults
        if (dto.IsDefault)
        {
            foreach (var addr in user.Addresses)
                addr.IsDefault = false;
        }

        var address = new Address
        {
            UserId = userId,
            Type = dto.Type,
            StreetLine1 = dto.StreetLine1,
            StreetLine2 = dto.StreetLine2,
            City = dto.City,
            StateProvince = dto.StateProvince,
            PostalCode = dto.PostalCode,
            Country = dto.Country,
            ContactPhone = dto.ContactPhone,
            IsDefault = dto.IsDefault || !user.Addresses.Any(),
            CreatedAt = DateTime.UtcNow
        };

        user.Addresses.Add(address);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Address added for user {UserId}", userId);

        return MapToAddressDto(address);
    }

    public async Task<AddressDto> UpdateAddressAsync(Guid userId, Guid addressId, UpdateAddressDto dto)
    {
        var address = await _context.Addresses
            .FirstOrDefaultAsync(a => a.Id == addressId && a.UserId == userId && a.DeletedAt == null)
            ?? throw new KeyNotFoundException("Address not found");

        if (dto.Type.HasValue)
            address.Type = dto.Type.Value;

        if (dto.StreetLine1 != null)
            address.StreetLine1 = dto.StreetLine1;

        if (dto.StreetLine2 != null)
            address.StreetLine2 = dto.StreetLine2;

        if (dto.City != null)
            address.City = dto.City;

        if (dto.StateProvince != null)
            address.StateProvince = dto.StateProvince;

        if (dto.PostalCode != null)
            address.PostalCode = dto.PostalCode;

        if (dto.Country != null)
            address.Country = dto.Country;

        if (dto.ContactPhone != null)
            address.ContactPhone = dto.ContactPhone;

        if (dto.IsDefault.HasValue && dto.IsDefault.Value)
        {
            var allAddresses = await _context.Addresses
                .Where(a => a.UserId == userId && a.DeletedAt == null)
                .ToListAsync();

            foreach (var addr in allAddresses)
                addr.IsDefault = addr.Id == addressId;
        }
        address.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        _logger.LogInformation("Address updated for user {UserId}: {AddressId}", userId, addressId);

        return MapToAddressDto(address);
    }

    public async Task DeleteAddressAsync(Guid userId, Guid addressId)
    {
        var address = await _context.Addresses
            .FirstOrDefaultAsync(a => a.Id == addressId && a.UserId == userId && a.DeletedAt == null)
            ?? throw new KeyNotFoundException("Address not found");

        // Soft delete
        address.DeletedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        _logger.LogInformation("Address deleted for user {UserId}: {AddressId}", userId, addressId);
    }

    public async Task SetDefaultAddressAsync(Guid userId, Guid addressId)
    {
        var addresses = await _context.Addresses
            .Where(a => a.UserId == userId && a.DeletedAt == null)
            .ToListAsync();

        var targetAddress = addresses.FirstOrDefault(a => a.Id == addressId)
            ?? throw new KeyNotFoundException("Address not found");

        foreach (var addr in addresses)
            addr.IsDefault = addr.Id == addressId;

        await _context.SaveChangesAsync();

        _logger.LogInformation("Default address set for user {UserId}: {AddressId}", userId, addressId);
    }

    // --- Mapping Helpers ---

    private ProfileDto MapToProfileDto(User user)
    {
        // string? profileImageUrl = null;

        // if (user.ProfileImageFile != null)
        // {
        //     profileImageUrl = await _objectStorage.GenerateViewUrlAsync(
        //         user.ProfileImageFile.Bucket,
        //         user.ProfileImageFile.FilePath,
        //         _minioSettings.PresignedUrlExpiryMinutes);
        // }

        return new ProfileDto
        {
            Id = user.Id,
            Name = user.Name,
            Email = user.Email ?? string.Empty,
            Bio = user.Bio,
            Gender = user.Gender,
            DateOfBirth = user.DateOfBirth,
            Nationality = user.Nationality,
            ProfileImageUrl = user.ProfileImageFile != null
                ? _objectStorage.GetPublicUrl(
                    user.ProfileImageFile.Bucket,
                    user.ProfileImageFile.FilePath)
                : null,
            CreatedAt = user.CreatedAt,
            Phones = user.UserPhones.Select(MapToPhoneDto).ToList(),
            Addresses = user.Addresses
                .Where(a => a.DeletedAt == null)
                .Select(MapToAddressDto)
                .ToList()
        };
    }

    private static PhoneDto MapToPhoneDto(UserPhone phone)
    {
        return new PhoneDto
        {
            Id = phone.Id,
            PhoneNumber = phone.PhoneNumber,
            Type = phone.Type,
            IsVerified = phone.IsVerified,
            IsDefault = phone.IsDefault
        };
    }

    private static AddressDto MapToAddressDto(Address address)
    {
        return new AddressDto
        {
            Id = address.Id,
            Type = address.Type,
            StreetLine1 = address.StreetLine1,
            StreetLine2 = address.StreetLine2,
            City = address.City,
            StateProvince = address.StateProvince,
            PostalCode = address.PostalCode,
            Country = address.Country,
            ContactPhone = address.ContactPhone,
            IsDefault = address.IsDefault
        };
    }
}
