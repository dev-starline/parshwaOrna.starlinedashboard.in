getRateDifferance();
$("body").on("click", ".updateReferance", async function () {
    let data = new Object();
    data.id = parseInt($(this).attr("id"));
    data.name = $(this).closest('tr').find(".name").val();
    data.isView = $(this).closest('tr').find(".isView").is(':checked');
    var response = ajaxPost('Setting/updateReferance', JSON.stringify(data));
    console.log(response);
});
function getRateDifferance() {
    var data = ajaxGet('Setting/getRateDifferance');
    if (data.length > 0) {
        $('.gold').val(data[0].goldDifferance);
        $('.silver').val(data[0].silverDifferance);
        $('.freezOuter').val(data[0].freezOuter);
        $('.freezInner').val(data[0].freezInner);
        $('.offQuotes').val(data[0].offQuotes);
        $('.pendingOrderDelete').val(data[0].plDelete);
    }
}

$("body").on("click", ".updateRateDifferance", async function () {
    let data = new Object();
    data.gold = $(".gold").val();
    data.silver = $(".silver").val();
    var response = ajaxPost('Setting/updateRateDifferance', JSON.stringify(data));
    console.log(response);
});



$("body").on("click", ".updatePassword", async function () {
    let data = new Object();
    data.oldPassword = $(".oldPassword").val();
    data.newPassword = $(".newPassword").val();
    data.confirmPassword = $(".confirmPassword").val();
    var response = ajaxPost('Setting/updatePassword', JSON.stringify(data));
    console.log(response);
});
$("body").on("click", ".updateOrderSetting", async function () {
    let data = new Object();
    data.freezOuter = parseInt($(".freezOuter").val());
    data.freezInner = parseInt($(".freezInner").val());
    data.offQuotes = parseInt($(".offQuotes").val());
    data.pendingOrderDelete = $(".pendingOrderDelete").val();
    var response = ajaxPost('Setting/updateOrderSetting', JSON.stringify(data));
    console.log(response);
});


$("body").on("click", ".delOrder", async function () {
    let data = new Object();
  
    data.historyType = $("#drpHistoryType").val();
    data.toDate = $("#toDate").val();
    data.fromDate = $("#fromDate").val();

   
    if (data.historyType === "0") {
        alert("Please select a valid order history type.");
        return;
    }
    if (!data.toDate || !data.fromDate) {
        alert("Please select both From and To dates.");
        return;
    }
    var response = await ajaxPost('Setting/deleteOrderHistory', JSON.stringify(data));
    console.log("Delete response:", response);
});
