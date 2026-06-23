using System.Text.RegularExpressions;
using Athary.Application.DTOs.Profile;
using Athary.Application.Interfaces.Media;
using Athary.Application.Interfaces.Profile;
using Athary.Domain.Entities;
using Athary.Domain.Enums;
using Athary.Domain.Interfaces;
using Athary.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Athary.Infrastructure.Services.Profile;

public sealed class ProfileService : IProfileService
{
    private readonly ApplicationDbContext _context;
    private readonly IRepository<User> _userRepo;
    private readonly IRepository<UserPhone> _phoneRepo;
    private readonly IRepository<Address> _addressRepo;
    private readonly IRepository<UploadedFile> _fileRepo;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IObjectStorage _objectStorage;
    private readonly ILogger<ProfileService> _logger;

    public ProfileService(
        ApplicationDbContext context,
        IRepository<User> userRepo,
        IRepository<UserPhone> phoneRepo,
        IRepository<Address> addressRepo,
        IRepository<UploadedFile> fileRepo,
        IUnitOfWork unitOfWork,
        IObjectStorage objectStorage,
        ILogger<ProfileService> logger)
    {
        _context = context;
        _userRepo = userRepo;
        _phoneRepo = phoneRepo;
        _addressRepo = addressRepo;
        _fileRepo = fileRepo;
        _unitOfWork = unitOfWork;
        _objectStorage = objectStorage;
        _logger = logger;
    }

    public async Task<ProfileDto> GetProfileAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var user = await _context.Users
            .Include(u => u.UserPhones)
            .Include(u => u.Addresses.Where(a => a.DeletedAt == null))
            .Include(u => u.ProfileImageFile)
            .FirstOrDefaultAsync(u => u.Id == userId && u.DeletedAt == null, cancellationToken)
            ?? throw new KeyNotFoundException("User not found");

