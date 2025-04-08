/*var dataTable;

$(document).ready(function () {
    loadList(); // Load the roles table on page load
});

// Function to initialize and populate the DataTable with role data
function loadList() {
    dataTable = $('#DT_load').DataTable({
        ajax: {
            url: "/api/roles",     // API endpoint that returns all roles
            type: "GET",
            datatype: "json"
        },
        columns: [
            { data: "name", title: "Role Name", width: "50%" }, // Display role name (e.g., Admin, Manager)
            {
                data: "id", width: "50%",
                render: function (data) {
                    return `<div class="text-center">
                                <a href="/Admin/Roles/Upsert?id=${data}" 
                                   class="btn btn-custom-blue text-white" 
                                   style="cursor:pointer; width:100px;">
                                   <i class="far fa-edit"></i> Edit
                                </a>
                                <a onClick="Delete('/api/roles/${data}')" 
                                   class="btn btn-custom-grey text-white" 
                                   style="cursor:pointer; width:100px;">
                                   <i class="far fa-trash-alt"></i> Delete
                                </a>
                            </div>`;
                }
            }
        ],
        language: {
            emptyTable: "No roles found." // Message shown if there are no roles in the system
        },
        width: "100%" // Ensure the table uses full width of its container
    });
}

// Function to delete a role with confirmation dialog
function Delete(url) {
    swal({
        title: "Are you sure you want to delete?",
        text: "This role will be permanently removed.",
        icon: "warning",
        buttons: true,
        dangerMode: true
    }).then((willDelete) => {
        if (willDelete) {
            $.ajax({
                url: url,
                type: "DELETE",  // Sends delete request to backend
                success: function (data) {
                    if (data.success) {
                        toastr.success(data.message);            // Show success toast
                        $('#DT_load').DataTable().ajax.reload(); // Reload the table
                    } else {
                        toastr.error(data.message);              // Show error toast
                    }
                }
            });
        }
    });
}*/
