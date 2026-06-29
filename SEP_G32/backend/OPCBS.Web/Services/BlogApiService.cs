using OPCBS.Web.Constants;
using OPCBS.Web.DTOs;
using OPCBS.Web.Helpers;

namespace OPCBS.Web.Services;

public class BlogApiService : ApiServiceBase, IBlogApiService
{
    public BlogApiService(HttpClient client, JwtCookieService jwt) : base(client, jwt) { }

    public async Task<(List<BlogListItemDto> Data, PaginationDto? Pagination, string? Error)> GetAllAsync(BlogFilterDto? filter = null)
    {
        var url = ApiRoutes.Blogs;
        if (filter != null)
        {
            var parts = new List<string>();
            if (!string.IsNullOrEmpty(filter.Search)) parts.Add($"search={Uri.EscapeDataString(filter.Search)}");
            if (!string.IsNullOrEmpty(filter.Category)) parts.Add($"category={Uri.EscapeDataString(filter.Category)}");
            parts.Add($"page={filter.Page}");
            parts.Add($"pageSize={filter.PageSize}");
            if (parts.Count > 0) url += "?" + string.Join("&", parts);
        }
        var (data, pagination, error) = await GetAsync<List<BlogListItemDto>>(url);
        return (data ?? new(), pagination, error);
    }

    public async Task<(BlogDto? Data, string? Error)> GetByIdAsync(Guid id)
    {
        var (data, _, error) = await GetAsync<BlogDto>($"{ApiRoutes.Blogs}/{id}");
        return (data, error);
    }

    public async Task<(List<BlogListItemDto> Data, PaginationDto? Pagination, string? Error)> GetMyBlogsAsync(BlogFilterDto? filter = null)
    {
        var url = $"{ApiRoutes.Blogs}/my-blogs";
        if (filter != null)
        {
            var parts = new List<string>();
            if (!string.IsNullOrEmpty(filter.Status)) parts.Add($"status={filter.Status}");
            parts.Add($"page={filter.Page}");
            parts.Add($"pageSize={filter.PageSize}");
            if (parts.Count > 0) url += "?" + string.Join("&", parts);
        }
        var (data, pagination, error) = await GetAsync<List<BlogListItemDto>>(url);
        return (data ?? new(), pagination, error);
    }

    public async Task<(BlogDto? Data, string? Error)> CreateAsync(CreateBlogDto dto)
        => await PostAsync<BlogDto>(ApiRoutes.Blogs, dto);

    public async Task<(bool Success, string? Error)> UpdateAsync(Guid id, UpdateBlogDto dto)
        => await PutAsync($"{ApiRoutes.Blogs}/{id}", dto);

    public async Task<(bool Success, string? Error)> DeleteAsync(Guid id)
        => await base.DeleteAsync($"{ApiRoutes.Blogs}/{id}");

    public async Task<(bool Success, string? Error)> SubmitForReviewAsync(Guid id)
        => await PostAsync($"{ApiRoutes.Blogs}/submit-review/{id}");
}
