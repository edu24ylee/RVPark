var dataTable;

$(document).ready(function () {
    loadList(); 
});

function loadList() {
    dataTable = $('#DT_load').DataTable({
        "ajax": {
            "url": "/api/parks",   
            "type": "GET",         
            "datatype": "json"    
        },
        "columns": [
            { data: "name", width: "25%" },      
            { data: "address", width: "25%" },   
            { data: "city", width: "15%" },      
            { data: "state", width: "10%" },     
            { data: "zipcode", width: "10%" },   
            {
                data: "id",                  
                width: "15%",
                render: function (data) {
                    return `<div class="text-center">
                                <a href="/Admin/Parks/Upsert?id=${data}" 
                                   class="btn btn-custom-blue" 
                                   style="cursor:pointer; width:100px;">
                                   <i class="far fa-edit"></i> Edit
                                </a>
                                <a onClick="Delete('/api/parks/${data}')" 
                                   class="btn btn-custom-grey" 
                                   style="cursor:pointer; width:100px;">
                                   <i class="far fa-trash-alt"></i> Delete
                                </a>
                            </div>`;
                }
            }
        ],
        "language": {
            "emptyTable": "No parks found."
        },
        "width": "100%" 
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
