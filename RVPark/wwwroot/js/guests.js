function Delete(url) {
    swal({
        title: "Are you sure?",
        text: "This guest and their affiliation will be permanently deleted.",
        icon: "warning",
        buttons: true,
        dangerMode: true
    }).then((willDelete) => {
        if (willDelete) {
            $.ajax({
                type: "DELETE",
                url: url,
                success: function (data) {
                    if (data.success) {
                        toastr.success(data.message);
                        $('#DT_load').DataTable().ajax.reload();
                    } else {
                        toastr.error("Failed to delete guest.");
                    }
                },
                error: function () {
                    toastr.error("An error occurred while deleting the guest.");
                }
            });
        }
    });
}

let guestTable;

$(document).ready(function () {
    guestTable = $('#DT_load').DataTable({
        "ajax": {
            "url": "/api/guest",
            "type": "GET",
            "datatype": "json"
        },
        "columns": [
            {
                data: "user",
                render: data => `${data.firstName} ${data.lastName}`,
                width: "20%"
            },
            { "data": "user.email", "width": "15%" },
            { "data": "user.phone", "width": "10%" },
            { "data": "dodId", "width": "10%" },
            { "data": "branch", "width": "10%" },
            { "data": "status", "width": "10%" },
            { "data": "rank", "width": "10%" },
            {
                "data": "guestId",
                "render": function (data, type, row) {
                    const isArchived = row.user.isArchived;
                    const isSuperAdmin = window.isSuperAdmin === true || window.isSuperAdmin === "true";

                    const archiveBtn = isArchived
                        ? (isSuperAdmin
                            ? `<button class="btn btn-sm btn-outline-custom-blue" onclick="unarchiveGuest(${data})">
                                   <i class="fas fa-box-open"></i> Unarchive
                               </button>`
                            : ``)
                        : `<button class="btn btn-sm btn-custom-grey" onclick="archiveGuest(${data})">
                               <i class="fas fa-archive"></i> Archive
                           </button>`;
                    return `
                        <div class="text-center">
                            <a href="/Admin/Guests/Upsert?id=${data}" class="btn btn-sm btn-primary mx-1">
                                <i class="fas fa-edit"></i> Update
                            </a>
                            ${archiveBtn}
                        </div>
                    `;
                },
                "orderable": false,
                "width": "20%"
            }
        ],
        "language": {
            "emptyTable": "No guests found."
        },
        "width": "100%"
    });
});

function archiveGuest(id) {
    $.ajax({
        url: `/api/guest/archive/${id}`,
        type: "POST",
        success: function (data) {
            if (data.success) {
                toastr.success(data.message);
                guestTable.ajax.reload(null, false);
            } else {
                toastr.error(data.message);
            }
        }
    });
}

function unarchiveGuest(id) {
    $.ajax({
        url: `/api/guest/unarchive/${id}`,
        type: "POST",
        success: function (data) {
            if (data.success) {
                toastr.success(data.message);
                guestTable.ajax.reload(null, false);
            } else {
                toastr.error(data.message);
            }
        }
    });
}