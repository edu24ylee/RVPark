$(document).ready(function () {
    const parkId = new URLSearchParams(window.location.search).get("SelectedParkId");

    if (parkId) {
        $('#DT_load').DataTable({
            ajax: {
                url: `/api/LotTypes/GetByPark/${parkId}`,
                dataSrc: 'data'
            },
            columns: [
                { data: 'name' },
                { data: 'rate' },
                {
                    data: 'id',
                    render: function (data) {
                        return `
                            <a href="/Admin/LotTypes/Upsert?id=${data}" class="btn btn-sm btn-custom-blue">
                                <i class="fas fa-edit"></i> Edit
                            </a>
                            <button class="btn btn-sm btn-custom-grey" onclick="deleteLotType(${data})">
                                <i class="fas fa-trash"></i> Delete
                            </button>`;
                    },
                    orderable: false
                }
            ]
        });
    }
});

function deleteLotType(id) {
    swal({
        title: "Are you sure?",
        text: "This will permanently delete the Lot Type.",
        icon: "warning",
        buttons: true,
        dangerMode: true
    }).then((confirmed) => {
        if (confirmed) {
            $.ajax({
                url: `/api/LotTypes/${id}`,
                type: 'DELETE',
                success: function (res) {
                    if (res.success) {
                        toastr.success(res.message);
                        $('#DT_load').DataTable().ajax.reload();
                    } else {
                        toastr.error(res.message);
                    }
                }
            });
        }
    });
}
