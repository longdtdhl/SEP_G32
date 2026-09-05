using OPCBS.Web.DTOs;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;

namespace OPCBS.Web.Helpers;

public static class PatientClinicalPdfGenerator
{
    static PatientClinicalPdfGenerator()
    {
        QuestPDF.Settings.License = LicenseType.Community;
    }

    public static byte[] GeneratePatientClinicalReport(
        PatientRecordDto patient,
        DoctorDto? doctor,
        List<ConsultationNoteDto> consultationNotes,
        List<TreatmentCaseListWebDto>? cases = null,
        List<TreatmentGoalWebDto>? goals = null)
    {
        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(30);
                page.PageColor(Colors.White);
                page.DefaultTextStyle(x => x.FontSize(9.5f).FontFamily(Fonts.Arial).FontColor("#1e293b"));

                page.Header().Element(headerContainer => ComposeHeader(headerContainer, patient, doctor));
                page.Content().Element(contentContainer => ComposeContent(contentContainer, patient, doctor, consultationNotes, cases, goals));
                page.Footer().Element(footerContainer => ComposeFooter(footerContainer, doctor));
            });
        });

        return document.GeneratePdf();
    }

    private static void ComposeHeader(IContainer container, PatientRecordDto patient, DoctorDto? doctor)
    {
        container.Column(col =>
        {
            col.Item().Row(row =>
            {
                row.RelativeItem().Column(titleCol =>
                {
                    titleCol.Item().Text("MINDBRIDGE HEALTHCARE").FontSize(16).Bold().FontColor("#0f766e");
                    titleCol.Item().Text("Online Psychology Counseling Booking System (OPCBS)").FontSize(8.5f).FontColor("#64748b");
                    titleCol.Item().Text("PATIENT CLINICAL & MEDICAL RECORD").FontSize(13).Bold().FontColor("#0f172a");
                });

                row.ConstantItem(180).AlignRight().Column(metaCol =>
                {
                    metaCol.Item().Border(1).BorderColor("#ccfbf1").Background("#f0fdfa").Padding(6).Column(box =>
                    {
                        box.Item().Text(text =>
                        {
                            text.Span("CONFIDENTIAL MEDICAL FILE").FontSize(7.5f).Bold().FontColor("#0f766e");
                        });
                        box.Item().Text($"Report Date: {DateTime.Now:dd/MM/yyyy HH:mm}").FontSize(8).FontColor("#475569");
                        box.Item().Text($"Record ID: #{patient.Id.ToString()[..8].ToUpperInvariant()}").FontSize(8).FontColor("#475569");
                    });
                });
            });

            col.Item().PaddingTop(8).PaddingBottom(8).LineHorizontal(1.5f).LineColor("#0f766e");
        });
    }

    private static void ComposeContent(
        IContainer container,
        PatientRecordDto patient,
        DoctorDto? doctor,
        List<ConsultationNoteDto> notes,
        List<TreatmentCaseListWebDto>? cases,
        List<TreatmentGoalWebDto>? goals)
    {
        container.PaddingTop(4).Column(col =>
        {
            // ── Section 1: Demographics & Doctor Info ──
            col.Item().Row(row =>
            {
                // Left: Patient Info
                row.RelativeItem().Border(1).BorderColor("#e2e8f0").Background("#f8fafc").Padding(10).Column(pCol =>
                {
                    pCol.Item().Text("PATIENT DEMOGRAPHICS").FontSize(9).Bold().FontColor("#0f766e");
                    pCol.Item().PaddingTop(4);

                    pCol.Item().Text(t =>
                    {
                        t.Span("Full Name: ").Bold();
                        t.Span(patient.ResolvedDisplayName);
                    });

                    var status = patient.IsGuest ? "Guest Patient" : "Registered Patient";
                    pCol.Item().Text(t =>
                    {
                        t.Span("Patient Type: ").Bold();
                        t.Span(status);
                    });

                    var dobStr = patient.ResolvedDateOfBirth.HasValue
                        ? patient.ResolvedDateOfBirth.Value.ToString("dd/MM/yyyy")
                        : "Not recorded";
                    var ageStr = patient.ResolvedDateOfBirth.HasValue
                        ? $" ({DateTime.Now.Year - patient.ResolvedDateOfBirth.Value.Year} yrs)"
                        : "";
                    pCol.Item().Text(t =>
                    {
                        t.Span("Date of Birth: ").Bold();
                        t.Span(dobStr + ageStr);
                    });

                    pCol.Item().Text(t =>
                    {
                        t.Span("Gender: ").Bold();
                        t.Span(patient.ResolvedGender ?? "Not specified");
                    });

                    pCol.Item().Text(t =>
                    {
                        t.Span("Phone: ").Bold();
                        t.Span(patient.ResolvedDisplayPhone ?? "N/A");
                    });

                    pCol.Item().Text(t =>
                    {
                        t.Span("Email: ").Bold();
                        t.Span(patient.ResolvedDisplayEmail ?? "N/A");
                    });

                    pCol.Item().Text(t =>
                    {
                        t.Span("Address: ").Bold();
                        t.Span(patient.ResolvedAddress ?? "Not specified");
                    });

                    if (!string.IsNullOrWhiteSpace(patient.EmergencyContactName) || !string.IsNullOrWhiteSpace(patient.EmergencyContactPhone))
                    {
                        pCol.Item().Text(t =>
                        {
                            t.Span("Emergency Contact: ").Bold();
                            t.Span($"{patient.EmergencyContactName ?? "N/A"} - {patient.EmergencyContactPhone ?? ""}");
                        });
                    }
                });

                row.ConstantItem(12);

                // Right: Attending Doctor Info
                row.RelativeItem().Border(1).BorderColor("#e2e8f0").Background("#f8fafc").Padding(10).Column(dCol =>
                {
                    dCol.Item().Text("ATTENDING CLINICIAN").FontSize(9).Bold().FontColor("#0f766e");
                    dCol.Item().PaddingTop(4);

                    var docName = doctor != null ? doctor.FullName : "Dr. Specialist";
                    dCol.Item().Text(t =>
                    {
                        t.Span("Clinician: ").Bold();
                        t.Span(docName);
                    });

                    if (doctor != null && !string.IsNullOrWhiteSpace(doctor.Specialization))
                    {
                        dCol.Item().Text(t =>
                        {
                            t.Span("Specialization: ").Bold();
                            t.Span(doctor.Specialization);
                        });
                    }

                    if (doctor != null && !string.IsNullOrWhiteSpace(doctor.LicenseNumber))
                    {
                        dCol.Item().Text(t =>
                        {
                            t.Span("License Number: ").Bold();
                            t.Span(doctor.LicenseNumber);
                        });
                    }

                    dCol.Item().Text(t =>
                    {
                        t.Span("Platform: ").Bold();
                        t.Span("MindBridge Clinical Care Network");
                    });

                    dCol.Item().Text(t =>
                    {
                        t.Span("Session History: ").Bold();
                        t.Span($"{notes.Count} consultation note(s) recorded");
                    });
                });
            });

            // ── Section 2: Clinical Summary & Medical Assessment ──
            col.Item().PaddingTop(12).Column(cCol =>
            {
                cCol.Item().Background("#0f766e").Padding(5).PaddingLeft(8).Text("CLINICAL ASSESSMENT & PSYCHOLOGICAL PROFILE").FontSize(9.5f).Bold().FontColor(Colors.White);

                cCol.Item().Border(1).BorderColor("#e2e8f0").Padding(8).Column(box =>
                {
                    box.Item().Text(t =>
                    {
                        t.Span("Psychological History: ").Bold().FontColor("#0f766e");
                        t.Span(!string.IsNullOrWhiteSpace(patient.PsychologicalHistory) ? patient.PsychologicalHistory : "None recorded");
                    });

                    box.Item().PaddingTop(4).Text(t =>
                    {
                        t.Span("Current Symptoms & Presenting Concerns: ").Bold().FontColor("#0f766e");
                        t.Span(!string.IsNullOrWhiteSpace(patient.CurrentSymptoms) ? patient.CurrentSymptoms : "None recorded");
                    });

                    box.Item().PaddingTop(4).Text(t =>
                    {
                        t.Span("Stress & Trauma Factors: ").Bold().FontColor("#0f766e");
                        t.Span(!string.IsNullOrWhiteSpace(patient.StressFactors) ? patient.StressFactors : "None recorded");
                    });

                    if (!string.IsNullOrWhiteSpace(patient.GeneralNotes))
                    {
                        box.Item().PaddingTop(4).Text(t =>
                        {
                            t.Span("Clinician General Notes: ").Bold().FontColor("#0f766e");
                            t.Span(patient.GeneralNotes);
                        });
                    }
                });
            });

            // ── Section 3: Consultation Notes & Doctor's Remarks ──
            col.Item().PaddingTop(12).Column(nCol =>
            {
                nCol.Item().Background("#0f766e").Padding(5).PaddingLeft(8).Text($"CONSULTATION NOTES & CLINICAL REMARKS ({notes.Count})").FontSize(9.5f).Bold().FontColor(Colors.White);

                if (notes.Count == 0)
                {
                    nCol.Item().Border(1).BorderColor("#e2e8f0").Padding(10).AlignCenter().Text("No consultation notes recorded for this patient.").Italic().FontColor("#64748b");
                }
                else
                {
                    foreach (var note in notes.OrderByDescending(n => n.DisplayConsultationDate))
                    {
                        nCol.Item().PaddingTop(6).Border(1).BorderColor("#cbd5e1").Background("#ffffff").Padding(8).Column(noteBox =>
                        {
                            noteBox.Item().Row(r =>
                            {
                                r.RelativeItem().Text(t =>
                                {
                                    t.Span("Date & Time: ").Bold();
                                    t.Span(note.DisplayConsultationDate.ToLocalTime().ToString("dd/MM/yyyy HH:mm"));
                                    t.Span(" | Clinician: ").Bold();
                                    t.Span($"Dr. {note.DoctorName}");
                                });
                            });

                            if (!string.IsNullOrWhiteSpace(note.Diagnosis))
                            {
                                noteBox.Item().PaddingTop(3).Text(t =>
                                {
                                    t.Span("Diagnosis / Clinical Assessment: ").Bold().FontColor("#0f766e");
                                    t.Span(note.Diagnosis);
                                });
                            }

                            if (!string.IsNullOrWhiteSpace(note.ConsultationSummary))
                            {
                                noteBox.Item().PaddingTop(3).Text(t =>
                                {
                                    t.Span("Consultation Summary & Observations: ").Bold();
                                    t.Span(note.ConsultationSummary);
                                });
                            }

                            if (!string.IsNullOrWhiteSpace(note.TherapyPlan))
                            {
                                noteBox.Item().PaddingTop(3).Text(t =>
                                {
                                    t.Span("Therapy / Treatment Plan: ").Bold();
                                    t.Span(note.TherapyPlan);
                                });
                            }

                            if (!string.IsNullOrWhiteSpace(note.Recommendation))
                            {
                                noteBox.Item().PaddingTop(3).Text(t =>
                                {
                                    t.Span("Recommendations & Interventions: ").Bold();
                                    t.Span(note.Recommendation);
                                });
                            }

                            if (!string.IsNullOrWhiteSpace(note.FollowUpNotes))
                            {
                                noteBox.Item().PaddingTop(3).Text(t =>
                                {
                                    t.Span("Follow-Up / Doctor Remarks: ").Bold().FontColor("#b45309");
                                    t.Span(note.FollowUpNotes);
                                });
                            }
                        });
                    }
                }
            });

            // ── Section 4: Treatment Cases & Goals (Optional) ──
            if (cases != null && cases.Count > 0)
            {
                col.Item().PaddingTop(12).Column(caseCol =>
                {
                    caseCol.Item().Background("#0f766e").Padding(5).PaddingLeft(8).Text($"TREATMENT CASES & CLINICAL GOALS ({cases.Count})").FontSize(9.5f).Bold().FontColor(Colors.White);

                    foreach (var tc in cases)
                    {
                        caseCol.Item().PaddingTop(4).Border(1).BorderColor("#e2e8f0").Padding(8).Column(cBox =>
                        {
                            cBox.Item().Row(r =>
                            {
                                r.RelativeItem().Text(t =>
                                {
                                    t.Span("Case: ").Bold();
                                    t.Span(tc.CaseName);
                                    t.Span($" (Progress: {tc.OverallProgressPercent}% - {tc.StatusText})").Bold().FontColor("#0f766e");
                                });
                            });

                            if (!string.IsNullOrWhiteSpace(tc.PackageName))
                            {
                                cBox.Item().Text(t =>
                                {
                                    t.Span("Treatment Package: ").Bold();
                                    t.Span(tc.PackageName);
                                });
                            }

                            if (goals != null && goals.Count > 0)
                            {
                                var caseGoals = goals.Where(g => g.TreatmentCaseId == tc.Id).ToList();
                                if (caseGoals.Count > 0)
                                {
                                    cBox.Item().PaddingTop(3).Text("Active Goals:").Bold().FontSize(8.5f);
                                    foreach (var g in caseGoals)
                                    {
                                        cBox.Item().PaddingLeft(10).Text($"• {g.Title} (Status: {g.StatusText})").FontSize(8.5f);
                                    }
                                }
                            }
                        });
                    }
                });
            }
        });
    }

    private static void ComposeFooter(IContainer container, DoctorDto? doctor)
    {
        container.Column(col =>
        {
            col.Item().PaddingTop(8).LineHorizontal(1).LineColor("#cbd5e1");

            col.Item().PaddingTop(4).Row(row =>
            {
                row.RelativeItem().Column(legalCol =>
                {
                    legalCol.Item().Text("CONFIDENTIAL MEDICAL RECORD").FontSize(7.5f).Bold().FontColor("#64748b");
                    legalCol.Item().Text("This document is generated from MindBridge OPCBS and contains confidential patient medical history. Unauthorized copying, distribution, or alteration is strictly prohibited.").FontSize(7).FontColor("#94a3b8");
                });

                row.ConstantItem(150).AlignRight().Column(sigCol =>
                {
                    sigCol.Item().Text("Clinician Signature:").FontSize(8).Bold().FontColor("#475569");
                    sigCol.Item().PaddingTop(12).Text(doctor != null ? $"Dr. {doctor.FullName}" : "Attending Doctor").FontSize(8.5f).Bold().FontColor("#0f766e");
                });
            });

            col.Item().PaddingTop(4).AlignCenter().Text(x =>
            {
                x.Span("Page ");
                x.CurrentPageNumber();
                x.Span(" of ");
                x.TotalPages();
            });
        });
    }
}
