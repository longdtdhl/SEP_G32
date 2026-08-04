using OPCBS.Web.Constants;
using OPCBS.Web.DTOs;
using OPCBS.Web.Helpers;

namespace OPCBS.Web.Services;

public class DoctorApiService : ApiServiceBase, IDoctorApiService
{
    public DoctorApiService(HttpClient client, JwtCookieService jwt) : base(client, jwt) { }

    public async Task<(List<DoctorListItemDto> Data, PaginationDto? Pagination, string? Error)> GetAllAsync(DoctorFilterDto? filter = null)
    {
        var query = ApiRoutes.Doctors;
        if (filter != null)
        {
            var parts = new List<string>();
            if (!string.IsNullOrEmpty(filter.Search)) parts.Add($"keyword={Uri.EscapeDataString(filter.Search)}");
            if (!string.IsNullOrEmpty(filter.Specialization)) parts.Add($"specialization={Uri.EscapeDataString(filter.Specialization)}");
            if (filter.MinRating.HasValue) parts.Add($"minRating={filter.MinRating}");
            if (filter.MaxFee.HasValue) parts.Add($"maxFee={filter.MaxFee}");
            if (!string.IsNullOrEmpty(filter.Gender)) parts.Add($"gender={filter.Gender}");
            parts.Add($"page={filter.Page}");
            parts.Add($"pageSize={filter.PageSize}");
            if (parts.Count > 0) query += "?" + string.Join("&", parts);
        }
        var (data, pagination, error) = await GetAsync<List<DoctorListItemDto>>(query);
        return (data ?? new List<DoctorListItemDto>(), pagination, error);
    }

    public async Task<(DoctorDto? Data, string? Error)> GetByIdAsync(Guid id)
    {
        var (data, _, error) = await GetAsync<DoctorDto>($"{ApiRoutes.Doctors}/{id}");
        return (data, error);
    }

    public async Task<(List<ReviewDto> Data, PaginationDto? Pagination, string? Error)> GetReviewsAsync(Guid doctorId, int page = 1)
    {
        var (data, pagination, error) = await GetAsync<List<ReviewDto>>($"{ApiRoutes.Doctors}/{doctorId}/reviews?page={page}");
        return (data ?? new List<ReviewDto>(), pagination, error);
    }

    public async Task<(List<TimeSlotDto> Data, string? Error)> GetAvailableSlotsAsync(Guid doctorId, DateTime date)
    {
        var (data, _, error) = await GetAsync<List<TimeSlotDto>>($"{ApiRoutes.Doctors}/{doctorId}/slots?date={date:yyyy-MM-dd}");
        return (data ?? new List<TimeSlotDto>(), error);
    }

    public async Task<List<string>> GetSpecializationsAsync()
    {
        var (data, _, _) = await GetAsync<List<SpecializationDto>>($"{ApiRoutes.Doctors}/specializations");
        return data?.Select(s => s.Name).Where(n => !string.IsNullOrEmpty(n)).ToList() ?? new List<string>();
    }

    public async Task<List<SpecializationDto>> GetSpecializationDtosAsync()
    {
        var (data, _, _) = await GetAsync<List<SpecializationDto>>($"{ApiRoutes.Doctors}/specializations");
        return data ?? new List<SpecializationDto>();
    }

    public async Task<(DoctorDto? Data, string? Error)> GetMyProfileAsync()
    {
        var (data, _, error) = await GetAsync<DoctorDto>("api/v1/doctor-profile");
        return (data, error);
    }

    public async Task<(bool Success, string? Error)> UpdateMyProfileAsync(UpdateDoctorProfileDto dto) =>
        await PutAsync("api/v1/doctor-profile", dto);

    public async Task<(string? Url, string? Error)> UploadAvatarAsync(Stream fileStream, string fileName)
        => await UploadFileAsync("api/v1/doctor-profile/avatar", fileStream, fileName, "avatarUrl");

    public async Task<(string? Url, string? Error)> UploadCertificateAsync(Stream fileStream, string fileName)
        => await UploadFileAsync("api/v1/doctor-profile/certificates", fileStream, fileName, "certificateUrl");

    public async Task<(CertificateUploadResultDto? Data, string? Error)> UploadCertificateFullAsync(Stream fileStream, string fileName)
    {
        AttachToken();
        try
        {
            using var content = new MultipartFormDataContent();
            var streamContent = new StreamContent(fileStream);
            streamContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(GetMimeType(fileName));
            content.Add(streamContent, "file", fileName);

            var response = await Http.PostAsync("api/v1/doctor-profile/certificates", content);
            var json = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                try
                {
                    var errorObj = System.Text.Json.JsonSerializer.Deserialize<System.Text.Json.JsonElement>(json);
                    if (errorObj.TryGetProperty("message", out var msg))
                        return (null, msg.GetString());
                }
                catch { }
                return (null, $"Upload failed with status {(int)response.StatusCode}");
            }

            var responseObj = System.Text.Json.JsonSerializer.Deserialize<System.Text.Json.JsonElement>(json);
            if (responseObj.TryGetProperty("data", out var data))
            {
                var result = new CertificateUploadResultDto
                {
                    CertificateUrl = data.TryGetProperty("certificateUrl", out var urlVal) ? urlVal.GetString() : null,
                    CertificatePublicId = data.TryGetProperty("certificatePublicId", out var pidVal) ? pidVal.GetString() : null,
                    CertificateFileName = data.TryGetProperty("certificateFileName", out var fnVal) ? fnVal.GetString() : null,
                    CertificateContentType = data.TryGetProperty("certificateContentType", out var ctVal) ? ctVal.GetString() : null
                };
                return (result, null);
            }
            return (null, "No upload data returned");
        }
        catch (Exception ex)
        {
            return (null, ex.Message);
        }
    }

    private async Task<(string? Url, string? Error)> UploadFileAsync(string url, Stream fileStream, string fileName, string urlKey)
    {
        AttachToken();
        try
        {
            using var content = new MultipartFormDataContent();
            var streamContent = new StreamContent(fileStream);
            streamContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(GetMimeType(fileName));
            content.Add(streamContent, "file", fileName);

            var response = await Http.PostAsync(url, content);
            var json = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                // Try to extract error message from API response
                try
                {
                    var errorObj = System.Text.Json.JsonSerializer.Deserialize<System.Text.Json.JsonElement>(json);
                    if (errorObj.TryGetProperty("message", out var msg))
                        return (null, msg.GetString());
                }
                catch { }
                return (null, $"Upload failed with status {(int)response.StatusCode}");
            }

            // Parse the API response envelope to get the URL
            var responseObj = System.Text.Json.JsonSerializer.Deserialize<System.Text.Json.JsonElement>(json);
            if (responseObj.TryGetProperty("data", out var data) && data.TryGetProperty(urlKey, out var urlValue))
            {
                return (urlValue.GetString(), null);
            }
            return (null, null); // Success but no URL extracted
        }
        catch (Exception ex)
        {
            return (null, ex.Message);
        }
    }

    private static string GetMimeType(string fileName)
    {
        var ext = Path.GetExtension(fileName).ToLowerInvariant();
        return ext switch
        {
            ".jpg" or ".jpeg" => "image/jpeg",
            ".png" => "image/png",
            ".webp" => "image/webp",
            ".pdf" => "application/pdf",
            _ => "application/octet-stream"
        };
    }
}
