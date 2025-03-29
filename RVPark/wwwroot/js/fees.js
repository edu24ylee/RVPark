var dataTable;

$(document).ready(function () {
    loadList();
});

function loadList() {
    dataTable = $('#DT_load').DataTable({
        "ajax": {
            "url": "/api/fees",
            "type": "GET",
            "datatype": "json"
        },
        "columns": [
            { data: "id", width: "10%" },
            { data: "feeType.feeTypeName", width: "30%" },
            {
                data: "triggeringPolicy.policyName",
                width: "30%",
                "render": function (data) {
                    return data ? data : "N/A";
                }
            },
            {
                data: "feeTotal",
                width: "15%",
                "render": $.fn.dataTable.render.number(',', '.', 2, '$')
            },
            {
                data: "id",
                width: "15%",
                "render": function (data) {
                    return `<div class="text-center">
                                <a href="/Admin/Fees/Upsert?id=${data}" 
                                   class="btn btn-success text-white" 
                                   style="cursor:pointer; width:100px;">
                                    <i class="far fa-edit"></i> Edit
                                </a>
                                <a onClick="Delete('/api/fees/' + ${data})" 
                                   class="btn btn-danger text-white" 
                                   style="cursor:pointer; width:100px;">
                                    <i class="far fa-trash-alt"></i> Delete
                                </a>
                            </div>`;
                }
            }
        ],
        "language": {
            "emptyTable": "No data found."
        },
        "width": "100%"
    });
}

function Delete(url) {
    swal({
        title: "Are you sure you want to delete?",
        text: "You will not be able to restore the data!",
        icon: "warning",
        buttons: true,
        dangerMode: true
    }).then((willDelete) => {
        if (willDelete) {
            $.ajax({
                url: url,
                type: "DELETE",
                success: function (data) {
                    if (data.success) {
                        toastr.success(data.message);
                        dataTable.ajax.reload();
                    } else {
                        toastr.error(data.message);
                    }
                }
            });
        }
    });
}
