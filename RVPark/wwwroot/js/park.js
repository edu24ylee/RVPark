let dataTable;

$(document).ready(function () {
    loadList();
});

function loadList() {
    $.ajax({
        url: "/api/parks",
        type: "GET",
        dataType: "json",
        success: function (response) {
            const data = response?.data ?? [];

            const nameSet = new Set();
            const addressSet = new Set();
            const citySet = new Set();
            const stateSet = new Set();
            const zipSet = new Set();

            data.forEach(item => {
                if (item.name) nameSet.add(item.name);
                if (item.address) addressSet.add(item.address);
                if (item.city) citySet.add(item.city);
                if (item.state) stateSet.add(item.state);
                if (item.zipcode) zipSet.add(item.zipcode);
            });

            const fillFilter = (selector, values) => {
                const $select = $(selector);
                $select.find('option:not(:first)').remove();
                [...values].sort().forEach(val => {
                    $select.append(`<option value="${val}">${val}</option>`);
                });
                $select.off('change').on('change', function () {
                    const val = $.fn.dataTable.util.escapeRegex($(this).val());
                    const colIndex = $(this).closest('th').index();
                    dataTable.column(colIndex).search(val ? `^${val}$` : '', true, false).draw();
                });
            };

            fillFilter('#nameFilter', nameSet);
            fillFilter('#addressFilter', addressSet);
            fillFilter('#cityFilter', citySet);
            fillFilter('#stateFilter', stateSet);
            fillFilter('#zipcodeFilter', zipSet);

            dataTable = $('#DT_load').DataTable({
                data: data,
                columns: [
                    { data: "name", width: "25%" },
                    { data: "address", width: "25%" },
                    { data: "city", width: "15%" },
                    { data: "state", width: "10%" },
                    { data: "zipcode", width: "10%" },
                    {
                        data: "id",
                        render: function (data, type, row) {
                            const isArchived = row.isArchived;
                            const archiveBtn = isArchived
                                ? `<button class="btn btn-sm btn-custom-blue-header" onclick="unarchivePark(${data})" style="width: 100px;">
                                       <i class="fas fa-box-open"></i> Unarchive
                                   </button>`
                                : `<button class="btn btn-sm btn-custom-grey" onclick="archivePark(${data})" style="width: 100px;">
                                       <i class="fas fa-archive"></i> Archive
                                   </button>`;
                            return `
                                <div class="text-center d-flex flex-column gap-2 align-items-center">
                                    <a href="/Admin/Parks/Upsert?id=${data}" class="btn btn-sm btn-custom-blue" style="width: 100px;">
                                        <i class="far fa-edit"></i> Edit
                                    </a>
                                    ${archiveBtn}
                                </div>`;
                        },
                        width: "15%"
                    }
                ],
                dom: '<"row mb-2"<"col-sm-12 col-md-6"l><"col-sm-12 col-md-6 text-end"f>>rt<"row mt-2"<"col-sm-12 col-md-5"i><"col-sm-12 col-md-7"p>>',
                language: {
                    emptyTable: "No parks found.",
                    search: "Search:"
                },
                destroy: true 
            });
        },
        error: function () {
            toastr.error("Failed to load park data.");
        }
    });
}

function archivePark(id) {
    $.post(`/api/parks/archive/${id}`, function (data) {
        if (data.success) {
            toastr.success(data.message);
            dataTable.ajax.reload();
        } else {
            toastr.error(data.message);
        }
    });
}

function unarchivePark(id) {
    $.post(`/api/parks/unarchive/${id}`, function (data) {
        if (data.success) {
            toastr.success(data.message);
            dataTable.ajax.reload();
        } else {
            toastr.error(data.message);
        }
    });
}
