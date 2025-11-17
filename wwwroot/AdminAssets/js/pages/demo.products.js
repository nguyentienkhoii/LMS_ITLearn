$(document).ready(function () {
    $('#products-datatable').DataTable({
        language: {
            paginate: {
                previous: "<i class='mdi mdi-chevron-left'>",
                next: "<i class='mdi mdi-chevron-right'>"
            },
            info: "Hiển thị chương _START_ to _END_ of _TOTAL_",
            lengthMenu: 'Hiển thị <select class=\'form-select form-select-sm ms-1 me-1\'><option value="5">5</option><option value="10">10</option><option value="20">20</option><option value="-1">All</option></select> chương'
        },
        pageLength: 5,
        columns: [
            { orderable: false, targets: 0 },
            { orderable: true },
            { orderable: true },
            { orderable: true },
            { orderable: false }
        ],
        select: {
            style: "multi"
        },
        order: [
            [1, "asc"]
        ],
        drawCallback: function () {
            $(".dataTables_paginate > .pagination").addClass("pagination-rounded"), $("#products-datatable_length label").addClass("form-label")
        }
    });
});
