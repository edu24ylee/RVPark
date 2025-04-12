let dataTable;

$(document).ready(function () {
    loadList();
});

function loadList() {
    dataTable = $('#DT_load').DataTable({
        dom: "<'row mb-3'<'col-sm-6'l><'col-sm-6 text-end'f>>" +
            "<'row'<'col-sm-12'tr>>" +
            "<'row mt-3'<'col-sm-5'i><'col-sm-7'p>>",
        ajax: {
            url: "/api/feetypes",
            type: "GET",
            dataType: "json",
            dataSrc: "data"
        },
        columns: [
            { data: "feeTypeName", width: "30%" },
            { data: "triggerType", width: "30%" },
            {
                data: "id",
                width: "20%",
                render: function (data, type, row) {
                    const isArchived = row.isArchived;

                    const archiveBtn = isArchived
                        ? `<button class="btn btn-sm btn-outline-custom-blue" onclick="unarchiveFeeType(${data})">
                           <i class="fas fa-box-open"></i> Unarchive
                       </button>`
                        : `<button class="btn btn-sm btn-custom-grey text-white" onclick="archiveFeeType(${data})">
                           <i class="fas fa-archive"></i> Archive
                       </button>`;

                    return `
                    <div class="d-flex justify-content-center gap-2">
                        <a href="/Admin/FeeTypes/Upsert?id=${data}" class="btn btn-sm btn-custom-blue-header text-white">
                            <i class="far fa-edit"></i> Edit
                        </a>
                        ${archiveBtn}
                    </div>`;
                }
            }
        ],
        drawCallback: function () {
            applyFilter('#feeTypeFilter', 0);
            applyFilter('#triggerTypeFilter', 1);
        },
        language: {
            emptyTable: "No fee types found."
        },
        width: "100%"
    });
}

function applyFilter(selector, columnIndex) {
    if (!dataTable || !dataTable.column) return;

    const $filter = $(selector);
    if ($filter.children('option').length > 1) return;

    const values = new Set();
    dataTable.column(columnIndex).nodes().each(cell => {
        const val = $(cell).text().trim();
        if (val) values.add(val);
    });

    [...values].sort().forEach(val => {
        $filter.append(`<option value="${val}">${val}</option>`);
    });

    $filter.on('change', function () {
        const val = $.fn.dataTable.util.escapeRegex($(this).val());
        dataTable.column(columnIndex).search(val ? `^${val}$` : '', true, false).draw();
    });
}


function archiveFeeType(id) {
    $.post(`/api/feetypes/archive/${id}`, function (data) {
        if (data.success) {
            toastr.success(data.message);
            dataTable.ajax.reload();
        } else {
            toastr.error(data.message);
        }
    });
}

function unarchiveFeeType(id) {
    $.post(`/api/feetypes/unarchive/${id}`, function (data) {
        if (data.success) {
            toastr.success(data.message);
            dataTable.ajax.reload();
        } else {
            toastr.error(data.message);
        }
    });
}
