var dataTable;

$(document).ready(function () {
    loadList(); // Initializes the table when the page is ready
});

// Loads and binds the park data into a DataTable
function loadList() {
    dataTable = $('#DT_load').DataTable({
        "ajax": {
            "url": "/api/parks",   // API endpoint to retrieve all parks
            "type": "GET",         // HTTP method
            "datatype": "json"     // Expected response format
        },
        "columns": [
            { data: "name", width: "25%" },      // Park name (e.g., "Desert Eagle")
            { data: "address", width: "25%" },   // Full street address
            { data: "city", width: "15%" },      // City name
            { data: "state", width: "10%" },     // State/region
            { data: "zipcode", width: "10%" },   // ZIP or postal code
            {
                data: "id",                      // Action buttons column
                width: "15%",
                render: function (data) {
                    return `<div class="text-center">
                                <a href="/Admin/Parks/Upsert?id=${data}" 
                                   class="btn btn-custom-blue" 
                                   style="cursor:pointer; width:100px;">
                                   <i class="far fa-edit"></i> Edit
                                </a>
                                <a onClick="Delete('/api/parks/${data}')" 
                                   class="btn btn-custom-grey" 
                                   style="cursor:pointer; width:100px;">
                                   <i class="far fa-trash-alt"></i> Delete
                                </a>
                            </div>`;
                }
            }
        ],
        "language": {
            "emptyTable": "No parks found." // Message when there’s no data
        },
        "width": "100%" // Use full width for layout consistency
    });
}

// Prompts the user to confirm before deleting a park
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
                url: url,
                type: "DELETE",  // Sends a DELETE request to the specified API URL
                success: function (data) {
                    if (data.success) {
                        toastr.success(data.message);             // Notify success
                        $('#DT_load').DataTable().ajax.reload();  // Reload DataTable data
                    } else {
                        toastr.error(data.message);               // Notify error
                    }
                }
            });
        }
    });
}
