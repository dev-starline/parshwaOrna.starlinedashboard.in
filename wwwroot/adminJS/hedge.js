getHedgeDetails();
function getHedgeDetails() {
    var data = ajaxGet('Hedge/getHedgeDetails');
    if (data.hedge.length > 0) {
        var html = "";
        for (var i = 0; i < data.hedge.length; i++) {
             html += '<tr>' +
                '   <td class="wfive ds">' +
                 '      <span>' + data.hedge[i].name+'</span>' +
                '   </td>' +
                '   <td class="wfive">' +
                '      <div class="content ds">' +
                 '         <button id=' + data.hedge[i].id +' class="deleteHedgeSymbol" onclick="return confirmDelete()">' +
                '         <i class="fa fa-trash-o text-danger"' +
                '            aria-hidden="true"></i>' +
                '         </button>' +
                '      </div>' +
                '   </td>' +
                '</tr>';
        }
        $(".hedgeSymbolList").html(html);
    }
    if (data.contact.length > 0) {
        $('input[name="isHedge"][value=' + data.contact[0].isHedge + ']').prop("checked", true);
    }
}
$(".isSwitch").on("change", function () {
    let data = new Object();
    data.isHedge = $('input[name="isHedge"]:checked').val();
    var response = ajaxPost('Hedge/isHedgeUpdate', JSON.stringify(data));
    if (response == 200) {
    }
});
$("body").on("click", ".saveSymbol", async function () {
    let data = new Object();
    data.name = $(".name").val();
    var response = ajaxPost('Hedge/saveHedgeSymbol', JSON.stringify(data));
    getHedgeDetails();
});
$("body").on("click", ".updateHedge", async function () {
    let data = new Object();
    data.id = $(this)[0].id;
    data.hedgeSymbolID = $(this).closest('tr').find(".hedgeSymbol").val();
    data.division = $(this).closest('tr').find(".division").val();
    data.status = $(this).closest('tr').find(".status").is(':checked');
    var response = ajaxPost('Hedge/updateHedge', JSON.stringify(data));
    console.log(response);
});
$("body").on("click", ".deleteHedgeSymbol", async function () {
    let data = new Object();
    data.id = $(this)[0].id;
    var response = ajaxPost('Hedge/deleteHedgeSymbol', JSON.stringify(data));
    getHedgeDetails();
});