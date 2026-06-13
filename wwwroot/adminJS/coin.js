getBankCalculation();
function getBankCalculation() {
    var data = ajaxGet('Coin/getBankCalculation');
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
        $('input[name=isRate][value="' + data.contact[0].isRate + '"]').prop("checked", true);
        $('input[name=isRateType][value="' + data.contact[0].isRateType + '"]').prop("checked", true);
        $('input[name=isTrade][value="' + data.contact[0].isCoinTrade + '"]').prop("checked", true);
        $('input[name=isGoldCoinHeader][type=checkbox]').prop("checked", data.contact[0].isGoldCoinHeader);
        $('input[name=isSilverCoinHeader][type=checkbox]').prop("checked", data.contact[0].isSilverCoinHeader);
        $('.goldCoinHeader').val(data.contact[0].goldCoinHeader);
        $('.silverCoinHeader').val(data.contact[0].silverCoinHeader);
    }
    if (data.master.length > 0) {
        $("#txtTimestart").val(data.master[0].coinStartTime);
        $("#txtTimeEnd").val(data.master[0].coinEndTime);
    }
}
$("body").on("click", ".updatePremium", async function () {
    let data = new Object();
    data.id = $(this)[0].id;
    data.isView = $(this).closest('tr').find(".isView").prop("checked");
    data.isStock = $(this).closest('tr').find(".isStock").prop("checked");
    data.name = $(this).closest('tr').find(".name").val();
    data.buyPremium = $(this).closest('tr').find(".buyPremium").val();
    data.sellPremium = $(this).closest('tr').find(".sellPremium").val();
    data.division = $(this).closest('tr').find(".division").val();
    data.multiply = $(this).closest('tr').find(".multiply").val();
    data.coinStock = $(this).closest('tr').find(".coinStock").val();
    data.gst = $(this).closest('tr').find(".gst").val();
    data.coinId = $(this).closest('tr').find(".coinId").val();

    if (!validateArray([data])) {
        return false;
    }

    var response = ajaxPost('Coin/updatePremium', JSON.stringify(data));
    //console.log(response);
});
function validateArray(arr) {
    for (let i = 0; i < arr.length; i++) {
        const data = arr[i];

        // Required fields
        const requiredFields = {
            buyPremium: "Please enter buy premium",
            sellPremium: "Please enter sell premium",
            division: "Please enter division",
            multiply: "Please enter multiply"
        };

        for (const [field, message] of Object.entries(requiredFields)) {
            if (!data[field] || data[field].trim() === "") {
                alert(`Row ${i + 1}: ${message}`);
                return false;
            }
        }

        // Numeric validations
        const numericChecks = [
            { field: "division", message: "Division must be greater than zero" },
            { field: "multiply", message: "Multiply must be greater than zero" }
        ];

        for (const { field, message } of numericChecks) {
            const value = parseInt(data[field], 10);
            if (isNaN(value) || value <= 0) {
                alert(`Row ${i + 1}: ${message}`);
                return false;
            }
        }
    }

    return true;
}

$("body").on("click", ".saveAll", function () {
    var array = [];
    $(".symbolList tr").each(async function () {
        var data = new Object();
        data.id = $(this).closest('tr').find(".updatePremium").attr('id');
        data.isView = $(this).closest('tr').find(".isView").prop("checked");
        data.isStock = $(this).closest('tr').find(".isStock").prop("checked");
        data.name = $(this).closest('tr').find(".name").val();
        data.buyPremium = $(this).closest('tr').find(".buyPremium").val();
        data.sellPremium = $(this).closest('tr').find(".sellPremium").val();
        data.division = $(this).closest('tr').find(".division").val();
        data.multiply = $(this).closest('tr').find(".multiply").val();
        data.coinStock = $(this).closest('tr').find(".coinStock").val();
        data.gst = $(this).closest('tr').find(".gst").val();
        data.coinId = $(this).closest('tr').find(".coinId").val();
        array.push(data);
    })
    if (!validateArray(array)) {
        return false;
    }
    var response = ajaxPost('Coin/saveAll', JSON.stringify(array));
});
function changeRateType(type) {
    let data = new Object();
    data.rateType = type;
    var response = ajaxPost('Coin/changeRateType', JSON.stringify(data));
    if (response == 200) {
    }
}

$(".isTrade").on("change", function () {
    let data = new Object();
    data.isCoinTrade = $(this).val();
    var response = ajaxPost('Coin/isTradeUpdate', JSON.stringify(data));
    if (response == 200) {
    }
});

$(".isRate").on("change", function () {
    let data = new Object();
    data.isRate = $(this).val();
    var response = ajaxPost('Coin/isRateUpdate', JSON.stringify(data));
    if (response == 200) {
    }
});
function setCommonPremium() {
    let data = new Object();
    data.goldBuyCommonPremium = $('.goldBuyCommonPremium').val();
    data.goldSellCommonPremium = $('.goldSellCommonPremium').val();
    data.silverBuyCommonPremium = $('.silverBuyCommonPremium').val();
    data.silverSellCommonPremium = $('.silverSellCommonPremium').val();
    var response = ajaxPost('Coin/setCommonPremium', JSON.stringify(data));
    if (response == 200) {
    }
}

function setCoinHeader() {
    let data = new Object();
    data.goldCoinHeader = $('.goldCoinHeader').val();
    data.silverCoinHeader = $('.silverCoinHeader').val();
    data.isGoldCoinHeader = $('.isGoldCoinHeader').prop('checked');
    data.isSilverCoinHeader = $('.isSilverCoinHeader').prop('checked');
    var response = ajaxPost('Coin/setCoinHeader', JSON.stringify(data));
    if (response == 200) {
    }
}
function setCoinTradeTime() {
    let data = new Object();
    data.coinStartTime = $('#txtTimestart').val();
    data.coinEndTime = $('#txtTimeEnd').val();
    var response = ajaxPost('Coin/setCoinTradeTime', JSON.stringify(data));
    if (response == 200) {
    }
}
$(function () {
    $("#sortable-table tbody").sortable({
        update: function (event, ui) {
            var sortedIds = $("#sortable-table tbody .updatePremium").map(function () {
                return this.id;
            }).get();
            var response = ajaxPost('Coin/updateSequance', JSON.stringify(sortedIds));
            if (response == 200) {
            }

        }
    });
    $("#sortable-table tbody").disableSelection();
});