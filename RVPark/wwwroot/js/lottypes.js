let dataTable;

$(document).ready(function () {
    loadList();

    const rawToday = new Date();
    const today = new Date(rawToday.getFullYear(), rawToday.getMonth(), rawToday.getDate());
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
            url: "/api/lottype",
            type: "GET",
            dataType: "json",
            dataSrc: "data",
            complete: function (xhr) {
                const today = new Date();
                const data = xhr.responseJSON?.data ?? [];
                const nameSet = new Set();

                data.forEach(item => {
                    if (item.name) nameSet.add(item.name);
                    if (item.endDate) {
                        const endDate = new Date(item.endDate);
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

                const $nameFilter = $('#nameFilter');
                $nameFilter.find('option:not(:first)').remove();
                [...nameSet].sort().forEach(val => {
                    $nameFilter.append(`<option value="${val}">${val}</option>`);
                });
                $nameFilter.on('change', function () {
                    const val = $.fn.dataTable.util.escapeRegex($(this).val());
                    dataTable.column(0).search(val ? `^${val}$` : '', true, false).draw();
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
                render: data => data ? new Date(data).toLocaleDateString() : "—"
            },
            {
                data: "endDate",
                width: "15%",
                render: data => data ? new Date(data).toLocaleDateString() : "—"
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
        dom: '<"row mb-2"<"col-sm-12 col-md-6"l><"col-sm-12 col-md-6 text-end"f>>rt<"row mt-2"<"col-sm-12 col-md-5"i><"col-sm-12 col-md-7"p>>',
        language: {
            emptyTable: "No lot types found.",
            search: "Search:"
        },
        width: "100%"
    });

    dataTable.on('draw', function () {
        const $rateFilter = $('#rateFilter');
        if ($rateFilter.children('option').length <= 1) {
            const values = new Set();
            dataTable.column(1).nodes().each(cell => {
                const val = $(cell).text().trim().replace('$', '');
                if (val) values.add(val);
            });
            [...values].sort((a, b) => parseFloat(a) - parseFloat(b)).forEach(v => {
                $rateFilter.append(`<option value="${v}">${v}</option>`);
            });
        }
        $rateFilter.off('change').on('change', function () {
            const val = $.fn.dataTable.util.escapeRegex($(this).val());
            dataTable.column(1).search(val ? `^\\$?${val}$` : '', true, false).draw();
        });

        const $startDateFilter = $('#startDateFilter');
        if ($startDateFilter.children('option').length <= 1) {
            const values = new Set();
            dataTable.column(2).nodes().each(cell => {
                const val = $(cell).text().trim();
                if (val && val !== "—") values.add(val);
            });
            [...values].sort().forEach(v => {
                $startDateFilter.append(`<option value="${v}">${v}</option>`);
            });
        }
        $startDateFilter.off('change').on('change', function () {
            const val = $.fn.dataTable.util.escapeRegex($(this).val());
            dataTable.column(2).search(val ? `^${val}$` : '', true, false).draw();
        });

        const $endDateFilter = $('#endDateFilter');
        if ($endDateFilter.children('option').length <= 1) {
            const values = new Set();
            dataTable.column(3).nodes().each(cell => {
                const val = $(cell).text().trim();
                if (val && val !== "—") values.add(val);
            });
            [...values].sort().forEach(v => {
                $endDateFilter.append(`<option value="${v}">${v}</option>`);
            });
        }
        $endDateFilter.off('change').on('change', function () {
            const val = $.fn.dataTable.util.escapeRegex($(this).val());
            dataTable.column(3).search(val ? `^${val}$` : '', true, false).draw();
        });
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
