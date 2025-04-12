let dataTable;

$(document).ready(function () {
    loadList();
});

function loadList() {
    dataTable = $('#DT_load').DataTable({
        ajax: {
            url: "/api/fees",
            type: "GET",
            dataType: "json",
            dataSrc: "data"
        },
        columns: [
            { data: "id", width: "10%" },
            { data: "feeType.feeTypeName", width: "30%" },
            {
                data: "triggeringPolicy.policyName",
                width: "30%",
                render: function (data) {
                    return data || "N/A";
                }
            },
            {
                data: "feeTotal",
                width: "15%",
                render: $.fn.dataTable.render.number(',', '.', 2, '$')
            },
            {
                data: null,
                width: "15%",
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
