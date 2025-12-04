$(function () {
    if (!$.fn.DataTable) {
        console.error('DataTables chưa sẵn sàng');
        return;
    }

    const $table = $('#products-datatable');
    if (!$table.length) {
        // Không có bảng chương trên trang này thì thôi
        return;
    }

    // 1) KHỞI TẠO DATATABLE
    const dt = $table.DataTable({
        dom:
            '<"row align-items-center mb-2"'
                + '<"col-md-6"l>'
                + '<"col-md-6 text-md-end"f>'
            + '>'
            + 'rt'
            + '<"row align-items-center mt-2"'
                + '<"col-md-6"i>'
                + '<"col-md-6"p>'
            + '>',
        responsive: true,
        autoWidth: false,
        pageLength: 5,
        language: {
            paginate: {
                previous: "<i class='mdi mdi-chevron-left'></i>",
                next: "<i class='mdi mdi-chevron-right'></i>"
            },
            info: "Hiển thị _START_ đến _END_ trong _TOTAL_ chương",
            lengthMenu:
                "Hiển thị <select class='form-select form-select-sm ms-1 me-1'>"
                + "<option value='5'>5</option>"
                + "<option value='10'>10</option>"
                + "<option value='20'>20</option>"
                + "<option value='-1'>All</option>"
                + "</select> chương"
        },
        columnDefs: [
            { targets: -1, orderable: false, className: 'text-center' },
            { targets: "_all", className: "align-middle" }
        ],
        order: [[1, "asc"]],
        drawCallback: function () {
            $(".dataTables_paginate > .pagination").addClass("pagination-rounded");
            $("#products-datatable_length label").addClass("form-label");
        }
    });

    // 2) FILTER THEO LỚP: hook chung cho DataTables
    function chuongFilter(settings, data, dataIndex) {
        if (settings.nTable.id !== 'products-datatable') return true;

        const row = settings.aoData[dataIndex].nTr;
        const pickedLop = $('#lopFilterChuong').val();

        if (pickedLop && String(row.dataset.lop) !== String(pickedLop)) {
            return false;
        }

        return true;
    }

    if (!window.__chuongFilterHook) {
        $.fn.dataTable.ext.search.push(chuongFilter);
        window.__chuongFilterHook = true;
    }

    // 3) CLONE UI "Bộ lọc" vào bên cạnh "Hiển thị X chương"
    const $template = $('#chuongFiltersTemplate .filter-bar');
    if ($template.length) {
        const $group = $template.clone();
        $('#products-datatable_wrapper .dataTables_length').append($group);

        const $lop = $group.find('#lopFilterChuongTemplate')
                           .attr('id', 'lopFilterChuong');

        // 4) EVENT: đổi lớp -> filter lại
        $lop.on('change', function () {
            dt.draw();
        });

        // init lần đầu
        $('#lopFilterChuong').val('');
        dt.draw();
    }
});
