"use strict";

var connection = new signalR.HubConnectionBuilder().withUrl("http://localhost:55202/trackServiceHub").build();

connection.on("ReceiveAllInstitutionVehicleData", function (result) {
    var li = document.createElement("li");
    li.textContent = result;
    document.getElementById("messagesList").appendChild(li);
});

connection.on("ReceiveSubscribedVehicleData", function (result) {
    var li = document.createElement("li");
    li.textContent = result;
    document.getElementById("messagesList").appendChild(li);
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
            var functionName = document.getElementById("SubscribedFunction").value;
            var value = document.getElementById("SubscribedValue").value;
            if (functionName == 'SendAllVehicleDataForInstitutionId') {
                connection.invoke("SendAllVehicleDataForInstitutionId", value)
            }
            if (functionName == 'SendSubscribedVehicleData') {
                connection.invoke("SendSubscribedVehicleData", value)
            }
        }).catch(function (err) {
            return console.error(err.toString());
        });
    }
}

setInterval(restartConnectionIfStopped, 10000);

connection.start().then(function () {
    
}).catch(function (err) {
    return console.error(err.toString());
});

document.getElementById("SendAllVehicleDataForInstitutionId").addEventListener("click", function (event) {
    var InstitutionId = document.getElementById("InstitutionId").value;
    connection.invoke("SendAllVehicleDataForInstitutionId", InstitutionId).catch(function (err) {
        return console.error(err.toString());
    });
    $('#SubscribedFunction').val('SendAllVehicleDataForInstitutionId');
    $('#SubscribedValue').val(InstitutionId);
    event.preventDefault();
});

document.getElementById("SendSubscribedVehicleData").addEventListener("click", function (event) {
    var VehicleId = document.getElementById("VehicleId").value;
    connection.invoke("SendSubscribedVehicleData", VehicleId).catch(function (err) {
        return console.error(err.toString());
    });
    $('#SubscribedFunction').val('SendSubscribedVehicleData');
    $('#SubscribedValue').val(VehicleId);
    event.preventDefault();
});

document.getElementById("UnsubscribeVehicleData").addEventListener("click", function (event) {
    connection.invoke("UnsubscribeVehicleData").catch(function (err) {
        return console.error(err.toString());
    });
    $('#SubscribedFunction').val('');
    $('#SubscribedValue').val('');
    event.preventDefault();
});