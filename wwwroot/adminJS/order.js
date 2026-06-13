
function selectOrder(type) {
    if (!confirm(`Are you sure you want to ${type} selected records?`)) return;

    const array = [];
    document.querySelectorAll('input[name="selectedItems"]:checked').forEach(item => {
        array.push({
            id: item.id,
            type: type
        });
    });

    if (array.length === 0) {
        alert("Please select at least one row.");
        return;
    }

    fetch('/admin/OpenOrder/selectedItems', {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json'
        },
        body: JSON.stringify(array)
    })
        .then(response => {
            if (response.ok) location.reload();
            else throw new Error("Server error.");
        })
        .catch(error => alert("Failed: " + error));
}


connection.on('adminAlertDetails', function (data) {
    const action = data?.action || null;

    if (!data || action === 'open' || action === 'pass' || action === 'update') {
        location.reload();
    }
});
function showOrderPopup(loginId) {
    $('#orderModal').modal('show');
    $.ajax({
        url: '/api/terminal/getOpenOrdersbyLoginId',
        type: 'GET',
        data: { loginId: loginId },
        success: function (data) {
            let content = '';
            let totalGoldQtyBuy = 0;
            let totalSilverQtyBuy = 0;
            let totalGoldQtySell = 0;
            let totalSilverQtySell = 0;
            let NameAndFirm = '';
            let LoginId = '';
            let groupName = '';
            let totalOrderCount = 0;
            let grandTotal = 0;
            if (data.data && data.data.length > 0) {
                totalOrderCount = data.data.length;
                content += '<table class="table table-bordered">';
                content += '<thead><tr><th class="b1">OrderNo</th><th class="b1">LoginId</th><th class="b2">Name/Firm</th><th class="b1">Symbol</th><th class="b1">RateType</th><th class="b1">TradeType</th><th class="b1">Quantity</th><th class="b1">Margin</th><th class="b1">Exchange</th><th class="b1">Diffrence</th><th class="b1">Price</th><th class="b1">Total</th><th class="b1">Total+Tax</th><th class="b1">From</th><th class="b2">Time</th><th class="b2">UpdateTime</th></tr></thead>';
                content += '<tbody>';

                let lastFormattedDate = null;
               
                data.data.forEach(function (order) {

                    const orderDate = new Date(order.orderTime);
                    const day = String(orderDate.getDate()).padStart(2, '0');
                    const month = String(orderDate.getMonth() + 1).padStart(2, '0');
                    const year = orderDate.getFullYear();
                    const formattedDate = `${day}-${month}-${year}`;

                    NameAndFirm = `${order.name.charAt(0).toUpperCase() + order.name.slice(1)} (${order.firm.charAt(0).toUpperCase() + order.firm.slice(1)})`;
                    LoginId = order.loginId;
                    groupName = order.groupName;

                    grandTotal += parseFloat(order.total) || 0;
                    if (formattedDate !== lastFormattedDate) {
                        content += `<tr><td colspan="16"><strong>${formattedDate}</strong></td></tr>`;
                        lastFormattedDate = formattedDate;
                    }
                    if (order.source.toUpperCase().includes('GOLD') && order.tradeType == 1) {
                        totalGoldQtyBuy += parseFloat(order.volume) || 0;
                    } else if (order.source.toUpperCase().includes('SILVER') && order.tradeType == 1) {
                        totalSilverQtyBuy += parseFloat(order.volume) || 0;
                    } else if (order.source.toUpperCase().includes('GOLD') && order.tradeType == 2) {
                        totalGoldQtySell += parseFloat(order.volume) || 0;
                    } else if (order.source.toUpperCase().includes('SILVER') && order.tradeType == 2) {
                        totalSilverQtySell += parseFloat(order.volume) || 0;
                    }
                    content += `<tr>
                        <td class="b1">${order.dealNo}</td>
                        <td class="b1">${order.loginId}</td>
                        <td class="b2">${order.name} | ${order.firm}</td>
                        <td class="b1">${order.symbolName}</td>
                        <td class="b1">${order.rateType}</td>
                        <td class="b1">${order.tradeTypeView}</td>
                        <td class="b1">${order.volume}</td>
                        <td class="b1">${order.margin}</td>
                        <td class="b1">${order.exchange}</td>
                        <td class="b1">${order.differenceRate}</td>
                        <td class="b1">${order.rate}</td>
                        <td class="b1">${order.total}</td>
                        <td class="b1">${order.tax}</td>
                        <td class="b1">${order.deviceType}</td>
                        <td class="b2">${new Date(order.orderTime).toLocaleString()}</td>
                        <td class="b2">${new Date(order.editorderTime).toLocaleString()}</td>
                    </tr>`;
                });
                
                content += '</tbody></table>';
                $('#txtnameandfirm').text(NameAndFirm);
                $('#txtloginid').text(LoginId);
                $('#txtgroup').text(groupName);
                $('#txtOcount').text(totalOrderCount);
                $('#txtBTGoldQty').text(totalGoldQtyBuy.toFixed(2));
                $('#txtBTSilverQty').text(totalSilverQtyBuy.toFixed(2));
                $('#txtSTGoldQty').text(totalGoldQtySell.toFixed(2));
                $('#txtSTSilverQty').text(totalSilverQtySell.toFixed(2));
                $('#txtGrandTotal').text(grandTotal.toFixed(2));
            } else {
                content = '<p>No open orders found for this user.</p>';
            }
            $('#orderDetailsContent').html(content);          
        },
        error: function () {
            $('#orderDetailsContent').html('<p>Error loading order details. Please try again.</p>');
        }
    });
}



