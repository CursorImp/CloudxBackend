
namespace SignalRHub
{
    public enum SignalRClientsType
    {
        WEBBROWSER = 1,
        DESKTOP = 2,
        ANDROID = 3,
        IOS = 4,
        ANONYMOUS = 5
    }

    public enum SignalRClientsStatus
    {
        CONNECTED =1,
        DISCONNECTED = 2,
        RECONNECTING =3,
    }

    public enum SignalRUserType
    {
        WEB = 1,
        DESKTOP = 2,
        DRIVER = 3,
        CLIENT = 4,
    }
}