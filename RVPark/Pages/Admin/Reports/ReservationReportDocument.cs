using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using ApplicationCore.Models;
using System;
using System.Collections.Generic;

public class ReservationReportDocument : IDocument
{
    private readonly List<Reservation> _reservations;

    private readonly DateTime _start;
    private readonly DateTime _end;

    public ReservationReportDocument(List<Reservation> reservations, DateTime start, DateTime end)
    {
        _reservations = reservations;
        _start = start;
        _end = end;
    }

    public DocumentMetadata GetMetadata() => DocumentMetadata.Default;

    public void Compose(IDocumentContainer container)
    {
        container.Page(page =>
        {
            page.Size(PageSizes.A4); 
            page.Margin(30);     

            page.Header()
                .Text($"Reservation Report: {_start:MM/dd/yyyy} - {_end:MM/dd/yyyy}")
                .SemiBold()
                .FontSize(18);

            page.Content().Table(table =>
            {

                table.ColumnsDefinition(columns =>
                {
                    columns.RelativeColumn(2); // Name
                    columns.RelativeColumn(2); // Email
                    columns.RelativeColumn(1); // Phone
                    columns.RelativeColumn(1); // Lot
                    columns.RelativeColumn(1); // Status
                    columns.RelativeColumn(1); // Start Date
                    columns.RelativeColumn(1); // End Date
                    columns.RelativeColumn(1); // Duration
                });

                table.Header(header =>
                {
                    header.Cell().Text("Name").Bold();
                    header.Cell().Text("Email").Bold();
                    header.Cell().Text("Phone").Bold();
                    header.Cell().Text("Lot").Bold();
                    header.Cell().Text("Status").Bold();
                    header.Cell().Text("Start").Bold();
                    header.Cell().Text("End").Bold();
                    header.Cell().Text("Duration").Bold();
                });

                foreach (var r in _reservations)
                {
                    table.Cell().Text($"{r.Guest.User.FirstName} {r.Guest.User.LastName}");
                    table.Cell().Text(r.Guest.User.Email);
                    table.Cell().Text(r.Guest.User.Phone);
                    table.Cell().Text(r.Lot.Location);
                    table.Cell().Text(r.Status);
                    table.Cell().Text(r.StartDate.ToShortDateString());
                    table.Cell().Text(r.EndDate.ToShortDateString());
                    table.Cell().Text($"{r.Duration} days");
                }
            });
        });
    }
}
