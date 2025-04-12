let dataTable;

$(document).ready(function () {
    loadList(window.selectedParkId);
});

function loadList(parkId) {
    dataTable = $('#DT_load').DataTable({
        ajax: {
            url: `/api/lot/bypark/${parkId}`,
            type: "GET",
            dataSrc: json => json?.data ?? []
        },
        columns: [
            { data: "lotType.name", title: "Lot Type", width: "12%" },
            { data: "park.name", title: "Park", width: "12%" },
            { data: "location", title: "Location", width: "12%" },
            { data: "width", title: "Width", width: "8%" },
            { data: "length", title: "Length", width: "8%" },
            {
                data: "isAvailable",
                title: "Available",
                width: "10%",
                render: data => data ? "Yes" : "No"
            },
            { data: "description", title: "Description", defaultContent: "", width: "15%" },
            {
                data: null,
                title: "Image",
                width: "12%",
                render: function (data, type, row) {
                    const featured = row.featuredImage;
                    const allImages = (row.image || "").split(',').filter(x => x.trim() !== "");
                    const count = allImages.length;

                    const imageTag = (featured && featured.trim() !== "")
                        ? `<img src="${featured.trim()}" class="img-thumbnail" style="max-height:60px;" alt="Featured Image" onerror="this.outerHTML='<span class=text-muted>No image</span>'" />`
                        : `<span class="text-muted">No image</span>`;

                    return `
                        <div class="d-flex flex-column align-items-center text-center" style="white-space: normal;">
                            ${imageTag}
                            <span class="fw-semibold text-custom-blue mt-1">${count} image${count !== 1 ? 's' : ''}</span>
                        </div>`;
                }
            },
            {
                data: null,
                title: "Actions",
                orderable: false,
                width: "13%",
                render: function (data, type, row) {
                    const isArchived = row.isArchived === true;
                    const isSuperAdmin = window.isSuperAdmin === "true";

                    const archiveBtn = isArchived
                        ? (isSuperAdmin
                            ? `<button class="btn btn-sm btn-outline-custom-blue" onclick="unarchiveLot(${row.id})">
                                   <i class="fas fa-box-open"></i> Unarchive
                               </button>` : "")
                        : `<button class="btn btn-sm btn-custom-grey text-white" onclick="archiveLot(${row.id})">
                               <i class="fas fa-archive"></i> Archive
                           </button>`;

                    return `
                        <div class="d-flex justify-content-center gap-2">
                            <a href="/Admin/Lots/Upsert?id=${row.id}" class="btn btn-sm btn-custom-blue text-white">
                                <i class="fas fa-edit"></i> Edit
                            </a>
                            ${archiveBtn}
                        </div>`;
                }
            }
            {
                data: "isFeatured",
                width: "10%",
                render: function (data, type, row) {
                    const icon = data ? "fas fa-star text-warning" : "far fa-star text-muted";
                    return `
            <button class="btn btn-sm btn-outline-custom-blue" onclick="toggleFeatured(${row.id})">
                <i class="${icon}"></i> ${data ? "Featured" : "Make Featured"}
            </button>`;
                }
            }

        ],
        initComplete: function () {
            this.api().columns([0, 1, 2, 3]).every(function () {
                const column = this;
                const columnIndex = column.index();
                const originalTitle = $('#DT_load thead th').eq(columnIndex).text();

                const $wrapper = $('<div class="d-flex flex-column align-items-start"></div>');
                const $label = $(`<label class="fw-semibold small mb-1">${originalTitle}</label>`);
                const $select = $(`<select class="form-select form-select-sm"><option value="">All ${originalTitle}</option></select>`);

                column.data().unique().sort().each(function (d) {
                    if (d || d === 0) {
                        $select.append(`<option value="${d}">${d}</option>`);
                    }
                });

                $select.on('change', function () {
                    const val = $.fn.dataTable.util.escapeRegex($(this).val());
                    column.search(val ? `^${val}$` : '', true, false).draw();
                });

                $wrapper.append($label).append($select);
                $(column.header()).empty().append($wrapper);
            });
        }, 
        dom: '<"top"f>rt<"bottom"lip><"clear">',
        language: {
            emptyTable: "No lots found.",
            search: "Search lots:"
        },
        scrollX: true,
        autoWidth: false,
        responsive: true,
        columnDefs: [
            { targets: "_all", className: "text-nowrap" }
        ]
    });
}

function archiveLot(id) {
    $.post(`/api/lot/archive/${id}`, function (data) {
        if (data.success) {
            toastr.success(data.message);
            dataTable.ajax.reload(null, false);
        } else {
            toastr.error(data.message);
        }
    });
}

function unarchiveLot(id) {
    $.post(`/api/lot/unarchive/${id}`, function (data) {
        if (data.success) {
            toastr.success(data.message);
            dataTable.ajax.reload(null, false);
        } else {
            toastr.error(data.message);
        }
    });
}
function toggleFeatured(id) {
    $.post(`/api/lots/feature/${id}`, function (data) {
        if (data.success) {
            toastr.success(data.message);
            dataTable.ajax.reload();
        } else {
            toastr.error("Something went wrong.");
        }
    });
}
