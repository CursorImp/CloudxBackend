let ClientData = {
  _ConnectedOn: "",
  _IPAddress: "",
  _ConnectionStatus: "",
  _ConnectionID: "",
  _UserId: "",

  _UserType: "",

  get ConnectedOn() {
    return this._ConnectedOn;
  },
  set ConnectedOn(value) {
    this._ConnectedOn = value;
  },

  get IPAddress() {
    return this._IPAddress;
  },
  set IPAddress(value) {
    this._IPAddress = value;
  },

  get ConnectionStatus() {
    return this._ConnectionStatus;
  },
  set ConnectionStatus(value) {
    this._ConnectionStatus = value;
  },

  get ConnectionID() {
    return this._ConnectionID;
  },
  set ConnectionID(value) {
    this._ConnectionID = value;
  },
  get UserId() {
    return this._UserId;
  },
  set UserId(value) {
    this._UserId = value;
  },

  get UserType() {
    return this._UserType;
  },
  set UserType(value) {
    this._UserType = value;
  }
};

module.exports = ClientData;
