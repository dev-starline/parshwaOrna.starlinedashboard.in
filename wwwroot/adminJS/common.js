/*const socketUrl = window.location.hostname+':10001';*/
const url = 'http://localhost:1001';
const user = $('.userName').text();
const connection = new signalR.HubConnectionBuilder()
    .withUrl(`${url}/bullion?user=${user}&auth=sl@123&type=web`, { transport: signalR.HttpTransportType.WebSockets, skipNegotiation: true })
    .withAutomaticReconnect()
    .build();




async function startConnection() {
    try {
        await connection.start();
        connection.invoke("client", user);
        connection.invoke("admin", user);
    } catch (err) {
        console.error("Connection failed: ", err);
        setTimeout(startConnection, 5000);
    }
}
connection.onclose(async () => {
    await startConnection();
});
connection.on("activeUsers", (count) => {
    $('.activeUsers').text(count);
});
startConnection();

connection.on('adminAlertDetails', function (data) {
  
    if (Notification.permission === 'granted') {
        showNotification(data);
    } else if (Notification.permission !== 'denied') {
        Notification.requestPermission().then(permission => {
            if (permission === 'granted') {
          
                showNotification(data);
            }
        });
    }
});
function showNotification(data) {

    let notification = new Notification(data.user, {
        icon: window.location.origin + '/img/logo-dark.png',
        body: data.message,
    });

    notification.onclick = function () {
        window.open(window.location.origin + '/OpenOrder', '_blank');
    };
}


connection.on('orderFailure', function (data) {

    console.log("orderFailure", data);
    debugger;

    if (Notification.permission === 'granted') {
        showOrderFailureNotification(data);
    }
    else if (Notification.permission !== 'denied') {
        Notification.requestPermission().then(permission => {
            console.log(permission);

            if (permission === 'granted') {
                showOrderFailureNotification(data);
            }
        });
    }
});

function showOrderFailureNotification(data) {

    let notification = new Notification("Order Failure", {
        icon: window.location.origin + '/img/logo-dark.png',
        body: data.message
    });

    notification.onclick = function () {
        window.focus();
        window.location.href = window.location.origin + '/OrderLog/List';
    };
}



function ajaxPost(url, data) {
    var Response
    $.ajax({
        type: 'POST',
        dataType: 'json',
        contentType: 'application/json; charset=utf-8',
        url: url,
        data: data,
        async: false,
        success: function (response) {
            Response = response;
        },
        error: function (xhr, textStatus, error) {
            console.log("Error: " + error);
            Response = error;
        }
    });
    return Response;
}
function ajaxGet(url) {
    var Response
    $.ajax({
        type: 'GET',
        dataType: 'json',
        contentType: 'application/json; charset=utf-8',
        url: url,
        async: false,
        success: function (response) {
            Response = response;
        },
        error: function (xhr, textStatus, error) {
            console.log("Error: " + error);
            Response = error;
        }
    });
    return Response;
}

$("body").on("click", ".ajax-model", async function () {
    var divElement = $("#modelPopup");
    var url = $(this).data('url');
    var decodeUrl = decodeURIComponent(url);

    $.get(decodeUrl)
        .done(function (data) {
            divElement.html(data);
            var editModal = divElement.find('#editModal');
            if (editModal.length > 0) {
                editModal.modal('show');
            } else {
                console.log('Modal element not found');
            }
        })
        .fail(function (xhr, status, error) {
            console.error(status + " : " + error);
        });
})
$("body").on("click", ".ajax-model-master", async function () {
    var divElement = $("#modelPopup");
    var url = $(this).data('url');
    var decodeUrl = decodeURIComponent(url);
    var id = parseInt($(this).attr("id"));
    $.get(decodeUrl)
        .done(function (data) {
            divElement.html(data);
            var editModal = divElement.find('#editModal');
            if (editModal.length > 0) {
                editModal.modal('show');
                getReferance(id);
            } else {
                console.log('Modal element not found');
            }
        })
        .fail(function (xhr, status, error) {
            console.error(status + " : " + error);
        });
})
function confirmDelete(type) {
    var change = "";
    if (type == 'open') {
        change = type;
    }
    else {
        change = 'delete';
    }
    return confirm('Are you sure you want to ' + change +' this item?');
}