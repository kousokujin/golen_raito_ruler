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
        private IpcServerChannel SrvChannel;
        private IpcClientChannel CltChannel;

        /// <summary>
        /// プロセス間通信で共有するクラス
        /// </summary>
        public IpcDataClass shareData;

        private const string channel_name = "Golden_Raito_Ruler";
        private const string channel_public_name = "status";

        /// <summary>
        /// このインスタンスがサーバとして動作している場合はtrue
        /// </summary>
        public bool isServer;

        /// <summary>
        /// パラメータが変わったとき
        /// </summary>
        public event EventHandler MessageReceive;

        private string address
        {
            get
            {
                return string.Format("ipc://{0}/{1}", channel_name, channel_public_name);
            }
        }

        /// <summary>
        /// ipcサーバを開始。
        /// </summary>
        public void startServer()
        {
            isServer = true;

            shareData = new IpcDataClass
            {
                status = "run"
            };
            SrvChannel = new IpcServerChannel(channel_name);
            ChannelServices.RegisterChannel(SrvChannel, true);
            RemotingServices.Marshal(shareData, channel_public_name);

            shareData.changeValue += recieve;
            //shareData.callEvent();
        }

        private void recieve(object sender, EventArgs e)
        {
            MessageReceive?.Invoke(sender, e);
        }

        /// <summary>
        /// ipcクライアントとして接続。サーバが起動されてない場合はfalseを返す。
        /// </summary>
        /// <returns>サーバが起動してるか</returns>
        public bool startClient(string status = "")
        {
            isServer = false;

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

        /// <summary>
        /// サーバが存在しない場合はサーバとして起動
        /// </summary>
        /// <returns>サーバとして起動した場合はtrue</returns>
        public bool startServer_isExist(string status = "")
        {
            bool isExist = startClient(status);

            if (isExist == false)
            {
                startServer();
                return true;
            }

            return false;
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
