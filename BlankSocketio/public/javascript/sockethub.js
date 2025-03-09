var connections = [];
var listofJobs = [];
const objPolicy = null;
var listofPolyVertices = [];
var listOfZone = [];
var currentBalance = [];
var _objDriver=[];
var objFleet=[];
var msg ="true";
var objFleet_Fleet_DriverQueueLists=[];
const LoginDrvOnExpiredDoc=false;
const express = require("express");
const app = express();
const http = require("http").createServer(app);
const io = require("socket.io")(http);
const util = require('util');
const ClientData = require("./clientdata");
const Logs = require("./Logs");
const db = require("./db");
const Method = require("./Method");

//Method.js
//JobMeter
const SignalRClientsType = {
    WEBBROWSER: 1,
    DESKTOP: 2,
    ANDROID: 3,
    IOS: 4,
    ANONYMOUS: 5
};

const SignalRClientsStatus = {
    CONNECTED: 1,
    DISCONNECTED: 2,
    RECONNECTING: 3
};

const SignalRUserType = {
    WEB: 1,
    DESKTOP: 2,
    DRIVER: 3,
    CLIENT: 4
};
const eMessageTypes = {
    JOB: 1,
    RECALLJOB: 2,
    CLEAREDJOB: 3,
    MESSAGING: 4,
    AUTHORIZATION: 5,
    BIDALERT: 6,
    UPDATEPLOT: 7,
    UPDATEJOB: 8,
    ONBIDDESPATCH: 9,
    LOGOUTAUTHORIZATION: 10,
    FORCE_ACTION_BUTTON: 11,
    UPDATE_SETTINGS: 12,
    BIDPRICEALERT: 13,
    PLANJOB: 14
};
const BIDDING_TYPES = {
    FASTEST_FINGER: 0,
    NEAREST_DRIVER: 0,
    LONGEST_WAITING_QUEUE: 0
};
const AUTODESPATCH_TYPES = {
    TOP_STANDING_QUEUE: 0,
    TOP_STANDING_QUEUE_NEAREST_DRIVER: 0,
    NEAREST_DRIVER: 0,
    LONGEST_WAITING_QUEUE: 0,
    NEAREST_DRIVER_LONGEST_WAITING_QUEUE: 0
};
const SMSACCOUNT_TYPE = {
    BULKSMS: 0,
    CLICKATELL: 0,
    NONE: 0,
    MODEMSMS: 0,
    CLICKATELLANDMODEMSMS: 0
};
const MAP_TYPE = {
    GOOGLEMAPS: 0,
    MAPPOlet: 0,
    NONE: 0
};
const SYSGEN_COMPANY = {
    COMCAB: 0,
    CREDITCARD: 0,
    PAYPAL: 0,
    KARHOO: 0,
    KABBEE: 0
};
const GEN_DOCUMENTS = {
    COMMISIONTRANSACTIONNO: 0,
    BOOKINGNO: 0,
    INVOICENO: 0,
    ACC_PRE_INVOICENO: 0,
    CUSTOMER_PRE_INVOICENO: 0,
    ESCORT_INVOICENO: 0,
    COMPLAINNO: 0,
    TRANSACTIONNO: 0,
    LOSEPROPERTY: 0,
    GROUPJOB: 0
};
const LOCATION_TYPENAMES = {
    ADDRESS: 0,
    POSTCODE: 0,
    STATION: 0,
    STATIONS: 0,
    RAILWAYSTATION: 0,
    SEAPORTS: 0,
    SEABPORTS: 0,
    CRUISEPORT: 0,
    CRUISEPORTS: 0,
    AIRPORT: 0
};
const JOURNEY_TYPES = {
    ONEWAY: 0,
    RETURN: 0,
    WAITANDRETURN: 0
};
const Driver_WORKINGSTATUS = {
    AVAILABLE: 0,
    NOTAVAILABLE: 0,
    ONBREAK: 0,
    ONROUTE: 0,
    SOONTOCLEAR: 0,
    ARRIVED: 0,
    SINBIN: 0,
    FOJ: 0
};
const BOOKING_TYPES = {
    LOCAL: 0,
    WEB: 0,
    ONROAD: 0,
    VIP: 0,
    ONLINE: 0,
    OUTOFTOWN: 0,
    SHUTTLE: 0,
    COURIER: 0,
    ONBIDDING: 0,
    IMPORT: 0,
    THIRDPARTY: 0,
    IVR: 0
};
const ANALOG_CLITYPE = {
    ETHERNET: 0,
    SERIALPORT: 0
};
const INVOICE_TYPE = {
    ACCOUNT: 0,
    CUSTOMER: 0,
    PRE_INVOICE_ACCOUNT: 0,
    PRE_INVOICE_CUSTOMER: 0,
    ESCORT_INVOICE: 0
};
const ACCOUNT_TYPE = {
    ACCOUNT: 0,
    CASH: 0
};
const INVOICE_PAYMENTTYPES = {
    UNPAID: 0,
    HALFPAID: 0,
    FULLPAID: 0,
    CUSTOM: 0
};
const LOCATION_TYPES = {
    AIRPORT: 0,
    UNIVERSITIES: 0,
    SCHOOLS: 0,
    PHARMACIES: 0,
    SPORTCENTER: 0,
    HOSPITAL: 0,
    HOTELS: 0,
    SEAPORTS: 0,
    STORE: 0,
    ADDRESS: 0,
    TOWN: 0,
    RESTAURANT: 0,
    CLUBBAR: 0,
    THEATRE: 0,
    UNDERGROUNDSTATION: 0,
    POSTCODE: 0,
    BASE: 0
};
const BOOKING_TRIPSTATUS = {
    WAITING: 0,
    INPROGRESS: 0,
    COMPLETED: 0,
    CANCELLED: 0
};
const PAYMENT_GATEWAY = {
    CARDSAVE: 0,
    PAYPAL: 0,
    WORLDPAY: 0,
    BARCLAY: 0,
    ATLANTE_CONNECTPAY: 0,
    JUDO: 0,
    STRIPE: 0
};
const DRIVERTYPES = {
    RENT: 0,
    COMMISSION: 0
};
const ZONE_KIND = {
    INNER: 0,
    OUTER: 0
};
const TRANSACTIONTYPE_DRIVERRENT = {
    EXPENSE1: 0,
    EXPENSE2: 0,
    EXPENSE3: 0
};
const TRANSACTIONTYPE = {
    DRIVER_COMMISSION_EXPENSE1: 0,
    DRIVER_COMMISSION_EXPENSE2: 0,
    DRIVER_COMMISSION_EXPENSE3: 0,
    DRIVER_COMMISSION_EXPENSE4: 0,
    DRIVER_COMMISSION_EXPENSE5: 0,
    DRIVER_RENT_EXPENSE1: 0,
    DRIVER_RENT_EXPENSE2: 0,
    DRIVER_RENT_EXPENSE3: 0
};
const DRIVER_DOCUMENTS = {
    PCOVehicle: 0,
    PCODriver: 0,
    MOT: 0,
    Insurance: 0,
    MOT2: 0,
    LICENSE: 0,
    ROADTAX: 0
};
const PAYMENTCOLUMNS = {
    ShowFares: 0,
    ShowTip: 0,
    ShowSurcharge: 0,
    ShowTotal: 0,
    AirportPickup: 0,
    AirportDropoff: 0
};
const PAYMENT_CHARGESTYPE = {
    CHARGESTYPE1: 0,
    CHARGESTYPE2: 0,
    CHARGESTYPE3: 0,
    CHARGESTYPE4: 0,
    CHARGESTYPE5: 0
};
const UM_FUNCTIONS = {
    BOOKING_MANDATORY_CUSTOMERCONTACTNO: 0
};
const PAYMENT_TYPES = {
    CASH: 0,
    CREDIT_CARD: 0,
    BANK_ACCOUNT: 0,
    PAYPAL: 0,
    TAXICARD: 0,
    CREDIT_CARD_PAID: 0,
    ROOM_CHARGE: 0
};
const BOOKINGSTATUS = {
    WAITING: 0,
    WAITING_WEBBOOKING: 0,
    PENDING_WEBBOOKING: 0,
    PENDING_START: 0,
    FOJ: 0,
    BID: 0,
    ONHOLD: 0,
    NOPICKUP: 0,
    NOTACCEPTED: 0,
    REJECTED_WEBBOOKING: 0,
    REJECTED: 0,
    COMPLETED: 0,
    STC: 0,
    POB: 0,
    ARRIVED: 0,
    ONROUTE: 0,
    PENDING: 0,
    CANCELLED: 0,
    DISPATCHED: 0,
    NOSHOW: 0,
    ARRIVED_GHOST: 0
};

///---------------------------------------- [ Start Class ] --------------------------------------------------///

class SocketHub {
    Getobj(res)
    {
      return  util.inspect(res, false, null, true /* enable colors */);
    }
    removeItemAll(arr, value) {
        var i = 0;
        while (i < arr.length) {
            if (arr[i] === value) {
                arr.splice(i, 1);
            } else {
                ++i;
            }
        }
        return arr;
    }
    removeItemOnce(arr, value) {
        var index = arr.indexOf(value);
        if (index > -1) {
            arr.splice(index, 1);
        }
        return arr;
    }
    getParameterByName(name, url) {
        name = name.replace(/[\[\]]/g, "\\$&");
        var regex = new RegExp("[?&]" + name + "(=([^&#]*)|&|#|$)"),
        results = regex.exec(url);
        if (!results)
            return null;
        if (!results[2])
            return "";
        return decodeURIComponent(results[2].replace(/\+/g, " "));
    }

    addProperty(myKey, myValue) {
        if (!connections.some(e => e.Key == myKey && e.Value == myValue)) {
            connections[myKey] = myValue;
        }
    }

    addSignalRClient(socket) {
        try {
            ClientData.ConnectedOn = datetime;
            ClientData.connectionstatus = SignalRClientsStatus.CONNECTED;
            ClientData.ConnectionID = socket.id;
            //  console.log('new user login');
            var qs = this.getParameterByName(
                    "SignalRClientsType",
                    socket.handshake.url);
            var qsUserType = this.getParameterByName(
                    "SignalRUserType",
                    socket.handshake.url);
            var qsID = this.getParameterByName(
                    "SignalRClientDomainId",
                    socket.handshake.url);
            var datetime = new Date();
            ClientData.UserId = qsID;

            if (qs == null) {
                ClientData.ConnectionType = SignalRClientsType.ANONYMOUS;
            } else {
                if (parseInt(qs) > 0) {
                    let ClientTypeID = qs;
                    if (ClientData.ConnectionType == ClientTypeID) {
                        ClientData.ConnectionType = SignalRClientsType.ANDROID;
                    } else if (ClientData.ConnectionType == ClientTypeID) {
                        ClientData.ConnectionType = SignalRClientsType.WEBBROWSER;
                    } else if (ClientData.ConnectionType == ClientTypeID) {
                        ClientData.ConnectionType = SignalRClientsType.IOS;
                    } else if (ClientData.ConnectionType == ClientTypeID) {
                        ClientData.ConnectionType = SignalRClientsType.DESKTOP;
                    }
                } else {
                    ClientData.ConnectionType = SignalRClientsType.ANONYMOUS;
                }
            }
            switch (parseInt(qsUserType)) {
            case 1:
                ClientData.UserType = SignalRUserType.WEB;
                break;
            case 2:
                ClientData.UserType = SignalRUserType.DESKTOP;
                break;
            case 3:
                ClientData.UserType = SignalRUserType.DRIVER;
                break;
            case 4:
                ClientData.UserType = SignalRUserType.CLIENT;
                break;
            default:
                break;
            }
            if (qsID == null || qsID == "") {
                ClientData.UserId = null;
            } else {
                ClientData.UserId = parseInt(qsID);
            }

            //---- Save object in Client Data then Client Data obj push in connection array
            connections.push(ClientData);

            //-----------------------------------------------------
        } catch (error) {
            //console.log("addSignalRClient Error: " + error);
            Logs.CreateLog("addSignalRClient Error: ", socket.id, "addSignalRClient");
        }
    }

