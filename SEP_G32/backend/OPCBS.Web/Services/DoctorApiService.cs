using System.Net.Http.Json;
using OPCBS.Web.Constants;
using OPCBS.Web.DTOs;

namespace OPCBS.Web.Services;

public class DoctorApiService : IDoctorApiService
{
    private readonly HttpClient _client;

    public DoctorApiService(HttpClient client)
    {
        _client = client;
    }

    public async Task<IEnumerable<DoctorDto>> GetAllAsync()
    {
        var res = await _client.GetFromJsonAsync<IEnumerable<DoctorDto>>($"{ApiRoutes.Doctors}");
        return res ?? Enumerable.Empty<DoctorDto>();
    }

    public async Task<DoctorDto?> GetByIdAsync(Guid id)
    {
        return await _client.GetFromJsonAsync<DoctorDto>($"{ApiRoutes.Doctors}/{id}");
    }
}
