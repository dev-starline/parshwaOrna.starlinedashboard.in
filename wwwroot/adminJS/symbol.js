getBankCalculation();
function getBankCalculation() {
    var data = ajaxGet('admin/Symbol/getBankCalculation');
    if (data.bank.length > 0) {
        var response = data.bank;
        $('#bankId').val(response[0].id);
        $('#premiumGold').val(response[0].premiumGold);
        $('#premiumSilver').val(response[0].premiumSilver);
        $('#spotTypeGold').val(response[0].spotTypeGold);
        $('#spotTypeSilver').val(response[0].spotTypeSilver);
        $('#interBankGold').val(response[0].interBankGold);
        $('#interBankSilver').val(response[0].interBankSilver);
        $('#conversionGold').val(response[0].conversionGold);
        $('#conversionSilver').val(response[0].conversionSilver);
        $('#customDutyGold').val(response[0].customDutyGold);
        $('#customDutySilver').val(response[0].customDutySilver);
        $('#marginGold').val(response[0].marginGold);
        $('#marginSilver').val(response[0].marginSilver);
        $('#gstGold').val(response[0].gstGold);
        $('#gstSilver').val(response[0].gstSilver);
        $('#divisionGold').val(response[0].divisionGold);
        $('#divisionSilver').val(response[0].divisionSilver);
        $('#multiplyGold').val(response[0].multiplyGold);
        $('#multiplySilver').val(response[0].multiplySilver);
    }
    if (data.contact.length > 0) {
        $('input[name="isRate"][value=' + data.contact[0].isRate + ']').prop("checked", true);
        $('input[name="isLogin"][value=' + data.contact[0].isLogin + ']').prop("checked", true);
        $('input[name="isTrade"][value=' + data.contact[0].isTrade + ']').prop("checked", true);
       
    }
}

function cleanPremium($input, fieldName) {
    if (!$input || $input.length === 0) {
        return "0";
    }

    let val = $input.val();

    if (val === "--" && (fieldName === "Buy Premium" || fieldName === "Sell Premium")) {
        return val;
    }

    if (val !== null && val !== "" && !isNaN(val)) {
        return parseFloat(val);
    }

    alert(`${fieldName} is invalid. Allowed values: valid number (e.g., -1235, 0, 9999) or '--' for Buy/Sell Premium. Resetting to 0.`);
    $input.val("0");
    return "0";
}

function cleanCommonPremium($input, fieldName) {
    if (!$input || $input.length === 0) {
        return "0";
    }

    let val = $input.val();

    if (val !== null && val !== "" && !isNaN(val)) {
        return parseFloat(val);
    }

    alert(`${fieldName} is invalid. Allowed values: valid number (e.g., -123, 0, 45.6). Resetting to 0.`);
    $input.val("0");
    return "0";
}

$("body").on("click", ".updatePremium", async function () {
    const $row = $(this).closest("tr");

    const buyPremium = cleanPremium($row.find(".buyPremium"), "Buy Premium");
    const sellPremium = cleanPremium($row.find(".sellPremium"), "Sell Premium");

    const data = {
        id: this.id,
        isView: $row.find(".isView").is(":checked"),
        isTerminal: $row.find(".isTerminal").is(":checked"),
        isTrade: $row.find(".isTrade").is(":checked"),
        isComment: $row.find(".isComment").is(":checked"),
        name: $row.find(".sbName").val(),
        buyPremium: buyPremium,
        sellPremium: sellPremium
    };

    var response = ajaxPost('admin/Symbol/updatePremium', JSON.stringify(data));
    console.log(response);
});

$("body").on("click", ".updateCommonPremium", async function () {
    let buy, sell, type;

    if ($("#source").val() == "gold") {
        buy = cleanCommonPremium($(".goldBuyCommonPremium"), "Gold Buy Common Premium");
        sell = cleanCommonPremium($(".goldSellCommonPremium"), "Gold Sell Common Premium");
    } else {
        buy = cleanCommonPremium($(".silverBuyCommonPremium"), "Silver Buy Common Premium");
        sell = cleanCommonPremium($(".silverSellCommonPremium"), "Silver Sell Common Premium");
    }

    $("#buyCommonPremium").val(buy);
    $("#sellCommonPremium").val(sell);

    if ($("#symbolType").val() == "1") {
        type = cleanCommonPremium($(".gold999CommonPremium"), "999 Type Common Premium");
    } else if ($("#symbolType").val() == "2") {
        type = cleanCommonPremium($(".gold995CommonPremium"), "995 Type Common Premium");
    } else {
        type = "0";
    }

    $("#typeCommonPremium").val(type);
});

