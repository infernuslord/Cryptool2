using System;
using System.Text;
using System.Net.Sockets;
using System.Threading;
using System.Net;
using System.Collections.Generic;

class CryptoolServer
{
    public int Port {get;set;}
    
    public delegate void JobCompletedDelegate(EndPoint ipep, JobResult j);
    public event JobCompletedDelegate OnJobCompleted;


    public delegate void ClientConnectedDelegate(EndPoint ipep, String identification);
    public event ClientConnectedDelegate OnClientConnected;

    public delegate void ClientDisconnectedDelegate(EndPoint ipep);
    public event ClientDisconnectedDelegate OnClientDisconnected;

    private Dictionary<EndPoint, TcpClient> connectedClients = new Dictionary<EndPoint, TcpClient>();

    ///<summary>
    /// Starts the server. Will block forever, you might want to start this in an additional thread.
    ///</summary>
    public void Run()
    {
        var tcpListener = new TcpListener(IPAddress.Any, Port);
        tcpListener.Start();
        Console.WriteLine("Listening for client on port "+Port);
        while(true)
        {
            TcpClient client = tcpListener.AcceptTcpClient();
            Console.WriteLine("Got connection from "+client);
            lock(connectedClients)
            {
                connectedClients.Add(client.Client.RemoteEndPoint, client);
            }
            Thread clientThread = new Thread(new ParameterizedThreadStart(HandleClient));
            clientThread.Start(client);
        }
    }


    public void SendJob(JobInput j, EndPoint i)
    {
        TcpClient client = null;
        lock(connectedClients)
        {
            if(!connectedClients.TryGetValue(i, out client))
                throw new ArgumentException("Not connected to "+i);
        }

        lock(client)
        {
            var wrapped = new PlatformIndependentWrapper(client);

            wrapped.WriteInt((int)ServerOpcodes.NEW_JOB);
            wrapped.WriteString(j.Guid);
            wrapped.WriteString(j.Src);
            wrapped.WriteInt(j.Key.Length);
            wrapped.WriteBytes(j.Key);
            wrapped.WriteInt(j.LargerThen ? 1 : 0);
            wrapped.WriteInt(j.Size);
            wrapped.WriteInt(j.ResultSize);
        }
    }

    private void HandleClient(object obj)
    {
        var client = obj as TcpClient;
        EndPoint ep = client.Client.RemoteEndPoint;

        try
        {
            var wrapped = new PlatformIndependentWrapper(client);
            while(true)
            {
                switch((ClientOpcodes)wrapped.ReadInt())
                {
                    case ClientOpcodes.HELLO:
                        {
                            String identification = wrapped.ReadString();
                            if(OnClientConnected != null)
                                OnClientConnected(ep, identification);
                        }
                        break;

                    case ClientOpcodes.JOB_RESULT:
                        {
                            var jobGuid = wrapped.ReadString();
                            var resultList = new SortedDictionary<float, int>();
                            var resultListLength = wrapped.ReadInt();
                            for (int c = 0; c < resultListLength; c++)
                            {
                                var key = wrapped.ReadInt();
                                var cost = wrapped.ReadFloat();
                                resultList.Add(cost, key);
                            }

                            JobResult rs = new JobResult();
                            rs.Guid = jobGuid;
                            rs.ResultList = resultList;

                            if(OnJobCompleted != null)
                            {
                                OnJobCompleted(ep, rs);
                            }
                        }
                        break;
                }
            }
        }
        catch(Exception e)
        {
            Console.WriteLine("Client exited with exception "+e);
        }

        lock(connectedClients)
        {
            connectedClients.Remove(ep);
        }

        if(OnClientDisconnected != null)
            OnClientDisconnected(ep);
    }

}
