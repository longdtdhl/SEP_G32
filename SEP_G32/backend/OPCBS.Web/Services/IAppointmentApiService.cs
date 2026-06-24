using OPCBS.Web.DTOs;

namespace OPCBS.Web.Services;

public interface IAppointmentApiService
{
    Task<bool> BookAsync(CreateAppointmentDto dto);
}
