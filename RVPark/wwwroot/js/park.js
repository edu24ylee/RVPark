function loadList() {
    $('#DT_load').DataTable({
        ajax: {
            url: "/api/parks",
            type: "GET",
            datatype: "json"
        },
        columns: [
            { data: "name" },
            { data: "address" },
            { data: "city" },
            { data: "state" },
            { data: "zipcode" },
            {
                data: "id",
                render: function (data) {
                    return `<a href="/Admin/Parks/Upsert?id=${data}" class="btn btn-sm btn-outline-primary">Edit</a>`;
                }
            }
        ],
        language: {
            emptyTable: "No parks available"
        },
        width: "100%"
    });
}
