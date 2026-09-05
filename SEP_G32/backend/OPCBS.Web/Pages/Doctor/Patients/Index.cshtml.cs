using ClosedXML.Excel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OPCBS.Domain.Constants;
using OPCBS.Web.DTOs;
using OPCBS.Web.Services;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OPCBS.Web.Pages.Doctor.Patients;

[Authorize(Roles = RoleConstants.Doctor)]
public class IndexModel : PageModel
{
    private readonly IPatientRecordApiService _apiService;
    private readonly IConsultationNoteApiService _noteService;

    public IndexModel(IPatientRecordApiService apiService, IConsultationNoteApiService noteService)
    {
        _apiService = apiService;
        _noteService = noteService;
    }

    public List<PatientRecordDto> Patients { get; set; } = new();
    public List<ConsultationNoteDto> AllNotes { get; set; } = new();

    [BindProperty(SupportsGet = true)]
    public string? Tab { get; set; } = "all";

    [BindProperty(SupportsGet = true)]
    public string? Search { get; set; }

    public async Task<IActionResult> OnGetAsync()
    {
        var (data, error) = await _apiService.GetMyPatientsAsync();
        if (error != null)
            TempData["ErrorMessage"] = error;
        else
            Patients = data;

        // Fetch all consultation notes for note counts
        var (notes, _, noteError) = await _noteService.GetAllAsync(1, 500);
        if (noteError == null && notes != null)
            AllNotes = notes;

        // Tab filter
        if (Tab == "system")
            Patients = Patients.Where(p => !p.IsGuest).ToList();
        else if (Tab == "guest")
            Patients = Patients.Where(p => p.IsGuest).ToList();

        // Search filter
        if (!string.IsNullOrWhiteSpace(Search))
        {
            var q = Search.Trim().ToLowerInvariant();
            Patients = Patients.Where(p =>
                p.ResolvedDisplayName.ToLowerInvariant().Contains(q) ||
                (p.ResolvedDisplayEmail?.ToLowerInvariant().Contains(q) ?? false) ||
                (p.ResolvedDisplayPhone?.ToLowerInvariant().Contains(q) ?? false)
            ).ToList();
        }

        return Page();
    }

    public int GetNoteCount(Guid patientRecordId)
    {
        return AllNotes.Count(n => n.PatientRecordId == patientRecordId);
    }

    /// <summary>
    /// Download Excel template for importing guest patients
    /// </summary>
    public IActionResult OnGetDownloadTemplate()
    {
        using var workbook = new XLWorkbook();
        var worksheet = workbook.Worksheets.Add("Patients_Template");

        // Headers
        var headers = new[]
        {
            "Full Name *",
            "Email *",
            "Phone Number *",
            "Zalo Number",
            "Date of Birth (dd/MM/yyyy)",
            "Gender (Male/Female)",
            "Age",
            "Address",
            "Initial Notes"
        };

        for (int i = 0; i < headers.Length; i++)
        {
            var cell = worksheet.Cell(1, i + 1);
            cell.Value = headers[i];
            cell.Style.Font.Bold = true;
            cell.Style.Font.FontColor = XLColor.White;
            cell.Style.Fill.BackgroundColor = XLColor.FromHtml("#0f766e");
            cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            cell.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            cell.Style.Border.OutsideBorderColor = XLColor.FromHtml("#cbd5e1");
        }

        // Sample rows
        var sampleRows = new[]
        {
            new object[] { "Alex Nguyen", "alex.nguyen@example.com", "0901234567", "0901234567", "15/05/1992", "Male", 34, "123 Wellness Street, District 5", "Work-related stress and sleep concerns" },
            new object[] { "Mia Tran", "mia.tran@example.com", "0912345678", "0912345678", "20/11/1998", "Female", 28, "45 Green Avenue, District 1", "Persistent anxiety and insomnia" },
            new object[] { "Leo Le", "leo.le@example.com", "0987654321", "0987654321", "10/08/1984", "Male", 42, "78 Balance Road", "New patient seeking psychological counseling" }
        };

        for (int r = 0; r < sampleRows.Length; r++)
        {
            for (int c = 0; c < sampleRows[r].Length; c++)
            {
                var cell = worksheet.Cell(r + 2, c + 1);
                cell.Value = sampleRows[r][c]?.ToString() ?? "";
                cell.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                cell.Style.Border.OutsideBorderColor = XLColor.FromHtml("#e2e8f0");
            }
        }

        worksheet.Columns().AdjustToContents();

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        var content = stream.ToArray();

        return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Patient_Import_Template.xlsx");
    }

