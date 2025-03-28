@section Scripts {
    <script>
        var dataTable;

        $(document).ready(function () {
            loadList();
        });

        function loadList() {
            dataTable = $('#DT_load').DataTable({
                "ajax": {
                    "url": "/api/reservation",
                    "type": "GET",
                    "datatype": "json"
                },
                "columns": [
                    { data: "guest.user.firstName", width: "15%" },
                    { data: "guest.user.lastName", width: "15%" },
                    { data: "rv.licensePlate", width: "15%" },
                    { data: "lot.location", width: "10%" },
                    { data: "startDate", render: d => new Date(d).toLocaleDateString(), width: "10%" },
                    { data: "endDate", render: d => new Date(d).toLocaleDateString(), width: "10%" },
                    { data: "status", width: "10%" },
                    {
                        data: "reservationId",
                        width: "15%",
                        render: function (data) {
                            return `<div class="text-center">
                                <a href="/Admin/Reservations/Upsert?id=${data}"
                                   class="btn btn-success text-white" style="cursor:pointer; width:100px;">
                                    <i class="far fa-edit"></i> Edit
                                </a>
                                <a onClick="Delete('/api/reservation/${data}')" 
                                   class="btn btn-danger text-white" style="cursor:pointer; width:100px;">
                                    <i class="far fa-trash-alt"></i> Delete
                                </a>
                            </div>`;
                        }
                    }
                ],
                "language": {
                    "emptyTable": "No reservations found."
                },
                "width": "100%"
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

        function updateLength() {
            const selected = document.getElementById("LengthDropdown").value;
        const customInput = document.getElementById("CustomLength");
        const hiddenLength = document.getElementById("LengthHidden");

        if (selected === "custom") {
            customInput.style.display = "block";
        hiddenLength.value = customInput.value;
            } else {
            customInput.style.display = "none";
        hiddenLength.value = selected;
            }
        }

        function calculateDuration() {
            const start = new Date(document.getElementById("Reservation_StartDate").value);
        const end = new Date(document.getElementById("Reservation_EndDate").value);
            if (start && end && end > start) {
                const duration = (end - start) / (1000 * 3600 * 24);
        document.getElementById("Reservation_Duration").value = duration;
            }
        }
    </script>
}