    Connect(socket) {
        var client = function (id) {
            if (connections[id] != null)
                return connections[id].Value;
            else
                return null;
        };

        var oldClient = function (id) {
            return connections[id];
        };

        var isNewConnection = true;
        var AllExists = [];
        if (socket != null) {
            console.log("Connect Socket have data");
            var _client = client(socket.id);
            let UserIdInConnection = this.getParameterByName(
                    "SignalRClientDomainId",
                    socket.handshake.url);
            // console.log(UserIdInConnection);
            var id = socket.id;

            if (
                UserIdInConnection != 0 &&
                this.getParameterByName("SignalRUserType", socket.handshake.url) != null) {
                // console.log("UserIdInConnection: " + UserIdInConnection);
                try {
                    //  console.log("connections -- : " + connections.length);

                    for (var i = 0; i < connections.length; i++) {
                        if (
                            parseInt(connections[i].UserId) === parseInt(UserIdInConnection)) {
                            try {
                                connections.splice(i, 1);
                                Logs.CreateLog("Connection deleted ", socket.id, "Connect");
                            } catch (error) {
                                //console.log("error : " + error);
                                Logs.CreateLog(
                                    "Connection deleted Error:  " + error,
                                    socket.id,
                                    "Connect");
                            }
                        } else {
                            Logs.CreateLog("UserId- is not found  ", socket.id, "Connect");
                        }

                        //;
                    }
                } catch (error) {
                    Logs.CreateLog("User logout Error: " + error, socket.id, "Connect");
                }
            }

            for (var i = 0; i < connections.length; i++) {
                if (connections[i].ConnectionID === socket.id) {
                    console.log(
                        "Connection is Already Exists :" + connections[i].ConnectionID);

                    isNewConnection = false;
                    break;
                }

                //;
            }

            if (isNewConnection) {
                this.addSignalRClient(socket);
                Logs.CreateLog(
                    "connection after added length: (" + connections.length + ")",
                    socket.id,
                    "Connect");
            }
        }
    }

    Disconnect(socket) {
        for (var i = 0; i < connections.length; i++) {
            if (connections[i].ConnectionID == socket.id) {
                connections.splice(i, 1);
                Logs.CreateLog("connections count after deleted  : " + connections.length, socket.id, "Connect");
                break;
            }
        }
        // }
    }

    updaterc(socket, str) {
        console.log("socket: " + socket + " str: " + str);
    }
    Updatesignalrclient(socket, domainId) {
        if (socket != null) {
            for (var i = 0; i < connections.length; i++) {
                if (connections[i].ConnectionID == socket.id) {
                    connections.push(ClientData);

                    Logs.CreateLog(
                        "connections count after deleted  : " + connections.length,
                        socket.id,
                        "Connect");

                    break;
                }
            }
        }
    }

    RegisterClient(conId, domainId) {
        if (connections != null) {
            try {
                // SignalRClient client = Connections.Where(x => x.Value.ConnectionID == conId).FirstOrDefault().Value;
                // client.DomainId = domainId;
                // Connections.TryUpdate(conId, client, Connections[client.ConnectionID]);

                for (var i = 0; i < connections.length; i++) {
                    if (connections[i].ConnectionID == conId) {
                        //connections.splice(i, 1);

                        connections.push(connections[i].ConnectionID);
                        // console.log("connections count after deleted " + connections.length);
                        Logs.CreateLog(
                            "connections count after deleted  : " + connections.length,
                            conId,
                            "Connect");

                        //     console.log('connections count after deleted '+ connections[i].ConnectionID);
                        break;
                    }
                }
            } catch (error) {
                Logs.CreateLog(
                    "RegisterClient Error  : " + error,
                    conId,
                    "RegisterClient");

                ////////////log.Error(ex.Message, ex);
            }
        }
    }
    ReturnDriverConnections(driverID) {
        var DriverConnectionIDs = [];
        if (driverID == 0) {
            for (var i = 0; i < connections.length; i++) {
                if (connections[i].UserType == SignalRUserType.DRIVER) {
                    DriverConnectionIDs.push(connections);
                    break;
                }
            }

            // DriverConnectionIDs = Connections.Where(x => x.Value.UserType == SignalRUserType.DRIVER).Select(x => x.Key).ToList();
        } else {
            for (var i = 0; i < connections.length; i++) {
                if (
                    connections[i].UserType == SignalRUserType.DRIVER &&
                    connections[i].DomainId == driverID) {
                    DriverConnectionIDs.push(connections);
                }
            }
        }
        return DriverConnectionIDs;
    }

    ReturnConnections(connectionID) {
        var _connections = [];
        for (var i = 0; i < connections.length; i++) {
            if (connections[i].ConnectionID == connectionID) {
                _connections = connections;
                break;
            }
        }
        //  _connections = Connections.Where(x => x.Value.ConnectionID == connectionID).Select(x => x.Key).ToList();
        return _connections;
    }

    ReturnWebConnections(userID) {
        var UserConnectionIDs = [];
        if (userID == 0) {
            for (var i = 0; i < connections.length; i++) {
                if (connections[i].UserType == SignalRUserType.WEB) {
                    UserConnectionIDs = connections;
                    break;
                }
            }
            // UserConnectionIDs = Connections.Where(x => x.Value.UserType == SignalRUserType.WEB).Select(x => x.Key).ToList();
        } else {
            // UserConnectionIDs = Connections.Where(x => x.Value.UserType == SignalRUserType.DRIVER && x.Value.DomainId == userID).Select(x => x.Key).ToList();
            for (var i = 0; i < connections.length; i++) {
                if (
                    connections[i].UserType == SignalRUserType.DRIVER &&
                    connections[i].DomainId == userID) {
                    UserConnectionIDs = connections;
                    break;
                }
            }
        }
        return UserConnectionIDs;
    }

    ReturnDesktopConnections(userID) {
        var UserConnectionIDs = [];
        if (userID == 0) {
            for (var i = 0; i < connections.length; i++) {
                if (connections[i].UserType == SignalRUserType.DESKTOP) {
                    UserConnectionIDs = connections;
                    break;
                }
            }

            // UserConnectionIDs = Connections.Where(x => x.Value.UserType == SignalRUserType.DESKTOP).Select(x => x.Key).ToList();
        } else {
            // UserConnectionIDs = Connections.Where(x => x.Value.UserType == SignalRUserType.DRIVER && x.Value.DomainId == userID).Select(x => x.Key).ToList();

            for (var i = 0; i < connections.length; i++) {
                if (
                    connections[i].UserType == SignalRUserType.DRIVER &&
                    connections[i].DomainId == userID) {
                    UserConnectionIDs = connections;
                    break;
                }
            }
        }
        return UserConnectionIDs;
    }

