var dataTable;

$(document).ready(function () {
    // Get selected park ID from query string (e.g., ?SelectedParkId=2)
    const parkId = new URLSearchParams(window.location.search).get("SelectedParkId");

    // If a park ID is present, load the lot list filtered by that park
    if (parkId) {
        loadList(parkId);
    }
});

function loadList(parkId) {
    // If the DataTable is already initialized, destroy it first to reinitialize with new data
    if ($.fn.DataTable.isDataTable("#DT_load")) {
        $('#DT_load').DataTable().destroy();
    }

    // Initialize the DataTable and populate it using an API call
    dataTable = $('#DT_load').DataTable({
        ajax: {
            url: `/api/lot/bypark/${parkId}`, // API endpoint to get lots for a specific park
            type: "GET",
            datatype: "json",
            dataSrc: "data",
            cache: false
        },
        columns: [
            { data: "lotType.name", width: "15%" },     // Lot type (e.g., Premium, Standard)
            { data: "park.name", width: "15%" },        // Associated park name
            { data: "location", width: "10%" },         // Lot location/identifier
            { data: "width", width: "10%" },            // Width of the lot
            { data: "length", width: "10%" },           // Length of the lot
            {
                data: "isAvailable",                    // Boolean availability flag
                render: data => data ? "Yes" : "No",    // Renders as "Yes"/"No"
                width: "10%"
            },
            { data: "description", width: "15%" },      // Optional description
            {
                data: "image",                          // Displays an image preview if available
                render: function (data) {
                    if (data) {
                        return `<img src="${data}" alt="Lot Image" style="max-height: 60px;" class="img-fluid rounded shadow-sm"/>`;
                    } else {
                        return `<span class="text-muted">No image</span>`;
                    }
                },
                width: "10%"
            },
            {
                data: "id",                              // Action buttons for Edit/Delete
                render: function (data) {
                    return `
                        <div class="text-center">
                            <a href="/Admin/Lots/Upsert?id=${data}" class="btn btn-sm btn-custom-blue">
                                <i class="fas fa-edit"></i> Edit
                            </a>
                            <button class="btn btn-sm btn-custom-grey" onclick="deleteLot(${data})">
                                <i class="fas fa-trash"></i> Delete
                            </button>
                        </div>`;
                },
                orderable: false,
                width: "15%"
            }
        ],
        language: {
            emptyTable: "No lots found." // Fallback message when there is no data
        },
        width: "100%" // Sets full width for the table
    });
}

// Deletes a lot after user confirmation using SweetAlert
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
                url: `/api/lot/${id}`,  // API endpoint to delete a lot
                type: "DELETE",
                success: function (data) {
                    if (data.success) {
                        toastr.success(data.message);      // Show success toast
                        $('#DT_load').DataTable().ajax.reload(); // Refresh table
                    } else {
                        toastr.error(data.message);        // Show error toast
                    }
                }
            });
        }
    });
}