    /// <summary>
    /// Import patients from uploaded Excel (.xlsx) or CSV (.csv) file
    /// </summary>
    public async Task<IActionResult> OnPostImportExcelAsync(IFormFile? excelFile)
    {
        if (excelFile == null || excelFile.Length == 0)
        {
            TempData["ErrorMessage"] = "Please select an Excel (.xlsx) or CSV (.csv) file to upload.";
            return RedirectToPage(new { tab = "guest" });
        }

        var ext = Path.GetExtension(excelFile.FileName).ToLowerInvariant();
        if (ext != ".xlsx" && ext != ".xls" && ext != ".csv")
        {
            TempData["ErrorMessage"] = "Unsupported file type. Please upload an Excel (.xlsx, .xls) or CSV (.csv) file.";
            return RedirectToPage(new { tab = "guest" });
        }

        var dtos = new List<CreatePatientRecordDto>();

        try
        {
            if (ext == ".csv")
            {
                using var reader = new StreamReader(excelFile.OpenReadStream(), Encoding.UTF8);
                string? headerLine = await reader.ReadLineAsync();
                if (string.IsNullOrWhiteSpace(headerLine))
                {
                    TempData["ErrorMessage"] = "The uploaded CSV file is empty.";
                    return RedirectToPage(new { tab = "guest" });
                }

                var headerParts = headerLine.Split(new[] { ',', ';' }).Select(h => h.Trim().ToLowerInvariant()).ToList();
                int nameCol = headerParts.FindIndex(h => h.Contains("tên") || h.Contains("name") || h.Contains("họ"));
                int emailCol = headerParts.FindIndex(h => h.Contains("email") || h.Contains("thư"));
                int phoneCol = headerParts.FindIndex(h => (h.Contains("thoại") || h.Contains("phone") || h.Contains("sđt") || h.Contains("sdt")) && !h.Contains("zalo"));
                int zaloCol = headerParts.FindIndex(h => h.Contains("zalo"));
                int dobCol = headerParts.FindIndex(h => h.Contains("sinh") || h.Contains("dob") || h.Contains("birth"));
                int genderCol = headerParts.FindIndex(h => h.Contains("tính") || h.Contains("gender") || h.Contains("giới") || h.Contains("sex"));
                int ageCol = headerParts.FindIndex(h => h.Contains("tuổi") || h.Contains("age"));
                int addrCol = headerParts.FindIndex(h => h.Contains("chỉ") || h.Contains("address") || h.Contains("địa chỉ"));
                int notesCol = headerParts.FindIndex(h => h.Contains("chú") || h.Contains("note") || h.Contains("triệu chứng") || h.Contains("mô tả"));

                if (nameCol < 0) nameCol = 0;
                if (emailCol < 0) emailCol = 1;
                if (phoneCol < 0) phoneCol = 2;
                if (zaloCol < 0) zaloCol = 3;
                if (dobCol < 0) dobCol = 4;
                if (genderCol < 0) genderCol = 5;
                if (ageCol < 0) ageCol = 6;
                if (addrCol < 0) addrCol = 7;
                if (notesCol < 0) notesCol = 8;

                string? line;
                while ((line = await reader.ReadLineAsync()) != null)
                {
                    if (string.IsNullOrWhiteSpace(line)) continue;
                    var parts = line.Split(new[] { ',', ';' });
                    if (parts.Length > 0)
                    {
                        var name = parts.ElementAtOrDefault(nameCol)?.Trim();
                        if (string.IsNullOrWhiteSpace(name)) continue;

                        var dto = ParsePatientRow(
                            name: name,
                            email: parts.ElementAtOrDefault(emailCol),
                            phone: parts.ElementAtOrDefault(phoneCol),
                            zalo: parts.ElementAtOrDefault(zaloCol),
                            dobStr: parts.ElementAtOrDefault(dobCol),
                            ageStr: parts.ElementAtOrDefault(ageCol),
                            genderStr: parts.ElementAtOrDefault(genderCol),
                            address: parts.ElementAtOrDefault(addrCol),
                            notes: parts.ElementAtOrDefault(notesCol)
                        );
                        if (dto != null) dtos.Add(dto);
                    }
                }
            }
            else
            {
                using var stream = excelFile.OpenReadStream();
                using var workbook = new XLWorkbook(stream);
                var worksheet = workbook.Worksheets.FirstOrDefault();
                if (worksheet == null)
                {
                    TempData["ErrorMessage"] = "The uploaded Excel file contains no worksheets.";
                    return RedirectToPage(new { tab = "guest" });
                }

                var headerRow = worksheet.Row(1);
                int lastCol = headerRow.LastCellUsed()?.Address.ColumnNumber ?? 10;
                var colMap = new Dictionary<string, int>();

                for (int c = 1; c <= lastCol; c++)
                {
                    var headerVal = headerRow.Cell(c).GetString().Trim().ToLowerInvariant();
                    if (string.IsNullOrWhiteSpace(headerVal)) continue;

                    if ((headerVal.Contains("tên") || headerVal.Contains("name") || headerVal.Contains("họ")) && !colMap.ContainsKey("name"))
                        colMap["name"] = c;
                    else if ((headerVal.Contains("email") || headerVal.Contains("thư")) && !colMap.ContainsKey("email"))
                        colMap["email"] = c;
                    else if ((headerVal.Contains("thoại") || headerVal.Contains("phone") || headerVal.Contains("sđt") || headerVal.Contains("sdt")) && !headerVal.Contains("zalo") && !colMap.ContainsKey("phone"))
                        colMap["phone"] = c;
                    else if (headerVal.Contains("zalo") && !colMap.ContainsKey("zalo"))
                        colMap["zalo"] = c;
                    else if ((headerVal.Contains("sinh") || headerVal.Contains("dob") || headerVal.Contains("birth")) && !colMap.ContainsKey("dob"))
                        colMap["dob"] = c;
                    else if ((headerVal.Contains("tính") || headerVal.Contains("gender") || headerVal.Contains("giới") || headerVal.Contains("sex")) && !colMap.ContainsKey("gender"))
                        colMap["gender"] = c;
                    else if ((headerVal.Contains("tuổi") || headerVal.Contains("age")) && !colMap.ContainsKey("age"))
                        colMap["age"] = c;
                    else if ((headerVal.Contains("chỉ") || headerVal.Contains("address") || headerVal.Contains("địa chỉ")) && !colMap.ContainsKey("address"))
                        colMap["address"] = c;
                    else if ((headerVal.Contains("chú") || headerVal.Contains("note") || headerVal.Contains("triệu chứng") || headerVal.Contains("mô tả")) && !colMap.ContainsKey("notes"))
                        colMap["notes"] = c;
                }

                int nameCol = colMap.GetValueOrDefault("name", 1);
                int emailCol = colMap.GetValueOrDefault("email", 2);
                int phoneCol = colMap.GetValueOrDefault("phone", 3);
                int zaloCol = colMap.GetValueOrDefault("zalo", 4);
                int dobCol = colMap.GetValueOrDefault("dob", 5);
                int genderCol = colMap.GetValueOrDefault("gender", 6);
                int ageCol = colMap.GetValueOrDefault("age", 7);
                int addrCol = colMap.GetValueOrDefault("address", 8);
                int notesCol = colMap.GetValueOrDefault("notes", 9);

                var rows = worksheet.RowsUsed().Skip(1); // Skip header row
                foreach (var row in rows)
                {
                    var name = row.Cell(nameCol).GetString()?.Trim();
                    if (string.IsNullOrWhiteSpace(name)) continue;

                    var email = row.Cell(emailCol).GetString()?.Trim();
                    var phone = row.Cell(phoneCol).GetString()?.Trim();
                    var zalo = zaloCol > 0 ? row.Cell(zaloCol).GetString()?.Trim() : null;

                    string? dobStr = null;
                    var dobCell = dobCol > 0 ? row.Cell(dobCol) : null;
                    if (dobCell != null)
                    {
                        if (dobCell.DataType == XLDataType.DateTime)
                        {
                            dobStr = dobCell.GetDateTime().ToString("dd/MM/yyyy");
                        }
                        else
                        {
                            dobStr = dobCell.GetString()?.Trim();
                        }
                    }

                    var genderStr = genderCol > 0 ? row.Cell(genderCol).GetString()?.Trim() : null;
                    var ageStr = ageCol > 0 ? row.Cell(ageCol).GetString()?.Trim() : null;
                    var address = addrCol > 0 ? row.Cell(addrCol).GetString()?.Trim() : null;
                    var notes = notesCol > 0 ? row.Cell(notesCol).GetString()?.Trim() : null;

                    var dto = ParsePatientRow(name, email, phone, zalo, dobStr, ageStr, genderStr, address, notes);
                    if (dto != null) dtos.Add(dto);
                }
            }
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = $"Failed to parse file: {ex.Message}";
            return RedirectToPage(new { tab = "guest" });
        }

        if (dtos.Count == 0)
        {
            TempData["ErrorMessage"] = "No valid patient records were found. Each imported row must contain Full Name, Email, and Phone number.";
            return RedirectToPage(new { tab = "guest" });
        }

        var (success, error) = await _apiService.CreateBatchAsync(dtos);
        if (!success)
        {
            // Fallback: Try importing individually if batch is not supported or failed
            int successCount = 0;
            foreach (var dto in dtos)
            {
                var (singleSuccess, _) = await _apiService.CreateAsync(dto);
                if (singleSuccess) successCount++;
            }

            if (successCount > 0)
            {
                TempData["SuccessMessage"] = $"Successfully imported {successCount} / {dtos.Count} guest patient record(s)!";
                return RedirectToPage(new { tab = "guest" });
            }

            TempData["ErrorMessage"] = error ?? "Failed to import patient records.";
            return RedirectToPage(new { tab = "guest" });
        }

        TempData["SuccessMessage"] = $"Successfully imported {dtos.Count} guest patient record(s)!";
        return RedirectToPage(new { tab = "guest" });
    }

