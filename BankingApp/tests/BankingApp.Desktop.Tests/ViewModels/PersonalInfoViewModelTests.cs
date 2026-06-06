namespace BankingApp.Desktop.Tests.ViewModels;

using Desktop.ViewModels;
using Contracts.Features.UserProfile.Dtos;
using Contracts.Features.UserProfile.Services;
using ErrorOr;
using Microsoft.Extensions.Logging.Abstractions;
using Shared.Enums;

public class PersonalInfoViewModelTests
{
    private readonly Mock<IProfileService> _profileClientService = new(MockBehavior.Strict);

    [Fact]
    public async Task LoadProfile_WhenApiReturnsProfile_ShouldPopulateProfileAndSetSuccessState()
    {
        // Arrange
        const int userId = 7;
        const string fullName = "Test User";
        const string phoneNumber = "0712345678";
        var profile = new ProfileDto
        {
            UserId = userId,
            FullName = fullName,
            PhoneNumber = phoneNumber,
            Email = "test@example.com",
        };

        var viewModel = new PersonalInfoViewModel(_profileClientService.Object, NullLogger<PersonalInfoViewModel>.Instance);

        _profileClientService
            .Setup(service => service.GetProfileAsync())
            .ReturnsAsync(profile);

        // Act
        bool success = await viewModel.LoadProfile();

        // Assert
        success.Should().BeTrue();
        viewModel.ProfileInfo.Should().BeSameAs(profile);
        viewModel.State.Should().Be(ProfileState.UpdateSuccess);
        viewModel.HasPhoneNumber.Should().BeTrue();
    }

    [Fact]
    public async Task LoadProfile_WhenApiFails_ShouldSetErrorStateAndLeaveDefaultProfile()
    {
        // Arrange
        var viewModel = new PersonalInfoViewModel(_profileClientService.Object, NullLogger<PersonalInfoViewModel>.Instance);

        _profileClientService
            .Setup(profileClientService => profileClientService.GetProfileAsync())
            .ReturnsAsync(Error.Failure(description: "server down"));

        // Act
        bool success = await viewModel.LoadProfile();

        // Assert
        success.Should().BeFalse();
        viewModel.State.Should().Be(ProfileState.Error);
        viewModel.ProfileInfo.UserId.Should().BeNull();
        viewModel.HasPhoneNumber.Should().BeFalse();
    }

    [Fact]
    public async Task UpdatePersonalInfo_WhenUserIdIsNull_ShouldSetErrorState()
    {
        // Arrange
        var viewModel = new PersonalInfoViewModel(_profileClientService.Object, NullLogger<PersonalInfoViewModel>.Instance);

        // Act
        bool success = await viewModel.UpdatePersonalInfo("0712345678", "123 Main St", "password");

        // Assert
        success.Should().BeFalse();
        viewModel.State.Should().Be(ProfileState.Error);
    }

    [Fact]
    public async Task UpdatePersonalInfo_WhenApiSucceeds_ShouldTrimInputPreserveExistingFieldsAndUpdateProfile()
    {
        // Arrange
        const int userId = 7;
        DateTime dateOfBirth = new(1999, 12, 31);
        UpdateProfileRequest? sentRequest = null;
        var viewModel = new PersonalInfoViewModel(_profileClientService.Object, NullLogger<PersonalInfoViewModel>.Instance)
            {
                ProfileInfo =
                {
                    UserId = userId,
                    FullName = "Existing Name",
                    PhoneNumber = "0000",
                    Address = "Old Address",
                    DateOfBirth = dateOfBirth,
                    Nationality = "Romanian",
                    PreferredLanguage = "ro"
                }
            };

        _profileClientService
            .Setup(service => service.UpdateProfileAsync(It.IsAny<UpdateProfileRequest>()))
            .Callback<UpdateProfileRequest, CancellationToken>((request, _) => sentRequest = request)
            .ReturnsAsync(Result.Success);

        // Act
        bool success = await viewModel.UpdatePersonalInfo(" 0712345678 ", " 123 Main St ", "password", "  Updated Name ");

        // Assert
        success.Should().BeTrue();
        sentRequest.Should().NotBeNull();
        sentRequest!.UserId.Should().Be(userId);
        sentRequest.PhoneNumber.Should().Be("0712345678");
        sentRequest.Address.Should().Be("123 Main St");
        sentRequest.FullName.Should().Be("Updated Name");
        sentRequest.DateOfBirth.Should().Be(dateOfBirth);
        sentRequest.Nationality.Should().Be("Romanian");
        sentRequest.PreferredLanguage.Should().Be("ro");
        viewModel.ProfileInfo.FullName.Should().Be("Updated Name");
        viewModel.ProfileInfo.PhoneNumber.Should().Be("0712345678");
        viewModel.ProfileInfo.Address.Should().Be("123 Main St");
        viewModel.State.Should().Be(ProfileState.UpdateSuccess);
    }

