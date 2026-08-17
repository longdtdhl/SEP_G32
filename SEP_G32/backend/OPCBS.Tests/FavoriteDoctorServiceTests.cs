using Moq;
using OPCBS.Application.DTOs.Favorites;
using OPCBS.Application.Interfaces.Repositories;
using OPCBS.Application.Services;
using OPCBS.Domain.Entities;
using Xunit;

namespace OPCBS.Tests;

public class FavoriteDoctorServiceTests
{
    private readonly Mock<IRepository<FavoriteDoctor>> _mockFavRepo = new();
    private readonly Mock<IRepository<PatientProfile>> _mockPatientRepo = new();
    private readonly Mock<IRepository<DoctorProfile>> _mockDoctorRepo = new();
    private readonly Mock<IRepository<User>> _mockUserRepo = new();
    private readonly Mock<IUnitOfWork> _mockUow = new();

    private readonly FavoriteDoctorService _service;

    public FavoriteDoctorServiceTests()
    {
        _service = new FavoriteDoctorService(
            _mockFavRepo.Object,
            _mockPatientRepo.Object,
            _mockDoctorRepo.Object,
            _mockUserRepo.Object,
            _mockUow.Object);
    }

    [Fact]
    public async Task GetFavoritesAsync_NoFavorites_ReturnsEmptyList()
    {
        var patientUserId = Guid.NewGuid();
        _mockFavRepo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new List<FavoriteDoctor>());

        var result = await _service.GetFavoritesAsync(patientUserId, CancellationToken.None);

