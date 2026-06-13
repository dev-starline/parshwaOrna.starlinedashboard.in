let selectedSearchItem = null
let usePremium = []

$(function () {
    $("#searchBox").autocomplete({
        source: function (request, response) {
            $.ajax({
                url: '/Trade/search',
                data: { query: request.term },
                success: function (data) {
                    //response(data);
                    var formatted = data.map(function (item) {
                        const { loginId, name, firmName } = item.data
                        const displayString = `${loginId} || ${name} || ${firmName}`
                        return {
                            label: displayString,  
                            value: displayString,
                            fullObject: item.data
                        };
                    });
                    response(formatted);
                },
                error: function (err) {
                    alert("Search failed: " + err.statusText);
                }
            });
        },
        select: function (event, ui) {
            //selectedSearchItem = ui.item.fullObject; // store the full selected object
            //$("#searchBox").data("fullObject", ui.item.fullObject)
            if (ui.item.fullObject) {
                const { accId, groupId } = ui.item.fullObject
                connection.invoke("endUser", accId, groupId)
                connection.on('groupDetails', data => {
                    const bytes = Uint8Array.from(atob(data), c => c.charCodeAt(0));
                    const arrayProduct = JSON.parse(pako.inflate(bytes, { to: 'string' }));
                    usePremium = arrayProduct;
                })
            } else {
                connection.off('groupDetails')
                usePremium = [];
            }
        }
    });
    $("#searchBox").on('input', function () {
        const val = $(this).val().trim();
        if (val === "") {
            connection.off('groupDetails');
            usePremium = [];
        }
    });
    $('#tradeType').on('change', function () {
        const type = $(this).val();
        const labelText = (type === '3' || type === '4') ? 'Limit Price' : 'Open Price';
        $('#rateLabelText').html('<strong>' + labelText + '</strong>');
    });

    $('#rateType').on('change', function () {
        const rateType = $(this).val();
        //const loginId = $("#searchBox").val().split("||")[0].trim();
        const loginId = $("#searchBox").data("fullObject").loginId;

        if (rateType === 'rateCut') {
            $.get('/Trade/fillPremium', { rateType, loginId }, function (data) {
                const $premium = $('#premium');
                $premium.empty();
                $.each(data, function (index, item) {
                    $premium.append($('<option></option>').val(item.value).text(item.text));
                });
            });
        } else {
            $('#premium').empty().append('<option value="0">Select a Premium</option>');
        }
    });

    const selectedValue = $('input[type=radio][name=tradetype]:checked').val();
    toggleTradeTypeUI(selectedValue);

    $('input[type=radio][name=tradetype]').change(function () {
        toggleTradeTypeUI(this.value);
    });

});

function toggleLimitRow() {
    const selectedType = document.querySelector('input[name="tradetype"]:checked').value;
    const trLimit = document.querySelector('.trlimit');
    if (selectedType === 'P') {
        trLimit.style.display = '';
    } else {
        trLimit.style.display = 'none';
    }
}
document.querySelectorAll('input[name="tradetype"]').forEach((radio) => {
    radio.addEventListener('change', toggleLimitRow);
});
window.addEventListener('DOMContentLoaded', toggleLimitRow);

connection.on('connect', function () {
    connection.invoke('client', $('.userName').text());
});


