let dataTable;

$(document).ready(function () {
    loadList();
});

function loadList() {
    dataTable = $('#DT_load').DataTable({
        dom:
        "<'row mb-3'<'col-sm-12 col-md-6'l><'col-sm-12 col-md-6 text-end'f>>" +]
        "<'row'<'col-sm-12'tr>>" +
            "<'row mt-3'<'col-sm-12 col-md-5'i><'col-sm-12 col-md-7 text-end'p>>",
        ajax: {
            url: "/api/fees",
            type: "GET",
            dataType: "json",
            dataSrc: "data"
        },
        columns: [
            { data: "id", title: "ID", width: "10%" },
            { data: "feeType", title: "Fee Type", width: "25%" }, 
            {
                data: "triggeringPolicy",
                title: "Triggering Policy",
                width: "25%",
                render: function (data) {
                    return data || "N/A";
                }
            },
            {
                data: "triggerType",
                title: "Trigger Type",
                width: "15%",
                render: function (data) {
                    return data === 0 ? "Manual" : "Triggered";
                }
            },
            {
                data: "feeTotal",
                title: "Total",
                width: "15%",
                render: $.fn.dataTable.render.number(',', '.', 2, '$')
            },
            {
                data: null,
                title: "Actions",
                width: "20%",
                render: function (data) {
                    const editBtn = `
                        <a href="/Admin/Fees/Upsert?id=${data.id}" 
                           class="btn btn-sm btn-custom-blue me-1">
                            <i class="far fa-edit"></i> Edit
                        </a>`;

                    const archiveBtn = data.isArchived
                        ? `<button class="btn btn-sm btn-outline-custom-blue" onclick="unarchiveFee(${data.id})">
                               <i class="fas fa-box-open"></i> Unarchive
                           </button>`
                        : `<button class="btn btn-sm btn-custom-grey text-white" onclick="archiveFee(${data.id})">
                               <i class="fas fa-archive"></i> Archive
                           </button>`;

                    return `<div class="text-end">${editBtn}${archiveBtn}</div>`;
                }
            }
        ],
        initComplete: function () {
            this.api().columns([1, 2, 3]).every(function () {
                const column = this;
                const columnIndex = column.index();
                const originalTitle = $('#DT_load thead th').eq(columnIndex).text();

                const $wrapper = $('<div class="d-flex flex-column align-items-start"></div>');
                const $label = $(`<label class="fw-semibold small mb-1">${originalTitle}</label>`);
                const $select = $(`<select class="form-select form-select-sm"><option value="">All ${originalTitle}</option></select>`);

                column.data().unique().sort().each(function (d) {
                    if (d || d === 0) {
                        $select.append(`<option value="${d}">${d}</option>`);
                    }
                });

                $select.on('change', function () {
                    const val = $.fn.dataTable.util.escapeRegex($(this).val());
                    column.search(val ? `^${val}$` : '', true, false).draw();
                });

                $wrapper.append($label).append($select);
                $(column.header()).empty().append($wrapper);
            });
        },
        language: {
            emptyTable: "No fees found."
        },
        width: "100%"
    });
}

function archiveFee(id) {
    $.post(`/api/fees/archive/${id}`, function (data) {
        if (data.success) {
            toastr.success(data.message);
            dataTable.ajax.reload();
        } else {
            toastr.error(data.message);
        }
    });
}

function unarchiveFee(id) {
    $.post(`/api/fees/unarchive/${id}`, function (data) {
        if (data.success) {
            toastr.success(data.message);
            dataTable.ajax.reload();
        } else {
            toastr.error(data.message);
        }
    });
}
