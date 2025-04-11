let employeeTable;

$(document).ready(function () {
    loadEmployeeList();
});

function loadEmployeeList() {
    employeeTable = $('#DT_load').DataTable({
        ajax: {
            url: `/api/employees`,
            type: "GET",
            datatype: "json",
            dataSrc: "data"
        },
        columns: [
            {
                data: "user",
                render: data => `${data.firstName} ${data.lastName}`,
                width: "20%"
            },
            { data: "user.email", width: "20%" },
            { data: "user.phone", width: "15%" },
            { data: "role", width: "10%" },
            {
                data: "user.lockOutEnd",
                render: function (data, type, row) {
                    const isLocked = data && new Date(data) > new Date();
                    const btnText = isLocked ? "Unlock" : "Lock";
                    const icon = isLocked ? "fa-lock-open" : "fa-lock";
                    const btnClass = isLocked ? "btn-outline-custom-blue" : "btn-custom-blue";

                    return `
                        <button class="btn btn-sm ${btnClass}" onclick="toggleLock(${row.employeeID})">
                            <i class="fas ${icon}"></i> ${btnText}
                        </button>`;
                },
                orderable: false,
                width: "15%"
            },
            {
                data: null,
                render: function (data, type, row) {
                    const isArchived = row.user.isArchived;
                    const archiveBtn = isArchived
                        ? `<button class="btn btn-sm btn-outline-custom-blue" onclick="unarchiveEmployee(${row.employeeID})">
                               <i class="fas fa-box-open"></i> Unarchive
                           </button>`
                        : `<button class="btn btn-sm btn-custom-grey" onclick="archiveEmployee(${row.employeeID})">
                               <i class="fas fa-archive"></i> Archive
                           </button>`;

                    return `
                        <div class="text-center d-flex flex-column align-items-center gap-1">
                            <a href="/Admin/Employees/Upsert?id=${row.employeeID}" class="btn btn-sm btn-custom-blue">
                                <i class="fas fa-edit"></i> Update
                            </a>
                            ${archiveBtn}
                        </div>`;
                },
                orderable: false,
                width: "20%"
            }
        ],
        language: {
            emptyTable: "No employees found."
        },
        width: "100%"
    });
}

function toggleLock(id) {
    $.ajax({
        url: `/api/employees/lockunlock/${id}`,
        type: "POST",
        success: function (data) {
            if (data.success) {
                toastr.success(data.message);
                employeeTable.ajax.reload(null, false);
            } else {
                toastr.error(data.message);
            }
        }
    });
}

function archiveEmployee(id) {
    $.ajax({
        url: `/api/employees/archive/${id}`,
        type: "POST",
        success: function (data) {
            if (data.success) {
                toastr.success(data.message);
                employeeTable.ajax.reload(null, false);
            } else {
                toastr.error(data.message);
            }
        }
    });
}

function unarchiveEmployee(id) {
    $.ajax({
        url: `/api/employees/unarchive/${id}`,
        type: "POST",
        success: function (data) {
            if (data.success) {
                toastr.success(data.message);
                employeeTable.ajax.reload(null, false);
            } else {
                toastr.error(data.message);
            }
        }
    });
}
