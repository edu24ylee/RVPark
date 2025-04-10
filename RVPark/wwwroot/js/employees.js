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
            {
                data: "role",
                width: "10%"
            },
            {
                data: "user.lockOutEnd",
                render: function (data, type, row) {
                    const isLocked = data && new Date(data) > new Date();
                    const btnText = isLocked ? "Unlock" : "Lock";
                    const icon = isLocked ? "fa-lock-open" : "fa-lock";
                    return `
                        <button class="btn btn-outline-warning" onclick="toggleLock(${row.employeeID})">
                            <i class="fas ${icon}"></i> ${btnText}
                        </button>`;
                },
                orderable: false,
                width: "15%"
            },
            {
                data: "employeeID",
                render: function (data) {
                    return `
                        <div class="text-center">
                            <a href="/Admin/Employees/Upsert?id=${data}" class="btn btn-sm btn-custom-blue">
                                <i class="fas fa-edit"></i> Update
                            </a>
                            <button class="btn btn-sm btn-custom-grey" onclick="deleteEmployee(${data})">
                                <i class="fas fa-trash"></i> Delete
                            </button>
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
                employeeTable.ajax.reload();
            } else {
                toastr.error(data.message);
            }
        }
    });
}
