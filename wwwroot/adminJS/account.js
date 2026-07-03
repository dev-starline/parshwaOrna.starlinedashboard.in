$("body").on("click", ".addModel", async function () {
    var data = await ajaxGet('Account/viewAddModel');
    $('#addGroup').modal('toggle');
    $("input[name=password]").val(data.loginId);
    $("input[name=loginId]").val(data.loginId);
    let html = data.group.map(g => `<option value="${g.groupId}">${g.groupName}</option>`).join('');
    $("select[name=groupId]").html(html);

})

function updateAmount(type) {
    let data = new Object();
    data.loginId = $(".loginId").val();
    data.type = type;
    data.amount = $('.amount').val();   
    var response = ajaxPost('/admin/Account/updateAmount', JSON.stringify(data));
    if (response) {
        $('#viewMargin').text(response.margin);
        $('#margin').val(response.margin);
        $('#freeMargin').text(response.freeMargin);
    }
}

function confirmApprove() {
    return confirm('Are you sure you want to Approve this user?');
}
