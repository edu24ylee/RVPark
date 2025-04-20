let dataTable;

$(document).ready(function () {
    loadLotTypes(window.selectedParkId);
});

function loadLotTypes(parkId) {
    if (!parkId) return;

    $.ajax({
        url: `/api/lottypes?parkId=${parkId}`,
        type: "GET",
        dataType: "json",
        success: function (response) {
            const data = response?.data ?? [];

            const nameSet = new Set();
            const rateSet = new Set();
            const startSet = new Set();
            const endSet = new Set();

            data.forEach(item => {
                if (item.name) nameSet.add(item.name);
                if (item.rate) rateSet.add(item.rate);
                if (item.startDate) startSet.add(item.startDate.split("T")[0]);
                if (item.endDate) endSet.add(item.endDate.split("T")[0]);
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
            fillFilter('#rateFilter', rateSet);
            fillFilter('#startDateFilter', startSet);
            fillFilter('#endDateFilter', endSet);

            dataTable = $('#DT_load').DataTable({
                data: data,
                columns: [
                    { data: "name", width: "25%" },
                    { data: "rate", width: "15%" },
                    { data: "startDate", render: d => d.split("T")[0], width: "15%" },
                    { data: "endDate", render: d => d.split("T")[0], width: "15%" },
                    {
                        data: "id",
                        render: function (data, type, row) {
                            return `
                                <div class="text-center d-flex flex-column gap-2 align-items-center">
                                    <a href="/Admin/LotTypes/Upsert?id=${data}" class="btn btn-sm btn-custom-blue" style="width: 100px;">
                                        <i class="far fa-edit"></i> Edit
                                    </a>
                                </div>`;
                        },
                        width: "15%"
                    }
                ],
                destroy: true,
                language: { emptyTable: "No lot types found." }
            });
        },
        error: function () {
            toastr.error("Failed to load lot types.");
        }
    });
}
