using Athary.Application.Common;
using Athary.Application.DTOs.Auth;
using Athary.Application.DTOs.Category;
using Athary.Application.DTOs.Courses;
using Athary.Application.DTOs.Profile;
using Athary.Application.Validators;
using Athary.Application.Validators.Category;
using Athary.Domain.Enums;
using FluentAssertions;
using FluentValidation.TestHelper;

namespace Athary.Application.UnitTests.Validators;

public sealed class LoginDtoValidatorTests
{
    private readonly LoginDtoValidator _sut = new();

    [Fact]
    public void Should_BeValid_WhenAllFieldsCorrect()
    {
        var result = _sut.TestValidate(new LoginDto
        {
            Email = "user@example.com",
            Password = "Password123"
        });

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Should_Fail_WhenEmailEmpty()
    {
        var result = _sut.TestValidate(new LoginDto
        {
            Email = "",
            Password = "Password123"
        });

        result.IsValid.Should().BeFalse();
        result.ShouldHaveValidationErrorFor(x => x.Email);
    }

    [Fact]
    public void Should_Fail_WhenEmailInvalid()
    {
        var result = _sut.TestValidate(new LoginDto
        {
            Email = "not-an-email",
            Password = "Password123"
        });

        result.IsValid.Should().BeFalse();
        result.ShouldHaveValidationErrorFor(x => x.Email);
    }

    [Fact]
    public void Should_Fail_WhenPasswordEmpty()
    {
        var result = _sut.TestValidate(new LoginDto
        {
            Email = "user@example.com",
            Password = ""
        });

        result.IsValid.Should().BeFalse();
        result.ShouldHaveValidationErrorFor(x => x.Password);
    }
}

public sealed class RegisterDtoValidatorTests
{
    private readonly RegisterDtoValidator _sut = new();

    private readonly RegisterDto _validDto = new()
    {
        FirstName = "John",
        LastName = "Doe",
        Email = "john@example.com",
        Password = "Password123",
        ConfirmPassword = "Password123"
    };

    [Fact]
    public void Should_BeValid_WhenAllFieldsCorrect()
    {
        var result = _sut.TestValidate(_validDto);
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Should_Fail_WhenFirstNameEmpty()
    {
        var result = _sut.TestValidate(_validDto with { FirstName = "" });
        result.ShouldHaveValidationErrorFor(x => x.FirstName);
    }

    [Fact]
    public void Should_Fail_WhenLastNameEmpty()
    {
        var result = _sut.TestValidate(_validDto with { LastName = "" });
        result.ShouldHaveValidationErrorFor(x => x.LastName);
    }

    [Fact]
    public void Should_Fail_WhenEmailEmpty()
    {
        var result = _sut.TestValidate(_validDto with { Email = "" });
        result.ShouldHaveValidationErrorFor(x => x.Email);
    }

    [Fact]
    public void Should_Fail_WhenPasswordTooShort()
    {
        var result = _sut.TestValidate(_validDto with { Password = "Short1A" });
        result.ShouldHaveValidationErrorFor(x => x.Password);
    }

    [Fact]
    public void Should_Fail_WhenPasswordMissingDigit()
    {
        var result = _sut.TestValidate(_validDto with { Password = "PasswordOnly" });
        result.ShouldHaveValidationErrorFor(x => x.Password);
    }

    [Fact]
    public void Should_Fail_WhenPasswordMissingLowercase()
    {
        var result = _sut.TestValidate(_validDto with { Password = "PASSWORD123" });
        result.ShouldHaveValidationErrorFor(x => x.Password);
    }

    [Fact]
    public void Should_Fail_WhenPasswordMissingUppercase()
    {
        var result = _sut.TestValidate(_validDto with { Password = "password123" });
        result.ShouldHaveValidationErrorFor(x => x.Password);
    }

    [Fact]
    public void Should_Fail_WhenPasswordsDoNotMatch()
    {
        var result = _sut.TestValidate(_validDto with { ConfirmPassword = "Different123" });
        result.ShouldHaveValidationErrorFor(x => x.ConfirmPassword);
    }

    [Fact]
    public void Should_BeValid_WhenOptionalFieldsNull()
    {
        var result = _sut.TestValidate(_validDto with
        {
            PhoneNumber = null,
            Gender = null,
            DateOfBirth = null,
            Country = null,
            City = null,
            PostalCode = null
        });

        result.IsValid.Should().BeTrue();
    }
}

public sealed class ChangePasswordDtoValidatorTests
{
    private readonly ChangePasswordDtoValidator _sut = new();

    [Fact]
    public void Should_BeValid_WhenAllFieldsCorrect()
    {
        var result = _sut.TestValidate(new ChangePasswordDto
        {
            CurrentPassword = "OldPass123",
            NewPassword = "NewPass456"
        });

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Should_Fail_WhenCurrentPasswordEmpty()
    {
        var result = _sut.TestValidate(new ChangePasswordDto
        {
            CurrentPassword = "",
            NewPassword = "NewPass456"
        });

        result.ShouldHaveValidationErrorFor(x => x.CurrentPassword);
    }

    [Fact]
    public void Should_Fail_WhenNewPasswordTooShort()
    {
        var result = _sut.TestValidate(new ChangePasswordDto
        {
            CurrentPassword = "OldPass123",
            NewPassword = "Short1A"
        });

        result.ShouldHaveValidationErrorFor(x => x.NewPassword);
    }
}

public sealed class ForgotPasswordDtoValidatorTests
{
    private readonly ForgotPasswordDtoValidator _sut = new();

