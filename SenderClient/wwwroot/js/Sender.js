"use strict";

var connection = new signalR.HubConnectionBuilder().withUrl("http://localhost:55202/trackServiceHub").build();

//Disable send button until connection is established
document.getElementById("sendButton").disabled = true;

connection.start().then(function () {
    document.getElementById("sendButton").disabled = false;
}).catch(function (err) {
    return console.error(err.toString());
});

connection.on("ErrorMessage", function (result) {
    alert(result);
});

connection.onclose(e => {
    console.log('{ "code": "108", "message": "Server connection failed!" }')
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

document.getElementById("sendButton").addEventListener("click", function (event) {
    var Latitude = document.getElementById("Latitude").value;
    var Longitude = document.getElementById("Longitude").value;
    var VehicleId = document.getElementById("VehicleId").value;
    var Institution = document.getElementById("Institution").value;
    connection.invoke("SendLocation", VehicleId, Longitude, Latitude, Institution).catch(function (err) {
        return console.error(err.toString());
    });
    event.preventDefault();
});