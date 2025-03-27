using ApplicationCore.Models;
using Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ClosedXML.Excel;
using PdfSharpCore.Pdf;
using PdfSharpCore.Drawing;
using System.Text;
using System.Linq;

namespace RVPark.Pages.Admin.Reports
{
    public class FinancialReportsModel : PageModel
    {
        private readonly UnitOfWork _unitOfWork;

        public FinancialReportsModel(UnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        [BindProperty]
        public DateTime StartDate { get; set; }

        [BindProperty]
        public DateTime EndDate { get; set; }

        public decimal CollectedRevenue { get; set; }
        public decimal AnticipatedRevenue { get; set; }
        public List<string> RevenueBreakdown { get; set; } = new();
        public bool ReportGenerated { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            SetDefaultDatesIfNeeded();

            var reservations = await _unitOfWork.Reservation.GetAllAsync(
                r => r.StartDate >= StartDate && r.EndDate <= EndDate,
                includes: "Lot.LotType"
            );

            CollectedRevenue = 0;
            AnticipatedRevenue = 0;
            RevenueBreakdown.Clear();

            foreach (var r in reservations)
            {
                decimal rate = (decimal)(r.Lot?.LotType?.Rate ?? 0);
                decimal amount = r.Duration * rate;

                if (r.Status == "Active" || r.Status == "Completed")
                {
                    CollectedRevenue += amount;
                    RevenueBreakdown.Add($"Reservation {r.ReservationId} - Lot {r.Lot?.Location ?? "Unknown"}: ${amount:F2} (Collected)");
                }
                else if (r.Status == "Upcoming")
                {
                    AnticipatedRevenue += amount;
                    RevenueBreakdown.Add($"Reservation {r.ReservationId} - Lot {r.Lot?.Location ?? "Unknown"}: ${amount:F2} (Anticipated)");
                }
            }

            ReportGenerated = true;
            return Page();
        }

        public async Task<IActionResult> OnPostExportExcelAsync()
        {
            SetDefaultDatesIfNeeded();
            await OnGetAsync();

            var workbook = new XLWorkbook();
            var sheet = workbook.Worksheets.Add("Financial Report");

            sheet.Cell(1, 1).Value = "Start Date";
            sheet.Cell(1, 2).Value = StartDate.ToShortDateString();
            sheet.Cell(2, 1).Value = "End Date";
            sheet.Cell(2, 2).Value = EndDate.ToShortDateString();
            sheet.Cell(3, 1).Value = "Collected Revenue";
            sheet.Cell(3, 2).Value = CollectedRevenue;
            sheet.Cell(4, 1).Value = "Anticipated Revenue";
            sheet.Cell(4, 2).Value = AnticipatedRevenue;

            sheet.Cell(6, 1).Value = "Revenue Breakdown";
            for (int i = 0; i < RevenueBreakdown.Count; i++)
            {
                sheet.Cell(7 + i, 1).Value = RevenueBreakdown[i];
            }

            var stream = new MemoryStream(); 
            workbook.SaveAs(stream);
            stream.Position = 0;

            return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "FinancialReport.xlsx");
        }


        public async Task<IActionResult> OnGetExportPdfAsync(DateTime startDate, DateTime endDate)
        {
            StartDate = startDate;
            EndDate = endDate;
            await OnGetAsync(); 

            var document = new PdfDocument();
            var page = document.AddPage();
            var gfx = XGraphics.FromPdfPage(page);
            var font = new XFont("Verdana", 12);
            int y = 40;

            gfx.DrawString("Financial Report", new XFont("Verdana", 16, XFontStyle.Bold), XBrushes.Black, new XPoint(40, y));
            y += 30;
            gfx.DrawString($"Date Range: {StartDate:MM/dd/yyyy} - {EndDate:MM/dd/yyyy}", font, XBrushes.Black, new XPoint(40, y));
            y += 25;
            gfx.DrawString($"Collected Revenue: ${CollectedRevenue:F2}", font, XBrushes.Black, new XPoint(40, y));
            y += 20;
            gfx.DrawString($"Anticipated Revenue: ${AnticipatedRevenue:F2}", font, XBrushes.Black, new XPoint(40, y));
            y += 30;
            gfx.DrawString("Breakdown:", font, XBrushes.Black, new XPoint(40, y));
            y += 20;

            foreach (var line in RevenueBreakdown)
            {
                gfx.DrawString(line, font, XBrushes.Black, new XPoint(50, y));
                y += 18;
                if (y > page.Height - 50)
                {
                    page = document.AddPage();
                    gfx = XGraphics.FromPdfPage(page);
                    y = 40;
                }
            }

            var stream = new MemoryStream();
            document.Save(stream, false);
            stream.Position = 0;

            return File(stream, "application/pdf", "FinancialReport.pdf");
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
