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
            {
                data: "user.isActive",
                render: data => data ? "Active" : "Inactive",
                width: "10%"
            },
            { data: "role", width: "10%" },
            {
                data: "user.lockOutEnd",
                render: function (data, type, row) {
                    const isLocked = data && new Date(data) > new Date();
                    const btnText = isLocked ? "Unlock" : "Lock";
                    const icon = isLocked ? "fa-lock-open" : "fa-lock";
                    return `
                        <button class="btn btn-outline-warning btn-sm" onclick="toggleLock(${row.employeeID})">
                            <i class="fas ${icon}"></i> ${btnText}
                        </button>`;
                },
                orderable: false,
                width: "10%"
            },
            {
                data: "user.isActive",
                render: function (isActive, type, row) {
                    const action = isActive ? "archive" : "unarchive";
                    const label = isActive ? "Archive" : "Unarchive";
                    const btnClass = isActive ? "btn-custom-grey" : "btn-success";
                    return `
                        <button class="btn ${btnClass} btn-sm" onclick="${action}Employee(${row.employeeID})">
                            <i class="fas fa-box-archive"></i> ${label}
                        </button>`;
                },
                orderable: false,
                width: "10%"
            },
            {
                data: "employeeID",
                render: function (data) {
                    return `
                        <div class="text-center">
                            <a href="/Admin/Employees/Upsert?id=${data}" class="btn btn-sm btn-custom-blue">
                                <i class="fas fa-edit"></i> Update
                            </a>
                        </div>`;
                },
                orderable: false,
                width: "15%"
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
                employeeTable.ajax.reload();
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
                toastr.success("Employee archived");
                employeeTable.ajax.reload();
            } else {
                toastr.error("Archive failed");
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
                toastr.success("Employee unarchived");
                employeeTable.ajax.reload();
            } else {
                toastr.error("Unarchive failed");
            }
        }
    });
}
