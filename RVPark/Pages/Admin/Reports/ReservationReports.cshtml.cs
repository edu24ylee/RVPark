using ApplicationCore.Models;
using Infrastructure.Data;
using Infrastructure.Utilities; 
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ClosedXML.Excel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace RVPark.Pages.Admin.Reports
{
    public class ReservationReports : PageModel
    {
        private readonly UnitOfWork _unitOfWork;

        public ReservationReports(UnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        [BindProperty]
        public DateTime StartDate { get; set; }

        [BindProperty]
        public DateTime EndDate { get; set; }

        public List<Reservation> Reservations { get; set; } = new();

        public bool ReportGenerated { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            SetDefaultDatesIfNeeded();

            if (StartDate > EndDate)
            {
                ReportGenerated = false;
                return Page();
            }

            Reservations = (await _unitOfWork.Reservation.GetAllAsync(
                r => r.StartDate >= StartDate && r.EndDate <= EndDate,
                includes: "Guest.User,Lot"
            )).OrderBy(r => r.Status).ToList();

            ReportGenerated = true;
            return Page();
        }

        public async Task<IActionResult> OnPostExportExcelAsync()
        {
            SetDefaultDatesIfNeeded();

            var data = (await _unitOfWork.Reservation.GetAllAsync(
                r => r.StartDate >= StartDate && r.EndDate <= EndDate,
                includes: "Guest.User,Lot"
            )).OrderBy(r => r.Status).ToList();

            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Reservation Report");

            worksheet.Cell(1, 1).Value = "Guest";
            worksheet.Cell(1, 2).Value = "Email";
            worksheet.Cell(1, 3).Value = "Phone";
            worksheet.Cell(1, 4).Value = "Lot";
            worksheet.Cell(1, 5).Value = "Status";
            worksheet.Cell(1, 6).Value = "Start Date";
            worksheet.Cell(1, 7).Value = "End Date";
            worksheet.Cell(1, 8).Value = "Duration";

            for (int i = 0; i < data.Count; i++)
            {
                var r = data[i];
                worksheet.Cell(i + 2, 1).Value = $"{r.Guest.User.FirstName} {r.Guest.User.LastName}";
                worksheet.Cell(i + 2, 2).Value = r.Guest.User.Email;
                worksheet.Cell(i + 2, 3).Value = r.Guest.User.Phone;
                worksheet.Cell(i + 2, 4).Value = r.Lot.Location;
                worksheet.Cell(i + 2, 5).Value = r.Status;
                worksheet.Cell(i + 2, 6).Value = r.StartDate.ToShortDateString();
                worksheet.Cell(i + 2, 7).Value = r.EndDate.ToShortDateString();
                worksheet.Cell(i + 2, 8).Value = r.Duration;
            }

            var stream = new MemoryStream();
            workbook.SaveAs(stream);
            stream.Position = 0;

            return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "ReservationReport.xlsx");
        }

        public async Task<IActionResult> OnGetExportPdfAsync(DateTime startDate, DateTime endDate)
        {
            var reservations = (await _unitOfWork.Reservation.GetAllAsync(
                r => r.StartDate >= startDate && r.EndDate <= endDate,
                includes: "Guest.User,Lot"
            )).OrderBy(r => r.Status).ToList();

            var stream = PdfExporter.ExportReservationsToPdf(reservations, startDate, endDate);
            return File(stream, "application/pdf", "ReservationReport.pdf");
        }

        private void SetDefaultDatesIfNeeded()
        {
            if (StartDate == default || EndDate == default)
            {
                var today = DateTime.Today;
                int diff = (7 + (today.DayOfWeek - DayOfWeek.Monday)) % 7;
                StartDate = today.AddDays(-diff).Date;
                EndDate = StartDate.AddDays(6).Date;
            }
        }
    }
}
