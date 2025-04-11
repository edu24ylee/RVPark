let dataTable;

$(document).ready(function () {
    loadList();
});

function loadList() {
    dataTable = $('#DT_load').DataTable({
        ajax: {
            url: "/api/parks",
            type: "GET",
            datatype: "json"
        },
        columns: [
            { data: "name", width: "25%" },
            { data: "address", width: "25%" },
            { data: "city", width: "15%" },
            { data: "state", width: "10%" },
            { data: "zipcode", width: "10%" },
            {
                data: "id",
                render: function (data, type, row) {
                    const isArchived = row.isArchived;

                    const archiveBtn = isArchived
                        ? `<button class="btn btn-sm btn-custom-blue-header" onclick="unarchivePark(${data})" style="width: 100px;">
                               <i class="fas fa-box-open"></i> Unarchive
                           </button>`
                        : `<button class="btn btn-sm btn-custom-grey" onclick="archivePark(${data})" style="width: 100px;">
                               <i class="fas fa-archive"></i> Archive
                           </button>`;

                    return `
                        <div class="text-center d-flex flex-column gap-2 align-items-center">
                            <a href="/Admin/Parks/Upsert?id=${data}" 
                               class="btn btn-sm btn-custom-blue" 
                               style="width: 100px;">
                                <i class="far fa-edit"></i> Edit
                            </a>
                            ${archiveBtn}
                        </div>`;
                },
                width: "15%"
            }
        ],
        language: {
            emptyTable: "No parks found."
        },
        width: "100%"
    });
}

function archivePark(id) {
    $.ajax({
        url: `/api/parks/archive/${id}`,
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

function unarchivePark(id) {
    $.ajax({
        url: `/api/parks/unarchive/${id}`,
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
