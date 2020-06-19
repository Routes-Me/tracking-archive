"use strict";

var connection = new signalR.HubConnectionBuilder().withUrl("http://localhost:55202/trackServiceHub").build();

//Disable send button until connection is established
//document.getElementById("sendButton").disabled = true;

connection.on("ReceiveAll", function (result) {
    var li = document.createElement("li");
    li.textContent = result;
    document.getElementById("messagesList").appendChild(li);
});

connection.on("SendAccountInfo", function () {
    connection.invoke("MapAccountInfo", true, "Dashboard01").catch(function (err) {
        return console.error(err.toString());
    });
});

connection.on("ErrorMessage", function (result) {
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



//document.getElementById("UnsubscribeVehicleData").addEventListener("click", function (event) {
//    var InstitutionId = document.getElementById("InstitutionId").value;
//    var VehicleId = document.getElementById("VehicleId").value;
//    connection.invoke("UnsubscribeVehicleData", InstitutionId, VehicleId).catch(function (err) {
//        return console.error(err.toString());
//    });
//    event.preventDefault();
//});