    [Fact]
    public void Should_BeValid_WhenEmailCorrect()
    {
        var result = _sut.TestValidate(new ForgotPasswordDto { Email = "user@example.com" });
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Should_Fail_WhenEmailEmpty()
    {
        var result = _sut.TestValidate(new ForgotPasswordDto { Email = "" });
        result.ShouldHaveValidationErrorFor(x => x.Email);
    }
}

public sealed class RefreshTokenDtoValidatorTests
{
    private readonly RefreshTokenDtoValidator _sut = new();

    [Fact]
    public void Should_BeValid_WhenTokenProvided()
    {
        var result = _sut.TestValidate(new RefreshTokenDto { RefreshToken = "some-token" });
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Should_Fail_WhenTokenEmpty()
    {
        var result = _sut.TestValidate(new RefreshTokenDto { RefreshToken = "" });
        result.ShouldHaveValidationErrorFor(x => x.RefreshToken);
    }
}

public sealed class VerifyEmailDtoValidatorTests
{
    private readonly VerifyEmailDtoValidator _sut = new();

    [Fact]
    public void Should_BeValid_WhenAllCorrect()
    {
        var result = _sut.TestValidate(new VerifyEmailDto
        {
            Email = "user@example.com",
            Token = "abc123"
        });

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Should_Fail_WhenTokenEmpty()
    {
        var result = _sut.TestValidate(new VerifyEmailDto
        {
            Email = "user@example.com",
            Token = ""
        });

        result.ShouldHaveValidationErrorFor(x => x.Token);
    }
}

public sealed class CreateCourseDtoValidatorTests
{
    private readonly CreateCourseDtoValidator _sut = new();

    [Fact]
    public void Should_BeValid_WhenAllFieldsCorrect()
    {
        var result = _sut.TestValidate(new CreateCourseDto
        {
            Title = "ASP.NET Core Course",
            Description = "Learn ASP.NET Core",
            CategoryId = Guid.NewGuid(),
            Price = 49.99m,
            Level = CourseLevel.Beginner
        });

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Should_Fail_WhenTitleEmpty()
    {
        var result = _sut.TestValidate(new CreateCourseDto
        {
            Title = "",
            CategoryId = Guid.NewGuid(),
            Price = 0
        });

        result.ShouldHaveValidationErrorFor(x => x.Title);
    }

    [Fact]
    public void Should_Fail_WhenTitleTooLong()
    {
        var result = _sut.TestValidate(new CreateCourseDto
        {
            Title = new string('x', 201),
            CategoryId = Guid.NewGuid(),
            Price = 0
        });

        result.ShouldHaveValidationErrorFor(x => x.Title);
    }

    [Fact]
    public void Should_Fail_WhenCategoryIdEmpty()
    {
        var result = _sut.TestValidate(new CreateCourseDto
        {
            Title = "Course",
            CategoryId = Guid.Empty,
            Price = 0
        });

        result.ShouldHaveValidationErrorFor(x => x.CategoryId);
    }

    [Fact]
    public void Should_Fail_WhenPriceNegative()
    {
        var result = _sut.TestValidate(new CreateCourseDto
        {
            Title = "Course",
            CategoryId = Guid.NewGuid(),
            Price = -10
        });

        result.ShouldHaveValidationErrorFor(x => x.Price);
    }
}

public sealed class CreateCategoryValidatorTests
{
    private readonly CreateCategoryValidator _sut = new();

    [Fact]
    public void Should_BeValid_WhenAllFieldsCorrect()
    {
        var result = _sut.TestValidate(new CreateCategoryDto
        {
            Name = "Programming",
            Position = 1
        });

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Should_Fail_WhenNameEmpty()
    {
        var result = _sut.TestValidate(new CreateCategoryDto
        {
            Name = "",
            Position = 0
        });

        result.ShouldHaveValidationErrorFor(x => x.Name);
    }

    [Fact]
    public void Should_Fail_WhenPositionNegative()
    {
        var result = _sut.TestValidate(new CreateCategoryDto
        {
            Name = "Programming",
            Position = -1
        });

        result.ShouldHaveValidationErrorFor(x => x.Position);
    }
}

public sealed class AddPhoneDtoValidatorTests
{
    private readonly AddPhoneDtoValidator _sut = new();

    [Fact]
    public void Should_BeValid_WhenPhoneCorrect()
    {
        var result = _sut.TestValidate(new AddPhoneDto
        {
            PhoneNumber = "+201234567890",
            Type = PhoneType.Primary
        });

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Should_Fail_WhenPhoneEmpty()
    {
        var result = _sut.TestValidate(new AddPhoneDto
        {
            PhoneNumber = "",
            Type = PhoneType.Primary
        });

        result.ShouldHaveValidationErrorFor(x => x.PhoneNumber);
    }

    [Fact]
    public void Should_Fail_WhenPhoneInvalid()
    {
        var result = _sut.TestValidate(new AddPhoneDto
        {
            PhoneNumber = "abc",
            Type = PhoneType.Primary
        });

        result.ShouldHaveValidationErrorFor(x => x.PhoneNumber);
    }
}

public sealed class ApiResponseTests
{
    [Fact]
    public void SuccessResponse_Should_SetSuccessAndData()
    {
        var result = ApiResponse<string>.SuccessResponse("hello", "OK");

        result.Success.Should().BeTrue();
        result.Data.Should().Be("hello");
        result.Message.Should().Be("OK");
        result.Errors.Should().BeNull();
    }

    [Fact]
    public void FailureResponse_Should_SetFailureAndMessage()
    {
        var result = ApiResponse<int>.FailureResponse("Error occurred", ["Bad request"]);

        result.Success.Should().BeFalse();
        result.Message.Should().Be("Error occurred");
        result.Errors.Should().ContainSingle().Which.Should().Be("Bad request");
        result.Data.Should().Be(0);
    }
}
