function getReferance(type) {
    var data = ajaxGet('getReferance?id=' + type);
    if (data.length > 0) {
        var html = '';
        for (var i = 0; i < data.length; i++) {
            var check = '';
            if (data[i].isMaster) {
                check='checked'
            }
                html += '<tr>' +
                '        <td class="w-20"><input class="isView" type="checkbox" ' + check +'></td>' +
                '        <td class="w-30 source sm-font sym-mod">' + data [i].source+'</td>' +
                '        <td class="w-30"><input type="text" value="' + data[i].name +'" class="input-height2 name"></td>' +
                '        <td class="w-20 text-center"><input class="updateReferance btn common-bg sm-font" type="button" value="UPDATE" id="' + data[i].id +'"></td>' +
                '</tr>';
        }
        $('.printReferance').html(html);
    }
}
$("body").on("click", ".updateReferance", async function () {
    let data = new Object();
    data.id = parseInt($(this).attr("id"));
    data.name = $(this).closest('tr').find(".name").val();
    data.isView = $(this).closest('tr').find(".isView").is(':checked');
    var response = ajaxPost('updateReferance', JSON.stringify(data));
    console.log(response);
});