    public async Task<IActionResult> OnPostDeleteGuestPatientAsync(Guid id)
    {
        var (success, error) = await _apiService.DeleteAsync(id);
        if (!success)
        {
            TempData["ErrorMessage"] = error ?? "Failed to delete patient record.";
        }
        else
        {
            TempData["SuccessMessage"] = "Guest patient record deleted successfully.";
        }
        return RedirectToPage(new { tab = "guest" });
    }

    private static CreatePatientRecordDto? ParsePatientRow(
        string? name,
        string? email,
        string? phone,
        string? zalo,
        string? dobStr,
        string? ageStr,
        string? genderStr,
        string? address,
        string? notes)
    {
        if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(phone))
            return null;

        DateTime? dob = null;
        if (!string.IsNullOrWhiteSpace(dobStr))
        {
            var dateFormats = new[] { "dd/MM/yyyy", "d/M/yyyy", "yyyy-MM-dd", "MM/dd/yyyy", "dd-MM-yyyy", "d-M-yyyy" };
            if (DateTime.TryParseExact(dobStr.Trim(), dateFormats, CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsedDate))
            {
                dob = parsedDate;
            }
            else if (DateTime.TryParse(dobStr.Trim(), out var genericDate))
            {
                dob = genericDate;
            }
        }

        int? age = null;
        if (!string.IsNullOrWhiteSpace(ageStr) && int.TryParse(ageStr.Trim(), out var parsedAge) && parsedAge > 0 && parsedAge < 130)
        {
            age = parsedAge;
            if (!dob.HasValue)
            {
                dob = new DateTime(DateTime.UtcNow.Year - parsedAge, 1, 1);
            }
        }

