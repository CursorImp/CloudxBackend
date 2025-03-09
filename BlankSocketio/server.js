

const baseurl='https://server17.ctcloudxapi.com/A1TaxiMeasham/Api/DriverApp/'
const express = require("express");
var app = express();
var http = require('http').createServer(app);
const cors = require('cors');
//var io = require('socket.io')(http);
var io = require('socket.io')(http,{'pingTimeout' : 3000 , pingInterval: 3000}, {
  cors: {
    origin: '*',
  }
});

app.use(cors());
const SocketHub = require("./public/javascript/sockethub");
const Logs = require("./public/javascript/Logs");
const dotenv = require('dotenv');
const { DateTime } = require("mssql");
const { chownSync } = require("fs");
dotenv.config();
app.use(express.static("./public"));
var DictClientss = []
var clients = new Object();
var messagePool = [];

app.get('/', function(req, res){
    res.sendFile('index.html', { root: __dirname });
});

io.sockets.on('connection', function(socket) {
  
  

  try
  {
  
  var name = socket.handshake.query['name'];


  var driverId = socket.handshake.query['SignalRClientDomainId'];
  //console.log('SignalRClientDomainId: ' + driverId);
  if (driverId) {
    clients[driverId] = { "socket": socket, "sid": socket.id, "driverId": driverId };
   
    CallAPIOnConnect(driverId)

   

  }
}
catch(err)
{}

//-------------------End NewCode----------------------------------------
  //Logs.CreateLog("On Connection event Called ", name + "|" +socket.id, "OnConnect");
  
  





 
 // console.info(`Client connected [name=${name}] & [id=${socket.id}]  ----Connection Time: [${new Date()}]`);  
 
  
  
//--------------------disconnect---------------------

  socket.on('disconnect', function() {
 
  //console.info(`Client Disconnected [id=${socket.id}] reason is `);

  // for(var i=0;i<DictClientss.length;i++)
  // {
  //   if(socket.id == DictClientss[i].Valuee)
  //   {
  //     const index = DictClientss.indexOf(DictClientss[i]);
  //       if (index > -1) {
  //         DictClientss.splice(index, 1);

  //         console.log('Deleted successfully from Dict socket id: ' + socket.id);
  //       }
  //       break;
  //   }
  // }


  


     //io.emit("updatedclientlist", {VarDictClients : DictClientss});
});






//--------
  
// const userExists = DictClientss.some(DictClientss => DictClientss.Keyy === name); 
// if(userExists)
// {
// 	console.log(name);
// 	for(var i=0;i<DictClientss.length;i++)
// 	{
// 		if(name == DictClientss[i].Keyy)
// 		{
// 			const index = DictClientss.indexOf(DictClientss[i]);
// 				if (index > -1) {
// 					DictClientss.splice(index, 1);
// 				}
// 				break;
// 		}


// 	}
// 	DictClientss.push(
// 		{
// 			Keyy : name,
// 			Valuee: socket.id
// 		});
//     console.log(JSON.stringify(DictClientss));
// 	io.emit("updatedclientlist", {VarDictClients : DictClientss});
// }
// else
// {
// 	DictClientss.push(
// 		{
// 			Keyy : name,
// 			Valuee: socket.id
// 		});
//     console.log(JSON.stringify(DictClientss));
// 	io.emit("updatedclientlist", {VarDictClients : DictClientss});
// }

//-------------Group Work-----------------
  
// console.log('-------------Group Work-----------------');
 var ClientType = socket.handshake.query['type'];

if(ClientType == undefined)
{
  ClientType= "";
}
if(ClientType == "WebApp")
{            
    socket.join('WebApp Group');
}
// console.log('ClientType: '+ ClientType);  
// console.log('socket.id: '+ socket.id);  

// if(ClientType == "DriverApp")
// {            
//     socket.join('DriverApp Group');

//     console.log('socket.join("DriverApp Group")');
// }
// else if(ClientType == "WebApp")
// {            
//     socket.join('WebApp Group');

//     console.log('socket.join("WebApp Group")');
// }   
    
//-------------Send Message From Dispatch to Driver App-----------------

socket.on('ServerResponseSend', function (data)
{
 

try
{
  io.to(socket.id).emit('Ack', 'a');
   // io.to(data.fromsocketid).emit('Ack', 'ServerResponseSend recieved! ');
	
		 
    
  if(data.fromname=="WebApp")
  {
     
    socket.broadcast.to('WebApp Group').emit('ServerResponse', data);
     
  }
  else
  {
    
   
    clients[data.DriverId].socket.emit('ServerResponse', {'Id':data.Id, 'Method': data.MethodName, 'Data': data.Data });
    
  }


}
catch(err)
{

}
    
}


);

socket.on('ack',  (data)=> {

  try {
    // because JSON in in string
    //var input = JSON.parse(data);

   //  console.log("---------------------> ACKNOWLEDGE: " + data);
   // log.info(`ACK In > ${JSON.stringify(data)}`);
    //log.info(`ACK In > ${Object.keys(data)}`);
    //log.info(`ACK In > ${input.driverId}, ${input.method}`);

    // for (var i = 0; i < messagePool.length; i++) {

    //   if (messagePool[i].driverId == input.driverId && messagePool[i].method == input.method) {
    //     // log.info(`Removed ${messagePool[i].method} for driver ${messagePool[i].driverId} on ACK`);
    //     messagePool.splice(i, 1);
    //     SendToApi(data);
    //   }
    // }
    CallAPISendacknowledgementFromDriver(data);

  } catch (err) {
    // 
   // console.log(`ack error > ${err.message}`);
  }

});
  //Recieving an event from the client  {"message":"212121212","id":"0","fromsocketid":"bcUgyuxxXDwTn5mdAAAC","fromname":"209"}
 
  

  //-------------Send Location from Driver App to Server-----------------
  socket.on('sendToServer', (DriverLocationJson) => {
   io.to(socket.id).emit('Ack', 'a');
   
   try
   {
     const jsonObject = JSON.parse(DriverLocationJson);
    if(jsonObject.Method=="SendLocationData")
    {
    
      const latLongArray = jsonObject.Data.LatLong.split('=');
      
      const drvId = latLongArray[5];
    
      clients[drvId] = { "socket": socket, "sid": socket.id, "driverId": drvId };
     // console.info( "SendLocationData");
    }
    
  }
  

 catch (err) {
  // 
 // console.info(err.message);
}

    CallAPISendDriverLocation(DriverLocationJson);    
  
  });
  //-------------API Call Work-----------------
});





