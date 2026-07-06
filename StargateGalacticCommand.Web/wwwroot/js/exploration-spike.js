(function () {
    "use strict";

    var canvas = document.getElementById("exploration-canvas");
    var status = document.getElementById("exploration-status");
    if (!canvas || !window.signalR) {
        return;
    }

    var ctx = canvas.getContext("2d");
    var self = { x: 300, y: 200 };
    var others = {};

    function draw() {
        ctx.clearRect(0, 0, canvas.width, canvas.height);

        ctx.fillStyle = "#5fb3ff";
        ctx.fillRect(self.x - 8, self.y - 8, 16, 16);

        ctx.fillStyle = "#ff9f43";
        for (var id in others) {
            var p = others[id];
            ctx.fillRect(p.x - 8, p.y - 8, 16, 16);
            ctx.fillText(p.commanderName || "", p.x - 8, p.y - 12);
        }
    }

    var connection = new signalR.HubConnectionBuilder()
        .withUrl("/hubs/exploration")
        .withAutomaticReconnect()
        .build();

    connection.on("ExistingPlayers", function (players) {
        players.forEach(function (p) {
            others[p.connectionId] = p;
        });
        draw();
    });

    connection.on("PlayerJoined", function (p) {
        others[p.connectionId] = p;
        draw();
    });

    connection.on("PlayerMoved", function (p) {
        if (others[p.connectionId]) {
            others[p.connectionId].x = p.x;
            others[p.connectionId].y = p.y;
            draw();
        }
    });

    connection.on("PlayerLeft", function (p) {
        delete others[p.connectionId];
        draw();
    });

    connection.start()
        .then(function () {
            status.textContent = "Verbunden. Klicke ins Feld, um dich zu bewegen.";
            draw();
        })
        .catch(function (err) {
            status.textContent = "Verbindung fehlgeschlagen: " + err;
        });

    canvas.addEventListener("click", function (evt) {
        var rect = canvas.getBoundingClientRect();
        self.x = evt.clientX - rect.left;
        self.y = evt.clientY - rect.top;
        draw();

        if (connection.state === signalR.HubConnectionState.Connected) {
            connection.invoke("Move", self.x, self.y).catch(function (err) {
                console.error(err);
            });
        }
    });

    draw();
})();
