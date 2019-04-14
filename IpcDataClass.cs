using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Remoting.Channels.Ipc;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting;

using Golden_Raito_data;

namespace Golden_Raito_ruler
{

    class IpcComunication
    {
        //private IpcServerChannel SrvChannel;
        //private IpcClientChannel CltChannel;

        /// <summary>
        /// プロセス間通信で共有するクラス
        /// </summary>
        public IpcDataClass shareData;

        protected const string channel_name = "Golden_Raito_Ruler";
        protected const string channel_public_name = "status";

        /// <summary>
        /// パラメータが変わったとき
        /// </summary>
        public event EventHandler MessageReceive;

        protected string address
        {
            get
            {
                return string.Format("ipc://{0}/{1}", channel_name, channel_public_name);
            }
        }

        protected void recieve(object sender, EventArgs e)
        {
            MessageReceive?.Invoke(sender, e);
        }

    }

    class IpcServer : IpcComunication
    {
        private IpcServerChannel SrvChannel;

        /// <summary>
        /// ipcサーバを開始。
        /// </summary>
        /// <returns>サーバの開始に成功した場合はtrue</returns>
        public bool startServer(string mes="")
        {

            shareData = new IpcDataClass
            {
                status = mes
            };

            try
            {
                SrvChannel = new IpcServerChannel(channel_name);
                ChannelServices.RegisterChannel(SrvChannel, true);
                RemotingServices.Marshal(shareData, channel_public_name);

                shareData.changeValue += recieve;
            }
            catch(RemotingException)
            {
                return false;
            }
            //shareData.callEvent();

            return true;
        }

    }

    class IpcClient :IpcComunication
    {

        private IpcClientChannel CltChannel;
        /// <summary>
        /// ipcクライアントとして接続。サーバが起動されてない場合はfalseを返す。
        /// </summary>
        /// <returns>サーバが起動してるか</returns>
        public bool startClient(string status = "")
        {

            CltChannel = new IpcClientChannel();
            ChannelServices.RegisterChannel(CltChannel, true);

            try
            {
                shareData = (IpcDataClass)Activator.GetObject(typeof(IpcDataClass), address);

                if (status != "")
                {
                    shareData.status = status;
                    //shareData.callEvent();
                }

                //shareData.changeValue += recieve;
            }
            catch (ArgumentException)
            {
                return false;
            }
            //書き込めない->サーバが存在しない
            catch (RemotingException)
            {
                return false;
            }
            catch (InvalidOperationException)
            {
                return true;
            }

            return true;
        }
    }
}

namespace Golden_Raito_data {

    //public delegate void CallEventHandler(object sender,changeValueEvent e);

    public class IpcDataClass : MarshalByRefObject
    {
        private string inner_status;
        public event EventHandler changeValue;

        public string status
        {
            get
            {
                return inner_status;
            }
            set
            {
                inner_status = value;
                callEvent();
            }
        }

        public void callEvent()
        {
            //inner_status = newstatus;
            changeValueEvent e = new changeValueEvent()
            {
                newValue = status
            };
            changeValue?.Invoke(this, e);
        }
    }

    public class changeValueEvent : EventArgs
    {
        public string newValue;
    }

}
