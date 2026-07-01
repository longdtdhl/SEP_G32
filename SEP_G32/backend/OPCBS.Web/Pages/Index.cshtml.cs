using Microsoft.AspNetCore.Mvc.RazorPages;
using OPCBS.Web.DTOs;
using OPCBS.Web.Services;

namespace OPCBS.Web.Pages;

public class IndexModel : PageModel
{
    private readonly IDoctorApiService _doctorService;
    private readonly IBlogApiService _blogService;

    public List<DoctorListItemDto> FeaturedDoctors { get; set; } = new();
    public List<BlogListItemDto> LatestBlogs { get; set; } = new();

    public IndexModel(IDoctorApiService doctorService, IBlogApiService blogService)
    {
        _doctorService = doctorService;
        _blogService = blogService;
    }

    public async Task OnGetAsync()
    {
        try
        {
            var (doctors, _, _) = await _doctorService.GetAllAsync(new DoctorFilterDto { PageSize = 4 });
            FeaturedDoctors = doctors;
        }
        catch { /* API may not be running */ }

        try
        {
            var (blogs, _, _) = await _blogService.GetAllAsync(new BlogFilterDto { PageSize = 3 });
            LatestBlogs = blogs;
        }
        catch { /* API may not be running */ }
    }
}
