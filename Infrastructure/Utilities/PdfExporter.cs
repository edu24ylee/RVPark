using PdfSharpCore.Drawing;
using PdfSharpCore.Pdf;
using ApplicationCore.Models;
using System;
using System.Collections.Generic;
using System.IO;

namespace Infrastructure.Utilities
{
    public static class PdfExporter
    {
        public static MemoryStream ExportReservationsToPdf(List<Reservation> reservations, DateTime start, DateTime end)
        {
            var document = new PdfDocument();
            var page = document.AddPage();
            var gfx = XGraphics.FromPdfPage(page);
            var font = new XFont("Verdana", 10);

            double y = 40;
            gfx.DrawString($"Reservation Report: {start:MM/dd/yyyy} - {end:MM/dd/yyyy}",
                new XFont("Verdana", 14, XFontStyle.Bold),
                XBrushes.Black,
                new XRect(0, y, page.Width, 20),
                XStringFormats.TopCenter);
            y += 30;

            foreach (var r in reservations)
            {
                var line = $"{r.Guest.User.FirstName} {r.Guest.User.LastName}, {r.Guest.User.Email}, {r.Guest.User.Phone}, " +
                           $"{r.Lot.Location}, {r.Status}, {r.StartDate:MM/dd/yyyy} - {r.EndDate:MM/dd/yyyy}, {r.Duration} days";

                gfx.DrawString(line, font, XBrushes.Black, new XRect(40, y, page.Width - 80, page.Height), XStringFormats.TopLeft);
                y += 20;

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
            return stream;
        }

        public static MemoryStream ExportFinancialSummaryToPdf(DateTime start, DateTime end, decimal collected, decimal anticipated)
        {
            var document = new PdfDocument();
            var page = document.AddPage();
            var gfx = XGraphics.FromPdfPage(page);
            var titleFont = new XFont("Verdana", 14, XFontStyle.Bold);
            var bodyFont = new XFont("Verdana", 12);

            double y = 40;
            gfx.DrawString("Financial Report", titleFont, XBrushes.Black, new XRect(0, y, page.Width, 20), XStringFormats.TopCenter);
            y += 30;

            gfx.DrawString($"Date Range: {start:MM/dd/yyyy} - {end:MM/dd/yyyy}", bodyFont, XBrushes.Black, new XPoint(40, y));
            y += 30;

            gfx.DrawString($"Collected Revenue: ${collected:F2}", bodyFont, XBrushes.Black, new XPoint(40, y));
            y += 20;

            gfx.DrawString($"Anticipated Revenue: ${anticipated:F2}", bodyFont, XBrushes.Black, new XPoint(40, y));

            var stream = new MemoryStream();
            document.Save(stream, false);
            stream.Position = 0;
            return stream;
        }
    }
}