    [Fact]
    public async Task UpdatePersonalInfo_WhenOptionalInputsAreBlank_ShouldUseNullsAndKeepExistingFullName()
    {
        // Arrange
        UpdateProfileRequest? sentRequest = null;
        var viewModel = new PersonalInfoViewModel(_profileClientService.Object, NullLogger<PersonalInfoViewModel>.Instance)
            {
                ProfileInfo =
                {
                    UserId = 7,
                    FullName = "Existing Name",
                    PhoneNumber = "0711",
                    Address = "Old Address"
                }
            };

        _profileClientService
            .Setup(profileClientService => profileClientService.UpdateProfileAsync(It.IsAny<UpdateProfileRequest>()))
            .Callback<UpdateProfileRequest, CancellationToken>((request, _) => sentRequest = request)
            .ReturnsAsync(Result.Success);

        // Act
        bool success = await viewModel.UpdatePersonalInfo("   ", "   ", "password", "   ");

        // Assert
        success.Should().BeTrue();
        sentRequest.Should().NotBeNull();
        sentRequest!.PhoneNumber.Should().BeNull();
        sentRequest.Address.Should().BeNull();
        sentRequest.FullName.Should().Be("Existing Name");
        viewModel.ProfileInfo.PhoneNumber.Should().BeNull();
        viewModel.ProfileInfo.Address.Should().BeNull();
        viewModel.ProfileInfo.FullName.Should().Be("Existing Name");
        viewModel.State.Should().Be(ProfileState.UpdateSuccess);
    }

    [Fact]
    public async Task UpdatePersonalInfo_WhenApiFails_ShouldNotMutateProfileAndSetErrorState()
    {
        // Arrange
        var viewModel = new PersonalInfoViewModel(_profileClientService.Object, NullLogger<PersonalInfoViewModel>.Instance)
            {
                ProfileInfo =
                {
                    UserId = 7,
                    FullName = "Existing Name",
                    PhoneNumber = "0711",
                    Address = "Old Address"
                }
            };

        _profileClientService
            .Setup(profileClientService => profileClientService.UpdateProfileAsync(It.IsAny<UpdateProfileRequest>()))
            .ReturnsAsync(Error.Failure(description: "update failed"));

        // Act
        bool success = await viewModel.UpdatePersonalInfo("0722", "New Address", "password", "Updated Name");

        // Assert
        success.Should().BeFalse();
        viewModel.ProfileInfo.FullName.Should().Be("Existing Name");
        viewModel.ProfileInfo.PhoneNumber.Should().Be("0711");
        viewModel.ProfileInfo.Address.Should().Be("Old Address");
        viewModel.State.Should().Be(ProfileState.Error);
    }

    [Fact]
    public async Task VerifyPassword_WhenUserIdIsNull_ShouldSetErrorState()
    {
        // Arrange
        var viewModel = new PersonalInfoViewModel(_profileClientService.Object, NullLogger<PersonalInfoViewModel>.Instance);

        // Act
        bool success = await viewModel.VerifyPassword("password");

        // Assert
        success.Should().BeFalse();
        viewModel.State.Should().Be(ProfileState.Error);
    }

    [Fact]
    public async Task VerifyPassword_WhenApiReturnsTrue_ShouldSetSuccessState()
    {
        // Arrange
        var viewModel = new PersonalInfoViewModel(_profileClientService.Object, NullLogger<PersonalInfoViewModel>.Instance)
            {
                ProfileInfo =
                {
                    UserId = 7
                }
            };

        _profileClientService
            .Setup(profileClientService => profileClientService.VerifyPasswordAsync("correct-password"))
            .ReturnsAsync(true);

        // Act
        bool success = await viewModel.VerifyPassword("correct-password");

        // Assert
        success.Should().BeTrue();
        viewModel.State.Should().Be(ProfileState.UpdateSuccess);
    }

    [Fact]
    public async Task VerifyPassword_WhenApiReturnsFalse_ShouldSetErrorState()
    {
        // Arrange
        var viewModel = new PersonalInfoViewModel(_profileClientService.Object, NullLogger<PersonalInfoViewModel>.Instance)
            {
                ProfileInfo =
                {
                    UserId = 7
                }
            };

        _profileClientService
            .Setup(service => service.VerifyPasswordAsync("wrong-password"))
            .ReturnsAsync(false);

        // Act
        bool success = await viewModel.VerifyPassword("wrong-password");

        // Assert
        success.Should().BeFalse();
        viewModel.State.Should().Be(ProfileState.Error);
    }

    [Fact]
    public async Task VerifyPassword_WhenApiFails_ShouldSetErrorState()
    {
        // Arrange
        var viewModel = new PersonalInfoViewModel(_profileClientService.Object, NullLogger<PersonalInfoViewModel>.Instance)
            {
                ProfileInfo =
                {
                    UserId = 7
                }
            };

        _profileClientService
            .Setup(profileClientService => profileClientService.VerifyPasswordAsync("password"))
            .ReturnsAsync(Error.Failure(description: "verification failed"));

        // Act
        bool success = await viewModel.VerifyPassword("password");

        // Assert
        success.Should().BeFalse();
        viewModel.State.Should().Be(ProfileState.Error);
    }
}
