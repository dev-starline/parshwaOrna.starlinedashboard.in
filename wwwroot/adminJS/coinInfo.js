$(document).ready(function () {

    $.ajax({
        url: '/CoinInfo/getData',
        type: 'GET',
        success: function (result) {

            var html = '';

            $.each(result, function (i, item) {

                var date = new Date(item.createDate);

                var formattedDate =
                    date.getDate().toString().padStart(2, '0') + '-' +
                    (date.getMonth() + 1).toString().padStart(2, '0') + '-' +
                    date.getFullYear() + ' ' +
                    date.getHours().toString().padStart(2, '0') + ':' +
                    date.getMinutes().toString().padStart(2, '0') + ':' +
                    date.getSeconds().toString().padStart(2, '0');

                html += '<tr>';
                html += '<td>' + item.id + '</td>';
                html += '<td>' + item.user + '</td>';
                html += '<td>' + item.request + '</td>';
                html += '<td>' + item.response + '</td>';
                html += '<td>' + formattedDate + '</td>';
                html += '</tr>';
            });

            $('.coinInfoList').html(html);

            $('#example').DataTable({
                order: [[0, 'desc']]
            });
        }
    });

});