    SendMessage(socket) {
        //MessageToPDA
        socket.on("MessageToPDA", msg => {
            socket.emit("MessageToPDA", msg);
        });
    }
    requestPDA(msg) {
        var dt = new Date();
        var _dt = new Date();
        // Let dt = dt.setsSeconds(-45);
        dt.setSeconds(dt.getSeconds() - 45);
        try {
            // byte[] inputBuffer = Encoding.UTF8.GetBytes(msg);

            let data = msg;
            let buff = new Buffer(data, "base64");
            let text = buff.tolet("ascii");
            let dataValue = text;
            dataValue = dataValue;

            var values = [];

            values.push(dataValue.Split("="));
            let ddId = 0;
            if (parseInt(values[4]) == eMessageTypes.MESSAGING) {
                if (values[1].includes(",")) {
                    var value_dId = [];

                    for (let i = 0; i < values.length; i++) {
                        value_dId.push(values[1].Split(","));
                    }
                    value_dId.forEach((dId, index) => {
                        console.log(`i value: ${index} | Food Name:`, food);

                        let _clsPDA_variables = [];

                        //g[0] = new Tile();
                        //g[1] = new Tile();

                        _clsPDA_variables.push(new clsPDA());

                        for (var i = 0; i === _clsPDA_variables.length; i++) {
                            console.log(_clsPDA_variables[i]);
                        }

                        listofJobs.push((_clsPDA_variables[1].DriverId = parseInt(dId)));
                        listofJobs.push((_clsPDA_variables[2].JobId = values[2]));
                        listofJobs.push((_clsPDA_variables[3].MessageLet = dt));
                        listofJobs.push((_clsPDA_variables[4].JobMessage = values[3]));
                        listofJobs.push(
                            (_clsPDA_variables[5].MessageTypeId = parseInt(values[4])));
                    });
                    // for (let i = 0; i < value_data.length; i++) {
                    //console.log(`i value: ${i} | Food Name:`, foodArray[i]);
                    //}
                    // foreach (let dId in values[1].Split(','))
                    // {
                } else {
                    try {
                        if (values[3].toLowerCase().includes("fetchlogs")) {
                            const listOfConnections = [];

                            listOfConnections.push(
                                ReturnDriverConnections(Convert.Tolet32(values[1].Tolet())));
                            // Clients.Clients(listOfConnections).FetchLogs("");
                            io.to(listOfConnections).emit("message", msg);
                        } else {
                            let _clsPDA_variables = [];

                            //g[0] = new Tile();
                            //g[1] = new Tile();

                            _clsPDA_variables.push(new clsPDA());

                            for (var i = 0; i === _clsPDA_variables.length; i++) {
                                console.log(_clsPDA_variables[i]);
                            }

                            listofJobs.push((_clsPDA_variables[1].DriverId = parseInt(dId)));
                            listofJobs.push((_clsPDA_variables[2].JobId = values[2]));
                            listofJobs.push((_clsPDA_variables[3].MessageLet = dt));
                            listofJobs.push((_clsPDA_variables[4].JobMessage = values[3]));
                            listofJobs.push(
                                (_clsPDA_variables[5].MessageTypeId = parseInt(values[4])));
                        }
                        var _dt = new Date();
                        try {
                            Logs.CreateLog(
                                _dt +
                                "\t: listofjob.count :" +
                                listofJobs.length +
                                " :  ConnectionID = " +
                                ReturnDriverConnections(values[1]) +
                                "\n",
                                socket.id,
                                "requestpda");
                            //  File.AppendAllText(AppContext.BaseDirectory + "\\requestpda.txt", Let.Now.ToStr() + ": listofjob.count :" + listofJobs.Count + " :  ConnectionID = " + Instance.ReturnDriverConnections(values[1].Tolet()).FirstOrDefault() + Environment.NewLine);
                        } catch (ex) {}

                        ddId = parseInt(values[1]);
                    } catch (ex) {
                        try {
                            let _clsPDA_variables = [];

                            //g[0] = new Tile();
                            //g[1] = new Tile();

                            _clsPDA_variables.push(new clsPDA());

                            for (var i = 0; i === _clsPDA_variables.length; i++) {
                                console.log(_clsPDA_variables[i]);
                            }

                            listofJobs.push((_clsPDA_variables[1].DriverId = parseInt(dId)));
                            listofJobs.push((_clsPDA_variables[2].JobId = values[2]));
                            listofJobs.push((_clsPDA_variables[3].MessageLet = dt));
                            listofJobs.push((_clsPDA_variables[4].JobMessage = values[3]));
                            listofJobs.push(
                                (_clsPDA_variables[5].MessageTypeId = parseInt(values[4])));

                            Logs.CreateLog(
                                _dt + " FIXED: " + ex + "\n",
                                socket.id,
                                "exception_servertoclient");

                            //    File.AppendAllText(physicalPath + "\\exception_servertoclient.txt", Let.Now.ToStr() + " FIXED: " + ex.Message + Environment.NewLine);
                        } catch (ex2) {
                            Logs.CreateLog(
                                _dt + " NOTFIXED: " + ex2 + "\n",
                                socket.id,
                                "exception_servertoclient");

                            // File.AppendAllText( dt+ " NOTFIXED: " + ex2.Message + Environment.NewLine);
                        }
                    }
                }
            } else if (parseInt(values[4]) == eMessageTypes.JOB) {
                let drvId = parseInt(values[2]);
                try {
                    var counts_driver = [];
                    // counts_Job = [];

                    let obj_driver = listofJobs.find(
                            num =>
                            num.DriverId == parseInt(values[2]) &&
                            num.JobId == values[1] &&
                            num.MessageTypeId == eMessageTypes.RECALLJOB);
                    //  listofJobs = foodArray.find(c => c.name == "Burrito");
                    //console.log(obj + " " + obj.lenght);
                    if (obj_driver > -1) {
                        removeItemAll(listofJobs, obj_driver);
                    }
                    let counts_Job = listofJobs.find(
                            l => l.JobId == values[1] && l.DriverId == parseInt(values[2]));
                    //  listofJobs = foodArray.find(c => c.name == "Burrito");
                    //console.log(obj + " " + obj.lenght);
                    if (counts_Job.lenght > 0) {
                        removeItemAll(listofJobs, counts_Job);
                    }

                    let _clsPDA_variables = [];

                    //g[0] = new Tile();
                    //g[1] = new Tile();

                    _clsPDA_variables.push(new clsPDA());

                    for (var i = 0; i === _clsPDA_variables.length; i++) {
                        console.log(_clsPDA_variables[i]);
                    }

                    listofJobs.push((_clsPDA_variables[1].DriverId = parseInt(dId)));
                    listofJobs.push((_clsPDA_variables[2].JobId = values[2]));
                    listofJobs.push((_clsPDA_variables[3].MessageLet = dt));
                    listofJobs.push((_clsPDA_variables[4].JobMessage = values[3]));
                    listofJobs.push(
                        (_clsPDA_variables[5].MessageTypeId = parseInt(values[4])));
                } catch (ex) {
                    var counts_Job = [];
                    try {
                        let counts_Job = listofJobs.find(
                                c => c.JobId == values[1] && c.DriverId == parseInt(values[2]));
                        //  listofJobs = foodArray.find(c => c.name == "Burrito");
                        //console.log(obj + " " + obj.lenght);
                        if (counts_Job > 0) {
                            removeItemAll(listofJobs, counts_Job);
                        }

                        // if (listofJobs.Count(c => c.JobId == values[1].ToLong() && c.DriverId == values[2].Tolet()) > 0)
                        //    listofJobs.RemoveAll(c => c.JobId == values[1].ToLong() && c.DriverId == values[2].Tolet());
                        let _clsPDA_variables = [];

                        //g[0] = new Tile();
                        //g[1] = new Tile();

                        _clsPDA_variables.push(new clsPDA());

                        for (var i = 0; i === _clsPDA_variables.length; i++) {
                            console.log(_clsPDA_variables[i]);
                        }

                        listofJobs.push((_clsPDA_variables[1].DriverId = parseInt(dId)));
                        listofJobs.push((_clsPDA_variables[2].JobId = values[2]));
                        listofJobs.push((_clsPDA_variables[3].MessageLet = dt));
                        listofJobs.push((_clsPDA_variables[4].JobMessage = values[3]));
                        listofJobs.push(
                            (_clsPDA_variables[5].MessageTypeId = parseInt(values[4])));

                        Logs.CreateLog(_dt + " FIXED: ", "0", "exception_servertoclient");

                        //File.AppendAllText(physicalPath + "\\exception_servertoclient.txt", Let.Now.ToStr() + " FIXED: " + ex.Message + Environment.NewLine);
                    } catch (ex) {
                        try {
                            Logs.CreateLog(
                                _dt + " NOTFIXED: " + ex,
                                "0",
                                "exception_servertoclient");

                            //  File.AppendAllText(physicalPath + "\\exception_servertoclient.txt", Let.Now.ToStr() + " NOTFIXED: " + ex2.Message + Environment.NewLine);
                        } catch {}
                    }
                }

                //Method.SendToConnection(drvId);
                Method.SendToConnection_despatchBooking(drvId);

                //  Clients.Clients(listOfConnections).despatchBooking(listofJobs.find(c => c.DriverId == drvId).JobMessage);
            } else if (parseInt(values[4]) == eMessageTypes.CLEAREDJOB) {
                try {
                    let _clsPDA_variables = [];

                    _clsPDA_variables.push(new clsPDA());

                    listofJobs.push((_clsPDA_variables[1].DriverId = parseInt(dId)));
                    listofJobs.push((_clsPDA_variables[2].JobId = values[2]));
                    listofJobs.push((_clsPDA_variables[3].MessageLet = dt));
                    listofJobs.push((_clsPDA_variables[4].JobMessage = values[3]));
                    listofJobs.push(
                        (_clsPDA_variables[5].MessageTypeId = parseInt(values[4])));
                } catch (ex) {
                    try {
                        sleep.sleep(5); // 5 seconds
                        let _clsPDA_variables = [];

                        _clsPDA_variables.push(new clsPDA());

                        listofJobs.push((_clsPDA_variables[1].DriverId = parseInt(dId)));
                        listofJobs.push((_clsPDA_variables[2].JobId = values[2]));
                        listofJobs.push((_clsPDA_variables[3].MessageLet = dt));
                        listofJobs.push((_clsPDA_variables[4].JobMessage = values[3]));
                        listofJobs.push(
                            (_clsPDA_variables[5].MessageTypeId = parseInt(values[4])));

                        Logs.CreateLog(
                            _dt + "force clear job FIXED: " + ex + "\n",
                            "0",
                            "exception_servertoclient");
                    } catch (ex) {
                        Logs.CreateLog(
                            _dt + "force clear job NOTFIXED: " + ex + "\n",
                            "0",
                            "exception_servertoclient");
                    }
                }

                //List<let> listOfConnections = new List<let>();
                //listOfConnections = Instance.ReturnDriverConnections(parseInt(listofJobs[0].DriverId));
                Method.forceClearJob(parseInt(listofJobs[0].DriverId));
            } else if (parseInt(values[4]) == eMessageTypes.RECALLJOB) {
                try {
                    let _clsPDA_variables = [];

                    _clsPDA_variables.push(new clsPDA());

                    listofJobs.push((_clsPDA_variables[1].DriverId = parseInt(dId)));
                    listofJobs.push((_clsPDA_variables[2].JobId = values[2]));
                    listofJobs.push((_clsPDA_variables[3].MessageLet = dt));
                    listofJobs.push((_clsPDA_variables[4].JobMessage = values[3]));
                    listofJobs.push(
                        (_clsPDA_variables[5].MessageTypeId = parseInt(values[4])));
                } catch (ex) {
                    try {
                        sleep.Sleep(10);

                        let _clsPDA_variables = [];

                        _clsPDA_variables.push(new clsPDA());

                        listofJobs.push((_clsPDA_variables[1].DriverId = parseInt(dId)));
                        listofJobs.push((_clsPDA_variables[2].JobId = values[2]));
                        listofJobs.push((_clsPDA_variables[3].MessageLet = dt));
                        listofJobs.push((_clsPDA_variables[4].JobMessage = values[3]));
                        listofJobs.push(
                            (_clsPDA_variables[5].MessageTypeId = parseInt(values[4])));

                        Logs.CreateLog(
                            _dt + "recover job FIXED: " + ex + "\n",
                            "0",
                            "exception_servertoclient");
                    } catch (ex2) {
                        Logs.CreateLog(
                            _dt + "recover job NOTFIXED: " + ex2 + "\n",
                            "0",
                            "exception_servertoclient");

                        // File.AppendAllText(physicalPath + "\\exception_servertoclient.txt", Let.Now.ToStr() + "recover job NOTFIXED: " + ex2.Message + Environment.NewLine);
                    }
                }
            } else if (parseInt(values[4]) == eMessageTypes.UPDATEJOB) {
                let drvId = parseInt(values[1]);

                let _clsPDA_variables = [];

                _clsPDA_variables.push(new clsPDA());

                listofJobs.push((_clsPDA_variables[1].DriverId = parseInt(dId)));
                listofJobs.push((_clsPDA_variables[2].JobId = values[2]));
                listofJobs.push((_clsPDA_variables[3].MessageLet = dt));
                listofJobs.push((_clsPDA_variables[4].JobMessage = values[3]));
                listofJobs.push(
                    (_clsPDA_variables[5].MessageTypeId = parseInt(values[4])));

                var listOfConnections = [];
                listOfConnections = ReturnDriverConnections(
                        parseInt(listofJobs.find(c => c.DriverId == drvId).DriverId));
                io.to(listOfConnections).emit(
                    "message",
                    listofJobs.find(c => c.DriverId == drvId).JobMessage);

                Method.updateJob(drvId);
            } else if (parseInt(values[4]) == eMessageTypes.AUTHORIZATION) {
                let _clsPDA_variables = [];

                _clsPDA_variables.push(new clsPDA());

                listofJobs.push((_clsPDA_variables[1].DriverId = parseInt(dId)));
                listofJobs.push((_clsPDA_variables[2].JobId = values[2]));
                listofJobs.push((_clsPDA_variables[3].MessageLet = dt));
                listofJobs.push((_clsPDA_variables[4].JobMessage = values[3]));
                listofJobs.push(
                    (_clsPDA_variables[5].MessageTypeId = parseInt(values[4])));

                Method.sendMessage(parseInt(listofJobs[0].DriverId));

            } else if (parseInt(values[4]) == eMessageTypes.BIDALERT) {
                let _clsPDA_variables = [];

                _clsPDA_variables.push(new clsPDA());

                listofJobs.push((_clsPDA_variables[1].DriverId = parseInt(dId)));
                listofJobs.push((_clsPDA_variables[2].JobId = values[2]));
                listofJobs.push((_clsPDA_variables[3].MessageLet = dt));
                listofJobs.push((_clsPDA_variables[4].JobMessage = values[3]));
                listofJobs.push(
                    (_clsPDA_variables[5].MessageTypeId = parseInt(values[4])));

                Method.bidAlert(parseInt(listofJobs[0].DriverId));
            } else if (parseInt(values[4]) == eMessageTypes.BIDPRICEALERT) {
                var driverIds = [];
                driverIds = values[1].split(",");

                driverIds.forEach(function (a) {
                    let _clsPDA_variables = [];

                    _clsPDA_variables.push(new clsPDA());

                    listofJobs.push((_clsPDA_variables[1].DriverId = parseInt(dId)));
                    listofJobs.push((_clsPDA_variables[2].JobId = values[2]));
                    listofJobs.push((_clsPDA_variables[3].MessageLet = dt));
                    listofJobs.push((_clsPDA_variables[4].JobMessage = values[3]));
                    listofJobs.push(
                        (_clsPDA_variables[5].MessageTypeId = parseInt(values[4])));
                });

                Method.sendMessage(parseInt(listofJobs[0].DriverId));
            } else if (parseInt(values[4]) == eMessageTypes.UPDATEPLOT) {
                let _clsPDA_variables = [];

                _clsPDA_variables.push(new clsPDA());

                listofJobs.push((_clsPDA_variables[1].DriverId = parseInt(dId)));
                listofJobs.push((_clsPDA_variables[2].JobId = values[2]));
                listofJobs.push((_clsPDA_variables[3].MessageLet = dt));
                listofJobs.push((_clsPDA_variables[4].JobMessage = values[3]));
                listofJobs.push(
                    (_clsPDA_variables[5].MessageTypeId = parseInt(values[4])));

                Method.sendMessage(parseInt(listofJobs[0].DriverId));
            } else if (parseInt(values[4]) == eMessageTypes.LOGOUTAUTHORIZATION) {
                let _clsPDA_variables = [];

                _clsPDA_variables.push(new clsPDA());

                listofJobs.push((_clsPDA_variables[1].DriverId = parseInt(dId)));
                listofJobs.push((_clsPDA_variables[2].JobId = values[2]));
                listofJobs.push((_clsPDA_variables[3].MessageLet = dt));
                listofJobs.push((_clsPDA_variables[4].JobMessage = values[3]));
                listofJobs.push(
                    (_clsPDA_variables[5].MessageTypeId = parseInt(values[4])));

                Method.sendMessage(parseInt(listofJobs[0].DriverId));

            } else if (parseInt(values[4]) == eMessageTypes.FORCE_ACTION_BUTTON) {
                let _clsPDA_variables = [];

                _clsPDA_variables.push(new clsPDA());

                listofJobs.push((_clsPDA_variables[1].DriverId = parseInt(dId)));
                listofJobs.push((_clsPDA_variables[2].JobId = values[2]));
                listofJobs.push((_clsPDA_variables[3].MessageLet = dt));
                listofJobs.push((_clsPDA_variables[4].JobMessage = values[3]));
                listofJobs.push(
                    (_clsPDA_variables[5].MessageTypeId = parseInt(values[4])));

                Method.sendMessage(parseInt(listofJobs[0].DriverId));
            } else if (parseInt(values[4]) == eMessageTypes.UPDATE_SETTINGS) {
                let _clsPDA_variables = [];

                _clsPDA_variables.push(new clsPDA());

                listofJobs.push((_clsPDA_variables[1].DriverId = parseInt(dId)));
                listofJobs.push((_clsPDA_variables[2].JobId = values[2]));
                listofJobs.push((_clsPDA_variables[3].MessageLet = dt));
                listofJobs.push((_clsPDA_variables[4].JobMessage = values[3]));
                listofJobs.push(
                    (_clsPDA_variables[5].MessageTypeId = parseInt(values[4])));

                Method.sendMessage(parseInt(listofJobs[0].DriverId));
            }

            //send acknowledgement message to desktop
            Method.cMessageToDesktop("ok");
        } catch (ex) {
            Logs.CreateLog(
                _dt + " FIXED: " + ex + "\n",
                "0",
                "exception_servertoclient");

            // File.AppendAllText(physicalPath + "\\exception_servertoclient.txt", Let.Now.ToStr() + " FIXED: " + ex.Message + Environment.NewLine);
            Method.cMessageToDesktop("exceptionOccured" + ex.Message);
        }
    }

