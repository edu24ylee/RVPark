let dataTable;

$(document).ready(function () {
    loadPolicyTable();
});

function loadPolicyTable() {
    dataTable = $('#DT_load').DataTable({
        dom:
            "<'row mb-3'<'col-sm-12 col-md-6'l><'col-sm-12 col-md-6 text-end'f>>" + 
            "<'row'<'col-sm-12'tr>>" +
            "<'row mt-3'<'col-sm-12 col-md-5'i><'col-sm-12 col-md-7 text-end'p>>",
        ajax: {
            url: "/api/policy",
            type: "GET",
            dataSrc: json => json?.data ?? []
        },
        columns: [
            { data: "policyName", title: "Policy Name", width: "30%" },
            { data: "policyDescription", title: "Description", width: "50%" },
            {
                data: null,
                title: "Actions",
                orderable: false,
                width: "20%",
                render: function (data, type, row) {
                    const isSuperAdmin = window.isSuperAdmin === "true";

                    const archiveBtn = `
                        <button class="btn btn-sm btn-custom-grey text-white" onclick="archivePolicy(${row.id})">
                            <i class="fas fa-archive"></i> Archive
                        </button>`;

                    const unarchiveBtn = isSuperAdmin ? `
                        <button class="btn btn-sm btn-outline-custom-blue" onclick="unarchivePolicy(${row.id})">
                            <i class="fas fa-box-open"></i> Unarchive
                        </button>` : '';

                    const editBtn = `
                        <a href="/Admin/Policies/Upsert?id=${row.id}" class="btn btn-sm btn-custom-blue text-white">
                            <i class="fas fa-edit"></i> Edit
                        </a>`;

                    return `
                        <div class="d-flex justify-content-end gap-2">
                            ${editBtn}
                            ${row.isArchived ? unarchiveBtn : archiveBtn}
                        </div>`;
                }
            }
        ],
        initComplete: function () {
            this.api().columns([0, 1]).every(function () {
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
            emptyTable: "No policies found."
        },
        width: "100%"
    });
}

function archivePolicy(id) {
    $.post(`/api/policy/archive/${id}`, function (data) {
        if (data.success) {
            toastr.success(data.message);
            dataTable.ajax.reload();
        } else {
            toastr.error(data.message);
        }
    });
}

function unarchivePolicy(id) {
    $.post(`/api/policy/unarchive/${id}`, function (data) {
        if (data.success) {
            toastr.success(data.message);
            dataTable.ajax.reload();
        } else {
            toastr.error(data.message);
        }
    });
}
