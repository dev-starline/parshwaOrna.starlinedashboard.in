connection.on('workerPublish', function (data) {
    try {
        const compressed = Uint8Array.from(atob(data), c => c.charCodeAt(0));
        data = pako.ungzip(compressed, { to: 'string' });
        var arrayProduct = JSON.parse(data).refProduct;
        if (arrayProduct.length > 0) {
            for (var i = 0; i < 2; i++) {
                let spotRate = 0, comexRate = 0, comexType = 'XAGUSD', type = 'Silver';
                if (i == 0) {
                    type = 'Gold';
                    comexType = 'XAUUSD'
                }
                comexRate = arrayProduct.filter(activity => (activity.symbol == comexType));
                $(".comex" + type + "").html(comexRate[0].Ask);
                spotRate = arrayProduct.filter(activity => (activity.symbol == $("#spotType" + type + "").val()));

                $(".spot" + type + "").html(spotRate[0].Ask);

                let rate = parseFloat($(".comex" + type + "").html()) + parseFloat($("#premium" + type + "").val());
                rate = (parseFloat($("#interBank" + type + "").val()) + parseFloat($(".spot" + type + "").html())) * rate;
                rate = (rate * parseFloat($("#conversion" + type + "").val())) + parseFloat($("#customDuty" + type + "").val()) + parseFloat($("#margin" + type + "").val());
                rate = ((rate * parseFloat($("#gst" + type + "").val())) / 100) + rate;
                $(".total" + type + "").html(rate.toFixed(2));
                rate = (rate / parseFloat($("#division" + type + "").val()) * parseFloat($("#multiply" + type + "").val()));
                $(".grandTotal" + type + "").html(rate.toFixed(2));
                comexRate = arrayProduct.filter(activity => (activity.symbol == type.toLowerCase()));
                $(".mcx" + type + "").html(comexRate[0].Ask);
                $(".diff" + type + "").html((parseFloat(comexRate[0].Ask) - rate).toFixed(2));
            }
        }
    } catch (e) {

    }

    console.log();
});