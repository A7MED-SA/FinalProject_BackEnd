using Athary.Application.DTOs.Profile;

namespace Athary.Application.Interfaces.Profile;

public interface IProfileService
{
    Task<ProfileDto> GetProfileAsync(Guid userId, CancellationToken cancellationToken = default);

    Task<PublicProfileDto> GetPublicProfileAsync(Guid userId, CancellationToken cancellationToken = default);

    Task<PublicProfileDto?> GetBySlugAsync(string slug, CancellationToken cancellationToken = default);

    Task<bool> IsSlugAvailableAsync(string slug, CancellationToken cancellationToken = default);

    Task<string> GenerateSlugAsync(string fullName, CancellationToken cancellationToken = default);

    Task<List<PublicProfileDto>> SearchBySlugAsync(string query, CancellationToken cancellationToken = default);

    Task<ProfileDto> UpdateProfileAsync(Guid userId, UpdateProfileDto dto, CancellationToken cancellationToken = default);

    Task<ProfileDto> SetProfileImageAsync(Guid userId, Guid fileId, CancellationToken cancellationToken = default);

    Task DeleteProfilePictureAsync(Guid userId, CancellationToken cancellationToken = default);

    Task<PhoneDto> AddPhoneAsync(Guid userId, AddPhoneDto dto, CancellationToken cancellationToken = default);

    Task DeletePhoneAsync(Guid userId, Guid phoneId, CancellationToken cancellationToken = default);

    Task SetDefaultPhoneAsync(Guid userId, Guid phoneId, CancellationToken cancellationToken = default);

    Task<AddressDto> AddAddressAsync(Guid userId, AddAddressDto dto, CancellationToken cancellationToken = default);

    Task<AddressDto> UpdateAddressAsync(Guid userId, Guid addressId, UpdateAddressDto dto, CancellationToken cancellationToken = default);

    Task DeleteAddressAsync(Guid userId, Guid addressId, CancellationToken cancellationToken = default);

    Task SetDefaultAddressAsync(Guid userId, Guid addressId, CancellationToken cancellationToken = default);
}
