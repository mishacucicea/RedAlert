$(function () {
    $("#colorPicker").tinycolorpicker();
    $("#sendMessageButton").click(function () {
        var senderKey = "@ViewBag.SenderKey";
        if (!senderKey) {
            senderKey = localStorage.getItem("senderKey")
        }
        var picker = $("#colorPicker").data("plugin_tinycolorpicker");
        // it's assumed that a sender key is already in the local storage.
        $.get("/api/message/send", {
            senderkey: senderKey,
            color: picker.colorHex.substring(1)
        });
    });
});