let dataTable;

$(document).ready(function () {
    loadReservations();

    $('#statusFilter').on('change', function () {
        const filter = this.value;
        dataTable.ajax.url(`/api/reservation?filter=${filter}`).load();
    });

    $('#DT_load thead').on('keyup change', '.column-filter', function () {
        dataTable.draw();
    });

    $(document).on('change', '.status-dropdown:not([disabled])', function () {
        const reservationId = $(this).data('id');
        const newStatus = $(this).val();

        if (newStatus === 'Cancelled') {
            confirmCancel(reservationId);
        } else {
            $.ajax({
                url: `/api/reservation/status/${reservationId}`,
                type: "POST",
                contentType: "application/json",
                data: JSON.stringify({ status: newStatus }),
                success: response => {
                    if (response.success) {
                        toastr.success("Status updated.");
                        dataTable.ajax.reload();
                    } else {
                        toastr.error(response.message || "Status update failed.");
                    }
                },
                error: xhr => {
                    toastr.error("Error: " + xhr.responseText);
                }
            });
        }
    });
});

function loadReservations() {
    dataTable = $('#DT_load').DataTable({
        ajax: {
            url: `/api/reservation?filter=active`,
            type: "GET",
            dataSrc: json => json.data
        },
        columns: [
            { data: "reservationId" },
            { data: "guest.user.firstName" },
            { data: "guest.user.lastName" },
            { data: "rv.licensePlate" },
            { data: "lot.location" },
            {
                data: "startDate",
                render: d => new Date(d).toLocaleDateString()
            },
            {
                data: "endDate",
                render: d => new Date(d).toLocaleDateString()
            },
            {
                data: "status",
                render: (data, type, row) => {
                    const disabled = data === "Cancelled" ? "disabled" : "";
                    const options = ["Pending", "Confirmed", "CheckedIn", "Completed", "Cancelled"]
                        .map(opt => `<option value="${opt}" ${opt === data ? "selected" : ""}>${opt}</option>`)
                        .join('');
                    return `<select class="form-select form-select-sm status-dropdown" data-id="${row.reservationId}" ${disabled}>${options}</select>`;
                }
            },
            {
                data: "totalDue",
                render: b => `$${(parseFloat(b) || 0).toFixed(2)}`
            },
            {
                data: null,
                orderable: false,
                render: function (data, type, row) {
                    const id = row.reservationId;
                    const balance = parseFloat(row.totalDue || 0);
                    const edit = `<a href="/Admin/Reservations/Update/${id}" class="btn btn-sm btn-custom-blue text-white"><i class="fas fa-edit"></i> Edit</a>`;
                    const pay = balance > 0 ? `<a href="/Admin/Reservations/Payment/${id}" class="btn btn-sm btn-success text-white"><i class="fas fa-dollar-sign"></i> Pay</a>` : '';
                    return `<div class="d-flex gap-1 justify-content-center">${edit}${pay}</div>`;
                }
            }

        ],
        initComplete: function () {
            this.api().columns().every(function (index) {
                const column = this;
                $('input, select', $('#DT_load thead th').eq(index)).on('keyup change', function () {
                    column.search(this.value).draw();
                });
            });

            $.fn.dataTable.ext.search.push(function (settings, data) {
                const balanceFilter = $('#balanceFilter').val();
                const balanceText = data[8].replace('$', '').trim();
                const balance = parseFloat(balanceText) || 0;
                if (balanceFilter === "has" && balance <= 0) return false;
                if (balanceFilter === "none" && balance > 0) return false;
                return true;
            });

            $('#balanceFilter').on('change', function () {
                dataTable.draw();
            });
        },
        language: {
            emptyTable: "No reservations found."
        }
    });
}

function confirmCancel(id) {
    swal({
        title: "Confirm Cancellation",
        text: "Cancelling may apply a cancellation fee if within 24 hours of check-in.",
        icon: "warning",
        content: createOverrideForm(),
        buttons: {
            cancel: "Back",
            confirm: {
                text: "Confirm",
                closeModal: false
            }
        }
    }).then((willCancel) => {
        if (willCancel) {
            const override = document.getElementById("overrideCheckbox").checked;
            const percent = override ? parseInt(document.getElementById("overridePercent").value) : null;
            const reason = override ? document.getElementById("cancelOverrideReason").value : "";

            $.ajax({
                type: "POST",
                url: `/api/reservation/cancel/${id}`,
                data: JSON.stringify({ override, percent, reason }),
                contentType: "application/json",
                success: function (data) {
                    if (data.success) {
                        toastr.success(data.message);
                        if (window.dataTable) {
                            dataTable.ajax.reload(null, false);
                        }
                        swal.close();
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

function createOverrideForm() {
    const div = document.createElement("div");
    div.innerHTML = `
        <div class="form-check mb-2">
            <input class="form-check-input" type="checkbox" id="overrideCheckbox">
            <label class="form-check-label" for="overrideCheckbox">Override Cancellation Fee</label>
        </div>
        <div id="overrideFields" style="display:none;">
            <label class="form-label mt-2">Override Percentage:</label>
            <select id="overridePercent" class="form-select form-select-sm mb-2">
                ${[100, 90, 80, 70, 60, 50, 40, 30, 20, 10, 0].map(p => `<option value="${p}">${p}%</option>`).join('')}
            </select>
            <label class="form-label">Reason:</label>
            <textarea id="cancelOverrideReason" class="form-control form-control-sm" rows="2"></textarea>
        </div>
    `;
    setTimeout(() => {
        document.getElementById("overrideCheckbox").addEventListener("change", function () {
            document.getElementById("overrideFields").style.display = this.checked ? "block" : "none";
        });
    }, 10);
    return div;
}
