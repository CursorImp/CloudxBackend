const express = require("express");
const app = express();
const http = require("http").createServer(app);
const io = require("socket.io")(http);
const SocketHub = require("./sockethub");
const util = require('util');


class Method {

  ReturnIntValueByList(list) {
    var listcount = [];
    var count = 0;
    listcount.push(list);

    for (var i = 0; i < listcount.lenght; i++) {
      count = count + count[i];
    }

    return count;
  }
 provideSingleKey(obj) {
    for (var key in obj) {
      if (obj.hasOwnProperty(key)) {
        return key;
      }
    }
  }
  provideSingleKey(obj,keylength) {
    for (var key in obj) {
      if (obj.hasOwnProperty(key)) {
        var _key=key.substring(keylength);

        return _key;
      }
    }
  }
  ReturnValueByKey(object, key) {
    return Object.values(object).find(val => object[val] === key);
  }
  serialize(obj) {
    var serialized = {};

    for (var prop in obj) {
      if (obj.hasOwnProperty(prop) && typeof obj[prop] == "function") {
        if (/^get.*/.test(prop)) {
          var value = obj[prop]();
          var name = prop.replace("get", "");

          if (typeof value === "object") {
            serialized[name] = this.serialize(value);
            continue;
          }

          serialized[name] = value;
        }
      }
    }

    return serialized;
  }
  //shiftLogin
  shiftLogin(msg)
  {
    console.log('shiftLogin: >  '+msg)
    var socket=io;
    socket.on("ShiftLogin", msg => {
      socket.broadcast.emit('ShiftLogin', { receivers: msg});
      Logs.CreateLog("On Connection Called on BoardCastMessage \n\n"+msg, socket.id, "OnConnect");
    
    });
  }
    BoardCastMessage(msg)
    {
      var sockets=io();
      socket.on("BoardCastMessage", msg => {
        socket.broadcast.emit('BoardCastMessage', { receivers: msg});
        Logs.CreateLog("On Connection Called on BoardCastMessage \n\n"+msg, socket.id, "OnConnect");
      
      });
    }
despatchBooking(msg)
{
  var sockets=io();
    
  socket.on("despatchBooking", msg => {
    socket.broadcast.emit('despatchBooking', { receivers: msg});
    Logs.CreateLog("On Connection Called on BoardCastMessage \n\n"+msg, socket.id, "OnConnect");
  
  });

}
despatchBooking(drvId,msg)
{ var sockets=io();

var listOfConnections = [];
        listOfConnections = SocketHub.ReturnDriverConnections(
          parseInt(drvId)
        );
        for(var i=0;i<listOfConnections.lenght;i++)
        {
           io.to(listOfConnections[i].DomainId).emit(
          "despatchBooking",
          msg
        );
        }
       
}
SendToConnection_bydriverid(drvId)
{ var sockets=io();

var listOfConnections = [];
        listOfConnections = SocketHub.ReturnDriverConnections(
          parseInt(drvId)
        );
        for(var i=0;i<listOfConnections.lenght;i++)
        {
           io.to(listOfConnections[i].DomainId).emit(
          "despatchBooking",
          listofJobs.find(c => c.DriverId == drvId).JobMessage
        );
        }
       
}
forceClearJob(drvId)
{ var sockets=io();

var listOfConnections = [];
        listOfConnections = SocketHub.ReturnDriverConnections(
          parseInt(drvId)
        );
        for(var i=0;i<listOfConnections.lenght;i++)
        {
          var JobMessage= listofJobs.find(c => c.DriverId == drvId).JobMessage;
           io.to(listOfConnections[i].DomainId).emit(
          "forceClearJob",
         JobMessage
        );
        }
       
}
CallBackLog_(m)
{ var socket=io();
   socket.on("LatLong", function(data, callback) {
    // console.log('Socket (server-side): received message:', data);
    var responseData =
      "Message received Callback: " + callback + " data: " + data;
    var res = {
      ack: "received"
    };
  
    console.log(res+'\n\n '+m);

   
    callback(res);
    
  });
}
CallBackLog()
{ var socket=io();
   socket.on("LatLong", function(data, callback) {
    // console.log('Socket (server-side): received message:', data);
    var responseData =
      "Message received Callback: " + callback + " data: " + data;
    var res = {
      ack: "received"
    };
  
    console.log(res);

   
    callback(res);
    
  });
}

cMessageToDesktop(msg)
{

var listOfConnections = [];
        listOfConnections = SocketHub.ReturnDriverConnections(
          parseInt(drvId)
        );
        
           
           io.to(listOfConnections[i].DomainId).emit(
          "cMessageToDesktop",
          msg
        );
        
       
}
bidAlert(drvId)
{

var listOfConnections = [];
        listOfConnections = SocketHub.ReturnDriverConnections(
          parseInt(drvId)
        );
        for(var i=0;i<listOfConnections.lenght;i++)
        {
           var JobMessage= listofJobs.find(c => c.DriverId == drvId).JobMessage;
           io.to(listOfConnections[i].DomainId).emit(
          "bidAlert",
          JobMessage
        );
        }
       
}
updateJob(drvId)
{

var listOfConnections = [];
        listOfConnections = SocketHub.ReturnDriverConnections(
          parseInt(drvId)
        );
        for(var i=0;i<listOfConnections.lenght;i++)
        {
           var JobMessage= listofJobs.find(c => c.DriverId == drvId).JobMessage;
           io.to(listOfConnections[i].DomainId).emit(
          "updateJob",
          JobMessage
        );
        }
       
}

forceRecoverJob(drvId,msg)
{

var listOfConnections = [];
        listOfConnections = SocketHub.ReturnDriverConnections(
          parseInt(drvId)
        );
        for(var i=0;i<listOfConnections.lenght;i++)
        {
          
           io.to(listOfConnections[i].DomainId).emit(
          "forceRecoverJob",
          msg
        );
        }
       
}
updateSetting(drvId,msg)
{

var listOfConnections = [];
        listOfConnections = SocketHub.ReturnDriverConnections(
          parseInt(drvId)
        );
        for(var i=0;i<listOfConnections.lenght;i++)
        {
          
           io.to(listOfConnections[i].DomainId).emit(
          "updateSetting",
          msg
        );
        }
       
}
updateJob(drvId,msg)
{

var listOfConnections = [];
        listOfConnections = SocketHub.ReturnDriverConnections(
          parseInt(drvId)
        );
        for(var i=0;i<listOfConnections.lenght;i++)
        {
          
           io.to(listOfConnections[i].DomainId).emit(
          "updateJob",
          msg
        );
        }
       
}

sendMessage(drvId,msg)
{

var listOfConnections = [];
        listOfConnections = SocketHub.ReturnDriverConnections(
          parseInt(drvId)
        );
        for(var i=0;i<listOfConnections.lenght;i++)
        { 
           io.to(listOfConnections[i].DomainId).emit(
          "sendMessage",
        msg
        );
        }
       
}
sendMessage(drvId)
{

var listOfConnections = [];
        listOfConnections = SocketHub.ReturnDriverConnections(
          parseInt(drvId)
        );
        for(var i=0;i<listOfConnections.lenght;i++)
        { var JobMessage= listofJobs.find(c => c.DriverId == drvId).JobMessage;
           io.to(listOfConnections[i].DomainId).emit(
          "sendMessage",
          JobMessage
        );
        }
       
}

SendToConnection_despatchBooking_bydriverid(drvId)
{

var listOfConnections = [];
        listOfConnections = SocketHub.ReturnDriverConnections(
          parseInt(drvId)
        );
        for(var i=0;i<listOfConnections.lenght;i++)
        {
           var JobMessage= listofJobs.find(c => c.DriverId == drvId).JobMessage;
           io.to(listOfConnections[i].DomainId).emit(
          "despatchBooking",
         JobMessage
        );
        }
       
}
CountArray(words)
{
  var count=0;
for(var i=0;i<words.length;i++)
{
  count++;
}
return count;
}

 
}

module.exports = new Method();
