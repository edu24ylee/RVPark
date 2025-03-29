var dataTable;

$(document).ready(function () {
    loadList();
});

function loadList() {
    dataTable = $('#DT_load').DataTable({
        "ajax": {
            "url": "/api/parks",
            "type": "GET",
            "datatype": "json",
            "dataSrc": "data"
        },
        "columns": [
            { data: "name", width: "20%" },
            { data: "address", width: "25%" },
            { data: "city", width: "15%" },
            { data: "state", width: "10%" },
            { data: "zipcode", width: "10%" },
            {
                data: "id",
                width: "20%",
                render: function (data) {
                    return `<div class="text-center">
                        <a href="/Admin/Parks/Upsert?id=${data}" class="btn btn-custom-blue text-white" style="cursor:pointer; width:100px;">
                            <i class="far fa-edit"></i> Edit
                        </a>
                        <a onClick="Delete('/api/parks/${data}')" class="btn btn-custom-grey text-white" style="cursor:pointer; width:100px;">
                            <i class="far fa-trash-alt"></i> Delete
                        </a>
                    </div>`;
                }
            }
        ],
        "language": {
            "emptyTable": "No parks found."
        },
        "width": "100%"
    });
}

function Delete(url) {
    swal({
        title: "Are you sure you want to delete?",
        text: "You will not be able to restore this park!",
        icon: "warning",
        buttons: true,
        dangerMode: true
    }).then((willDelete) => {
        if (willDelete) {
            $.ajax({
                type: 'DELETE',
                url: url,
                success: function (data) {
                    if (data.success) {
                        toastr.success(data.message);
                        $('#DT_load').DataTable().ajax.reload();
                    } else {
                        toastr.error(data.message);
                    }
                },
                error: function (xhr) {
                    toastr.error("Error: " + xhr.responseText);
                }
            });
        }
    });
}
