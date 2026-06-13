
$(document).ready(function () {
    getHedgeInfoData()
});
function getHedgeInfoData() {
    var response = ajaxGet('HedgeInfo/getData');
    if (response.length > 0) {
        var html = '';
        for (var i = 0; i < response.length; i++) {
             html += '<tr>' +
                '   <td class="wone">' +
                '      <div class="content">' +
                '         <p>'+response[i].id+'</p>' +
                '</div > ' +
                '   </td>' +
                '   <td class="wtwo">' +
                '      <div class="content">' +
                 '         <p>' + response[i].user +'</p>' +
                '      </div>' +
                '   </td>' +
                '   <td class="wtwo">' +
                '      <div class="content">' +
                 '         <p>' + response[i].request +'</p>' +
                '      </div>' +
                '   </td>' +
                '   <td class="wtwo">' +
                '      <div class="content">' +
                 '         <p>' + response[i].response +'</p>' +
                '      </div>' +
                '   </td>' +
                '   <td class="wtwo">' +
                '      <div class="content">' +
                 '         <p>' + response[i].createDate +'</p>' +
                '      </div>' +
                '   </td>' +
                '</tr>';
        }
        $('.hedgeInfoList').html(html);
        $('#example').DataTable();
    }
}