http.listen(process.env.PORT, () => {
 // console.info('listening on *:' + process.env.PORT);
});
function SendToApi(request)
{
  try
  {
      let StatusCode = "";
      //---------------------------axios module------------------------------
      const axios = require('axios').default;
     
    // console.log("---------------------> DriverLocationJson: " + request);
      const sendPostRequest = async () => {
          try {
             
              const resp = await axios.post(baseurl+ 'Sendacknowledgement?request=' + request);
              
              //console.log('1.5 WAITING AREA----------------------------------------------CODE STATUS: '+ resp.status);
              StatusCode =`Status: ${resp.status}`;
          //    console.log(StatusCode);
              //console.log(resp.data);
          } catch (err) {
              // Handle Error Here
        //      console.error(err);
          }
      };
      sendPostRequest();
  }
  catch(err)
  {
     // console.log(err);
  }
}
function CallAPISendDriverLocation(ReqJSon)
{
    try
    {
        let StatusCode = "";
        //---------------------------axios module------------------------------
        const axios = require('axios').default;
        const newPost = ReqJSon;
     //  console.log("---------------------> DriverLocationJson: " + ReqJSon);
        const sendPostRequest = async () => {
            try {
            
                const resp = await axios.post(baseurl+ 'SendLocationDataApi?request=' + ReqJSon);
                
                //console.log('1.5 WAITING AREA----------------------------------------------CODE STATUS: '+ resp.status);
                StatusCode =`Status: ${resp.status}`;
              //  console.log(StatusCode);
                //console.log(resp.data);
            } catch (err) {
                // Handle Error Here
            //    console.error(err);
            }
        };
        sendPostRequest();
    }
    catch(err)
    {
     //   console.log(err);
    }
}


function CallAPISendacknowledgementFromDriver(ReqJSon)
{
    try
    {
        let StatusCode = "";
        //---------------------------axios module------------------------------
        const axios = require('axios').default;
        const newPost = ReqJSon;
       //console.log("---------------------> DriverLocationJson: " + ReqJSon);
        const sendPostRequest = async () => {
            try {
            
                const resp = await axios.post(baseurl+ 'Sendacknowledgement?request=' + ReqJSon);
                
                //console.log('1.5 WAITING AREA----------------------------------------------CODE STATUS: '+ resp.status);
             //   StatusCode =`Status: ${resp.status}`;
             //   console.log(StatusCode);
                //console.log(resp.data);
            } catch (err) {
                // Handle Error Here
             //   console.error(err);
            }
        };
        sendPostRequest();
    }
    catch(err)
    {
       // console.log(err);
    }
}


function CallAPIOnConnect(ReqJSon)
{
    try
    {
        let StatusCode = "";
        //---------------------------axios module------------------------------
        const axios = require('axios').default;
        const newPost = ReqJSon;
      // console.log("---------------------> CallAPIOnConnect: " + ReqJSon);
       
        const sendPostRequest = async () => {
            try {
            
                const resp = await axios.post(baseurl+ 'OnConnect?request=' + ReqJSon);
                
                //console.log('1.5 WAITING AREA----------------------------------------------CODE STATUS: '+ resp.status);
                StatusCode =`Status: ${resp.status}`;
            //    console.log(StatusCode);
                //console.log(resp.data);
            } catch (err) {
                // Handle Error Here
           //     console.error(err);
            }
        };
        sendPostRequest();
    }
    catch(err)
    {
       // console.log(err);
    }
}

















