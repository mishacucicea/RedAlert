var clipboard = new Clipboard('.btn');

$(function () {
    var apiUrl = window.location.protocol + "//" + window.location.host;
    var senderKey = localStorage.getItem("senderKey");
    var color = localStorage.getItem("color");
   

    if (senderKey != null) {
        $('#senderkeyid').val(senderKey);
    }

    if (senderKey != null) {
        $('#textBoxUrl').val(apiUrl + '/api/message/send?senderkey=' + senderKey + '&color=' + color);
    }
    if (color == null || "") {
        color = $('#colorboxId').val();
    }
    $("#colorboxId").change(function () {
        var senderKeyBox = $('#senderkeyid').val();
        var color = $('#colorboxId').val();
        if (senderKeyBox != null && color!= null) {
            $('#textBoxUrl').val(apiUrl + '/api/message/send?senderkey=' + senderKeyBox + '&color=' + color);
        }
    });
   
    $("#senderkeyid").change(function () {
        var senderKeyBox = $('#senderkeyid').val();
        if (senderKeyBox != null) {
            $('#textBoxUrl').val(apiUrl + '/api/message/send?senderkey=' + senderKeyBox + '&color=' + color);
        }
    });
});