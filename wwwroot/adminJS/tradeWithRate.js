$(document).ready(function () {
    $("#searchBox").autocomplete({
        source: function (request, responce) {
            $.ajax({
                url: "Trade/search",
                data: { query: request.term },
                success: function (data) {
                    responce(data);
                },
                error: function (err) {
                    alert(err);
                }
            });
        }
    });
});
$("#symbolId").change(function () {
    loginId = $("#searchBox").val().split('|')[0].trim();
    symbolId = $("#symbolId").val();
    var data = ajaxGet('Trade/getSymbolVolume?loginId=' + loginId + '&symbolId=' + symbolId);
    if (data.length > 0) {
        var oneClick = data[0].oneClick, inTotal = data[0].inTotal, step = data[0].step, html = "";
        if (step==0) {
            step = oneClick;
        }
        for (let i = oneClick; i <= inTotal; i) {
            html += "<option value="+i+">"+i+"</option>";
            i = i + step;
        }
        $("#symbolVolume").html(html);
        socketConnect(data[0].accountId, data[0].groupId);

    }
});
var groupDetails = [];
function socketConnect(accountId, groupId) {
    socket.emit('client', $('.userName').text());
    socket.emit('endUser', accountId, groupId);
}
socket.on('groupDetails', function (data) {
    groupDetails = JSON.parse(pako.inflate(data, { to: 'string' }));
});
socket.on('mainProducts', function (data) {
    var mainProduct = JSON.parse(pako.inflate(data, { to: 'string' }));
    if (mainProduct.length > 0 && groupDetails.length>0) {
        let objRate = mainProduct.filter(activity => (activity.id == $("#symbolId").val()));
        let objGroup = groupDetails.filter(activity => (activity.symbolId == $("#symbolId").val()));
        if (objRate.length > 0 && objGroup.length > 0) {
            const referenceIdentifier = (objRate[0].identifier || "").toString();
            const isReferenceRate = referenceIdentifier.startsWith("Rate_");
            const referenceName = isReferenceRate ? referenceIdentifier.replace(/^Rate_/, "").replace(/\s+/g, "") : "";
            const referenceRate = isReferenceRate
                ? mainProduct.find(activity => {
                    const name = (activity.name || "").toString().replace(/\s+/g, "");
                    return name && name === referenceName;
                })
                : null;

            const rateSource = referenceRate || objRate[0];
            let bid = 0, ask = 0;
            bid = parseInt(rateSource.bid) + objGroup[0].buyPremium;
            ask = parseInt(rateSource.ask) + objGroup[0].sellPremium;
            if (rateSource.src=='gold') {
                bid = bid + objGroup[0].buyPremiumGold;
                ask = ask + objGroup[0].sellPremiumGold;
            }
            else if (rateSource.src == 'silver') {
                bid = bid + objGroup[0].buyPremiumSilver;
                ask = ask + objGroup[0].sellPremiumSilver;
            }
            if (isNaN(ask)) {
                ask = '--'
            }
            if (isNaN(bid)) {
                bid = '--'
            }
            if ($("#autoExchange").is(":checked") === true) {
                $("#buyExchange").val(rateSource.sell);
                $("#sellExchange").val(rateSource.buy);
            }
            if ($("#autoRate").is(":checked") === true) {
                $("#buyRate").val(ask);
                $("#sellRate").val(bid);
            }
            if ($("#rateType").val() == "unFix") {
                $('#autoRate').prop('checked', false);
            }
            else {
                $('#autoRate').prop('checked', true);
            }
            
        }
        
        
    }
});
document.getElementById('buyButton').addEventListener('click', function () {
    document.getElementById('exchange').value = document.getElementById('buyExchange').value; // Set exchange value
    document.getElementById('rate').value = document.getElementById('buyRate').value;
});

document.getElementById('sellButton').addEventListener('click', function () {
    document.getElementById('exchange').value = document.getElementById('sellExchange').value; // Set exchange value
    document.getElementById('rate').value = document.getElementById('sellRate').value;
});
$('#rateType').change(function () {
    var rateType = $(this).val();
    var loginId = $("#searchBox").val().split("||")[0].trim();

    if (rateType === 'rateCut') {
        var data = ajaxGet('Trade/fillPremium?rateType=' + rateType + '&loginId=' + loginId);
        var premiumDropdown = $('#unFixPremium');
        premiumDropdown.empty(); 

        $.each(data, function (index, item) {
            premiumDropdown.append(
                $('<option></option>').val(item.value).text(item.text)
            );
        });
    } else {
        $('#unFixPremium').empty().append('<option value="">Select a Premium</option>');
    }
});
