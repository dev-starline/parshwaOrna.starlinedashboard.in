$("body").on("click", ".ajax-model-group", async function () {
    var divElement = $("#modelPopup");
    var url = $(this).data('url');
    var decodeUrl = decodeURIComponent(url);
    var id = parseInt($(this).attr("id"));
    $.get(decodeUrl)
        .done(function (data) {
            divElement.html(data);
            var editModal = divElement.find('#editModal');
            if (editModal.length > 0) {
                editModal.modal('show');
                getGroupSymbol(id);
            } else {
                console.log('Modal element not found');
            }
        })
        .fail(function (xhr, status, error) {
            console.error(status + " : " + error);
        });
})
//function getGroupSymbol(type) {
//    var data = JSON.parse(ajaxGet('/admin/Group/getGroupSymbol?id=' + type));
//    console.log(data);
//    if (data.length > 0) {
//        var html = '';
//        for (var i = 0; i < data.length; i++) {
//            if (data[i].id <= 3) {
//                continue;
//            }
//            var check = '';
//            if (data[i].isView) {
//                check = 'checked'
//            }

//            html += '<tr id=' + data[i].id + '>' +
//                '    <td class="numeric wtwo">' +
//                '        <span>' + data[i].name + '</span>' +
//                '    </td>' +
//                '    <td class="numeric wtwo">' +
//                '        <input class="isView" type="checkbox" ' + check + ' />' +
//                '    </td>' +
//                '    <td class="numeric wtwo">' +
//                '        <input type="text" class="input-height sm-font buyPremium" value=' + data[i].buyPremium + ' />' +
//                '    </td>' +
//                '    <td class="numeric wtwo">' +
//                '        <input type="text" class="input-height sm-font sellPremium" value=' + data[i].sellPremium + ' />' +
//                '    </td>' +
//                '    <td class="numeric wtwo">' +
//                '        <input type="text" class="input-height sm-font oneClick" value=' + data[i].oneClick + ' />' +
//                '    </td>' +
//                '    <td class="numeric wtwo">' +
//                '        <input type="text" class="input-height sm-font step" value=' + data[i].step + ' />' +
//                '    </td>' +
//                '    <td class="numeric wtwo">' +
//                '        <input type="text" class="input-height sm-font inTotal" value=' + data[i].inTotal + ' />' +
//                '    </td>' +
//                '</tr>';


//        }
//        $('.printSymbol').html(html);
//    }
//}


function getGroupSymbol(type) {
    var data = ajaxGet('/admin/Group/getGroupSymbol?id=' + type);
    // data is already an object (array of symbols)

    console.log(data); // check the structure

    if (data && data.length > 0) {
        var html = '';
        for (var i = 0; i < data.length; i++) {
            if (data[i].id < 3) continue; // skip only system groups

            var check = data[i].isView ? 'checked' : '';

            html += '<tr id="' + data[i].id + '">' +
                '<td class="numeric wtwo"><span>' + data[i].name + '</span></td>' +
                '<td class="numeric wtwo"><input class="isView" type="checkbox" ' + check + ' /></td>' +
                '<td class="numeric wtwo"><input type="text" class="input-height sm-font buyPremium" value="' + data[i].buyPremium + '" /></td>' +
                '<td class="numeric wtwo"><input type="text" class="input-height sm-font sellPremium" value="' + data[i].sellPremium + '" /></td>' +
                '<td class="numeric wtwo"><input type="text" class="input-height sm-font oneClick" value="' + data[i].oneClick + '" /></td>' +
                '<td class="numeric wtwo"><input type="text" class="input-height sm-font step" value="' + data[i].step + '" /></td>' +
                '<td class="numeric wtwo"><input type="text" class="input-height sm-font inTotal" value="' + data[i].inTotal + '" /></td>' +
                '</tr>';
        }
        $('.printSymbol').html(html);
    }
}

function edit(id) {
    var arraySymbol = [];
    let objGroup = new Object();
    $(".printSymbol tr").each(async function () {
        var data = new Object();
        data.id = $(this).closest('tr').attr('id');
        data.buyPremium = $(this).closest('tr').find(".buyPremium").val();
        data.sellPremium = $(this).closest('tr').find(".sellPremium").val();
        data.oneClick = $(this).closest('tr').find(".oneClick").val();
        data.inTotal = $(this).closest('tr').find(".inTotal").val();
        data.step = $(this).closest('tr').find(".step").val();
        data.isView = $(this).closest('tr').find(".isView").is(':checked');
        arraySymbol.push(data);
    })

    objGroup.id = parseInt(id);
    objGroup.name = $('#name').val();
    objGroup.buyPremiumGold = $('#buyPremiumGold').val();
    objGroup.sellPremiumGold = $('#sellPremiumGold').val();
    objGroup.buyPremiumSilver = $('#buyPremiumSilver').val();
    objGroup.sellPremiumSilver = $('#sellPremiumSilver').val();
    const body = {
        group: objGroup,
        symbol: arraySymbol
    };
    var response = ajaxPost('/admin/Group/saveAll', JSON.stringify(body));
    window.location.href = window.location.origin + '/admin/Group';
}