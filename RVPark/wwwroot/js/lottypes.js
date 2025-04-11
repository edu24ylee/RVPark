let dataTable;

$(document).ready(function () {
    const rawToday = new Date();
    const today = new Date(rawToday.getFullYear(), rawToday.getMonth(), rawToday.getDate());

    const fiscalStart = new Date(today.getFullYear(), 9, 1); // Oct 1
    const fiscalReminderStart = new Date(fiscalStart);
    fiscalReminderStart.setDate(fiscalStart.getDate() - 5);

    const isSuperAdmin = window.isSuperAdmin === true || window.isSuperAdmin === "true";

    if (isSuperAdmin && today >= fiscalReminderStart && today <= fiscalStart) {
        toastr.info(
            "Reminder: Review Lot Type pricing for the upcoming fiscal year (Oct 1).",
            "Fiscal Year Alert",
            { toastClass: 'toast toast-custom-blue' }
        );
    }

    loadList();
});

function loadList() {
    dataTable = $('#DT_load').DataTable({
        ajax: {
            url: "/api/lottype",
            type: "GET",
            datatype: "json",
            dataSrc: "data",
            error: function (xhr, error, thrown) {
                toastr.error("Failed to load lot types.");
                console.error("DataTable Load Error:", error, thrown);
            },
            complete: function (xhr) {
                const today = new Date();
                const response = xhr.responseJSON?.data ?? [];

                response.forEach(item => {
                    if (item.endDate) {
                        const endDateRaw = new Date(item.endDate);
                        const endDate = new Date(endDateRaw.getFullYear(), endDateRaw.getMonth(), endDateRaw.getDate());
                        const warningStart = new Date(endDate);
                        warningStart.setDate(endDate.getDate() - 5);

                        if (today >= warningStart && today <= endDate) {
                            toastr.warning(
                                `Lot Type "${item.name}" is expiring on ${endDate.toLocaleDateString()}.`,
                                "Expiration Warning",
                                { toastClass: 'toast toast-custom-blue' }
                            );
                        }

                        if (endDate < today) {
                            toastr.error(
                                `Lot Type "${item.name}" has expired.`,
                                "Expired Lot Type",
                                { toastClass: 'toast toast-custom-blue' }
                            );
                        }
                    }
                });
            }
        },
        columns: [
            { data: "name", width: "20%" },
            {
                data: "rate",
                width: "15%",
                render: $.fn.dataTable.render.number(',', '.', 2, '$')
            },
            {
                data: "startDate",
                width: "15%",
                render: function (data) {
                    return data ? new Date(data).toLocaleDateString() : "—";
                }
            },
            {
                data: "endDate",
                width: "15%",
                render: function (data) {
                    return data ? new Date(data).toLocaleDateString() : "—";
                }
            },
            {
                data: null,
                width: "20%",
                render: function (data, type, row) {
                    const isArchived = row.isArchived === true || row.isArchived === "true";

                    const archiveBtn = isArchived
                        ? `<button class="btn btn-sm btn-outline-custom-blue" onclick="unarchiveLotType(${row.id})">
                               <i class="fas fa-box-open"></i> Unarchive
                           </button>`
                        : `<button class="btn btn-sm btn-custom-grey text-white" onclick="archiveLotType(${row.id})">
                               <i class="fas fa-archive"></i> Archive
                           </button>`;

                    return `
                        <div class="text-center d-flex justify-content-center gap-2">
                            <a href="/Admin/LotTypes/Upsert?id=${row.id}" class="btn btn-sm btn-custom-blue text-white">
                                <i class="far fa-edit"></i> Edit
                            </a>
                            ${archiveBtn}
                        </div>`;
                }
            }
        ],
        language: {
            emptyTable: "No lot types found."
        },
        width: "100%"
    });
}

function archiveLotType(id) {
    $.ajax({
        url: `/api/lottype/archive/${id}`,
        type: "POST",
        success: function (data) {
            if (data.success) {
                toastr.success(data.message);
                dataTable.ajax.reload();
            } else {
                toastr.error(data.message || "Archive failed.");
            }
        },
        error: () => toastr.error("Failed to archive lot type.")
    });
}

function unarchiveLotType(id) {
    $.ajax({
        url: `/api/lottype/unarchive/${id}`,
        type: "POST",
        success: function (data) {
            if (data.success) {
                toastr.success(data.message);
                dataTable.ajax.reload();
            } else {
                toastr.error(data.message || "Unarchive failed.");
            }
        },
        error: () => toastr.error("Failed to unarchive lot type.")
    });
}