        // Normalize Gender
        string? gender = null;
        if (!string.IsNullOrWhiteSpace(genderStr))
        {
            var g = genderStr.Trim().ToLowerInvariant();
            if (g.StartsWith("nam") || g == "male" || g == "m" || g == "1")
                gender = "Male";
            else if (g.StartsWith("nữ") || g.StartsWith("nu") || g.StartsWith("female") || g == "f" || g == "0" || g == "2")
                gender = "Female";
            else if (g.StartsWith("khác") || g.StartsWith("khac") || g == "other")
                gender = "Other";
            else
                gender = genderStr.Trim();
        }

        // Build combined general notes
        var notesBuilder = new List<string>();
        if (!string.IsNullOrWhiteSpace(zalo))
        {
            notesBuilder.Add($"Zalo Number: {zalo.Trim()}");
        }
        if (dob.HasValue)
        {
            notesBuilder.Add($"Date of Birth: {dob.Value:dd/MM/yyyy}");
        }
        else if (age.HasValue)
        {
            notesBuilder.Add($"Age: {age.Value}");
        }
        if (!string.IsNullOrWhiteSpace(gender))
        {
            notesBuilder.Add($"Gender: {gender}");
        }
        if (!string.IsNullOrWhiteSpace(address))
        {
            notesBuilder.Add($"Address: {address.Trim()}");
        }
        if (!string.IsNullOrWhiteSpace(notes))
        {
            notesBuilder.Add(notes.Trim());
        }

        var generalNotes = notesBuilder.Count > 0 ? string.Join(" | ", notesBuilder) : null;

        return new CreatePatientRecordDto
        {
            GuestName = name.Trim(),
            GuestEmail = string.IsNullOrWhiteSpace(email) ? null : email.Trim(),
            GuestPhone = string.IsNullOrWhiteSpace(phone) ? null : phone.Trim(),
            GuestDateOfBirth = dob,
            GuestGender = gender,
            GuestAddress = string.IsNullOrWhiteSpace(address) ? null : address.Trim(),
            GeneralNotes = generalNotes
        };
    }
}

