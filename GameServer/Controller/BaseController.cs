using Common;
using GameServer.Servers;
namespace GameServer.Controller
{
    //Controller基类
    public abstract class BaseController
    {
        protected RequestCode requestCode = RequestCode.None;

        public RequestCode RequestCode
        {
            get
            {
                return requestCode;
            }
        }

        public virtual string DefaultHandle(string data, Client client, Servers.Server server) { return null; }
    }
}
