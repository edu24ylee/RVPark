var dataTable;

$(document).ready(function () {
    loadList();
});

// Initializes the DataTable for the Lot Types list
function loadList() {
    dataTable = $('#DT_load').DataTable({
        "ajax": {
            "url": "/api/lottypes",    // API endpoint to fetch all lot types
            "type": "GET",             // HTTP method
            "datatype": "json",        // Expected data format
            "dataSrc": "data"          // Extracts array from returned object (if API wraps it)
        },
        "columns": [
            { data: "name", width: "30%" },     // Lot type name (e.g., Standard, Premium)
            {
                data: "rate",
                width: "20%",
                render: $.fn.dataTable.render.number(',', '.', 2, '$') // Format as currency
            },
            { data: "park.name", width: "30%" }, // Park the lot type belongs to
            {
                data: "id",
                width: "20%",
                render: function (data) {
                    return `<div class="text-center">
                                <a href="/Admin/LotTypes/Upsert?id=${data}" 
                                   class="btn btn-custom-blue text-white" 
                                   style="cursor:pointer; width:100px;">
                                    <i class="far fa-edit"></i> Edit
                                </a>
                                <a onClick="Delete('/api/lottypes/${data}')" 
                                   class="btn btn-custom-grey text-white" 
                                   style="cursor:pointer; width:100px;">
                                    <i class="far fa-trash-alt"></i> Delete
                                </a>
                            </div>`;
                }
            }
        ],
        "language": {
            "emptyTable": "No lot types found." // Message when table is empty
        },
        "width": "100%"
    });
}

// Handles deletion of a lot type with confirmation
function Delete(url) {
    swal({
        title: "Are you sure you want to delete?",
        text: "This will permanently remove the lot type.",
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
                        toastr.success(data.message);             // Show success message
                        $('#DT_load').DataTable().ajax.reload();  // Refresh table data
                    } else {
                        toastr.error(data.message);               // Show error message
                    }
                },
                error: function (xhr) {
                    toastr.error("Error: " + xhr.responseText);   // Show technical error
                }
            });
        }
    });
}