connection.on('workerPublish', data =>{

    const symbolId = $('#symbolId').val();
    if (symbolId?.trim() == '0' || symbolId == null) {
        $("#buyPrice").val('')
        $("#sellPrice").val('')
        $("#exchangesellPrice").val('')
        $("#exchangebuyPrice").val('')
        return;
    }
   // const arrayProduct = JSON.parse(pako.inflate(data, { to: 'string' }));
    const compressed = Uint8Array.from(atob(data), c => c.charCodeAt(0));
    data = pako.ungzip(compressed, { to: 'string' });
    var arrayProduct = JSON.parse(data).products;
    const findSymbol = arrayProduct.find((item)=> item.id == symbolId);
    if (findSymbol) {
        
        const findGroup = usePremium.find((item) => item.symbolId == symbolId)
        const { ask, bid, buy, sell } = findSymbol;
        const isGold = findSymbol.src?.trim()?.toLowerCase() === 'gold'
        let premium = {
            buy: findGroup != undefined ? findGroup?.buyPremium + findGroup?.[
                isGold ? 'buyPremiumGold' : 'buyPremiumSilver'
            ] : 0,
            sell: findGroup != undefined ? findGroup?.sellPremium + findGroup?.[
                isGold ? 'sellPremiumGold' : 'sellPremiumSilver'
            ] : 0,
        }
        const isRateCheck = $("#ratecheck").is(":checked");
        const isExchangeCheck = $("#exchangecheck").is(":checked");
        if (isRateCheck) {
            //$("#buyPrice").val(parseInt(parseInt(ask) + premium.sell).toString())
            //$("#sellPrice").val(parseInt(parseInt(bid) + premium.buy).toString())
            $("#buyPrice").val(parseFloat((Number(ask) + Number(premium.sell)).toFixed(2)));
            $("#sellPrice").val(parseFloat((Number(bid) + Number(premium.buy)).toFixed(2)));

        } 
        if (isExchangeCheck) {
            $("#exchangesellPrice").val(buy)
            $("#exchangebuyPrice").val(sell)
        }
        //if ($("#ratecheck").is(":checked") === true) {
        //    $("#buyPrice").val(findSymbol.ask);
        //    $("#sellPrice").val(findSymbol.bid);
        //}
        //if ($("#ratecheck").is(":checked") === false) {
        //    $("#buyPrice").val('');
        //    $("#sellPrice").val('');
        //}
        //if ($("#exchangecheck").is(":checked") === true) {
        //    $("#exchangesellPrice").val(findSymbol.buy);
        //    $("#exchangebuyPrice").val(findSymbol.sell);
        //}
        //if ($("#exchangecheck").is(":checked") === false) {
        //    $("#exchangesellPrice").val('');
        //    $("#exchangebuyPrice").val('');
        //}
    }
});

$("body").on("click", "#saveOrder", async function (e) {
    e.preventDefault();
    let data = new Object();
    if ($("#tradeType").val() == "1") {
        data.exchange = $("#exchangebuyPrice").val();
        data.rate = $("#buyPrice").val();
    }
    else if ($("#tradeType").val() == "2") {
        data.exchange = $("#exchangesellPrice").val();
        data.rate = $("#sellPrice").val();
    }
    else if ($("#tradeType").val() == "3") {
        data.exchange = $("#exchangebuyPrice").val();
        data.rate = $("#buyPriceLimit").val();
    }
    else if ($("#tradeType").val() == "4") {
        data.exchange = $("#exchangesellPrice").val();
        data.rate = $("#sellPriceLimit").val();
    }
    data.loginId = $(".loginId").val();
    data.symbolId = $("#symbolId").val();
    data.volume = $("#volume").val();
    data.tradeType = $("#tradeType").val();
    data.rateType = $("#rateType").val();
    data.comment = $("#comment").val();
    data.fromDate = $("#fromDate").val();
    var response = ajaxPost('Trade/createTrade', JSON.stringify(data));
    console.log(response);
    $(".loginId").val('');
    $("#symbolId").val(0);
    $("#volume").val('');
    $("#tradeType").val(0);
    $("#comment").val('');
    $("#exchangebuyPrice").val('');
    $("#exchangesellPrice").val('');
    $("#buyPrice").val('');
    $("#sellPrice").val('');
    $("#buyPriceLimit").val('');
    $("#sellPriceLimit").val('');
});

function toggleTradeTypeUI(value) {
    if (value === 'M') {
        $(".optlimit").addClass("hide");
        $(".optmarket").removeClass("hide");
    } else if (value === 'P') {
        $(".optlimit").removeClass("hide");
        $(".optmarket").addClass("hide");
    }
}


