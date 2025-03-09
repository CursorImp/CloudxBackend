const sql = require("mssql/msnodesqlv8");
const util = require('util');
/*
var config = {
  driver: 'msnodesqlv8',
  server: 'DESKTOP-C9LSU5E',
  database: 'DTS',
  userName: 'sa',
  password: 'eurosoft',
  "options": {
    enableArithAbort: true,
    trustedConnection: true
    }
};*/

const conn = new sql.ConnectionPool({
    database: "Taxi",
    server: "DESKTOP-C9LSU5E",
    driver: "msnodesqlv8",
    options: {
      trustedConnection: true
    }
  });

class db {
    ConnectToSQL() {
        return conn;
    }
    ReturnList(lst=[]) {
        return lst;
    }
    Fleet_DriverQueueLists() {
        var list = [];

        const conn = this.ConnectToSQL();

        conn.connect().then((request) => {

            let sp_ = null;
            var request = new sql.Request(conn);

            conn.request().query('select * from Fleet_DriverQueueList', (err, result) => {
                // Logs.CreateLog(getobj(result), socket.id, "Fetch_record_by_sql");
                if (err) {
                    console.log('Failed to open a SQL Database connection.', err.stack);
                } else {
                    // response.send(result);
                    // console.log(getobj(result));
                    list.push(result);
                }
                // console.log(getobj(result));
            });

            console.log(request + ' Connected');
        }).catch((ex)=>{
            console.log(' not connected:  '+ex);
        });

        return list;

    }
    Bookings() {
        var list = [];

        const conn = this.ConnectToSQL();

        conn.connect().then((request) => {

            let sp_ = null;
            var request = new sql.Request(conn);

            conn.request().query('select * from Bookings', (err, result) => {
                // Logs.CreateLog(getobj(result), socket.id, "Fetch_record_by_sql");
                if (err) {
                    console.log('Failed to open a SQL Database connection.', err.stack);
                } else {
                    // response.send(result);
                    // console.log(getobj(result));
                    list.push(result);
                }
                // console.log(getobj(result));
            });

            //console.log(request+' Connected');
        });

        return list;

    }
     Getobj(res)
{
  return  util.inspect(res, false, null, true /* enable colors */);
}
sqllis(sql_q) {
    const conn = this.ConnectToSQL();
    sql.connect(conn, function (err) {
        if (err) console.log(err);
        var request = new sql.Request();
        return request.query(sql_q);
    });
}
 //Fleet_Drivers
    //SELECT ISNULL(BiddingType,0) FROM Gen_SysPolicy_Configurations
   Fleet_Drivers(driverId,password) {
        var list = [];
        let rec=null;
       const conn = this.ConnectToSQL();
var a=0;
        conn.connect().then((request) => {

            let sp_ = null;
            var request = new sql.Request(conn);

            conn.request().query(`select * from Fleet_Driver where Id = `+driverId+` and LoginPassword = `+password+` and IsActive = 1`, (err, result) => {
                // Logs.CreateLog(getobj(result), socket.id, "Fetch_record_by_sql");
                if (err) {
                    console.log('Failed to open a SQL Database connection.', err.stack);
                } else {
                   // return this.Getobj(result.recordset).split(',');
                  const a=this.Getobj(result.recordset).split(',');
                  
                  for(var i=0;i<a.length;i++)
                  {
                    list.push(a[i]);
                   
                  }
                  console.log('db ki length '+list.length ); 
                  return this.ReturnList(list);
                }
               // a++;
               
                //this.ReturnList(list);
                
                // console.log(getobj(result));
            });

            //console.log(request+' Connected');
        });
       

    }
    
