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

$(document).ready(function () {
    $('#DT_load').DataTable({
        "ajax": {
            "url": "/api/guest",
            "type": "GET",
            "datatype": "json"
        },
        "columns": [
            { "data": "fullName", "width": "15%" },
            { "data": "email", "width": "15%" },
            { "data": "phone", "width": "10%" },
            { "data": "dodId", "width": "10%" },
            { "data": "branch", "width": "10%" },
            { "data": "status", "width": "10%" },
            { "data": "rank", "width": "10%" },
            {
                "data": "guestId",
                "render": function (data) {
                    return `
                        <div class="text-center">
                            <a href="/Admin/Guests/Upsert?id=${data}" class="btn btn-sm btn-primary mx-1">
                                <i class="fas fa-edit"></i>
                            </a>
                            <a onClick="Delete('/api/guest/${data}')" class="btn btn-sm btn-danger mx-1">
                                <i class="fas fa-trash-alt"></i>
                            </a>
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
