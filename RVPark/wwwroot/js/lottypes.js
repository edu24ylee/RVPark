let dataTable;

$(document).ready(function () {
    loadList();

    const rawToday = new Date();
    const today = new Date(rawToday.getFullYear(), rawToday.getMonth(), rawToday.getDate());

    const fiscalStart = new Date(today.getFullYear(), 9, 1); // Oct 1
    const fiscalReminderStart = new Date(fiscalStart);
    fiscalReminderStart.setDate(fiscalStart.getDate() - 5);

    const isSuperAdmin = window.isSuperAdmin === true || window.isSuperAdmin === "true";

    // Custom blue reminder for SuperAdmin only
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
            datatype: "json",
            dataSrc: "data",
            complete: function (xhr) {
                const rawToday = new Date();
                const today = new Date(rawToday.getFullYear(), rawToday.getMonth(), rawToday.getDate());

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
                data: "id",
                width: "20%",
                render: function (data, type, row) {
                    const isArchived = row.isArchived;
                    const archiveBtn = isArchived
                        ? `<button class="btn btn-sm btn-outline-custom-blue" onclick="unarchiveLotType(${data})">
                               <i class="fas fa-box-open"></i> Unarchive
                           </button>`
                        : `<button class="btn btn-sm btn-custom-grey text-white" onclick="archiveLotType(${data})">
                               <i class="fas fa-archive"></i> Archive
                           </button>`;

                    return `
                        <div class="text-center d-flex justify-content-center gap-2">
                            <a href="/Admin/LotTypes/Upsert?id=${data}" class="btn btn-sm btn-custom-blue text-white">
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
        url: `/api/lottypes/archive/${id}`,
        type: "POST",
        success: function (data) {
            if (data.success) {
                toastr.success(data.message);
                dataTable.ajax.reload();
            } else {
                toastr.error(data.message);
            }
        }
    });
}

function unarchiveLotType(id) {
    $.ajax({
        url: `/api/lottypes/unarchive/${id}`,
        type: "POST",
        success: function (data) {
            if (data.success) {
                toastr.success(data.message);
                dataTable.ajax.reload();
            } else {
                toastr.error(data.message);
            }
        }
    });
}
