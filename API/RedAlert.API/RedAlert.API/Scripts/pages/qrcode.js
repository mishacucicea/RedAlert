﻿$(document).ready(function () {
    var senderKey = localStorage.getItem("senderKey");
    var color = localStorage.getItem("color");
    var url = "/QrCode/GenerateQr?senderKey=";

    if (senderKey == null) {
        $('#messageBox').text('Have you registered you device ?');
    } else {
        $('#senderkeyidQR').val(senderKey);
        $("#qr-Image").attr("src", url + senderKey + "&color=" + color);
    }
    $("#senderkeyidQR").change(function () {
        var senderKeyBox = $('#senderkeyidQR').val();
        if (senderKeyBox != null) {
            $("#qr-Image").attr("src", url + senderKeyBox + "&color=" + color);
        }
    });
});