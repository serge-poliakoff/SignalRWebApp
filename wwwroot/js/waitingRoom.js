"use strict";

var connection = new signalR.HubConnectionBuilder().withUrl("/groups").build();

//group managment
connection.on("AddGroup", function (group) {
    var li = document.createElement("li");
    li.id = `group-select-${group}`;
    var enterBut = document.createElement("button");
    enterBut.addEventListener("click", () => {
        connection.invoke("EnterGroup", group)
            .then(constructGroupBlock(group))
            .catch((err) => {
                alert(`An error occured while trying to join ${group}\nTry again later`);
                return console.error(err.toString());
        });
        
    });
    enterBut.innerText = `Join ${group}`
    enterBut.style.margin = "10px";
    enterBut.className = "join-group-button"

    document.getElementById("groupsList").appendChild(li);
    li.textContent = `${group}`;
    li.appendChild(enterBut);
});

connection.on("DropGroup", (group) => {
    document.getElementById(`group-select-${group}`).remove();
});

connection.start().then(function () {
    document.getElementById("groupsList").style.background = "#eeeeee";
}).catch(function (err) {
    return console.error(err.toString());
});

function quitGroup() {
    connection.invoke("QuitGroup").then(() => {
        removeGroupBlock();
    }).catch((err) => {
            alert("An error procured during disconnecting: " + err.toString());
    });
}

function addNewGroup() {
    var groupName = document.getElementById("groupName-input").value;
    if (groupName == '' || !groupName) {
        alert("You must give group a name");
        return;
    }
    connection.invoke("AddNewGroup", groupName).then(() => constructGroupBlock(groupName))
        .catch((err) => {
            alert("An error raised while adding the group " + groupName);
            removeGroupBlock();
        });
}


//table events
var cells = document.getElementsByClassName("clickable_cell");
for (let i = 0; i < cells.length; i++) {
    cells[i].addEventListener("click", function (event) {
        var color = document.getElementById("col-input").value;
        color = !color ? "pink" : color;
        colorCellById(event.target.id, color);
        connection.invoke("ColorCell", event.target.id, color).catch(
            function (err) {
                alert("An error raised while trying to color cell " + err.toString());
            }
        );
        //sendNamedMessage("colored cell " + event.target.id);
    });
}

connection.on("ColorTable", (id,color) => colorCellById(id, color));

function colorCellById(id, color) {
    document.getElementById(id).style.background = color;
}

function recolorTable() {
    var cells = document.getElementsByClassName("clickable_cell");
    for (let i = 0; i < cells.length; i++) {
        cells[i].style.background = "white";
    }
}

function constructGroupBlock(groupName) {
    var playarea = document.getElementById("play-area");
    playarea.hidden = false;
    playarea.querySelector("h3").textContent = groupName;
    var join_bts = document.getElementsByClassName("join-group-button");
    for (let i = 0; i < join_bts.length; i++) {
        join_bts[i].disabled = true;
    }
}

function removeGroupBlock() {
    document.getElementById("play-area").hidden = true;
    let join_bts = document.getElementsByClassName("join-group-button");
    for (let i = 0; i < join_bts.length; i++) {
        join_bts[i].disabled = false;
    }
    recolorTable();
}