    LoadDataList(forceRefresh) {
        let conn = db.ConnectToSQL();
        if (objPolicy == null || forceRefresh) {
            conn.connect().then(request => {
                let sp_ = null;
                conn
                .request()
                .query(
                    "select * from Gen_SysPolicy_Configuration where SysPolicyId==1 ",
                    (err, result) => {
                    // Logs.CreateLog(getobj(result), socket.id, "Fetch_record_by_sql");
                    if (err) {
                        //   console.log('Failed to open a SQL Database connection.', err.stack);
                        Logs.CreateLog(
                            "Failed to open a SQL Database connection.\n" + err.stack,
                            "0",
                            "LoadDataList");
                    } else {
                        response.send(result);
                        objPolicy = getobj(result);
                    }
                    //console.log(getobj(result));
                });

                //console.log(request+' Connected');
            });
            //objPolicy = General.GetObject<Gen_SysPolicy_Configuration>(c => c.SysPolicyId == 1);
        }

        if (listofPolyVertices == null || forceRefresh) {
            conn.connect().then(request => {
                let sp_ = null;
                conn
                .request()
                .query(
                    "select * from Gen_Zone_PolyVertice where ZoneId != 0 ",
                    (err, result) => {
                    // Logs.CreateLog(getobj(result), socket.id, "Fetch_record_by_sql");
                    if (err) {
                        //   console.log('Failed to open a SQL Database connection.', err.stack);
                        Logs.CreateLog(
                            "Failed to open a SQL Database connection.\n" + err.stack,
                            "0",
                            "LoadDataList");
                    } else {
                        response.send(result);
                        listofPolyVertices.push(getobj(result));
                    }
                    //console.log(getobj(result));
                });

                //console.log(request+' Connected');
            });
            //listofPolyVertices = General.GetQueryable<Gen_Zone_PolyVertice>(c => c.ZoneId != 0).ToList();
        }

        if (listOfZone == null || forceRefresh) {
            conn.connect().then(request => {
                let sp_ = null;
                let query =
                    "select" +
                    "Id," +
                    "ZoneName as [Area] ," +
                    "PostCode," +
                    "'Fixed' as ZoneType," +
                    "IsBase as [IsBaseZone]," +
                    "MinLatitude as  [MinLat]," +
                    "MaxLatitude as [MaxLat]," +
                    "MinLongitude as [MinLng]," +
                    "MaxLongitude as [MaxLng]," +
                    "PlotLimit as [PlotLimit]," +
                    "PlotEntranceMessage as [PlotEntranceMessage]," +
                    "PlotLimitExceedMessage as [PlotOverLimitMessage]," +
                    "ShapeType as [shapeType]," +
                    "case when Shapetype='circle' then Shapetype when  (select isnull(Diameter,0) from Gen_Zone_PolyVertices where Diameter=null) ==0 then 0 else 0 end as [radius] ," +
                    "DisableDriverRank as [DisableRank]," +
                    "PlotKind" +
                    "from Gen_Zones where ShapeCategory != null Order By OrderNo";
                conn.request().query(query, (err, result) => {
                    // Logs.CreateLog(getobj(result), socket.id, "Fetch_record_by_sql");
                    if (err) {
                        //   console.log('Failed to open a SQL Database connection.', err.stack);
                        Logs.CreateLog(
                            "Failed to open a SQL Database connection.\n" + err.stack,
                            "0",
                            "LoadDataList");
                    } else {
                        response.send(result);
                        listOfZone.push(getobj(result));
                    }
                    //console.log(getobj(result));
                });

                //console.log(request+' Connected');
            });
        }
    }

    MessageToPDA(message) {
        try {
            if (message.startsWith("request pda=")) {
                requestPDA(message);
            } else if (message.startsWith("request refreshzones")) {
                try {
                    LoadDataList(true);
                    //Clients.Caller.cMessageToDesktop("ok");
                } catch (ex) {
                    //  Clients.Caller.cMessageToDesktop("exceptionOccured" + ex.Message);
                }
            } else if (message.startsWith("request dispatchsms=")) {
                //  Instance.listofSMS.Add(message);
            } else if (message.startsWith("request broadcast=")) {
                //Clients.Caller.cMessageToDesktop("exceptionOccured" + ex.Message);
                // let data = message.Split('=')[1];
                // List<let> listOfConnections = new List<let>();
                // listOfConnections = Instance.ReturnDesktopConnections();
                // var finalList= listOfConnections.Remove()
                // Clients.Clients(listOfConnections).cMessageToDesktop(data);
            } else if (message.startsWith("request force logout=")) {
                // let[] values = message.Split('=');

                try {
                    let _clsPDA_variables = [];

                    _clsPDA_variables.push(new clsPDA());

                    listofJobs.push((_clsPDA_variables[1].DriverId = parseInt(dId)));
                    listofJobs.push((_clsPDA_variables[2].JobId = values[2]));
                    listofJobs.push((_clsPDA_variables[3].MessageLet = dt));
                    listofJobs.push((_clsPDA_variables[4].JobMessage = values[3]));
                    listofJobs.push(
                        (_clsPDA_variables[5].MessageTypeId = parseInt(values[4])));
                } catch (ex) {}
            } else if (message.startsWith("**")) {
                //Clients.Caller.cMessageToDesktop("exceptionOccured" + ex.Message);
                // List<let> listOfConnections = new List<let>();
                // listOfConnections = Instance.ReturnDesktopConnections();
                // var finalList= listOfConnections.Remove()
                // Clients.Clients(listOfConnections).cMessageToDesktop(message);
                // if(message.ToStr().includes("**autodespatchmode"))
                // {
                //    Instance.objPolicy = General.GetObject<Gen_SysPolicy_Configuration>(c => c.SysPolicyId == 1);
                //}
            }

            try {
                // File.AppendAllText(AppContext.BaseDirectory + "\\MessageToPDA.txt", Let.Now.ToStr() + ": Message :" + message + " :  Total Connections = " + Instance.ReturnDesktopConnections().Count + Environment.NewLine);
            } catch (ex) {}
        } catch {
            try {
                //  File.AppendAllText(AppContext.BaseDirectory + "\\MessageToPDA_catch.txt", Let.Now.ToStr() + ": Message :" + message + " :  Total Connections = " + Instance.ReturnDesktopConnections().Count + Environment.NewLine);
            } catch (ex) {}
        }
    }