        Assert.True(result.Success);
        Assert.Empty(result.Data!);
    }

    [Fact]
    public async Task GetFavoritesAsync_ValidPatient_ReturnsFavorites()
    {
        var patientUserId = Guid.NewGuid();
        var doctorUserId = Guid.NewGuid();
        var doctorProfileId = Guid.NewGuid();

        var pUser = new User { Id = patientUserId, Email = "p@test.com", PasswordHash = "h", FullName = "Patient Alice", PhoneNumber = "123", Role = new Role { Name = "Patient" } };
        var dUser = new User { Id = doctorUserId, Email = "d@test.com", PasswordHash = "h", FullName = "Dr Bob", PhoneNumber = "456", Role = new Role { Name = "Doctor" } };
        var patient = new PatientProfile { Id = Guid.NewGuid(), UserId = patientUserId, User = pUser };
        var doctor = new DoctorProfile { Id = doctorProfileId, UserId = doctorUserId, User = dUser };
        var fav = new FavoriteDoctor { Id = Guid.NewGuid(), PatientId = patientUserId, DoctorId = doctorUserId, Patient = patient, Doctor = doctor };

        _mockFavRepo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new List<FavoriteDoctor> { fav });
        _mockDoctorRepo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new List<DoctorProfile> { doctor });
        _mockUserRepo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new List<User> { dUser });

        var result = await _service.GetFavoritesAsync(patientUserId, CancellationToken.None);

        Assert.True(result.Success);
        Assert.Single(result.Data!);
        Assert.Equal("Dr Bob", result.Data![0].DoctorName);
    }

    [Fact]
    public async Task AddFavoriteAsync_PatientNotFound_ReturnsError()
    {
        _mockPatientRepo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new List<PatientProfile>());

        var result = await _service.AddFavoriteAsync(Guid.NewGuid(), Guid.NewGuid(), CancellationToken.None);

        Assert.False(result.Success);
    }

    [Fact]
    public async Task AddFavoriteAsync_DoctorNotFound_ReturnsError()
    {
        var patientUserId = Guid.NewGuid();
        var patient = new PatientProfile { Id = Guid.NewGuid(), UserId = patientUserId, User = new User { Email = "p@test.com", PasswordHash = "h", FullName = "P", PhoneNumber = "1", Role = new Role { Name = "Patient" } } };
        _mockPatientRepo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new List<PatientProfile> { patient });
        _mockDoctorRepo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new List<DoctorProfile>());

        var result = await _service.AddFavoriteAsync(patientUserId, Guid.NewGuid(), CancellationToken.None);

        Assert.False(result.Success);
    }

    [Fact]
    public async Task AddFavoriteAsync_AlreadyFavorited_ReturnsError()
    {
        var patientUserId = Guid.NewGuid();
        var doctorUserId = Guid.NewGuid();
        var doctorProfileId = Guid.NewGuid();

        var pUser = new User { Id = patientUserId, Email = "p@test.com", PasswordHash = "h", FullName = "Patient Alice", PhoneNumber = "123", Role = new Role { Name = "Patient" } };
        var dUser = new User { Id = doctorUserId, Email = "d@test.com", PasswordHash = "h", FullName = "Dr Bob", PhoneNumber = "456", Role = new Role { Name = "Doctor" } };
        var patient = new PatientProfile { Id = Guid.NewGuid(), UserId = patientUserId, User = pUser };
        var doctor = new DoctorProfile { Id = doctorProfileId, UserId = doctorUserId, User = dUser };
        var existingFav = new FavoriteDoctor { Id = Guid.NewGuid(), PatientId = patientUserId, DoctorId = doctorUserId, Patient = patient, Doctor = doctor };

        _mockPatientRepo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new List<PatientProfile> { patient });
        _mockDoctorRepo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new List<DoctorProfile> { doctor });
        _mockFavRepo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new List<FavoriteDoctor> { existingFav });
        _mockUserRepo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new List<User> { dUser });

        var result = await _service.AddFavoriteAsync(patientUserId, doctorProfileId, CancellationToken.None);

        Assert.False(result.Success);
    }

    [Fact]
    public async Task AddFavoriteAsync_ValidNewFavorite_AddsAndSaves()
    {
        var patientUserId = Guid.NewGuid();
        var doctorUserId = Guid.NewGuid();
        var doctorProfileId = Guid.NewGuid();

        var pUser = new User { Id = patientUserId, Email = "p@test.com", PasswordHash = "h", FullName = "Patient Alice", PhoneNumber = "123", Role = new Role { Name = "Patient" } };
        var dUser = new User { Id = doctorUserId, Email = "d@test.com", PasswordHash = "h", FullName = "Dr Bob", PhoneNumber = "456", Role = new Role { Name = "Doctor" } };
        var patient = new PatientProfile { Id = Guid.NewGuid(), UserId = patientUserId, User = pUser };
        var doctor = new DoctorProfile { Id = doctorProfileId, UserId = doctorUserId, User = dUser };

        _mockPatientRepo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new List<PatientProfile> { patient });
        _mockDoctorRepo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new List<DoctorProfile> { doctor });
        _mockFavRepo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new List<FavoriteDoctor>());
        _mockUserRepo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new List<User> { dUser });

        var result = await _service.AddFavoriteAsync(patientUserId, doctorProfileId, CancellationToken.None);

        Assert.True(result.Success);
        _mockFavRepo.Verify(r => r.AddAsync(It.IsAny<FavoriteDoctor>(), It.IsAny<CancellationToken>()), Times.Once);
        _mockUow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task RemoveFavoriteAsync_FavoriteNotFound_ReturnsError()
    {
        var patientUserId = Guid.NewGuid();
        var doctorId = Guid.NewGuid();

        _mockDoctorRepo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new List<DoctorProfile>());
        _mockFavRepo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new List<FavoriteDoctor>());

        var result = await _service.RemoveFavoriteAsync(patientUserId, doctorId, CancellationToken.None);

        Assert.False(result.Success);
    }

    [Fact]
    public async Task RemoveFavoriteAsync_Valid_DeletesAndSaves()
    {
        var patientUserId = Guid.NewGuid();
        var doctorUserId = Guid.NewGuid();
        var doctorProfileId = Guid.NewGuid();

        var pUser = new User { Id = patientUserId, Email = "p@test.com", PasswordHash = "h", FullName = "Patient Alice", PhoneNumber = "123", Role = new Role { Name = "Patient" } };
        var dUser = new User { Id = doctorUserId, Email = "d@test.com", PasswordHash = "h", FullName = "Dr Bob", PhoneNumber = "456", Role = new Role { Name = "Doctor" } };
        var patient = new PatientProfile { Id = Guid.NewGuid(), UserId = patientUserId, User = pUser };
        var doctor = new DoctorProfile { Id = doctorProfileId, UserId = doctorUserId, User = dUser };
        var fav = new FavoriteDoctor { Id = Guid.NewGuid(), PatientId = patientUserId, DoctorId = doctorUserId, Patient = patient, Doctor = doctor };

        _mockDoctorRepo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new List<DoctorProfile> { doctor });
        _mockFavRepo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new List<FavoriteDoctor> { fav });

        var result = await _service.RemoveFavoriteAsync(patientUserId, doctorProfileId, CancellationToken.None);

        Assert.True(result.Success);
        _mockFavRepo.Verify(r => r.Delete(fav), Times.Once);
        _mockUow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task IsFavoriteAsync_Favorited_ReturnsTrue()
    {
        var patientUserId = Guid.NewGuid();
        var doctorUserId = Guid.NewGuid();
        var doctorProfileId = Guid.NewGuid();

        var pUser = new User { Id = patientUserId, Email = "p@test.com", PasswordHash = "h", FullName = "Patient Alice", PhoneNumber = "123", Role = new Role { Name = "Patient" } };
        var dUser = new User { Id = doctorUserId, Email = "d@test.com", PasswordHash = "h", FullName = "Dr Bob", PhoneNumber = "456", Role = new Role { Name = "Doctor" } };
        var patient = new PatientProfile { Id = Guid.NewGuid(), UserId = patientUserId, User = pUser };
        var doctor = new DoctorProfile { Id = doctorProfileId, UserId = doctorUserId, User = dUser };
        var fav = new FavoriteDoctor { Id = Guid.NewGuid(), PatientId = patientUserId, DoctorId = doctorUserId, Patient = patient, Doctor = doctor };

        _mockDoctorRepo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new List<DoctorProfile> { doctor });
        _mockFavRepo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new List<FavoriteDoctor> { fav });

        var result = await _service.IsFavoriteAsync(patientUserId, doctorProfileId, CancellationToken.None);

        Assert.True(result.Success);
        Assert.True(result.Data);
    }

    [Fact]
    public async Task IsFavoriteAsync_NotFavorited_ReturnsFalse()
    {
        var patientUserId = Guid.NewGuid();
        var doctorId = Guid.NewGuid();

        _mockDoctorRepo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new List<DoctorProfile>());
        _mockFavRepo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new List<FavoriteDoctor>());

        var result = await _service.IsFavoriteAsync(patientUserId, doctorId, CancellationToken.None);

        Assert.True(result.Success);
        Assert.False(result.Data);
    }

    [Fact]
    public async Task GetFavoritesAsync_MultipleFavorites_ReturnsAllEnriched()
    {
        var patientUserId = Guid.NewGuid();
        var docId1 = Guid.NewGuid();
        var docId2 = Guid.NewGuid();
        var docUserId1 = Guid.NewGuid();
        var docUserId2 = Guid.NewGuid();

        var pUser = new User { Id = patientUserId, Email = "p@test.com", PasswordHash = "h", FullName = "Patient Alice", PhoneNumber = "123", Role = new Role { Name = "Patient" } };
        var dUser1 = new User { Id = docUserId1, Email = "d1@test.com", PasswordHash = "h", FullName = "Dr 1", PhoneNumber = "1", Role = new Role { Name = "Doctor" } };
        var dUser2 = new User { Id = docUserId2, Email = "d2@test.com", PasswordHash = "h", FullName = "Dr 2", PhoneNumber = "2", Role = new Role { Name = "Doctor" } };
        var patient = new PatientProfile { Id = Guid.NewGuid(), UserId = patientUserId, User = pUser };
        var doc1 = new DoctorProfile { Id = docId1, UserId = docUserId1, User = dUser1 };
        var doc2 = new DoctorProfile { Id = docId2, UserId = docUserId2, User = dUser2 };

        var favs = new List<FavoriteDoctor>
        {
            new() { Id = Guid.NewGuid(), PatientId = patientUserId, DoctorId = docUserId1, Patient = patient, Doctor = doc1 },
            new() { Id = Guid.NewGuid(), PatientId = patientUserId, DoctorId = docUserId2, Patient = patient, Doctor = doc2 }
        };

        _mockFavRepo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(favs);
        _mockDoctorRepo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new List<DoctorProfile> { doc1, doc2 });
        _mockUserRepo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new List<User> { dUser1, dUser2 });

        var result = await _service.GetFavoritesAsync(patientUserId, CancellationToken.None);

        Assert.True(result.Success);
        Assert.Equal(2, result.Data!.Count);
    }

    [Fact]
    public async Task GetFavoritesAsync_DoctorNoLongerExists_HandlesGracefully()
    {
        var patientUserId = Guid.NewGuid();
        var orphanDoctorId = Guid.NewGuid();

        var pUser = new User { Id = patientUserId, Email = "p@test.com", PasswordHash = "h", FullName = "Patient Alice", PhoneNumber = "123", Role = new Role { Name = "Patient" } };
        var patient = new PatientProfile { Id = Guid.NewGuid(), UserId = patientUserId, User = pUser };
        var dummyDoc = new DoctorProfile { Id = orphanDoctorId, User = new User { Email = "d@test.com", PasswordHash = "h", FullName = "D", PhoneNumber = "1", Role = new Role { Name = "Doctor" } } };
        var fav = new FavoriteDoctor { Id = Guid.NewGuid(), PatientId = patientUserId, DoctorId = orphanDoctorId, Patient = patient, Doctor = dummyDoc };

        _mockFavRepo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new List<FavoriteDoctor> { fav });
        _mockDoctorRepo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new List<DoctorProfile>()); // Doctor deleted
        _mockUserRepo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new List<User>());

        var result = await _service.GetFavoritesAsync(patientUserId, CancellationToken.None);

        Assert.True(result.Success);
        Assert.Single(result.Data!);
    }

    [Fact]
    public async Task AddFavoriteAsync_DoctorWithoutUser_CreatesFavorite()
    {
        var patientUserId = Guid.NewGuid();
        var doctorProfileId = Guid.NewGuid();
        var doctorUserId = Guid.NewGuid();

        var patient = new PatientProfile { Id = Guid.NewGuid(), UserId = patientUserId, User = new User { Email = "p@test.com", PasswordHash = "h", FullName = "P", PhoneNumber = "1", Role = new Role { Name = "Patient" } } };
        var doctor = new DoctorProfile { Id = doctorProfileId, UserId = doctorUserId, User = new User { Email = "d@test.com", PasswordHash = "h", FullName = "D", PhoneNumber = "2", Role = new Role { Name = "Doctor" } } };

        _mockPatientRepo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new List<PatientProfile> { patient });
        _mockDoctorRepo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new List<DoctorProfile> { doctor });
        _mockFavRepo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new List<FavoriteDoctor>());
        _mockUserRepo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new List<User>());

        var result = await _service.AddFavoriteAsync(patientUserId, doctorProfileId, CancellationToken.None);

        Assert.True(result.Success);
    }

    [Fact]
    public async Task RemoveFavoriteAsync_PatientWithNoFavorites_ReturnsError()
    {
        var patientUserId = Guid.NewGuid();
        var doctorId = Guid.NewGuid();
        var doctor = new DoctorProfile { Id = doctorId, UserId = Guid.NewGuid(), User = new User { Email = "d@test.com", PasswordHash = "h", FullName = "D", PhoneNumber = "2", Role = new Role { Name = "Doctor" } } };

        _mockDoctorRepo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new List<DoctorProfile> { doctor });
        _mockFavRepo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new List<FavoriteDoctor>());

        var result = await _service.RemoveFavoriteAsync(patientUserId, doctorId, CancellationToken.None);

        Assert.False(result.Success);
    }

    [Fact]
    public async Task IsFavoriteAsync_EmptyDoctorList_ReturnsFalse()
    {
        _mockDoctorRepo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new List<DoctorProfile>());
        _mockFavRepo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new List<FavoriteDoctor>());

        var result = await _service.IsFavoriteAsync(Guid.NewGuid(), Guid.NewGuid(), CancellationToken.None);

        Assert.True(result.Success);
        Assert.False(result.Data);
    }
}
