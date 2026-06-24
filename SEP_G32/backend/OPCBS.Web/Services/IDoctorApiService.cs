using OPCBS.Web.DTOs;

namespace OPCBS.Web.Services;

public interface IDoctorApiService
{
    Task<IEnumerable<DoctorDto>> GetAllAsync();
    Task<DoctorDto?> GetByIdAsync(Guid id);
}
