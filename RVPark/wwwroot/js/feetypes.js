// Declare the DataTable variable at the top level so it's accessible across functions.
var dataTable;

// When the page is fully loaded, initialize the data table.
$(document).ready(function () {
    loadList(); // Calls the function to populate the table with fee type data from the API.
});

// Function that initializes the DataTable with remote data.
function loadList() {
    dataTable = $('#DT_load').DataTable({
        "ajax": {
            "url": "/api/feetype",       // API endpoint for fetching all fee types.
            "type": "GET",               // HTTP GET request.
            "datatype": "json"           // Expected data type of the response.
        },
        "columns": [
            { data: "feeTypeName", width: "70%" }, // Displays the fee type's name in 70% of the row.

            // Adds buttons for editing and deleting a fee type.
            {
                data: "id", width: "30%",
                "render": function (data) {
                    return `<div class="text-center">
                            <a href="/Admin/FeeTypes/Upsert?id=${data}"
                            class="btn btn-custom-blue" style="cursor:pointer; width:100px;">
                                <i class="far fa-edit"></i>Edit
                            </a>
                            <a onClick=Delete('/api/feetype/'+${data})
                            class="btn btn-custom-grey" style="cursor:pointer; width:100px;">
                                <i class="far fa-trash-alt"></i>Delete
                            </a>
                        </div>`;
                }
            }
        ],
        "language": {
            "emptyTable": "no data found." // Message displayed when there are no fee types to show.
        },
        "width": "100%" // Sets the table to take up full width of the container.
    });
}

// Custom function to delete a record using a SweetAlert confirmation dialog.
function Delete(url) {
    swal({
        title: "Are you sure you want to delete?",
        text: "You will not be able to restore the data!",
        icon: "warning",
        buttons: true,
        dangerMode: true
    }).then((willDelete) => {
        if (willDelete) {
            $.ajax({
                url: url,
                type: "DELETE", // Sends a DELETE request to the API endpoint.
                success: function (data) {
                    if (data.success) {
                        toastr.success(data.message);     // Shows success message.
                        dataTable.ajax.reload();          // Reloads the DataTable with updated data.
                    } else {
                        toastr.error(data.message);       // Shows error message if deletion failed.
                    }
                }
            });
        }
    })
}
