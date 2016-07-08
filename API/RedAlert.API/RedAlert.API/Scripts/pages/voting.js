$(document).ready(function () {
    var senderKey = localStorage.getItem("senderKey");
    var senderKeyBox;

    $("#qr-Imageup").attr("src", "/QrCode/GenerateVotingQr?answer=Up");

    $("#qr-Imagedown").attr("src", "/QrCode/GenerateVotingQr?answer=Down");

    if (senderKey == null) {
        $('#messageBox').text('Have you registered you device ?');
    } else {
        $('#senderkeyidVotingQR').val(senderKey);
        $.post("/Color/Voting?senderKey=" + senderKey);

    }
    $("#senderkeyidVotingQR").change(function () {
        senderKeyBox = $('#senderkeyidVotingQR').val();
        if (senderKeyBox != null) {
            $.post("/Color/Voting?senderKey=" + senderKeyBox);
        }
    });

    $("#resetButton").click(function () {

        // it's assumed that a sender key is already in the local storage.
        $.get("/Voting/Reset", {
        });
    });
    setInterval(function () {
        $.post("/Color/Voting?senderKey=" + senderKey);
    }, 2000);
});