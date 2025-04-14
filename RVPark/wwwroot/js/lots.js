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
            { data: "lotType.name", title: "Lot Type" },
            { data: "park.name", title: "Park" },
            { data: "location", title: "Location" },
            { data: "width", title: "Width" },
            { data: "length", title: "Length" },
            {
                data: "isAvailable",
                title: "Available",
                render: data => data ? "Yes" : "No"
            },
            { data: "description", title: "Description" },
            {
                data: null,
                title: "Image",
                width: "160px", 
                render: function (data, type, row) {
                    const imageListRaw = row.imageList || row.image || "";
                    const images = imageListRaw.split(',').map(i => i.trim()).filter(i => i);
                    const img = row.featuredImage || images[0];

                    if (!img) {
                        return `<span class="text-muted">No image</span>`;
                    }

                    const imageCount = images.length;

                    return `
            <div class="d-flex align-items-center gap-2" style="white-space: nowrap;">
                <img src="${img}" class="img-thumbnail" style="max-height:60px; width:auto;" />
                <span class="badge bg-custom-blue">${imageCount}</span>
            </div>
        `;
                }
            },
            {
                data: null,
                title: "Actions",
                render: function (data, type, row) {
                    const isSuperAdmin = window.isSuperAdmin === "true";
                    const isReserved = row.isReserved;

                    const editBtn = `<a href="/Admin/Lots/Upsert?id=${row.id}" class="btn btn-sm btn-custom-blue text-white">
                        <i class="fas fa-edit"></i> Edit</a>`;

                    if (row.isArchived) {
                        if (!isSuperAdmin) return editBtn;
                        return `<div class="d-flex justify-content-center gap-2">
                            ${editBtn}
                            <button class="btn btn-sm btn-outline-custom-blue" onclick="toggleArchive(${row.id}, true)">
                                <i class="fas fa-box-open"></i> Unarchive</button>
                        </div>`;
                    }

                    if (row.isFeatured) {
                        return `<div class="d-flex justify-content-center gap-2">
                            ${editBtn}
                            <button class="btn btn-sm btn-secondary" disabled title="Cannot archive featured lot.">
                                <i class="fas fa-star"></i> Featured</button>
                        </div>`;
                    }

                    if (isReserved) {
                        return `<div class="d-flex justify-content-center gap-2">
                            ${editBtn}
                            <button class="btn btn-sm btn-secondary" disabled title="Lot has active reservations.">
                                <i class="fas fa-lock"></i> Reserved</button>
                        </div>`;
                    }

                    return `<div class="d-flex justify-content-center gap-2">
                        ${editBtn}
                        <button class="btn btn-sm btn-custom-grey text-white" onclick="toggleArchive(${row.id}, false)">
                            <i class="fas fa-archive"></i> Archive</button>
                    </div>`;
                }
            },
            {
                data: "isFeatured",
                title: "Featured",
                render: function (data, type, row) {
                    return `<div class="text-center">
                        <input type="radio" name="featuredLot" value="${row.id}" ${data ? "checked" : ""}
                        onchange="setFeatured(${row.id})" ${row.isArchived ? "disabled" : ""} />
                    </div>`;
                }
            }
        ],
        scrollX: true,
        autoWidth: false,
        responsive: true,
        language: {
            emptyTable: "No lots found."
        }
    });
}

function toggleArchive(id, isArchived) {
    const url = isArchived ? `/api/lot/unarchive/${id}` : `/api/lot/archive/${id}`;

    $.post(url, function (data) {
        if (data.success) {
            showCustomToast(data.message);
        } else {
            toastr.error(data.message || "Error toggling archive status.");
        }
        dataTable.ajax.reload(null, false);
    });
}

function setFeatured(id) {
    $.post(`/api/lot/feature/${id}`, function (data) {
        if (data.success) {
            showCustomToast(data.message);
        } else {
            toastr.error(data.message || "Something went wrong.");
        }
        dataTable.ajax.reload(null, false);
    });
}

function showCustomToast(message) {
    toastr.success(message, "", {
        toastClass: 'toast toast-custom-blue',
        closeButton: true,
        progressBar: true,
        positionClass: "toast-top-right",
        timeOut: "3000"
    });
}