    Fleet_Masters() {
        var list = [];

       const conn = this.ConnectToSQL();

        conn.connect().then((request) => {

            let sp_ = null;
            var request = new sql.Request(conn);

            conn.request().query('select * from Fleet_Master', (err, result) => {
                // Logs.CreateLog(getobj(result), socket.id, "Fetch_record_by_sql");
                if (err) {
                    console.log('Failed to open a SQL Database connection.', err.stack);
                } else {
                    // response.send(result);
                    // console.log(getobj(result));
                    for(var i=0; i<result.recordset.length;i++)
                    { list.push(result.recordset[i]);
                        //console.log(' data retrive from db '+list[i] + ' \n\n list '+result.recordset[i]);        
                    }
                }
                // console.log(getobj(result));
            });

            //console.log(request+' Connected');
        });

        return list;

    }
    Fleet_DriverCommisions() {
        var list = [];

       const conn = this.ConnectToSQL();

        conn.connect().then((request) => {

            let sp_ = null;
            var request = new sql.Request(conn);

            conn.request().query('select * from Fleet_DriverCommision', (err, result) => {
                // Logs.CreateLog(getobj(result), socket.id, "Fetch_record_by_sql");
                if (err) {
                    console.log('Failed to open a SQL Database connection.', err.stack);
                } else {
                    // response.send(result);
                    // console.log(getobj(result));
                    for(var i=0; i<result.recordset.length;i++)
                    { list.push(result.recordset[i]);
                        //console.log(' data retrive from db '+list[i] + ' \n\n list '+result.recordset[i]);        
                    }
                }
                // console.log(getobj(result));
            });

            //console.log(request+' Connected');
        });

        return list;

    }
    
    Gen_SysPolicy_Configurations() {
        var list = [];

       const conn = this.ConnectToSQL();

        conn.connect().then((request) => {

            let sp_ = null;
            var request = new sql.Request(conn);

            conn.request().query('select * from Gen_SysPolicy_Configurations', (err, result) => {
                // Logs.CreateLog(getobj(result), socket.id, "Fetch_record_by_sql");
                if (err) {
                    console.log('Failed to open a SQL Database connection.', err.stack);
                } else {
                    // response.send(result);
                    // console.log(getobj(result));
                    for(var i=0; i<result.recordset.length;i++)
                    { list.push(result.recordset[i]);
                        //console.log(' data retrive from db '+list[i] + ' \n\n list '+result.recordset[i]);        
                    }
                }
                // console.log(getobj(result));
            });

            //console.log(request+' Connected');
        });

        return list;

    }
   
     stp_SaveDriverLocationByZone(DriverId,Latitude,Longitude,Speed,jobId) {
        // var list = [];
        var IsSave=0;
       const conn = this.ConnectToSQL();

        conn.connect().then((request) => {

            
           // console.log('connect to sp');

            var request = new sql.Request(conn);
            // request.execute('stp_SaveDriverLocationByZone '+DriverId+','+Latitude+','+Longitude+','+Speed+','+jobId).then(function (err, recordsets, returnValue, affected) {
            console.log(`server side pe set he `+DriverId+`,`+Latitude+`,`+Longitude+`,`+Speed+`,`+jobId);
           // request.execute(`Sp_demo `+DriverId+`,`+Latitude).then(function (err, recordsets, returnValue, affected) {
            request.execute(`stp_SaveDriverLocationByZone `+DriverId+`,`+Latitude+`,`+Longitude+`,`+Speed+`,`+jobId).then(function (err, recordsets, returnValue, affected) {
             
           console.log('recordsets: '+recordsets);
                console.log('err '+err);
                console.log('returnValue: '+returnValue);
                console.log('affected: '+affected);
                console.log('save 272 db');
            }).catch(function (err) {
                console.log('execute nhi hwa: '+err);
                console.log(' query is not perform action so plz check own query ');
                IsSave=0;
            });
            var query=`stp_SaveDriverLocationByZone `+DriverId+`,`+Latitude+`,`+Longitude+`,`+Speed+`,`+jobId;
            console.log('function run hogia 279 db '+query);
          
        });
        return IsSave;

        // return list;

    }

