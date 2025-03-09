var fs = require("fs");
var stream;
var dateFormat = require("dateformat");
class Logs {
  CreateLog(log, socketid, fun_name) {
    // var datetime = new Date().toISOString().replace(/T/, ' ').replace(/\..+/, '');
    var datetime = dateFormat(new Date(), "yyyy-mm-dd h-MM-ss tt");
    //
    var dir_logs_dt = "C://Project/ChilternTaxis/ChilternTaxisSocketio/logs/" ;
    if (!fs.existsSync(dir_logs_dt)) {
      fs.mkdirSync(dir_logs_dt);
    }

    stream = fs.createWriteStream(
      dir_logs_dt + "/" + fun_name + "--" + datetime + ".txt"
    );

    stream.write(datetime + "- [" + socketid + "] \n\n " + log);
  }
}

module.exports = new Logs();
