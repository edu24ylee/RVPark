// Declare a global variable to store the DataTable instance
var dataTable;

$(document).ready(function () {
    // When the DOM is ready, populate the reservations list
    loadList();
});

function loadList() {
    // Initialize the DataTable and bind it to the #DT_load element
    dataTable = $('#DT_load').DataTable({
        ajax: {
            // Fetch reservation data from the API
            // Adding a timestamp query parameter (`_`) ensures the request isn't cached
            url: "/api/reservation?_=" + new Date().getTime(),
            type: "GET",
            datatype: "json",
            cache: false
        },
        columns: [
            // Maps and displays reservation fields into table columns

            { data: "reservationId", title: "Reservation ID", width: "10%" },

            // Retrieves guest first and last name through nested navigation (Guest → User)
            { data: "guest.user.firstName", title: "First Name", width: "15%" },
            { data: "guest.user.lastName", title: "Last Name", width: "15%" },

            // RV's license plate number
            { data: "rv.licensePlate", title: "License Plate", width: "15%" },

            // Lot location (e.g., "B12", "North Pad 7")
            { data: "lot.location", title: "Lot", width: "10%" },

            // Reservation start and end dates, formatted as local strings
            {
                data: "startDate",
                render: d => new Date(d).toLocaleDateString(),
                title: "Start Date",
                width: "10%"
            },
            {
                data: "endDate",
                render: d => new Date(d).toLocaleDateString(),
                title: "End Date",
                width: "10%"
            },

            // Reservation status (e.g., "Upcoming", "Active", "Completed")
            { data: "status", title: "Status", width: "10%" },

            // Action column with Edit and Delete buttons
            {
                data: "reservationId",
                width: "15%",
                render: function (data) {
                    return `
                        <div class="text-center">
                            <a href="/Admin/Reservations/Update/${data}" 
                               class="btn btn-custom-blue text-white mb-1" 
                               style="width:100px;">
                                <i class="far fa-edit"></i> Edit
                            </a>
                            <a onClick="Delete('/api/reservation/${data}')" 
                               class="btn btn-custom-grey text-white" 
                               style="width:100px;">
                                <i class="far fa-trash-alt"></i> Delete
                            </a>
                        </div>`;
                }
            }
        ],
        // Fallback message when no reservation records exist
        language: { emptyTable: "No reservations found." },

        // Use the full width of the parent container
        width: "100%"
    });
}

// Called when the Delete button is clicked
// Uses SweetAlert to confirm and then sends a DELETE request
function Delete(url) {
    swal({
        title: "Are you sure you want to delete?",
        text: "You will not be able to restore this reservation!",
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
                        toastr.success(data.message);            // Show success toast
                        $('#DT_load').DataTable().ajax.reload(); // Refresh the table
                    } else {
                        toastr.error(data.message);              // Show error toast
                    }
                },
                error: function (xhr) {
                    toastr.error("Error: " + xhr.responseText);  // Display technical error
                }
            });
        }
    });
}
