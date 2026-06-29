using OPCBS.Web.DTOs;

namespace OPCBS.Web.Services;

public interface IBlogApiService
{
    Task<(List<BlogListItemDto> Data, PaginationDto? Pagination, string? Error)> GetAllAsync(BlogFilterDto? filter = null);
    Task<(BlogDto? Data, string? Error)> GetByIdAsync(Guid id);
    Task<(List<BlogListItemDto> Data, PaginationDto? Pagination, string? Error)> GetMyBlogsAsync(BlogFilterDto? filter = null);
    Task<(BlogDto? Data, string? Error)> CreateAsync(CreateBlogDto dto);
    Task<(bool Success, string? Error)> UpdateAsync(Guid id, UpdateBlogDto dto);
    Task<(bool Success, string? Error)> DeleteAsync(Guid id);
    Task<(bool Success, string? Error)> SubmitForReviewAsync(Guid id);
}
