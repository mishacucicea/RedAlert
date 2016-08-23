var clipboard = new Clipboard('.btn');

$(function () {
    var apiUrl = window.location.protocol + "//" + window.location.host;
    var senderKey = localStorage.getItem("senderKey");
    var color = localStorage.getItem("color");
    var url = '/api/message/send?senderkey=';
    var colorParameter = '&color=';

    if (senderKey != null) {
        $('#senderkeyid').val(senderKey);
    }

    if (senderKey != null) {
        $('#textBoxUrl').val(apiUrl + url + senderKey + colorParameter + color);
    }
    if (color == null || "") {
        color = $('#colorboxId').val();
    }
    $("#colorboxId").change(function () {
        var senderKeyBox = $('#senderkeyid').val();
        var color = $('#colorboxId').val();
        if (senderKeyBox != null && color!= null) {
            $('#textBoxUrl').val(apiUrl + url + senderKeyBox + colorParameter + color);
        }
    });
   
    $("#senderkeyid").change(function () {
        var senderKeyBox = $('#senderkeyid').val();
        if (senderKeyBox != null) {
            $('#textBoxUrl').val(apiUrl + url + senderKeyBox + colorParameter + color);
        }
    });
});