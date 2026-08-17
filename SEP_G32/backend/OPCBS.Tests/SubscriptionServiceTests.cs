using AutoMapper;
using Moq;
using OPCBS.Application.Interfaces.Repositories;
using OPCBS.Application.Interfaces.Services;
using OPCBS.Application.Services;
using OPCBS.Domain.Entities;
using OPCBS.Domain.Enums;
using Xunit;

namespace OPCBS.Tests;

public class SubscriptionServiceTests
{
    private readonly Mock<IRepository<DoctorSubscription>> _mockSubRepo = new();
    private readonly Mock<IRepository<ServicePackage>> _mockPkgRepo = new();
    private readonly Mock<IRepository<DoctorProfile>> _mockDoctorRepo = new();
    private readonly Mock<IRepository<User>> _mockUserRepo = new();
    private readonly Mock<IRepository<PaymentTransaction>> _mockPaymentRepo = new();
    private readonly Mock<IPaymentService> _mockPaymentService = new();
    private readonly Mock<IUnitOfWork> _mockUow = new();
    private readonly Mock<IMapper> _mockMapper = new();

    private readonly SubscriptionService _service;

    public SubscriptionServiceTests()
    {
        _service = new SubscriptionService(
            _mockSubRepo.Object,
            _mockPkgRepo.Object,
            _mockDoctorRepo.Object,
            _mockUserRepo.Object,
            _mockPaymentRepo.Object,
            _mockPaymentService.Object,
            _mockUow.Object,
            _mockMapper.Object);
    }

    [Fact]
    public async Task PurchaseAsync_DoctorNotFound_ReturnsError()
    {
        _mockDoctorRepo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new List<DoctorProfile>());

        var result = await _service.PurchaseAsync(Guid.NewGuid(), Guid.NewGuid(), "http://return", CancellationToken.None);