    //stp_UpdateDriverDeviceId(driverId, values[5].ToStr())
    stp_UpdateDriverDeviceId(driverId, val) {
        // var list = [];

        const conn = this.ConnectToSQL();

        conn.connect().then((request) => {

            let sp_ = null;
           

            var request = new sql.Request(conn);
          
            request.execute(`stp_UpdateDriverDeviceId `+driverId+`,`+val).then(function (err, recordsets, returnValue, affected) {
                console.dir(recordsets);
                
                console.dir(err);
                console.log('save');
            }).catch(function (err) {
                console.log(err);
            });
            
        });

        // return list;

    }
    stp_LoginLogoutDriverVeh(driverId, fleetMasterId,bl, pdaversion) {
        // var list = [];

        const conn = this.ConnectToSQL();

        conn.connect().then((request) => {

            let sp_ = null;
           

            var request = new sql.Request(conn);
            // request.input(JobId);
            // request.input(DriverId);
            // request.input(Price);
            // request.input(i);
            // request.input("");
            // request.input("Job Despatched");
            request.execute(`stp_LoginLogoutDriverVeh `+driverId+`,`+fleetMasterId+`, `+bl+`, `+pdaversion).then(function (err, recordsets, returnValue, affected) {
                console.dir(recordsets);
                
                console.dir(err);
                console.log('save');
            }).catch(function (err) {
                console.log('Error: 338 db file'+err);
            });
            /*conn.request().query('select * from Gen_SysPolicy_Configurations', (err, result) => {
            // Logs.CreateLog(getobj(result), socket.id, "Fetch_record_by_sql");
            if (err) {
            console.log('Failed to open a SQL Database connection.', err.stack);
            } else {
            // response.send(result);
            // console.log(getobj(result));
            list.push(result);
            }
            // console.log(getobj(result));
            });
             */

            //console.log(request+' Connected');
        });

        // return list;

    }
    stp_LoginLogoutDriver(driverId, bl, pdaversion) {
        // var list = [];

        const conn = this.ConnectToSQL();

        conn.connect().then((request) => {

            let sp_ = null;
           

            var request = new sql.Request(conn);
            // request.input(JobId);
            // request.input(DriverId);
            // request.input(Price);
            // request.input(i);
            // request.input("");
            // request.input("Job Despatched");
            pdaversion=12.0
            request.execute(`stp_LoginLogoutDriver `+driverId+`, `+bl+`, `+pdaversion).then(function (err, recordsets, returnValue, affected) {
                console.dir(recordsets);
                
                console.dir(err);
                console.log('save 380');
            }).catch(function (err) {
                console.log('Error:  382'+err);
            });
            /*conn.request().query('select * from Gen_SysPolicy_Configurations', (err, result) => {
            // Logs.CreateLog(getobj(result), socket.id, "Fetch_record_by_sql");
            if (err) {
            console.log('Failed to open a SQL Database connection.', err.stack);
            } else {
            // response.send(result);
            // console.log(getobj(result));
            list.push(result);
            }
            // console.log(getobj(result));
            });
             */

            //console.log(request+' Connected');
        });

        // return list;

    }
    SP_SaveBid(jobId,driverId,bidRate,bidStatusId,driverNo,message) {
        // var list = [];

        const conn = this.ConnectToSQL();

        conn.connect().then((request) => {

            let sp_ = null;
           

            var request = new sql.Request(conn);
          
            request.execute(`stp_UpdateDriverBid `+jobId+`,`+driverId+`,`+bidRate+`,`+bidStatusId+`,`+driverNo+`,`+message).then(function (err, recordsets, returnValue, affected) {
                console.dir(recordsets);
                
                console.dir(err);
            }).catch(function (err) {
                console.log('Error: 425 db file'+err);
            });
            /*conn.request().query('select * from Gen_SysPolicy_Configurations', (err, result) => {
            // Logs.CreateLog(getobj(result), socket.id, "Fetch_record_by_sql");
            if (err) {
            console.log('Failed to open a SQL Database connection.', err.stack);
            } else {
            // response.send(result);
            // console.log(getobj(result));
            list.push(result);
            }
            // console.log(getobj(result));
            });
             */

            //console.log(request+' Connected');
        });

        // return list;

    }

}

module.exports = new db();