    LatLong(mesg) 
    {
        let conn = db.ConnectToSQL();
        console.log(mesg+' LatLong k andr agya ' );
        var dt = new Date();
        //try {

              console.log(mesg+' try catch ' );
            console.log(mesg);
             // MetHod.CallBackLog();
            // MetHod.CallBackLog_(mesg);
           // try {
                //Clients.Caller.LatLongChanged("true");
           // } catch {}
           // let data = mesg;
           // let buff = new Buffer(data, "base64");
            //let text = buff.tolet("ascii");
            // byte[] inputBuffer = Encoding.UTF8.GetBytes(mesg);

            //if (mesg.startsWith("meter=>>>"))
              //  return;

            /* if(mesg.startsWith("meter=") ){

            var arr = mesg.split(">>>");

            let faremeterlet= arr[0];

            mesg = arr[1];

            try{
            const meterArray =[];
            meterArray.push(faremeterlet.split('='));
            jobMeter.serialize(meterArray[1].tolet());

            // JobMeter _jobMeter = new JavaScriptSerializer().Deserialize<JobMeter>(meterArray[1].ToStr());

            let response = null;


            let rtnFares = 0.00m;


            let miles = jobMeter.Miles.Tolet();
            let IsWaiting = jobMeter.IsWaiting.ToStr();
            let waitingCharges = jobMeter.WaitingCharges.Tolet();
            let waitingTime = jobMeter.WaitingTime.Tolet();
            let vehicleType = jobMeter.VehicleType.ToStr();
            //  let waitingSpeed = jobMeter.WaitingSpeed.Tolet();
            let SpeedSecs = jobMeter.SpeedSecs.Tolet();

            Let pickupDate;


            if (jobMeter.PickupLet.length > 0){
            let pickupLet =  jobMeter.PickupLet.replace("  ", " ");

            //Let.TryParse(pickupLet, out pickupDate);

            }
            else{
            pickupDate = dt;

            }


            // InitializeMeterList();

            var objFareMeter = listofMeter.find(c => c.VehicleType.toLower() == vehicleType.toLower());


            //  Let dateValue = new Let(1900, 1, 1, 0, 0, 0);
            //   pickupDate = let.Format("{0:dd/MM/yyyy HH:mm}", dateValue.ToDate() + pickupDate.TimeOfDay).ToLet();

            if (miles > 0 && objPolicy.RoundJourneyMiles > 0){
            miles = Math.Ceiling(miles / objPolicy.RoundJourneyMiles) * Instance.objPolicy.RoundJourneyMiles.Tolet();
            }


            if (jobMeter.Speed.Tolet() > 0 || jobMeter.Fares.Tolet() == 0 || jobMeter.IsWaiting == "1"){
            using (TaxiDataContext db = new TaxiDataContext()){
            try{
            var objFare = db.stp_CalculateMeterFares(objFareMeter.VehicleTypeId, jobMeter.CompanyId.Tolet(), miles, pickupDate, jobMeter.SubCompanyId.Tolet()).FirstOrDefault();


            if (objFare != null){
            rtnFares = objFare.totalFares.Tolet();


            if (Instance.objPolicy.RoundMileageFares.ToBool() == false){
            let roundUp = Instance.objPolicy.RoundUpTo.Tolet();
            if (roundUp > 0){

            if (objFare.Result.ToStr().some(i => !Number.isInteger(i)) && objFare.CompanyFareExist.ToBool()){

            rtnFares = rtnFares.Tolet();
            }
            else{

            rtnFares = (let)Math.Ceiling(rtnFares / roundUp) * roundUp;
            }
            }
            }
            else{
            let ff = let.Format("{0:f2}", rtnFares);
            if (ff == let.Empty)
            ff = "0";

            rtnFares = ff.Tolet();
            }
            }
            }
            catch (Exception ex){
            try{

            File.AppendAllText(physicalPath + "\\" + "exception_meterletcatch.txt", Let.Now.Tolet("dd/MM/yyyy HH:mm:ss") + ":" + mesg + "," + ex.Message + Environment.NewLine);
            }
            catch{


            }

            }


            }
            }
            else{
            rtnFares = jobMeter.Fares.Tolet();

            }





            if (jobMeter.IsWaiting == "1"){

            if (objFareMeter.AutoStopWaitingOnSpeed.Tolet() > 0 && jobMeter.Speed.Tolet() >= objFareMeter.AutoStopWaitingOnSpeed.Tolet()){
            IsWaiting = "0";
            }
            else{
            if (waitingTime > 0){
            if (objFareMeter.AccWaitingChargesPerMin == null || objFareMeter.AccWaitingChargesPerMin.Tolet() == 0){
            let waitingMins = waitingTime / 60;
            waitingMins = Math.Ceiling(waitingMins);
            waitingCharges = waitingMins * objFareMeter.DrvWaitingChargesPerMin.Tolet();
            }
            else{


            let waitingMins = Math.Floor((waitingTime / objFareMeter.AccWaitingChargesPerMin.Tolet()));
            // waitingMins = Math.Ceiling(waitingMins);
            waitingCharges = waitingMins * objFareMeter.DrvWaitingChargesPerMin.Tolet();
            }
            }
            }

            }
            else{

            if (objFareMeter.AutoStartWaiting.ToBool() && SpeedSecs >= objFareMeter.AutoStartWaitingBelowSpeedSeconds.Tolet()){
            IsWaiting = "1";

            }



            }

            jobMeter.WaitingSpeed = objFareMeter.AutoStartWaitingBelowSpeed.Tolet();





            if (rtnFares < jobMeter.Fares.Tolet() && rtnFares < (jobMeter.Fares.Tolet() - 2))
            rtnFares = jobMeter.Fares.Tolet();


            jobMeter.Fares = rtnFares;
            jobMeter.IsWaiting = IsWaiting;
            jobMeter.WaitingTime = waitingTime;
            jobMeter.WaitingCharges = waitingCharges;




            let res = new JavaScriptSerializer().Serialize(jobMeter);



            response = "meter=" + res;

            try{
            Clients.Caller.fareMeter(response);
            }
            catch{

            }






            try{

            File.AppendAllText(physicalPath + "\\" + jobMeter.JobID.ToStr() + ".txt", "logon:" + Let.Now.Tolet("dd/MM/yyyy HH:mm:ss") + ", miles:" + jobMeter.Miles + ", fares:" + jobMeter.Fares + ",waitingtime:" + jobMeter.WaitingTime.Tolet() + ",speed:" + jobMeter.Speed.ToStr() + ",lat lng:" + jobMeter.lg.ToStr() + "," + jobMeter.lt.ToStr() + Environment.NewLine);
            }
            catch{


            }
            }
            catch (Exception ex){
            try{

            File.AppendAllText(physicalPath+ "\\" + "exception_faremeter.txt", Let.Now.Tolet("dd/MM/yyyy HH:mm:ss") + ", data:" + mesg + ":" + ex.Message + Environment.NewLine);
            //  RestartProgram();

            }
            catch{


            }

            }


            }*/

            console.log(' dataValue ');
                        var values = [];
                    
                    
                    const words = mesg.message.split('=');
                    
                    // console.log(words[0].Lat);
                    //const a=JSON.stringify(mesg.message).split(',');
                        
                         
                  //  jparse_objdriver.splice(0,  jparse_objdriver.length); //[]
                    // for(var i=0;i<a.length;i++)
                    // {
                       
                         // jparse_objdriver.push(a[i]);

                   // }
                   // _objDriver = JSON.parse(jparse_objdriver);
            for(var i=0;i<words.length;i++)
            {
            console.log(words[i]);
            
                values.push(words[i]);
            }           //dataValue = dataValue;
            // old   // lat=34234=L=0.5=d=DRIVERID=S=speed=j=jobid
            // new only meter case   // meter=json>>>lat=34234=L=0.5=d=DRIVERID=S=speed=j=jobid
           
            

            let latitude = values[1];
            let letitude = values[3];
            let speed = values[7];
            let driverId = parseInt(values[5]);
            console.log('Driverid : '+driverId)

            let jobId = 0;
            var count=Method.CountArray(values);
            if (count >= 10 && parseInt(values[9])) {
                jobId = values[9];
            }

            //var  listcount=[];
            //listcount.push(listofJobs.find(c => c.DriverId == parseInt(values[5]) && c.MessageLet.length < 2));

            var lstcount = Method.ReturnIntValueByList(
                    listofJobs.find(
                        c => c.DriverId == parseInt(values[5]) && c.MessageLet.length < 2));
            console.log(lstcount+'  lstcount');

            if (lstcount > 0) {
                console.log(lstcount+'  lstcount zero nhi he ');
                clsPDA = listofJobs.find(
                        c => c.DriverId == parseInt(values[5]) && c.MessageLet < 2);
                console.log(clsPDA+'  prlet  ');

                if (clsPDA != null) {
                console.log(clsPDA+'  clsPDA null nhi he  ');
                    try {
                        if (
                            jobId == 0 ||
                            clsPDA.JobId == 0 ||
                            clsPDA.MessageTypeId != eMessageTypes.JOB) {
                            if (clsPDA.MessageTypeId == eMessageTypes.JOB) {
                               
                                console.log(clsPDA.MessageTypeId+'  eMessageTypes.JOB  ');
                                    

                                Method.despatchBooking(parseInt(clsPDA.DriverId),clsPDA.JobMessage);
                            } else if (clsPDA.MessageTypeId == eMessageTypes.RECALLJOB) {
                               
                            console.log(clsPDA.MessageTypeId+'  eMessageTypes.RECALLJOB  ');
                                Method.forceRecoverJob(parseInt(clsPDA.DriverId),clsPDA.JobMessage);
                            } else if (clsPDA.MessageTypeId == eMessageTypes.UPDATEJOB) {
                              
                             console.log(clsPDA.MessageTypeId+'  eMessageTypes.UPDATEJOB  ');
                                 Method.updateJob(parseInt(clsPDA.DriverId),clsPDA.JobMessage);
                            } else if (
                                clsPDA.MessageTypeId == eMessageTypes.UPDATE_SETTINGS) {
                               console.log(clsPDA.MessageTypeId+'  UPDATE_SETTINGS  ');

                                Method.updateSetting(parseInt(clsPDA.DriverId),clsPDA.JobMessage);
                            }
                            else
                            {
                                 console.log(clsPDA.MessageTypeId+' else ');
                                 Method.sendMessage(parseInt(clsPDA.DriverId),clsPDA.JobMessage);

                            }
                        }

                        try {
                            var CONNid = ReturnDriverConnections(parseInt(clsPDA.DriverId));

                            //File.AppendAllText(AppContext.BaseDirectory + "\\LATLNG.txt", Let.Now + "," + "connectionID() " + CONNid + Environment.NewLine);
                        } catch (ex) {
                            try {
                                // File.AppendAllText(AppContext.BaseDirectory + "\\LATLNG.txt", Let.Now + "," + ex.Message + Environment.NewLine);
                            } catch {}
                        }
                    } catch (ex) {}

                    if (
                        jobId != 0 &&
                        clsPDA.MessageTypeId == eMessageTypes.JOB &&
                        jobId == clsPDA.JobId) {
                        let pickup = null;
                        let destination = null;
                        try {
                            if (clsPDA.JobMessage.startsWith('JobId:{ "JobId"')) {
                                Method.serialize(clsPDA.JobMessage);
                                //ClsJobMessageParser objParser = new JavaScriptSerializer().Deserialize<ClsJobMessageParser>(clsPDA.JobMessage.ToStr().Sublet(6));

                                if (objParser != null) {
                                    pickup = objParser.Pickup.ToStr();
                                    destination = objParser.Destination.ToStr();
                                }
                            } else {
                                pickup = clsPDA.JobMessage.Sublet(
                                        clsPDA.JobMessage.IndexOf(":Pickup:") + 8);
                                pickup = pickup.Remove(pickup.IndexOf(":Destination:"));

                                destination = clsPDA.JobMessage.Sublet(
                                        clsPDA.JobMessage.IndexOf(":Destination:") + 13);
                                destination = destination.Remove(
                                        destination.IndexOf(":PickupLet:"));
                            }

                            BroadCastMessage(
                                "**job received>>" +
                                clsPDA.DriverNo +
                                ">>" +
                                pickup +
                                ">>" +
                                destination);
                        } catch {}

                        listofJobs.Remove(clsPDA);
                    } else if (
                        clsPDA.MessageTypeId == eMessageTypes.FORCE_ACTION_BUTTON) {
                        if (jobId == 0 || jobId != clsPDA.JobId) {
                            listofJobs.Remove(clsPDA);
                        } else {
                            //Send message to PDA
                            if (clsPDA.JobMessage.ToStr().includes("<<Arrive Job>>")) {
                                Clients.Caller.forceArriveJob(clsPDA.JobMessage.ToStr());
                            } else if (clsPDA.JobMessage.ToStr().includes("<<POB Job>>")) {
                                Clients.Caller.forcePobJob(clsPDA.JobMessage.ToStr());
                            }

                            listofJobs.Remove(clsPDA);
                        }
                    } else if (clsPDA.MessageTypeId == eMessageTypes.AUTHORIZATION) {
                        //Send message to PDA
                        Clients.Caller.authStatus(clsPDA.JobMessage.ToStr());

                        //Byte[] byteResponse2 = Encoding.UTF8.GetBytes(clsPDA.JobMessage.ToStr());
                        //tcpClient.GetStream().Write(byteResponse2, 0, byteResponse2.Length);

                        try {
                            if (
                                clsPDA.JobMessage.includes("yes") &&
                                (jobId == 0 || jobId != clsPDA.JobId)) {
                                listofJobs.Remove(clsPDA);
                            }
                            //else if (clsPDA.JobMessage.includes("no") && clsPDA.JobId == jobId)
                            //{
                            else if (clsPDA.JobMessage.includes("no")) {
                                listofJobs.Remove(clsPDA);
                            }
                        } catch {}
                    } else if (
                        clsPDA.MessageTypeId == eMessageTypes.LOGOUTAUTHORIZATION) {
                        //Send message to PDA
                        //Clients.Caller.authStatus(clsPDA.JobMessage.ToStr());

                        //Byte[] byteResponse2 = Encoding.UTF8.GetBytes(clsPDA.JobMessage.ToStr());
                        //tcpClient.GetStream().Write(byteResponse2, 0, byteResponse2.Length);

                        if (clsPDA.MessageLet > 1) {
                            listofJobs.Remove(clsPDA);
                        }
                    } else if (clsPDA.MessageTypeId == eMessageTypes.ONBIDDESPATCH) {
                        let AvailCnter = 0;
                        if (jobId > 0) {

                            // using (TaxiDataContext db = new TaxiDataContext())
                            {
                                try {
                                    //db.CommandTimeout = 6;

                                    var Fleet_DriverQueueLists = [];
                                    var Bookings = [];
                                    Fleet_DriverQueueLists.push(db.Fleet_DriverQueueLists());
                                    AvailCnter = Method.ReturnIntValueByList(Fleet_DriverQueueLists.find(c => c.Status == true && c.DriverId == clsPDA.DriverId && c.DriverWorkStatusId == Driver_WORKINGSTATUS.AVAILABLE));

                                    //  AvailCnter = db.Fleet_DriverQueueLists().lenght;

                                    if (AvailCnter > 0) {
                                        Bookings.push(db.Bookings());

                                        let valCnt = Method.ReturnIntValueByList(Bookings.find(c => c.BookingStatusId == Enums.BOOKINGSTATUS.PENDING && c.DriverId == clsPDA.DriverId && c.PickupLet > dt.AddDays(-1)));
                                        // let valCnt = db.Bookings.Count(c => c.BookingStatusId == Enums.BOOKINGSTATUS.PENDING && c.DriverId == clsPDA.DriverId && c.PickupLet > Let.Now.AddDays(-1));

                                        if (valCnt > 0) {
                                            AvailCnter = 0;
                                        }
                                    }

                                    if (AvailCnter == 0) {
                                        //try
                                        //{
                                        //    File.AppendAllText(physicalPath + "\\onbiddespatchALREADYJOB.txt", Let.Now + ":" + ",jobid=" + clsPDA.JobId + Environment.NewLine);
                                        //}
                                        //catch
                                        //{

                                        //}

                                    }
                                } catch (ex) {
                                    AvailCnter = 1;

                                    //try
                                    //{
                                    //    File.AppendAllText(physicalPath + "\\onbiddespatchALREADYJOB_catch.txt", Let.Now + ":" + ex.Message + ",jobid=" + clsPDA.JobId + Environment.NewLine);
                                    //}
                                    //catch
                                    //{

                                    //}
                                }
                            }
                        } else
                            AvailCnter = 1;

                        if (AvailCnter > 0) {
                            let msg;

                            var Gen_SysPolicy_Configurations = [];
                            Gen_SysPolicy_Configurations.push(db.Gen_SysPolicy_Configurations());
                            if (
                                Gen_SysPolicy_Configurations[0].parseInt(BiddingType) ==
                                BIDDING_TYPES.NEAREST_DRIVER)
                                msg = "Bidding Job has been Despatched to Nearest driver";
                            else if (
                                Gen_SysPolicy_Configurations[0].parseInt(BiddingType) ==
                                BIDDING_TYPES.LONGEST_WAITING_QUEUE)
                                msg = "Job Despatch successfully to letest waiting driver";
                            else if (
                                Gen_SysPolicy_Configurations[0].parseInt(BiddingType) ==
                                BIDDING_TYPES.FASTEST_FINGER)
                                msg = "Job Received to Fastest Finger driver";

                            try {
                                //Send message to PDA
                                MetHod.despAtchbooking(clsPDA.JobMessage.ToStr());
                                db.SP_SaveBid(clsPDA.JobId, clsPDA.DriverId, clsPDA.Price, 2);
                                   MetHod.CallBackLog();
                                ///---- ye tak hogia he 5-nov-2020

                                clsPDA.MessageTypeId = eMessageTypes.JOB;

                                Method.BoardCastMessage("**onbid despatch>>" +
                                    clsPDA.JobId +
                                    ">>" +
                                    clsPDA.DriverId +
                                    ">>" +
                                    msg);

                            } catch {
                                try {
                                    //Send message to PDA
                                    Method.despatchBooking(clsPDA.JobMessage.ToStr());

                                    Method.BoardCastMessage(
                                        "**onbid despatch>>" +
                                        clsPDA.JobId +
                                        ">>" +
                                        clsPDA.DriverId +
                                        ">>" +
                                        msg);

                                    listofJobs.Remove(clsPDA);
                                } catch {
                                    try {
                                        Method.BoardCastMessage(
                                            "**onbid despatch>>" +
                                            clsPDA.JobId +
                                            ">>" +
                                            clsPDA.DriverId +
                                            ">>" +
                                            msg);
                                    } catch {}
                                }
                            }
                        } else {
                            try {
                                listofJobs.Remove(clsPDA);
                            } catch {}
                        }
                    } else {
                        //Send message to PDA

                        if (clsPDA.MessageTypeId == eMessageTypes.JOB) {
                            Method.dEspatchbooking(clsPDA.JobMessage.ToStr());
                            listofJobs.Remove(clsPDA);
                            // List<let> listOfConnections = new List<let>();
                            // listOfConnections = Instance.ReturnDriverConnections(Convert.Tolet32(clsPDA.DriverId));
                            // Clients.Clients(listOfConnections).despatchBooking(clsPDA.JobMessage.ToStr());
                        } else if (clsPDA.MessageTypeId == eMessageTypes.RECALLJOB) {
                            //List<let> listOfConnections = new List<let>();
                            //listOfConnections = Instance.ReturnDriverConnections(Convert.Tolet32(clsPDA.DriverId));
                            //Clients.Clients(listOfConnections).forceRecoverJob(clsPDA.JobMessage.ToStr());

                           // Clients.Caller.forceRecoverJob(clsPDA.JobMessage.ToStr());
                            listofJobs.Remove(clsPDA);
                        } else if (clsPDA.MessageTypeId == eMessageTypes.CLEAREDJOB) {
                           // Clients.Caller.forceClearJob(clsPDA.JobMessage.ToStr());
                            listofJobs.Remove(clsPDA);
                        } else if (clsPDA.MessageTypeId == eMessageTypes.UPDATE_SETTINGS) {
                            //Clients.Caller.updateSetting(clsPDA.JobMessage.ToStr());
                            listofJobs.Remove(clsPDA);
                        } else if (clsPDA.MessageTypeId == eMessageTypes.UPDATEJOB) {
                            //List<let> listOfConnections = new List<let>();
                            //listOfConnections = Instance.ReturnDriverConnections(Convert.Tolet32(clsPDA.DriverId));
                            //Clients.Clients(listOfConnections).updateJob(clsPDA.JobMessage.ToStr());

                           // Clients.Caller.updateJob(clsPDA.JobMessage.ToStr());
                            listofJobs.Remove(clsPDA);
                        } else if (
                            clsPDA.MessageTypeId == eMessageTypes.BIDALERT &&
                            values.lenght >= 10 &&
                            (values[9] == "bidack" ||
                                dataValue.EndsWith("bidack") ||
                                dataValue.EndsWith("bidack="))) {
                            listofJobs.Remove(clsPDA);
                        } else if (
                            clsPDA.MessageTypeId == eMessageTypes.BIDPRICEALERT &&
                            values.lenght >= 11 &&
                            values[10] == "bidprack") {
                            listofJobs.Remove(clsPDA);
                        } else if (clsPDA.MessageTypeId == eMessageTypes.MESSAGING) {
                            if (
                                clsPDA.JobMessage.ToStr()
                                
                                .ToLower() == "force logout") {
                                Clients.Caller.forceLogout(clsPDA.JobMessage.ToStr());
                            } else
                                Clients.Caller.sendMessage(clsPDA.JobMessage.ToStr());

                            listofJobs.Remove(clsPDA);
                        } else if (
                            (clsPDA.MessageTypeId == eMessageTypes.UPDATEPLOT ||
                                clsPDA.MessageTypeId == eMessageTypes.UPDATEJOB) &&
                            (jobId == 0 ||
                                (jobId != 0 && values.lenght >= 11 && values[10] == "modj"))) {
                            let REjOBiD = clsPDA.JobId;
                            let driveriD = clsPDA.DriverId;
                            let sMsg = clsPDA.JobMessage.ToStr();

                            listofJobs.Remove(clsPDA);
                        } else if (
                            clsPDA.MessageTypeId == eMessageTypes.BIDALERT &&
                            values.lenght >= 11 &&
                            values[10] == "bidack") {
                            listofJobs.Remove(clsPDA);
                        } else if (
                            clsPDA.MessageTypeId == eMessageTypes.JOB &&
                            values.lenght >= 11 &&
                            values[10] == "fojack") {
                            listofJobs.Remove(clsPDA);
                        } else if (
                            clsPDA.MessageTypeId == eMessageTypes.UPDATE_SETTINGS &&
                            values.lenght >= 11 &&
                            values[10].startsWith("updatesettingsack")) {
                            listofJobs.Remove(clsPDA);
                        } else {
                            if (clsPDA.MessageTypeId == eMessageTypes.BIDALERT) {
                                Clients.Caller.bidAlert(clsPDA.JobMessage.ToStr());
                            } else {
                                Clients.Caller.LatLongChanged(clsPDA.JobMessage.ToStr());
                            }

                            listofJobs.Remove(clsPDA);
                        }

                        //   Clients.Caller.LatLongChanged("true");
                    }
                    console.log('db me entry gir gai lastSaveDriverLocationTimeout null hy');
                }
            } else {
                //Send message to PDA
                //Clients.Caller.LatLongChanged("true");
            }

              //using (TaxiDataContext db = new TaxiDataContext()){
            //try{

              
            //if (lastSaveDriverLocationTimeout == null || lastSaveDriverLocationTimeout <dt){
            //db.CommandTimeout = 3;
            console.log('Driverid : '+driverId)
            var oldzoneId=db.stp_SaveDriverLocationByZone(driverId,latitude,letitude,speed,jobId);
           // let oldzoneId = db.stp_SaveDriverLocationByZone(driverId, latitude, letitude, speed, jobId).FirstOrDefault().Column1.Tolet();
           let lastConnectionLet = dt;
            let lastSaveDriverLocationTimeout = null;
           let retryDriverLocTimeout = 1;

           console.log(oldzoneId+' db me save ho gaia he !');
            //if (oldzoneId == 0 || speed > 0)
             if (oldzoneId == 1)
            {
                console.log('db me entry gir gai');
            }
            else
            {console.log('db me entry nhi gir gai');}
            //(oldzoneId == 0 ||
            //      speed > 0
            //      )
            //      ){

            //this.LoadDataList(true);


            let returnLoc = null;
            let postcode = null;
            let hasChanges = false;
            let zoneId = 0;
            let newZoneName = null;

           // try{
                /*
            if (latitude > 0){
            let[] plot = (from a in Instance.listOfZone.Where(c =>
            (c.shapeType != "" && c.shapeType == "circle")
            || ((latitude >= c.MinLat && latitude <= c.MaxLat)
            && (letitude <= c.MaxLng && letitude >= c.MinLng))
            )
            orderby a.PlotKind
            select a.Id
            ).ToArray<let>();

            if (plot.lenght > 0){


            foreach (let plotId in plot){
            if (FindPolet(latitude, letitude, Instance.listofPolyVertices.Where(c => c.ZoneId == plotId).ToList())){
            zoneId = plotId;
            break;

            }

            }

            }

            }*/

                /*
            if (zoneId == 0 && latitude > 0){

            if (Instance.objPolicy.EnablePOI.ToBool()){

            try{
            returnLoc = db.PostCodesNearLatLong(latitude, letitude).FirstOrDefault().DefaultIfEmpty().Street.ToStr();

            if (returnLoc.Length > 0){

            postcode = GetPostCodeMatchWithBase(returnLoc, true);

            if (!let.IsNullOrEmpty(returnLoc) && let.IsNullOrEmpty(postcode)){
            postcode = GetPostCodeMatch(returnLoc);

            }
            }
            }
            catch{

            }
            }




            if (!let.IsNullOrEmpty(returnLoc)){

            if (Instance.objPolicy.AutoZonePlotType.ToStr() == "postcode"){
            newZoneName = GetHalfPostCodeMatch(returnLoc.ToStr().ToUpper());
            }

            if (let.IsNullOrEmpty(newZoneName)){

            let Loclet =[];
            Loclet.push(returnLoc.toUpper().split(','));

           var count= Method.ReturnIntValueByList(Loclet);
            if (count == 4){
            newZoneName = Loclet[1].Split(' ').LastOrDefault().ToUpper();
            }
            else if (Loclet.lenght == 3){
            newZoneName = Loclet[1].Split(' ').LastOrDefault().ToUpper();
            }
            else if (Loclet.lenght == 5){
            newZoneName = Loclet[2].Split(' ').LastOrDefault().ToUpper();
            }
            else if (Loclet.lenght == 6){
            newZoneName = Loclet[4].Split(' ').LastOrDefault().ToUpper();
            }
            else if (Loclet.lenght == 1){
            newZoneName = postcode;

            if (newZoneName.ToStr().Length == 0){
            try{
            if (Loclet[0].includes(" ")){
            newZoneName = Loclet[0].Split(new char[] { ' ' })[0];
            }
            else{
            newZoneName = Loclet[0].ToStr();

            }
            }
            catch{


            }

            }
            }
            else if (Loclet.lenght == 2){
            newZoneName = Loclet[0].Split(' ').LastOrDefault().ToUpper();
            }
            else if (Loclet.lenght == 0){
            newZoneName = postcode;
            }
            }





            if (newZoneName.ToStr().Length > 0)
            zoneId = -1;


            }
            }

            if (zoneId > 0 || zoneId == -1){
            Fleet_Driver_Location item = db.Fleet_Driver_Locations.FirstOrDefault(c => c.DriverId == driverId);

            if (item != null){
            hasChanges = false;
            item.LocationName = "";

            if ((item.DisableAutoPlotting.ToBool() == false || Instance.objPolicy.EnableFixedPlotting == false)){





            if ((item.ZoneId == null)
            || (item.ZoneId != null && item.NewZoneName != "SIN BIN")
            || (item.ZoneId != null && item.NewZoneName == "SIN BIN"
            && item.PlotDate != null
            && item.SinBletillOn != null && item.SinBletillOn.Value < Let.Now)
            ){




            //let[] plot = (from a in listOfZone.Where(c =>
            //          (c.shapeType != "" && c.shapeType == "circle")
            //           || ((latitude >= c.MinLat && latitude <= c.MaxLat)
            //                     && (letitude <= c.MaxLng && letitude >= c.MinLng))



            //                     )
            //              orderby a.PlotKind

            //              select a.Id
            //           ).ToArray<let>();

            //if (plot.lenght > 0)
            //{


            //    foreach (let plotId in plot)
            //    {
            //        if (FindPolet(latitude, letitude, listofPolyVertices.Where(c => c.ZoneId == plotId).ToList()))
            //        {
            //            zoneId = plotId;
            //            break;

            //        }

            //    }

            //}
            }
            //else
            //{
            //    zoneId = -1;

            //}

            if (zoneId > 0 && (item.PrevZoneId.Tolet() != zoneId || item.ZoneId == null)){



            hasChanges = true;

            item.PrevZoneId = zoneId;
            item.ZoneId = zoneId;

            //   item.PreviousZone = "";
            item.NewZoneName = "";
            item.PlotDate = Let.Now;

            }

            if (zoneId == -1){
            if (let.IsNullOrEmpty(item.PreviousZone) || item.PreviousZone.ToStr() != newZoneName.ToStr()){
            hasChanges = true;

            item.PreviousZone = newZoneName;
            item.NewZoneName = newZoneName;
            item.ZoneId = null;
            //  item.PreviousZone = null;
            item.PlotDate = Let.Now;
            }
            }


            }


            if (hasChanges){
            db.SubmitChanges();



            BroadCastMessage("**refresh plots");

            try{
            Clients.Caller.requestZoneUpdates("true");
            }
            catch{

            }

            }
            }
            }*/



           // }
           // catch (ex){
            //     WriteLog("LOG X:" + Let.Now.ToStr() + ":" + ex.Message + Environment.NewLine);



           // }





            


            
           // }
           // catch (ex){
            //try{
            //lastSaveDriverLocationTimeout = dt.setsSeconds(4);
           // retryDriverLocTimeout++;
            //   File.AppendAllText("excep_savedriverlocation.txt", Let.Now.Tolet("dd/MM/yyyy HH:mm:ss tt") + ":" + dataValue + " ," + ex.Message + ",retrycnt:" + retryDriverLocTimeout.ToStr() + Environment.NewLine);
           // }
           // catch{

           // }

            //    if (retryDriverLocTimeout >= 3)
            //        RestartProgram();
          //  }
            //}
            
            // GC.Collect();

           // return;
            // } catch (ex) {
            //Clients.Caller.exceptionOccured(ex.Message);
            // }
   // }
}


requestShiftLogin(dataValue)
{
    try
    {
        var map = new Map();
        let conn = db.ConnectToSQL();
        console.log('func k andr agya '+dataValue);
     
        try
        {
    //         const words = mesg.message.split('=');
    //         // console.log(words[0].Lat);
    // for(var i=0;i<words.length;i++)
    // {
    // //console.log(words[i]);
    
    //     values.push(words[i]);
    // } 
            const values=[];
         
            const dv=dataValue.split(',');
           // var valcount=0;
            
            for(var i=0; i<dv.length;i++)
            {

                values.push(dv[i]);
               // console.log(dv[i]);
                
            }
            //console.log('value me agya record'+values[0]);

           var valueCnt = values.length;

            let driverId = values[1];
            let driverno = values[2];
            let password = values[3];
            
            let alertMsg = null;
             
            
            
            let fleetMasterId = 0;
            let jparse_objdriver=[];
            //using (TaxiDataContext db = new TaxiDataContext())
           // {
                try
                {
                    console.log(' try me agya ');
                  const lst_=[];
                   conn.connect().then(request => {
                    let sp_ = null;
                    conn
                    .request()
                    .query(
                        `select * from Fleet_Driver where Id = `+driverId+` and LoginPassword = `+password+` and IsActive = 1 `,
                        (err, result) => {
                       
                        if (err) {
                              console.log('Failed to open a SQL Database connection.', err.stack);
                            Logs.CreateLog(
                                "Failed to open a SQL Database connection.\n" + err.stack,
                                "0",
                                "requestshiftLogin");
                        } else {
                         
                          const a=JSON.stringify(result.recordset).split(',');
                        
                         
                          jparse_objdriver.splice(0,  jparse_objdriver.length); //[]
                           for(var i=0;i<a.length;i++)
                           {
                             
                                jparse_objdriver.push(a[i]);

                          }
                          _objDriver = JSON.parse(jparse_objdriver);
                         // console.log('full : '+_objDriver[0].Id);
                        
                            
                        }
                        
                    });
    
                    //console.log(request+' Connected');
                });
            
                
                  
                    if (_objDriver!=null && _objDriver.length>0)
                    {   console.log(' obj driver ka data le aya '+_objDriver[0].Id);
                        var now = new Date();
                        
                            
                        if (_objDriver[0].PDALoginBlocked==false)
                        {
                            console.log('pda false he');
                            msg = msg.replace("true", "");
                            msg += `Your Rent is due. Please call office,`;
                        }
                        else
                        {
                            console.log('pda true he');
                            
                            console.log('RentLimit:>  '+_objDriver[0].RentLimit);
                            if (_objDriver[0].RentLimit==null)//RentLimit
                            {  
                                console.log('RentLimit:>  k andar agya '+_objDriver[0].RentLimit);
                                //---
                                conn.connect().then(request => {
                                    console.log(' in sql');
                                    conn
                                    .request()
                                    .query(
                                        `select isnull(sum(Balance),0.000000) as [Balance] from Fleet_DriverCommision where DriverId =`+driverId+`    `,
                                        (err, result) => {

                                        // Logs.CreateLog(getobj(result), socket.id, "Fetch_record_by_sql");
                                        if (err) {
                                            //   console.log('Failed to open a SQL Database connection.', err.stack);
                                            Logs.CreateLog(
                                                "Failed to open a SQL Database connection.\n" + err.stack,
                                                "0",
                                                "requestshiftLogin");
                                                console.log(' query error : '+err);
                                        } else {
                                         
                                         currentBalance.splice(0,currentBalance.length);
                                         const a=JSON.stringify(result.recordset).split(',');
                        //const a=JSON.stringify(result.recordset).split(',');
                                           let ab=[];
                                           
                                             for(var i=0;i<a.length;i++)
                                             {
                                              
                                                ab.push(a[i]);
                                              //  console.log(' >> '+a[i]);
                 
                                            }
                                         
                                            currentBalance = JSON.parse(ab);
                                      
                                            
                                        }
                                    
                                    });
                                  
                                }).catch((ex)=>{
                                    console.log(' not connected:  '+ex);
                                });

                                

                            }
                            

                           if(currentBalance[0].Balance !=null)
                           {
                            if (currentBalance[0].Balance > 0 &&  _objDriver[0].RentLimit<currentBalance[0].Balance)
                            {
                                try
                                {
                                    console.log(' balance wali condition me he ');
                                    msg = msg.replace("true", "");
                                    msg += `Your commission amount limit exceed please contact office,`;

                                    Logs.CreateLog(" Error: "+DateTime.Now.ToStr() + ":" + "Driver : " + _objDriver[0].DriverNo.ToStr() + " , Limit: " + _objDriver[0].RentLimit.Tolet() + ", Balance" + currentBalance, 0, "requestShiftLogin");
                                   // File.AppendAllText(physicalPath + "\\rentlimitlog.txt", DateTime.Now.ToStr() + ":" + "Driver : " + _objDriver.DriverNo.ToStr() + " , Limit: " + _objDriver.RentLimit.Tolet() + ", Balance" + currentBalance + Environment.NewLine);
                                }
                                catch (er)
                                {
                                    console.log(' balance wali condition me error:  '+er);
                                }
                            }
                        }

                            console.log(' PDALoginBlocked wala function end ho gia  ');
                        }

                        let expiredmsg = '';




                        if (msg == "true")
                        {
                            console.log(' msg true he ');
                            console.log(' _objDriver[0].DrivingLicenseExpiryDate: '+_objDriver[0].DrivingLicenseExpiryDate);
                            if (_objDriver[0].DrivingLicenseExpiryDate!=null)
                            {
                                if (_objDriver[0].DrivingLicenseExpiryDate.value<now)
                                {
                                    if (LoginDrvOnExpiredDoc == false)
                                    {
                                        msg = msg.replace("true", "");
                                        msg += `Driving License is Expired` + `,`;
                                    }
                                    else
                                    {
                                        expiredmsg += `Driving License is Expired : ` + `_objDriver.DrivingLicenseExpiryDate.Value) ,`;
                                    }
                                }

                                if (_objDriver[0].DrivingLicenseExpiryDate  >= now.Date && _objDriver[0].DrivingLicenseExpiryDate <= now.addDays(this.LicenseDays))
                                {
                                    if (alertMsg!=null)
                                        alertMsg = "alert";

                                    alertMsg += ",License expiry=_objDriver.DrivingLicenseExpiryDate)";
                                }
                            }
                            console.log(' _objDriver[0].InsuranceExpiryDate:  '+_objDriver[0].InsuranceExpiryDate);
                            if (_objDriver[0].InsuranceExpiryDate != null)
                            {
                                if (_objDriver[0].InsuranceExpiryDate.Value < now)
                                {
                                    if (LoginDrvOnExpiredDoc == false)
                                    {
                                        msg = msg.replace("true", "");
                                        msg += "Driver Insurance is Expired" + ",";
                                    }
                                    else
                                    {
                                        expiredmsg += "Driver Insurance is Expired : " + " _objDriver.InsuranceExpiryDate.Value) ";
                                    }
                                }

                                if (_objDriver[0].InsuranceExpiryDate >= now.Date && _objDriver[0].InsuranceExpiryDate <= now.addDays(this.InsuranceDays))
                                {
                                    if (alertMsg!=null)
                                        alertMsg = "alert";

                                    alertMsg += ",Insurance Expiry= _objDriver.InsuranceExpiryDate)";
                                }
                            }
                            console.log(' _objDriver[0].MOTExpiryDate:  '+_objDriver[0].MOTExpiryDate);
                            if (_objDriver[0].MOTExpiryDate != null)
                            {
                                if (_objDriver[0].MOTExpiryDate.Value < now)
                                {
                                    if (LoginDrvOnExpiredDoc == false)
                                    {
                                        msg = msg.replace("true", "");
                                        msg += "Driver MOT is Expired" + ",";
                                    }
                                    else
                                    {
                                        expiredmsg += "Driver MOT is Expired : "+_objDriver[0].MOT2ExpiryDate+",";
                                    }
                                }

                                if (_objDriver[0].MOTExpiryDate >= now.Date && _objDriver[0].MOTExpiryDate <= now.addDays(this.MOTDays))
                                {
                                    if (alertMsg!=null)
                                        alertMsg = "alert";

                                    alertMsg += ",MOT Expiry= "+_objDriver[0].MOT2ExpiryDate;
                                }
                            }
                            console.log(' _objDriver[0].MOT2ExpiryDate:  '+_objDriver[0].MOT2ExpiryDate);
                            if (_objDriver[0].MOT2ExpiryDate != null)
                            {
                                if (_objDriver[0].MOT2ExpiryDate.Value < now && LoginDrvOnExpiredDoc == false)
                                {
                                    msg = msg.replace("true", "");
                                    msg += "Driver MOT 2 is Expired" + ",";
                                }

                                if (_objDriver[0].MOT2ExpiryDate >= now.Date  && _objDriver[0].MOT2ExpiryDate <= now.addDays(this.MOT2Days))
                                {
                                    if (alertMsg!=null)
                                        alertMsg = "alert";

                                    alertMsg += ",MOT 2 Expiry= "+_objDriver[0].MOT2ExpiryDate;
                                }
                            }
                            console.log('_objDriver.PCODriverExpiryDate:  '+_objDriver.PCODriverExpiryDate);
                            if (_objDriver.PCODriverExpiryDate != null)
                            {
                                if (_objDriver.PCODriverExpiryDate.Value < now)
                                {
                                    if (LoginDrvOnExpiredDoc == false)
                                    {
                                        msg = msg.replace("true", "");
                                        msg += "Driver PCO is Expired" + ",";
                                    }
                                    else
                                    {
                                        expiredmsg += "Driver PCO is Expired : "+_objDriver[0].MOT2ExpiryDate.value;
                                    }
                                }

                                if (_objDriver[0].PCODriverExpiryDate >= now.Date && _objDriver[0].PCODriverExpiryDate <= now.AddDays(this.PHCDriverDays))
                                {
                                    if (alertMsg!=null)
                                        alertMsg = "alert";

                                    alertMsg += ",PCO Expiry= "+_objDriver[0].PCODriverExpiryDate;
                                }
                            }
                            console.log('_objDriver.PCOVehicleExpiryDate :  '+_objDriver.PCOVehicleExpiryDate);
                            if (_objDriver.PCOVehicleExpiryDate != null)
                            {
                                if (_objDriver.PCOVehicleExpiryDate.Value < now)
                                {
                                    if (LoginDrvOnExpiredDoc == false)
                                    {
                                        msg = msg.replace("true", "");
                                        msg += "Driver Vehicle PHC is Expired";
                                    }
                                    else
                                    {
                                        expiredmsg += "Driver Vehicle PHC is Expired : "+_objDriver[0].PCOVehicleExpiryDate.Value+ ",";
                                    }
                                }

                                if (_objDriver[0].PCOVehicleExpiryDate >= now.Date  && _objDriver[0].PCOVehicleExpiryDate <= now.AddDays(this.PHCVehicleDays))
                                {
                                    if (alertMsg!=null)
                                        alertMsg = "alert";

                                    alertMsg += ",Vehicle PHC Expiry= "+_objDriver[0].PCOVehicleExpiryDate;
                                }
                            }
                            console.log('expiredmsg :  '+expiredmsg);
                            if (expiredmsg==null)
                            {
                                let pdaMsg = "request pda=" + _objDriver[0].Id + "=" + 0 + "="
                                                     + "Message>>" + expiredmsg + ">>" + now + "=4";

                                let splitArr =[];
                                for(var i=0;i<pdaMsg.lenght;i++)
                                {
                                    
                                splitArr.push(pdaMsg[i].split("="));
                                }
                                

                                clsPDA.DriverId = splitArr[1];
                                clsPDA.JobId = 0;
                                clsPDA. MessageDateTime = now.addseconds(-50);
                                clsPDA. JobMessage = splitArr[3];
                                clsPDA. MessageTypeId = splitArr[4];
                                 listofJobs.push(clsPDA);
                                
                            }
                            console.log('valueCnt :  '+valueCnt);
                            if (valueCnt > 6)
                            {
                                console.log('valueCnt k andar agya  :  '+valueCnt);
                                let vehNo = values[4];
                                console.log('vehNo  :  '+values[4]);
                                if (vehNo.ToStr() =="9988")
                                {
                                    //let vehNo = "APH-319411";
                                  
                                    // var objFleet = db.Fleet_Masters().find(c => c.Plateno == vehNo);
                                    conn.connect().then(request => {
                                        let sp_ = null;
                                        conn
                                        .request()
                                        .query(
                                            `select * from Fleet_Master where Plateno = `+vehNo+`  `,
                                            (err, result) => {
                                            // Logs.CreateLog(getobj(result), socket.id, "Fetch_record_by_sql");
                                            if (err) {
                                                //   console.log('Failed to open a SQL Database connection.', err.stack);
                                                Logs.CreateLog(
                                                    "Failed to open a SQL Database connection.\n" + err.stack,
                                                    "0",
                                                    "requestshiftLogin");
                                            } else {
                                               // response.send(result);
                                            
                                          
                                        objFleet.splice(0,objFleet.length);
                                        const a=JSON.stringify(result.recordset).split(',');
                       
                                          let ab=[];
                                          
                                            for(var i=0;i<a.length;i++)
                                            {
                                             
                                               ab.push(a[i]);
                                             //  console.log(' >> '+a[i]);
                
                                           }
                                        
                                           objFleet = JSON.parse(ab);
                                                
                                            }
                                           // console.log('Fleet Driver ka record he '+_objDriver.lenght);
                                        });
                        
                                        //console.log(request+' Connected');
                                    });

                                    if (objFleet.lenght==0)
                                    {
                                        msg = msg.replace("true", "");
                                        msg += "Invalid Vehicle Plate No";
                                    }
                                    else
                                    {
                                       
                                        conn.connect().then(request => {
                                            let sp_ = null;
                                            conn
                                            .request()
                                            .query(
                                                `select isnull(Fleet_DriverQueueList,0) as Fleet_DriverQueueList from Fleet_DriverQueueList where DriverId != `+driverId+` and Status=1 `,
                                                (err, result) => {
                                                // Logs.CreateLog(getobj(result), socket.id, "Fetch_record_by_sql");
                                                if (err) {
                                                    //   console.log('Failed to open a SQL Database connection.', err.stack);
                                                    Logs.CreateLog(
                                                        "Failed to open a SQL Database connection.\n" + err.stack,
                                                        "0",
                                                        "requestshiftLogin");
                                                } else {
                                                   // response.send(result);
                                                   //=result;

                                                   objFleet_Fleet_DriverQueueLists.splice(0,objFleet_Fleet_DriverQueueLists.length);
                                                   const a=JSON.stringify(result.recordset).split(',');
                                  
                                                     let ab=[];
                                                     
                                                       for(var i=0;i<a.length;i++)
                                                       {
                                                        
                                                          ab.push(a[i]);
                                                        //  console.log(' >> '+a[i]);
                           
                                                      }
                                                   
                                                      objFleet_Fleet_DriverQueueLists = JSON.parse(ab);
                                                }
                                            });
                            
                                            //console.log(request+' Connected');
                                        });
                                        //DateTime prevDate = DateTime.Now.AddDays(-1);
                                        if (objFleet_Fleet_DriverQueueLists[0].Fleet_DriverQueueList > 0)
                                        {
                                            msg = msg.replace("true", "");
                                            msg += "Vehicle already in use";
                                        }
                                        else
                                        {
                                            fleetMasterId = objFleet[0].Id;
                                        }
                                    }
                                }
                            }
                            console.log('mg endwith: 2602'+msg.endsWith(","));

                            if (msg.endsWith(","))
                            {
                                console.log('mg endwith: '+msg);
                                msg = msg.removeItemOnce(msg.lastIndexOf(','));
                            }

                        }
                    }
                    else
                    {
                        msg = "false";
                        console.log(' msg false he  ');
                    }

                   // }
                   //console.log(' objPolicy.PDAVersion is sy bhar he   ');//&& objPolicy.PDAVersion >= 1.7
                    if (msg == "true" && alertMsg==null )
                    {
                        console.log(' message: '+msg);
                        msg = alertMsg;
                    }
                        
                    console.log(' message not !- null  2626: '+msg);
                        if(msg!=null)
                        {

                            console.log(' message not !- null  2630: '+msg);
                    if (msg == "true" )
                    {
                        console.log(' message not !- null  2632: '+msg);
                       var IsExist = false;



                        IsExist = true;

                     

                        if (!IsExist)
                        {
                            if (msg.startsWith("true"))
                            {
                                console.log('   2646: '+msg);
                                msg = msg.replace("true", "overshift");
                            }
                            else if (msg.startsWith("alert,"))
                            {
                                console.log(' message not !- null  2650: '+msg);
                                msg = msg.replace("alert", "overshift");
                            }
                            console.log(' message not !- null  2652: '+msg);
                        }
                    }
                
                    console.log(' true or false check krye ga   2658: '+msg);
                    if (msg != "true" && msg != "false" )
                        msg += ">> ";

                        console.log(' true or false check krye ga   2662: '+msg);
                    //send message back to PDA
                    console.log('shiftLogin: 2664> '+msg)
                    Method.shiftLogin(msg);
                   
                    console.log(' shiftLogin: 2667> : '+msg);
                    
                   // GC.Collect();
                       // console.log(msg.startsWith("true") +"||"+ msg.startsWith("alert,")+'  2670');
                    if (msg.startsWith("true") )
                    {
                        console.log('  2673');
                        let pdaversion = null;

                        if ( (values.lenght == 5 || values.lenght == 7))
                        {
                            console.log(' ver update ho gia  2678');
                            let ver = [];

                            if (values.lenght == 5)
                            {
                                ver.push(values[4].replace("ver=", ""));
                            }
                            else if (values.lenght == 7)
                            {
                                ver.push(values[6].replace("ver=", ""));

                                if (ver.ToStr().Length > 0 && ver.some(i => !Number.isInteger(i)) == false)
                                {
                                    let verVal = ver.ToStr();
                                    //ver = null;
                                    for (let i = 0; i <  ver.lenght; i++)
                                    {
                                        if ( ver[i] == ".")
                                        {
                                            ver.push( ver[i].ToStr());

                                        }
                                        else
                                            break;
                                    }
                                }
                            }

                            if (ver.lenght <0)
                            {
                                if (ver.some(i => !Number.isInteger(i)))
                                {
                                    pdaversion = ver;
                                }
                                else
                                {
                                    if (ver.includes("."))
                                    {
                                        try
                                        {let arr=[];
                                            for(var i=0;i<ver.lenght;i++)
                                            {
                                                arr.push(ver[i].Split('.'));
                                            }
                                            

                                            if (arr[0].some(i => !Number.isInteger(i)))
                                            {
                                                pdaversion = arr[0];
                                            }

                                            if (arr.lenght > 1 )
                                            {
                                                pdaversion = (arr[0] + "." + arr[1][0] + arr[1][1]);
                                            }
                                        }
                                        catch
                                        {

                                        }
                                    }
                                }
                            }
                        }
                        //let pdaversion = '';
                        console.log(' fleetMasterId: > : '+fleetMasterId);
                        if (fleetMasterId == 0)
                        {
                            console.log(' stp_LoginLogoutDriver: > : '+driverId, true, '');
                            db.stp_LoginLogoutDriver(driverId, true, '');
                            console.log("stp_LoginLogoutDriver");
                        }
                        else
                        { console.log(' stp_LoginLogoutDriverVeh: > : '+driverId, fleetMasterId, true, '');
                            db.stp_LoginLogoutDriverVeh(driverId, fleetMasterId, true, '');
                            console.log("stp_LoginLogoutDriverVeh");
                        }
                        console.log(' valueCnt == : > : '+valueCnt );
                        if (valueCnt == 6)
                        {
                            db.stp_UpdateDriverDeviceId(driverId, values[5].ToStr());
                            console.log("stp_UpdateDriverDeviceId");
                        }
                        console.log("**login>>Drv " + driverno + " is Login" + ">>" + driverno);
                        Method.shiftLogin("**login>>Drv " + driverno + " is Login" + ">>" + driverno);
                        console.log( "msg send ho gia ");
                        
                    }
                }
                console.log(' fleetMasterId: > 2765: '+fleetMasterId);
                        if (fleetMasterId == 0)
                        {
                            console.log(' stp_LoginLogoutDriver: >2769 : 2769'+driverId+'--'+true+'--'+'-'+'');
                            db.stp_LoginLogoutDriver(driverId, true, '');
                            console.log("stp_LoginLogoutDriver");
                        }
                        else
                        { console.log(' stp_LoginLogoutDriverVeh: > : 2774'+driverId, fleetMasterId, true, '');
                        console.log("stp_LoginLogoutDriverVeh > 2775");
                        db.stp_LoginLogoutDriverVeh(driverId, fleetMasterId, true, '');
                            console.log("stp_LoginLogoutDriverVeh 2777");
                        }
                        console.log(' valueCnt == : > 2779: '+valueCnt );
                        if (valueCnt == 6)
                        {
                            console.log("stp_LoginLogoutDriverVeh > 2782");
                            db.stp_UpdateDriverDeviceId(driverId, values[5].ToStr());
                            console.log("stp_UpdateDriverDeviceId");
                        }
                        console.log("**login>>Drv " + driverno + " is Login" + ">>" + driverno+'2786');
                        Method.shiftLogin("**login>>Drv " + driverno + " is Login" + ">>" + driverno);
                        console.log( "msg send ho gia 2788");
                }
                catch (ex)
                {
                   
                    console.log( "error: in function "+ex+'  2769');
                    Logs.CreateLog(" request shift error \n\n"+ex, 0, "request shift error");
    
                }

            //}
        }
        catch (ex)
        {
            console.log( "main error 2777 "+ex.code);
            Method.shiftLogin(ex);
        }
        // });
    }
    catch (ex)
    {
        Method.shiftLogin(ex);
    }
}
}

module.exports = new SocketHub();