        Assert.False(result.Success);
    }

    [Fact]
    public async Task PurchaseAsync_PackageNotFoundOrInactive_ReturnsError()
    {
        var doctorUserId = Guid.NewGuid();
        var doc = new DoctorProfile { Id = Guid.NewGuid(), UserId = doctorUserId, User = new User { Email = "d@test.com", PasswordHash = "h", FullName = "D", PhoneNumber = "1", Role = new Role { Name = "Doctor" } } };
        _mockDoctorRepo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new List<DoctorProfile> { doc });
        _mockPkgRepo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync((ServicePackage?)null);

        var result = await _service.PurchaseAsync(doctorUserId, Guid.NewGuid(), "http://return", CancellationToken.None);

        Assert.False(result.Success);
    }

    [Fact]
    public async Task PurchaseAsync_FreePackage_ActivatesImmediately()
    {
        var doctorUserId = Guid.NewGuid();
        var doctorProfileId = Guid.NewGuid();
        var packageId = Guid.NewGuid();

        var doc = new DoctorProfile { Id = doctorProfileId, UserId = doctorUserId, User = new User { Email = "d@test.com", PasswordHash = "h", FullName = "D", PhoneNumber = "1", Role = new Role { Name = "Doctor" } } };
        var package = new ServicePackage { Id = packageId, Name = "Free Trial", Price = 0, DurationDays = 14, IsActive = true };

        _mockDoctorRepo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new List<DoctorProfile> { doc });
        _mockPkgRepo.Setup(r => r.GetByIdAsync(packageId, It.IsAny<CancellationToken>())).ReturnsAsync(package);
        _mockSubRepo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new List<DoctorSubscription>());

        var result = await _service.PurchaseAsync(doctorUserId, packageId, "http://return", CancellationToken.None);

        Assert.True(result.Success);
        Assert.Equal(SubscriptionStatus.Active.ToString(), result.Data!.Status);
        _mockSubRepo.Verify(r => r.AddAsync(It.IsAny<DoctorSubscription>(), It.IsAny<CancellationToken>()), Times.Once);
        _mockPaymentRepo.Verify(r => r.AddAsync(It.IsAny<PaymentTransaction>(), It.IsAny<CancellationToken>()), Times.Once);
        _mockUow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task PurchaseAsync_PaidPackage_GeneratesPaymentUrlAndSetsPending()
    {
        var doctorUserId = Guid.NewGuid();
        var doctorProfileId = Guid.NewGuid();
        var packageId = Guid.NewGuid();

        var doc = new DoctorProfile { Id = doctorProfileId, UserId = doctorUserId, User = new User { Email = "d@test.com", PasswordHash = "h", FullName = "D", PhoneNumber = "1", Role = new Role { Name = "Doctor" } } };
        var package = new ServicePackage { Id = packageId, Name = "Premium Doctor", Price = 500000, DurationDays = 30, IsActive = true };

        _mockDoctorRepo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new List<DoctorProfile> { doc });
        _mockPkgRepo.Setup(r => r.GetByIdAsync(packageId, It.IsAny<CancellationToken>())).ReturnsAsync(package);
        _mockPaymentService.Setup(p => p.CreatePaymentUrlAsync(It.IsAny<Guid>(), 500000, It.IsAny<string>(), "http://return", It.IsAny<CancellationToken>()))
            .ReturnsAsync("http://vnpay.vn/pay/123");

        var result = await _service.PurchaseAsync(doctorUserId, packageId, "http://return", CancellationToken.None);

        Assert.True(result.Success);
        Assert.Equal(SubscriptionStatus.PendingPayment.ToString(), result.Data!.Status);
        Assert.Equal("http://vnpay.vn/pay/123", result.Data.PaymentUrl);
        _mockSubRepo.Verify(r => r.AddAsync(It.IsAny<DoctorSubscription>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreateSubscriptionDirectAsync_DoctorNotFound_ReturnsError()
    {
        _mockDoctorRepo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new List<DoctorProfile>());

        var result = await _service.CreateSubscriptionDirectAsync(Guid.NewGuid(), Guid.NewGuid(), CancellationToken.None);

        Assert.False(result.Success);
    }

    [Fact]
    public async Task CreateSubscriptionDirectAsync_InactivePackage_ReturnsError()
    {
        var doctorUserId = Guid.NewGuid();
        var doc = new DoctorProfile { Id = Guid.NewGuid(), UserId = doctorUserId, User = new User { Email = "d@test.com", PasswordHash = "h", FullName = "D", PhoneNumber = "1", Role = new Role { Name = "Doctor" } } };
        var package = new ServicePackage { Id = Guid.NewGuid(), Name = "Inactive Plan", IsActive = false };

        _mockDoctorRepo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new List<DoctorProfile> { doc });
        _mockPkgRepo.Setup(r => r.GetByIdAsync(package.Id, It.IsAny<CancellationToken>())).ReturnsAsync(package);

        var result = await _service.CreateSubscriptionDirectAsync(doctorUserId, package.Id, CancellationToken.None);

        Assert.False(result.Success);
    }

    [Fact]
    public async Task GetCurrentSubscriptionAsync_NoSubscription_ReturnsNull()
    {
        var doctorUserId = Guid.NewGuid();
        var doc = new DoctorProfile { Id = Guid.NewGuid(), UserId = doctorUserId, User = new User { Email = "d@test.com", PasswordHash = "h", FullName = "D", PhoneNumber = "1", Role = new Role { Name = "Doctor" } } };
        _mockDoctorRepo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new List<DoctorProfile> { doc });
        _mockSubRepo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new List<DoctorSubscription>());

        var result = await _service.GetActiveSubscriptionAsync(doctorUserId, CancellationToken.None);

        Assert.False(result.Success);
    }

    [Fact]
    public async Task GetCurrentSubscriptionAsync_ActiveSubscription_ReturnsEnrichedDto()
    {
        var doctorUserId = Guid.NewGuid();
        var doctorProfileId = Guid.NewGuid();
        var packageId = Guid.NewGuid();

        var doc = new DoctorProfile { Id = doctorProfileId, UserId = doctorUserId, User = new User { Email = "d@test.com", PasswordHash = "h", FullName = "D", PhoneNumber = "1", Role = new Role { Name = "Doctor" } } };
        var package = new ServicePackage { Id = packageId, Name = "Pro Plan", DurationDays = 30 };
        var sub = new DoctorSubscription
        {
            Id = Guid.NewGuid(),
            DoctorProfileId = doctorProfileId,
            ServicePackageId = packageId,
            Status = SubscriptionStatus.Active,
            StartDate = DateTime.UtcNow.AddDays(-5),
            ExpirationDate = DateTime.UtcNow.AddDays(25),
            ServicePackage = package,
            DoctorProfile = doc
        };

        _mockDoctorRepo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new List<DoctorProfile> { doc });
        _mockSubRepo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new List<DoctorSubscription> { sub });
        _mockPkgRepo.Setup(r => r.GetByIdAsync(packageId, It.IsAny<CancellationToken>())).ReturnsAsync(package);

        var result = await _service.GetActiveSubscriptionAsync(doctorUserId, CancellationToken.None);

        Assert.True(result.Success);
        Assert.NotNull(result.Data);
        Assert.Equal("Pro Plan", result.Data.PackageName);
    }

    [Fact]
    public async Task GetHistoryAsync_ReturnsAllSubscriptionsForDoctor()
    {
        var doctorUserId = Guid.NewGuid();
        var doctorProfileId = Guid.NewGuid();
        var packageId = Guid.NewGuid();

        var doc = new DoctorProfile { Id = doctorProfileId, UserId = doctorUserId, User = new User { Email = "d@test.com", PasswordHash = "h", FullName = "D", PhoneNumber = "1", Role = new Role { Name = "Doctor" } } };
        var package = new ServicePackage { Id = packageId, Name = "Basic" };
        var subs = new List<DoctorSubscription>
        {
            new() { Id = Guid.NewGuid(), DoctorProfileId = doctorProfileId, ServicePackageId = packageId, Status = SubscriptionStatus.Active, ExpirationDate = DateTime.UtcNow.AddDays(10), ServicePackage = package, DoctorProfile = doc },
            new() { Id = Guid.NewGuid(), DoctorProfileId = doctorProfileId, ServicePackageId = packageId, Status = SubscriptionStatus.Expired, ExpirationDate = DateTime.UtcNow.AddDays(-20), ServicePackage = package, DoctorProfile = doc }
        };

        _mockDoctorRepo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new List<DoctorProfile> { doc });
        _mockSubRepo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(subs);
        _mockPkgRepo.Setup(r => r.GetByIdAsync(packageId, It.IsAny<CancellationToken>())).ReturnsAsync(package);

        var result = await _service.GetSubscriptionHistoryAsync(doctorUserId, CancellationToken.None);

        Assert.True(result.Success);
        Assert.Equal(2, result.Data!.Count);
    }

    [Fact]
    public async Task GetSubscriptionByIdAsync_Existing_ReturnsDto()
    {
        var subId = Guid.NewGuid();
        var packageId = Guid.NewGuid();
        var doc = new DoctorProfile { Id = Guid.NewGuid(), User = new User { Email = "d@test.com", PasswordHash = "h", FullName = "D", PhoneNumber = "1", Role = new Role { Name = "Doctor" } } };
        var package = new ServicePackage { Id = packageId, Name = "Basic Plan" };
        var sub = new DoctorSubscription { Id = subId, ServicePackageId = packageId, ServicePackage = package, Status = SubscriptionStatus.Active, DoctorProfile = doc };

        _mockSubRepo.Setup(r => r.GetByIdAsync(subId, It.IsAny<CancellationToken>())).ReturnsAsync(sub);
        _mockPkgRepo.Setup(r => r.GetByIdAsync(packageId, It.IsAny<CancellationToken>())).ReturnsAsync(package);
        _mockDoctorRepo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new List<DoctorProfile> { doc });
        _mockUserRepo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new List<User>());

        var result = await _service.GetSubscriptionByIdAsync(subId, CancellationToken.None);

        Assert.True(result.Success);
        Assert.NotNull(result.Data);
        Assert.Equal("Basic Plan", result.Data.PackageName);
    }

    [Fact]
    public async Task GetSubscriptionByIdAsync_NotFound_ReturnsError()
    {
        _mockSubRepo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync((DoctorSubscription?)null);

        var result = await _service.GetSubscriptionByIdAsync(Guid.NewGuid(), CancellationToken.None);

        Assert.False(result.Success);
    }

    [Fact]
    public async Task GetAllSubscriptionsAsync_ReturnsFilteredList()
    {
        var packageId = Guid.NewGuid();
        var doc = new DoctorProfile { Id = Guid.NewGuid(), User = new User { Email = "d@test.com", PasswordHash = "h", FullName = "D", PhoneNumber = "1", Role = new Role { Name = "Doctor" } } };
        var package = new ServicePackage { Id = packageId, Name = "Basic Plan" };
        var subs = new List<DoctorSubscription>
        {
            new() { Id = Guid.NewGuid(), ServicePackageId = packageId, ServicePackage = package, Status = SubscriptionStatus.Active, ExpirationDate = DateTime.UtcNow.AddDays(10), DoctorProfile = doc }
        };

        _mockSubRepo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(subs);
        _mockPkgRepo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new List<ServicePackage> { package });
        _mockDoctorRepo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new List<DoctorProfile> { doc });
        _mockUserRepo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new List<User>());

        var result = await _service.GetAllSubscriptionsAsync(null, null, 1, 10, CancellationToken.None);

        Assert.True(result.Success);
        Assert.Single(result.Data!);
    }

    [Fact]
    public async Task GetSubscriptionHistoryAsync_DoctorNotFound_ReturnsError()
    {
        _mockDoctorRepo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new List<DoctorProfile>());

        var result = await _service.GetSubscriptionHistoryAsync(Guid.NewGuid(), CancellationToken.None);

        Assert.False(result.Success);
    }

    [Fact]
    public async Task GetActiveSubscriptionAsync_DoctorNotFound_ReturnsError()
    {
        _mockDoctorRepo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new List<DoctorProfile>());

        var result = await _service.GetActiveSubscriptionAsync(Guid.NewGuid(), CancellationToken.None);

        Assert.False(result.Success);
    }

    [Fact]
    public async Task GetAllSubscriptionsAsync_StatusFilter_FiltersCorrectly()
    {
        var packageId = Guid.NewGuid();
        var doc = new DoctorProfile { Id = Guid.NewGuid(), User = new User { Email = "d@test.com", PasswordHash = "h", FullName = "D", PhoneNumber = "1", Role = new Role { Name = "Doctor" } } };
        var package = new ServicePackage { Id = packageId, Name = "Basic Plan" };
        var subs = new List<DoctorSubscription>
        {
            new() { Id = Guid.NewGuid(), ServicePackageId = packageId, ServicePackage = package, Status = SubscriptionStatus.Active, ExpirationDate = DateTime.UtcNow.AddDays(10), DoctorProfile = doc },
            new() { Id = Guid.NewGuid(), ServicePackageId = packageId, ServicePackage = package, Status = SubscriptionStatus.Cancelled, ExpirationDate = DateTime.UtcNow.AddDays(-5), DoctorProfile = doc }
        };

        _mockSubRepo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(subs);
        _mockPkgRepo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new List<ServicePackage> { package });
        _mockDoctorRepo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new List<DoctorProfile> { doc });
        _mockUserRepo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new List<User>());

        var result = await _service.GetAllSubscriptionsAsync("Active", null, 1, 10, CancellationToken.None);

        Assert.True(result.Success);
        Assert.Single(result.Data!);
    }
}
