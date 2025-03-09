const express = require("express");
var app = express();
var http = require('http').createServer(app);
var io = require('socket.io')(http);
const SocketHub = require("./public/javascript/sockethub");
const Logs = require("./public/javascript/Logs");
const dotenv = require('dotenv');
dotenv.config();
app.use(express.static("./public"));

//const sql = require("mssql/msnodesqlv8");
//var dateFormat = require("dateformat");
//const util = require('util');
//const path = require("path");
//const db=require("./public/javascript/db");

//var nsp = io.of('/DispatchServer');
/*const sql = require('mssql')
var config = {
  driver: 'msnodesqlv8',
  server: 'DESKTOP-C9LSU5E',
  database: 'DTS',
  user: 'sa',
  password: 'eurosoft',
  "options": {
    enableArithAbort: true,
    trustedConnection: true
    }
};*/
//app.use(express.static(__dirname + '/../../build'))

app.get('/', function(req, res){
    res.sendFile('index.html', { root: __dirname });
});

io.sockets.on('connection', function(socket) {
  Logs.CreateLog("On Connection event Called ", socket.id, "OnConnect");
  console.info(`Client connected [id=${socket.id}]`);

  //Recieving an event from the client
  socket.on('LatLong', function(data, callback) { 
    console.info(data); 
    socket.emit('Ack', 'Latitude Longitude recieved! ' + data); // return an response event
    callback('LatLong acknowledgement response....'); }); // return an event acknowledgment's callback
  socket.on('clientEvent', function(data, callback) { console.info(data); callback('clientEvent acknowledgement response....'); });

  setInterval(() => {
    socket.emit('ReceiveMessage', 'connection established message ! ');
    socket.emit('serverEvent', {name: 'test', description: 'A custom event named testerEvent!'});
   }, 5000);

  socket.on('disconnect', function(reason) {
    Logs.CreateLog("disconnect press on button : ", socket.id, "disconnect");
    SocketHub.Disconnect(socket);
    console.info(`Client gone [id=${socket.id}] reason is ${reason}`);
  });
});

http.listen(process.env.PORT, () => {
  console.info('listening on *:' + process.env.PORT);
});