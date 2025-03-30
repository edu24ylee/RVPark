var dataTable;

$(document).ready(function () {
    loadList();
});

function loadList() {
    dataTable = $('#DT_load').DataTable({
        ajax: {
            url: "/api/reservation?_=" + new Date().getTime(),
            type: "GET",
            datatype: "json",
            cache: false
        },
        columns: [
            { data: "reservationId", title: "Reservation ID", width: "10%" },
            { data: "guest.user.firstName", title: "First Name", width: "15%" },
            { data: "guest.user.lastName", title: "Last Name", width: "15%" },
            { data: "rv.licensePlate", title: "License Plate", width: "15%" },
            { data: "lot.location", title: "Lot", width: "10%" },
            { data: "startDate", render: d => new Date(d).toLocaleDateString(), title: "Start Date", width: "10%" },
            { data: "endDate", render: d => new Date(d).toLocaleDateString(), title: "End Date", width: "10%" },
            { data: "status", title: "Status", width: "10%" },
            {
                data: "reservationId",
                width: "15%",
                render: function (data) {
                    return `
                        <div class="text-center">
                            <a href="/Admin/Reservations/Update/${data}" class="btn btn-custom-blue text-white mb-1" style="width:100px;">
                                <i class="far fa-edit"></i> Edit
                            </a>
                            <a onClick="Delete('/api/reservation/${data}')" class="btn btn-custom-grey text-white" style="width:100px;">
                                <i class="far fa-trash-alt"></i> Delete
                            </a>
                        </div>`;
                }
            }
        ],
        language: { emptyTable: "No reservations found." },
        width: "100%"
    });
}

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
