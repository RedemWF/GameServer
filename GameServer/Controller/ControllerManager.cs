
using System.Reflection;
using Common;
using GameServer.Servers;

namespace GameServer.Controller
{
    /// <summary>
    /// 控制器管理器
    /// </summary>
    class ControllerManager
    {
        //控制器字典
        private Dictionary<RequestCode, BaseController> controllerDict = new Dictionary<RequestCode, BaseController>();
        private Server server;

        public ControllerManager(Server server)
        {
            this.server = server;
            InitController();
        }
        /// <summary>
        /// 初始化控制器
        /// </summary>
        void InitController()
        {
            DefaultController defaultController = new DefaultController();
            controllerDict.Add(defaultController.RequestCode, defaultController);
            controllerDict.Add(RequestCode.User, new UserController());
            controllerDict.Add(RequestCode.Room, new RoomController());
            controllerDict.Add(RequestCode.Game, new GameController());
        }
        /// <summary>
        /// 根据请求执行方法
        /// </summary>
        /// <param name="requestCode">请求代码</param>
        /// <param name="actionCode">执行代码</param>
        /// <param name="data">数据</param>
        /// <param name="client">客户端</param>
        public void HandleRequest(RequestCode requestCode, ActionCode actionCode, string data, Client client)
        {
            BaseController controller;
            //从字典中获取请求对应的控制器
            bool isGet = controllerDict.TryGetValue(requestCode, out controller);
            //判断是否得到
            if (isGet == false)
            {
                Console.WriteLine("无法得到[" + requestCode + "]所对应的Controller,无法处理请求"); return;
            }
            //跟据执行代码调用方法
            string methodName = Enum.GetName(typeof(ActionCode), actionCode);
            //反射获取a方法
            MethodInfo mi = controller.GetType().GetMethod(methodName);
            //判断是否获取到方法
            if (mi == null)
            {
                Console.WriteLine("[警告]在Controller[" + controller.GetType() + "]中没有对应的处理方法:[" + methodName + "]"); return;
            }
            //定义方法参数
            object[] parameters = new object[] { data, client, server };
            //调用方法获取返回值
            object o = mi.Invoke(controller, parameters);
            //判断返回值是否为空
            if (o == null || string.IsNullOrEmpty(o as string))
            {
                return;
            }
            //向客户端发送响应
            server.SendResponse(client, actionCode, o as string);
        }

    }
}
