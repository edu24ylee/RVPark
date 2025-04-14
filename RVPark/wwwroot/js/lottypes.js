let dataTable;

$(document).ready(function () {
    loadList();

    const today = new Date();
    const fiscalStart = new Date(today.getFullYear(), 9, 1);
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
});

function loadList() {
    dataTable = $('#DT_load').DataTable({
        ajax: {
            url: "/api/lottypes",
            type: "GET",
            dataSrc: "data"
        },
        columns: [
            { data: "name", title: "Name", width: "20%" },
            {
                data: "rate",
                title: "Rate",
                width: "15%",
                render: $.fn.dataTable.render.number(',', '.', 2, '$')
            },
            {
                data: "startDate",
                title: "Start Date",
                width: "15%",
                render: data => data ? new Date(data).toLocaleDateString() : "—"
            },
            {
                data: "endDate",
                title: "End Date",
                width: "15%",
                render: data => data ? new Date(data).toLocaleDateString() : "—"
            },
            {
                data: null,
                title: "Actions",
                width: "20%",
                render: function (data, type, row) {
                    const archiveBtn = row.isArchived
                        ? `<button class="btn btn-sm btn-outline-custom-blue" onclick="toggleArchive(${row.id}, true)">
                               <i class="fas fa-box-open"></i> Unarchive
                           </button>`
                        : `<button class="btn btn-sm btn-custom-grey text-white" onclick="toggleArchive(${row.id}, false)">
                               <i class="fas fa-archive"></i> Archive
                           </button>`;

                    return `
                        <div class="text-center d-flex justify-content-center gap-2">
                            <a href="/Admin/LotTypes/Upsert?id=${row.id}" class="btn btn-sm btn-custom-blue text-white">
                                <i class="fas fa-edit"></i> Edit
                            </a>
                            ${archiveBtn}
                        </div>`;
                }
            }
        ],
        dom: '<"row mb-2"<"col-sm-12 col-md-6"l><"col-sm-12 col-md-6 text-end"f>>rt<"row mt-2"<"col-sm-12 col-md-5"i><"col-sm-12 col-md-7"p>>',
        language: {
            emptyTable: "No lot types found.",
            search: "Search:"
        },
        responsive: true,
        autoWidth: false
    });
}

function toggleArchive(id, isArchived) {
    const url = isArchived
        ? `/api/lottypes/unarchive/${id}`
        : `/api/lottypes/archive/${id}`;

    $.post(url, function (data) {
        if (data.success) {
            toastr.success(data.message, '', {
                toastClass: 'toast toast-custom-blue',
                timeOut: 2000,
                progressBar: true
            });
        } else {
            toastr.error(data.message || "Error toggling archive status.");
        }
        dataTable.ajax.reload(null, false);
    });
}
