$(document).ready(function () {
    const parkId = new URLSearchParams(window.location.search).get("SelectedParkId");
    if (parkId) {
        loadList(parkId);
    }

    if ($('textarea').length) {
        tinymce.init({
            selector: 'textarea',
            plugins: 'lists',
            menubar: "file edit format"
        });
    }

});


function loadList(parkId) {
    dataTable = $('#DT_load').DataTable({
        "ajax": {
            "url": `/api/lot/bypark/${parkId}`,
            "type": "GET",
            "datatype": "json",
            "dataSrc": "data",
            "cache": false
        },
        "columns": [
            { data: "lotType.name", width: "15%" },
            { data: "park.name", width: "15%" },
            { data: "location", width: "10%" },
            { data: "width", width: "10%" },
            { data: "length", width: "10%" },
            {
                data: "isAvailable",
                render: data => data ? "Yes" : "No",
                width: "10%"
            },
            { data: "description", width: "20%" },
            {
                data: "id",
                render: data => `
                    <div class="text-center">
                        <a href="/Admin/Lots/Upsert?id=${data}" class="btn btn-sm btn-custom-blue">
                            <i class="fas fa-edit"></i> Edit
                        </a>
                        <button class="btn btn-sm btn-custom-grey" onclick="deleteLot(${data})">
                            <i class="fas fa-trash"></i> Delete
                        </button>
                    </div>`,
                orderable: false,
                width: "20%"
            }
        ],
        "language": {
            "emptyTable": "No lots found."
        },
        "width": "100%"
    });
}

function deleteLot(id) {
    swal({
        title: "Are you sure you want to delete?",
        text: "This lot will be permanently removed.",
        icon: "warning",
        buttons: true,
        dangerMode: true
    }).then((confirmed) => {
        if (confirmed) {
            $.ajax({
                url: `/api/lot/${id}`,
                type: "DELETE",
                success: function (data) {
                    if (data.success) {
                        toastr.success(data.message);
                        $('#DT_load').DataTable().ajax.reload();
                    } else {
                        toastr.error(data.message);
                    }
                }
            });
        }
    });
}
