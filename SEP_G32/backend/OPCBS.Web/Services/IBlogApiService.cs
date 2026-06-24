using OPCBS.Web.DTOs;

namespace OPCBS.Web.Services;

public interface IBlogApiService
{
    Task<IEnumerable<BlogDto>> GetAllAsync();
    Task<BlogDto?> GetByIdAsync(Guid id);
}
