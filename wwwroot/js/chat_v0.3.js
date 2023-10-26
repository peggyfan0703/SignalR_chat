"use strict";

var connection = new signalR.HubConnectionBuilder().withUrl("/chatHub").build();
document.getElementById("sendButton").disabled = true;

window.onload = function () {
    // 捲動到最底部
    var messagesList = document.getElementById("messagesList");
    messagesList.scrollTop = messagesList.scrollHeight;
}

connection.on("ReceiveMessage", function (user, message) {
    var li = document.createElement("li");
    li.textContent = `${user} ： ${message}`;
    li.style.color = "#447373";

    var messagesList = document.getElementById("messagesList");
    //messagesList.insertBefore(li, messagesList.firstChild); //新增至第一筆
    messagesList.insertAdjacentElement('beforeend', li); //新增至最後一筆

    messagesList.scrollTop = messagesList.scrollHeight; //捲動到最底部
});

connection.start().then(function () {
    document.getElementById("sendButton").disabled = false;
}).catch(function (err) {
    return console.error(err.toString());
});

document.getElementById("sendButton").addEventListener("click", function (event) {
    var userid = $('#InputuserID').val();
    var username = $('#InputuserName').val();
    var message = $('#InputMessage').val().trim();

    if (message == null || message == undefined || message == "") {
        alert("輸入訊息不可為空。");
    }
    else {
        connection.invoke("SendMessage", userid, username, message).catch(function (err) {
            return console.error(err.toString());
        });
        event.preventDefault();

    }
    document.getElementById("InputMessage").value = "";

});