let dataTable;

$(document).ready(function () {
    initializeReservationsTable();

    $('#statusFilter').on('change', function () {
        dataTable.draw();
    });

    $(document).on('change', '.status-dropdown', function () {
        const reservationId = $(this).data('id');
        const newStatus = $(this).val();

        $.ajax({
            url: `/api/reservation/status/${reservationId}`,
            type: "POST",
            contentType: "application/json",
            data: JSON.stringify({ status: newStatus }),
            success: response => {
                if (response.success) {
                    toastr.success("Status updated.");
                    dataTable.ajax.reload(null, false);
                } else {
                    toastr.error(response.message || "Status update failed.");
                }
            },
            error: xhr => toastr.error("Error: " + xhr.responseText)
        });
    });
});

function initializeReservationsTable() {
    $.fn.dataTable.ext.search.push(function (settings, data) {
        const statusFilter = $('#statusFilter').val();
        const status = data[7]?.trim();
        const balance = parseFloat(data[8].replace('$', '')) || 0;
        const balanceFilter = $('#balanceFilter').val();

        if (statusFilter === 'active' && (status === 'Cancelled' || status === 'Completed')) return false;
        if (balanceFilter === 'has' && balance <= 0) return false;
        if (balanceFilter === 'none' && balance > 0) return false;

        return true;
    });

    dataTable = $('#DT_load').DataTable({
        ajax: {
            url: `/api/reservation?filter=all`,
            type: "GET",
            dataSrc: json => json.data || []
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
                render: function (data, type, row) {
                    const options = ["Pending", "Confirmed", "CheckedIn", "Completed"]
                        .map(opt => `<option value="${opt}" ${opt === data ? "selected" : ""}>${opt}</option>`)
                        .join('');
                    const isCancelled = data === "Cancelled";
                    return `<select class="form-select form-select-sm status-dropdown" data-id="${row.reservationId}" ${isCancelled ? "disabled" : ""}>${options}</select>`;
                }
            },
            {
                data: "remainingBalance",
                render: b => `$${parseFloat(b).toFixed(2)}`
            },
            {
                data: null,
                orderable: false,
                render: function (data, type, row) {
                    const id = row.reservationId;
                    const balance = parseFloat(row.remainingBalance || 0);
                    const edit = `<a href="/Admin/Reservations/Update/${id}" class="btn btn-sm btn-custom-blue text-white"><i class="fas fa-edit"></i> Edit</a>`;
                    const pay = balance > 0
                        ? `<a href="/Admin/Reservations/Payment/${id}" class="btn btn-sm btn-success text-white"><i class="fas fa-dollar-sign"></i> Pay</a>`
                        : '';
                    const cancel = row.status !== "Cancelled"
                        ? `<button onclick="confirmCancel(${id})" class="btn btn-sm btn-danger"><i class="fas fa-ban"></i> Cancel</button>`
                        : '';
                    return `<div class="d-flex gap-1 justify-content-center">${edit}${pay}${cancel}</div>`;
                }
            }
        ],
        initComplete: function () {
            const api = this.api();

            // Ensure only one filter row
            $('#DT_load thead .filter-row').remove();
            $('#DT_load thead').append('<tr class="filter-row"></tr>');

            api.columns().every(function (index) {
                let filterHtml = '';

                if (index === 7) {
                    filterHtml = `
                        <select class="form-select form-select-sm column-filter">
                            <option value="">All</option>
                            <option value="Pending">Pending</option>
                            <option value="Confirmed">Confirmed</option>
                            <option value="CheckedIn">CheckedIn</option>
                            <option value="Completed">Completed</option>
                        </select>`;
                } else if (index === 8) {
                    filterHtml = `
                        <select id="balanceFilter" class="form-select form-select-sm column-filter">
                            <option value="">All</option>
                            <option value="has">Has Balance</option>
                            <option value="none">No Balance</option>
                        </select>`;
                } else {
                    filterHtml = '';
                }

                $('.filter-row').append(`<th>${filterHtml}</th>`);

                if (filterHtml) {
                    $('.filter-row th').eq(index).find('.column-filter').on('change', function () {
                        api.column(index).search(this.value).draw();
                    });
                }
            });

            $('#balanceFilter').on('change', function () {
                dataTable.draw();
            });
        },
        language: {
            emptyTable: "No reservations found."
        },
        scrollX: true,
        responsive: true,
        autoWidth: false
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
    }).then(willCancel => {
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
                        dataTable.ajax.reload(null, false);
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
