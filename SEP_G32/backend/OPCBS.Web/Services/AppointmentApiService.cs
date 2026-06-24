using System.Net.Http.Json;
using OPCBS.Web.Constants;
using OPCBS.Web.DTOs;

namespace OPCBS.Web.Services;

public class AppointmentApiService : IAppointmentApiService
{
    private readonly HttpClient _client;

    public AppointmentApiService(HttpClient client)
    {
        _client = client;
    }

    public async Task<bool> BookAsync(CreateAppointmentDto dto)
    {
        var res = await _client.PostAsJsonAsync($"{ApiRoutes.Appointments}", dto);
        return res.IsSuccessStatusCode;
    }
}
