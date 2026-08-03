using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OPCBS.Domain.Constants;
using OPCBS.Web.DTOs;
using OPCBS.Web.Services;

namespace OPCBS.Web.Pages.Patient.Appointments;

[Authorize(Roles = RoleConstants.Patient)]
public class IndexModel : PageModel
{
    private readonly IAppointmentApiService _service;
    public List<AppointmentListItemDto> Appointments { get; set; } = new();
    public List<string> UniqueDoctors { get; set; } = new();
    public PaginationDto? Pagination { get; set; }
    public string? Error { get; set; }

    public int PageNumber { get; set; } = 1;
    public string? Status { get; set; }
    public string? Doctor { get; set; }
    public string? Date { get; set; }

    public IndexModel(IAppointmentApiService service) { _service = service; }

    public async Task OnGetAsync(int page = 1, string? status = null, string? doctor = null, string? date = null)
    {
        PageNumber = page;
        Status = status;
        Doctor = doctor;
        Date = date;

        try
        {
            // Load active appointments
            var (allData, _, error) = await _service.GetMyAppointmentsAsync(new AppointmentFilterDto { View = "active", Page = 1, PageSize = 9999 });
            if (allData != null)
            {
                UniqueDoctors = allData
                    .Select(a => a.DoctorName)
                    .Distinct()
                    .Where(n => !string.IsNullOrEmpty(n))
                    .Cast<string>()
                    .ToList();

                var query = allData.AsEnumerable();

                // Apply Status filter
                if (!string.IsNullOrEmpty(Status))
                {
                    var filterStatus = Status;
                    if (filterStatus == "InProgress") filterStatus = "In Progress";
                    query = query.Where(a => string.Equals(a.StatusText, filterStatus, StringComparison.OrdinalIgnoreCase));
                }

                // Apply Doctor filter
                if (!string.IsNullOrEmpty(Doctor))
                {
                    query = query.Where(a => string.Equals(a.DoctorName, Doctor, StringComparison.OrdinalIgnoreCase));
                }

                // Apply Date filter
                if (!string.IsNullOrEmpty(Date))
                {
                    query = query.Where(a =>
                    {
                        if (DateTime.TryParse(a.AppointmentDate, out var ad))
                        {
                            return ad.ToString("yyyy-MM-dd") == Date;
                        }
                        return false;
                    });
                }

                var filteredList = query.ToList();
                var totalItems = filteredList.Count;
                var totalPages = (int)Math.Ceiling((double)totalItems / 10);

                if (PageNumber < 1) PageNumber = 1;
                if (PageNumber > totalPages && totalPages > 0) PageNumber = totalPages;

                Appointments = filteredList.Skip((PageNumber - 1) * 10).Take(10).ToList();

                Pagination = new PaginationDto
                {
                    Page = PageNumber,
                    PageSize = 10,
                    TotalItems = totalItems,
                    TotalPages = totalPages,
                    HasNextPage = PageNumber < totalPages,
                    HasPreviousPage = PageNumber > 1
                };
            }
            Error = error;
        }
        catch
        {
            Error = "Failed to load active appointments.";
        }
    }
}
