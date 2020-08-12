var connection = new signalR.HubConnectionBuilder().withUrl("http://localhost:55202/trackServiceHub").build();

connection.on("ReceiveAllData", function (result) {
    var li = document.createElement("li");
    li.textContent = result;
    document.getElementById("messagesList").appendChild(li);
});

connection.on("CommonMessage", function (result) {
    alert(result);
});

connection.onclose(e => {
    console.log('{ "code": "108", "message": "Server connection failed!" }')
});

connection.start().then(function () {
}).catch(function (err) {
    return console.error(err.toString());
});

function restartConnectionIfStopped() {
    if (connection.state == 'Disconnected') {
        connection.start().then(function () {
        }).catch(function (err) {
            return console.error(err.toString());
        });
    }
}
setInterval(restartConnectionIfStopped, 10000);
