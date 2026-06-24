using System.Net.Http.Json;
using OPCBS.Web.Constants;
using OPCBS.Web.DTOs;

namespace OPCBS.Web.Services;

public class BlogApiService : IBlogApiService
{
    private readonly HttpClient _client;

    public BlogApiService(HttpClient client)
    {
        _client = client;
    }

    public async Task<IEnumerable<BlogDto>> GetAllAsync()
    {
        var res = await _client.GetFromJsonAsync<IEnumerable<BlogDto>>($"{ApiRoutes.Blogs}");
        return res ?? Enumerable.Empty<BlogDto>();
    }

    public async Task<BlogDto?> GetByIdAsync(Guid id)
    {
        return await _client.GetFromJsonAsync<BlogDto>($"{ApiRoutes.Blogs}/{id}");
    }
}