$("body").on("click", ".saveAll", function () {
    var array = [];

    $(".symbolList tr").each(function () {
        const $row = $(this);

        const buyPremium = cleanPremium($row.find(".buyPremium"), "Buy Premium");
        const sellPremium = cleanPremium($row.find(".sellPremium"), "Sell Premium");

        var data = {
            id: $row.find(".updatePremium").attr('id'),
            isView: $row.find(".isView").is(':checked'),
            isTerminal: $row.find(".isTerminal").is(':checked'),
            isTrade: $row.find(".isTrade").is(':checked'),
            isComment: $row.find(".isComment").is(":checked"),
            name: $row.find(".sbName").val(),
            buyPremium: buyPremium,
            sellPremium: sellPremium
        };

        array.push(data);
    });

    var response = ajaxPost('admin/Symbol/saveAll', JSON.stringify(array));
});

function changeRateType(type) {
    let data = {
        rateType: type
    };
    var response = ajaxPost('Symbol/changeRateType', JSON.stringify(data));
    if (response == 200) {
        location.reload();
    }
}

$(".isSwitch").on("change", function () {
    let data = new Object();
    data.isRate = $('input[name="isRate"]:checked').val();
    data.isLogin = $('input[name="isLogin"]:checked').val();
    data.isTrade = $('input[name="isTrade"]:checked').val();
  
    var response = ajaxPost('Symbol/isRateUpdate', JSON.stringify(data));
    if (response == 200) {
    }
});

function setCommonPremium() {
    let data = {
        goldBuyCommonPremium: cleanCommonPremium($('.goldBuyCommonPremium'), "Gold Buy Common Premium"),
        goldSellCommonPremium: cleanCommonPremium($('.goldSellCommonPremium'), "Gold Sell Common Premium"),
        silverBuyCommonPremium: cleanCommonPremium($('.silverBuyCommonPremium'), "Silver Buy Common Premium"),
        silverSellCommonPremium: cleanCommonPremium($('.silverSellCommonPremium'), "Silver Sell Common Premium"),
        gold999CommonPremium: cleanCommonPremium($('.gold999CommonPremium'), "999 Type Common Premium"),
        gold995CommonPremium: cleanCommonPremium($('.gold995CommonPremium'), "995 Type Common Premium")
    };

    var response = ajaxPost('admin/Symbol/setCommonPremium', JSON.stringify(data));
}

$(function () {
    $("#sortable-table tbody").sortable({
        update: function () {
            var sortedIds = $("#sortable-table tbody .updatePremium").map(function () {
                return this.id;
            }).get();

            var response = ajaxPost('Symbol/updateSequance', JSON.stringify(sortedIds));
        }
    }).disableSelection();
});

$("body").on("click", "#btnStart", function () {
    $(".startTime").val($("#startTimeAll").val());
});

$("body").on("click", "#btnEnd", function () {
    $(".endTime").val($("#endTimeAll").val());
});

function getSession(symbolId) {
    var data = ajaxGet('admin/Symbol/getSession?symbolId=' + symbolId);
    $("#hdnSymbolId").val(symbolId);
    $(".startTime,.endTime").val('');
    $("#startTimeAll").val("09:01");
    $("#endTimeAll").val("23:00");

    if (data !== null) {
        data = JSON.parse(data.session);
        for (const day of ["Sunday", "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday"]) {
            $(`#start${day}`).val(data[`start${day}`]);
            $(`#end${day}`).val(data[`end${day}`]);
        }
    }
}

$("body").on("click", ".saveSession", function () {
    let data = {};
    for (const day of ["Sunday", "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday"]) {
        data[`start${day}`] = $(`#start${day}`).val();
        data[`end${day}`] = $(`#end${day}`).val();
    }
    data["symbolId"] = $("#hdnSymbolId").val();

    var response = ajaxPost('admin/Symbol/saveSessionDetails', JSON.stringify(data));
});

$(document).on("change", "#isBill", function () {
    $(".divhide").toggleClass("hide", !this.checked);
});


