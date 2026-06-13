getContactDetails();
function getContactDetails() {
    var response = ajaxGet('Contact/getContactDetails');
    if (response.length > 0) {
        $('#contactId').val(response[0].id);
        $('#marqueeTop').val(response[0].marqueeTop);
        $('#marqueeBottom').val(response[0].marqueeBottom);
        $('#number1').val(response[0].number1);
        $('#number2').val(response[0].number2);
        $('#number3').val(response[0].number3);
        $('#number4').val(response[0].number4);
        $('#number5').val(response[0].number5);
        $('#number6').val(response[0].number6);
        $('#number7').val(response[0].number7);
        $('#whatsAppNo').val(response[0].whatsAppNo);
        $('#address1').val(response[0].address1);
        $('#address2').val(response[0].address2);
        $('#address3').val(response[0].address3);
        $('#email1').val(response[0].email1);
        $('#email2').val(response[0].email2);
        $("#isBuy").prop("checked", response[0].isBuy)
        $("#isSell").prop("checked", response[0].isSell)
        $("#isHigh").prop("checked", response[0].isHigh)
        $("#isLow").prop("checked", response[0].isLow)
        $('#showWeb').attr('src', response[0]?.bannerWeb ?? window.location.origin + "/img/noimage.png");
        $('#showApp').attr('src', response[0]?.bannerApp ?? window.location.origin + "/img/noimage.png");
    }
}