        return MapToProfileDto(user);
    }

    public async Task<PublicProfileDto> GetPublicProfileAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var user = await _userRepo.FirstOrDefaultAsync(
            u => u.Id == userId && u.DeletedAt == null,
            q => q.Include(u => u.ProfileImageFile),
            cancellationToken)
            ?? throw new KeyNotFoundException("User not found");

        return new PublicProfileDto
        {
            Id = user.Id,
            FullName = user.FullName,
            Bio = user.Bio,
            Nationality = user.Nationality,
            ProfileImageUrl = user.ProfileImageFile is not null
                ? _objectStorage.GetPublicUrl(user.ProfileImageFile.Bucket, user.ProfileImageFile.FilePath)
                : null,
            CreatedAt = user.CreatedAt
        };
    }

    public async Task<PublicProfileDto?> GetBySlugAsync(string slug, CancellationToken cancellationToken = default)
    {
        var user = await _context.Users
            .Where(u => u.Slug == slug && u.DeletedAt == null)
            .Include(u => u.ProfileImageFile)
            .FirstOrDefaultAsync(cancellationToken);

        if (user is null)
            return null;

        return new PublicProfileDto
        {
            Id = user.Id,
            FullName = user.FullName,
            Slug = user.Slug,
            Bio = user.Bio,
            Nationality = user.Nationality,
            ProfileImageUrl = user.ProfileImageFile is not null
                ? _objectStorage.GetPublicUrl(user.ProfileImageFile.Bucket, user.ProfileImageFile.FilePath)
                : null,
            CreatedAt = user.CreatedAt
        };
    }

    public async Task<bool> IsSlugAvailableAsync(string slug, CancellationToken cancellationToken = default)
    {
        return !await _context.Users.AnyAsync(u => u.Slug == slug, cancellationToken);
    }

    public async Task<string> GenerateSlugAsync(string fullName, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(fullName))
            throw new ArgumentException("Full name cannot be empty", nameof(fullName));

        var baseSlug = fullName.Trim().ToLower()
            .Replace(" ", "-")
            .Replace(".", "")
            .Replace("'", "")
            .Replace("(", "")
            .Replace(")", "");

        baseSlug = Regex.Replace(baseSlug, @"[^a-z0-9-\u0600-\u06FF]", "");

        if (string.IsNullOrWhiteSpace(baseSlug) || baseSlug.Length < 3)
            baseSlug = $"user-{Guid.NewGuid().ToString()[..8]}";

        var slug = baseSlug;
        var counter = 1;

        while (!await IsSlugAvailableAsync(slug, cancellationToken))
        {
            slug = $"{baseSlug}-{counter}";
            counter++;
        }

        return slug;
    }

    public async Task<List<PublicProfileDto>> SearchBySlugAsync(string query, CancellationToken cancellationToken = default)
    {
        return await _context.Users
            .Where(u => u.DeletedAt == null &&
                        (u.Slug != null && u.Slug.Contains(query) ||
                         ($"{u.FirstName} {u.LastName}").Contains(query)))
            .Include(u => u.ProfileImageFile)
            .Select(u => new PublicProfileDto
            {
                Id = u.Id,
                FullName = u.FullName,
                Slug = u.Slug,
                Bio = u.Bio,
                ProfileImageUrl = u.ProfileImageFile != null
                    ? u.ProfileImageFile.Bucket + "/" + u.ProfileImageFile.FilePath
                    : null
            })
            .Take(10)
            .ToListAsync(cancellationToken);
    }

    public async Task<ProfileDto> UpdateProfileAsync(Guid userId, UpdateProfileDto dto, CancellationToken cancellationToken = default)
    {
        var user = await _context.Users
            .Include(u => u.UserPhones)
            .Include(u => u.Addresses.Where(a => a.DeletedAt == null))
            .Include(u => u.ProfileImageFile)
            .FirstOrDefaultAsync(u => u.Id == userId && u.DeletedAt == null, cancellationToken)
            ?? throw new KeyNotFoundException("User not found");

        if (dto.FirstName is not null)
            user.FirstName = dto.FirstName;

        if (dto.LastName is not null)
            user.LastName = dto.LastName;

        if (dto.Bio is not null)
            user.Bio = dto.Bio;

        if (dto.Gender.HasValue)
            user.Gender = dto.Gender;

        if (dto.DateOfBirth.HasValue)
            user.DateOfBirth = dto.DateOfBirth;

        if (dto.Nationality is not null)
            user.Nationality = dto.Nationality;

        user.UpdatedAt = DateTime.UtcNow;
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Profile updated for user {UserId}", userId);

        return MapToProfileDto(user);
    }

    public async Task<ProfileDto> SetProfileImageAsync(Guid userId, Guid fileId, CancellationToken cancellationToken = default)
    {
        var user = await _userRepo.FirstOrDefaultAsync(
            u => u.Id == userId && u.DeletedAt == null,
            q => q.Include(u => u.ProfileImageFile),
            cancellationToken)
            ?? throw new KeyNotFoundException("User not found");

        var file = await _fileRepo.FirstOrDefaultAsync(f =>
            f.Id == fileId &&
            f.FileType == StoredFileType.Image &&
            f.Status == FileStatus.Ready &&
            f.DeletedAt == null, cancellationToken: cancellationToken)
            ?? throw new InvalidOperationException("Invalid image file");

        if (file.UploadedBy != userId)
            throw new UnauthorizedAccessException("File does not belong to the user");

        if (user.ProfileImageFileId.HasValue && user.ProfileImageFileId != file.Id)
        {
            var oldFile = await _fileRepo.FirstOrDefaultAsync(
                f => f.Id == user.ProfileImageFileId && f.DeletedAt == null,
                cancellationToken: cancellationToken);

            if (oldFile is not null)
            {
                oldFile.DeletedAt = DateTime.UtcNow;
                oldFile.Status = FileStatus.Deleted;
            }
        }

        file.Visibility = FileVisibility.Public;
        user.ProfileImageFileId = file.Id;
        user.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Profile picture set for user {UserId}, file {FileId}", userId, fileId);

        user.ProfileImageFile = file;
        return MapToProfileDto(user);
    }

    public async Task DeleteProfilePictureAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var user = await _userRepo.FirstOrDefaultAsync(
            u => u.Id == userId && u.DeletedAt == null,
            q => q.Include(u => u.ProfileImageFile),
            cancellationToken)
            ?? throw new KeyNotFoundException("User not found");

        if (user.ProfileImageFile is null)
            throw new InvalidOperationException("User has no profile picture");

        var file = user.ProfileImageFile;
        file.DeletedAt = DateTime.UtcNow;
        file.Status = FileStatus.Deleted;
        user.ProfileImageFileId = null;
        user.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Profile picture deleted for user {UserId}", userId);
    }

    public async Task<PhoneDto> AddPhoneAsync(Guid userId, AddPhoneDto dto, CancellationToken cancellationToken = default)
    {
        var user = await _context.Users
            .Include(u => u.UserPhones)
            .FirstOrDefaultAsync(u => u.Id == userId && u.DeletedAt == null, cancellationToken)
            ?? throw new KeyNotFoundException("User not found");

        if (user.UserPhones.Any(p => p.PhoneNumber == dto.PhoneNumber))
            throw new InvalidOperationException("Phone number already exists");

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
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Phone added for user {UserId}: {PhoneNumber}", userId, dto.PhoneNumber);

        return MapToPhoneDto(userPhone);
    }

    public async Task DeletePhoneAsync(Guid userId, Guid phoneId, CancellationToken cancellationToken = default)
    {
        var phone = await _phoneRepo.FirstOrDefaultAsync(
            p => p.Id == phoneId && p.UserId == userId,
            cancellationToken: cancellationToken)
            ?? throw new KeyNotFoundException("Phone not found");

        if (phone.IsDefault)
        {
            var anotherPhone = await _phoneRepo.FirstOrDefaultAsync(
                p => p.UserId == userId && p.Id != phoneId,
                q => q.OrderBy(p => p.CreatedAt),
                cancellationToken);

            if (anotherPhone is not null)
                anotherPhone.IsDefault = true;
        }

        await _phoneRepo.DeleteAsync(phone, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Phone deleted for user {UserId}: {PhoneId}", userId, phoneId);
    }

    public async Task SetDefaultPhoneAsync(Guid userId, Guid phoneId, CancellationToken cancellationToken = default)
    {
        var phones = await _context.UserPhones
            .Where(p => p.UserId == userId)
            .ToListAsync(cancellationToken);

        var targetPhone = phones.FirstOrDefault(p => p.Id == phoneId)
            ?? throw new KeyNotFoundException("Phone not found");

        foreach (var phone in phones)
            phone.IsDefault = phone.Id == phoneId;

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Default phone set for user {UserId}: {PhoneId}", userId, phoneId);
    }

    public async Task<AddressDto> AddAddressAsync(Guid userId, AddAddressDto dto, CancellationToken cancellationToken = default)
    {
        var user = await _context.Users
            .Include(u => u.Addresses.Where(a => a.DeletedAt == null))
            .FirstOrDefaultAsync(u => u.Id == userId && u.DeletedAt == null, cancellationToken)
            ?? throw new KeyNotFoundException("User not found");

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
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Address added for user {UserId}", userId);

        return MapToAddressDto(address);
    }

    public async Task<AddressDto> UpdateAddressAsync(Guid userId, Guid addressId, UpdateAddressDto dto, CancellationToken cancellationToken = default)
    {
        var address = await _addressRepo.FirstOrDefaultAsync(
            a => a.Id == addressId && a.UserId == userId && a.DeletedAt == null,
            cancellationToken: cancellationToken)
            ?? throw new KeyNotFoundException("Address not found");

        if (dto.Type is not null)
            address.Type = dto.Type;

        if (dto.StreetLine1 is not null)
            address.StreetLine1 = dto.StreetLine1;

        if (dto.StreetLine2 is not null)
            address.StreetLine2 = dto.StreetLine2;

        if (dto.City is not null)
            address.City = dto.City;

        if (dto.StateProvince is not null)
            address.StateProvince = dto.StateProvince;

        if (dto.PostalCode is not null)
            address.PostalCode = dto.PostalCode;

        if (dto.Country is not null)
            address.Country = dto.Country;

        if (dto.ContactPhone is not null)
            address.ContactPhone = dto.ContactPhone;

        if (dto.IsDefault.HasValue && dto.IsDefault.Value)
        {
            var allAddresses = await _context.Addresses
                .Where(a => a.UserId == userId && a.DeletedAt == null)
                .ToListAsync(cancellationToken);

            foreach (var addr in allAddresses)
                addr.IsDefault = addr.Id == addressId;
        }

        address.UpdatedAt = DateTime.UtcNow;
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Address updated for user {UserId}: {AddressId}", userId, addressId);

        return MapToAddressDto(address);
    }

    public async Task DeleteAddressAsync(Guid userId, Guid addressId, CancellationToken cancellationToken = default)
    {
        var address = await _addressRepo.FirstOrDefaultAsync(
            a => a.Id == addressId && a.UserId == userId && a.DeletedAt == null,
            cancellationToken: cancellationToken)
            ?? throw new KeyNotFoundException("Address not found");

        address.DeletedAt = DateTime.UtcNow;
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Address deleted for user {UserId}: {AddressId}", userId, addressId);
    }

    public async Task SetDefaultAddressAsync(Guid userId, Guid addressId, CancellationToken cancellationToken = default)
    {
        var addresses = await _context.Addresses
            .Where(a => a.UserId == userId && a.DeletedAt == null)
            .ToListAsync(cancellationToken);

        var targetAddress = addresses.FirstOrDefault(a => a.Id == addressId)
            ?? throw new KeyNotFoundException("Address not found");

        foreach (var addr in addresses)
            addr.IsDefault = addr.Id == addressId;

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Default address set for user {UserId}: {AddressId}", userId, addressId);
    }

    private ProfileDto MapToProfileDto(User user)
    {
        return new ProfileDto
        {
            Id = user.Id,
            FullName = user.FullName,
            Email = user.Email ?? string.Empty,
            Bio = user.Bio,
            Gender = user.Gender,
            DateOfBirth = user.DateOfBirth,
            Nationality = user.Nationality,
            ProfileImageUrl = user.ProfileImageFile is not null
                ? _objectStorage.GetPublicUrl(user.ProfileImageFile.Bucket, user.ProfileImageFile.FilePath)
                : null,
            CreatedAt = user.CreatedAt,
            Phones = user.UserPhones.Select(MapToPhoneDto).ToList(),
            Addresses = user.Addresses.Where(a => a.DeletedAt == null).Select(MapToAddressDto).ToList()
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
