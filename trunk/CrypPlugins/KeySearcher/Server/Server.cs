using System;
using System.Text;
using System.Net.Sockets;
using System.Threading;
using System.Net;
using System.Collections.Generic;

class CryptoolServer
{
    #region Properties

    public int Port {get;set;}

    #endregion

    #region Events

    public delegate void JobCompletedDelegate(EndPoint ipep, JobResult j);
    public event JobCompletedDelegate OnJobCompleted;

    public delegate bool ClientConnectedDelegate(EndPoint ipep, String name, String password);
    public ClientConnectedDelegate OnClientAuth;

    public delegate void EndPointDelegate(EndPoint ipep);
    public event EndPointDelegate OnClientDisconnected;

    public event EndPointDelegate OnClientRequestedJob;

    public delegate void StringDelegate(String str);
    public event StringDelegate OnErrorLog;
    
    #endregion

    #region Variables

    private Dictionary<EndPoint, TcpClient> connectedClients = new Dictionary<EndPoint, TcpClient>();
    private TcpListener tcpListener;
    private bool running = false;

    #endregion

    ///<summary>
    /// Starts the server. Will block as long as the server runs, you might want to start this in an additional thread.
    ///</summary>
    public void Run()
    {
        if (OnJobCompleted == null ||
            OnClientAuth == null ||
            OnClientDisconnected == null ||
            OnClientRequestedJob == null ||
            OnErrorLog == null)
        {
            throw new Exception("One of the mandatory events was not bound");
        }

        lock (this)
        {
            if (running)
            {
                throw new Exception("Invalid state: Already running");
            }
            running = true;
        }

        try
        {
            tcpListener = new TcpListener(IPAddress.Any, Port);
            tcpListener.Start();
            while (running)
            {
                TcpClient client = tcpListener.AcceptTcpClient();
                lock (connectedClients)
                {
                    connectedClients.Add(client.Client.RemoteEndPoint, client);
                }
                Thread clientThread = new Thread(new ParameterizedThreadStart(HandleClient));
                clientThread.Start(client);
            }
        }
        catch (ThreadInterruptedException)
        {
        }
        catch (SocketException e)
        {
            if (running && OnErrorLog != null)
            {
                OnErrorLog("CryptoolServer: Got SocketException while running");
            }
        }
        finally
        {
            try
            {
                tcpListener.Stop();
            }
            catch (Exception)
            {
            }
            lock (connectedClients)
            {
                foreach (var client in connectedClients)
                    client.Value.Close();
            }
        }
    }
    
    public void SendJob(JobInput j, EndPoint i)
    {
        TcpClient client = null;
        lock(connectedClients)
        {
            if (!connectedClients.TryGetValue(i, out client))
            {
                if (OnErrorLog != null)
                    OnErrorLog("Tried to send job to not present external client " + i);
                return;
            }
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

        bool identified = false;
        try
        {
            var wrapped = new PlatformIndependentWrapper(client);
            while (true)
            {
                switch ((ClientOpcodes)wrapped.ReadInt())
                {
                    case ClientOpcodes.HELLO:
                        {
                            String name = wrapped.ReadString();
                            String password = wrapped.ReadString();
                            if (OnClientAuth == null || !OnClientAuth(ep, name, password))
                            {
                                wrapped.WriteInt((int)ServerOpcodes.WRONG_PASSWORD);
                                return;
                            }
                            identified = true;
                        }
                        break;

                    case ClientOpcodes.JOB_RESULT:
                        {
                            if (!identified)
                            {
                                if (OnErrorLog != null)
                                {
                                    OnErrorLog("Client '" + ep + "' tried to post result without identification");
                                }
                                return;
                            }

                            var jobGuid = wrapped.ReadString();
                            var resultList = new List<KeyValuePair<float, int>>();
                            var resultListLength = wrapped.ReadInt();
                            for (int c = 0; c < resultListLength; c++)
                            {
                                var key = wrapped.ReadInt();
                                var cost = wrapped.ReadFloat();
                                resultList.Add(new KeyValuePair<float, int>(cost, key));
                            }

                            JobResult rs = new JobResult();
                            rs.Guid = jobGuid;
                            rs.ResultList = resultList;

                            if (OnJobCompleted != null)
                            {
                                OnJobCompleted(ep, rs);
                            }
                        }
                        break;

                    case ClientOpcodes.JOB_REQUEST:
                        {
                            if (!identified)
                            {
                                if (OnErrorLog != null)
                                {
                                    OnErrorLog("Client '" + ep + "' tried to request job without identification");
                                }
                                return;
                            }

                            if (OnClientRequestedJob != null)
                            {
                                OnClientRequestedJob(ep);
                            }
                        }
                        break;
                }
            }
        }
        catch (SocketException)
        {
            // left blank intentionally. Will be thrown on client disconnect.
        }
        catch (Exception e)
        {
            if (OnErrorLog != null)
            {
                OnErrorLog("Client '" + ep + "' caused exception " + e);
            }
        }
        finally
        {
            // just to be sure..
            client.Close();

            lock (connectedClients)
            {
                connectedClients.Remove(ep);
            }

            if (OnClientDisconnected != null)
                OnClientDisconnected(ep);
        }
    }

    /// <summary>
    /// Closes this server. Any concurrent call to Run() in any other thread will return.
    /// </summary>
    public void Shutdown()
    {
        lock (this)
        {
            running = false;
            tcpListener.Stop();
        }
    }
}
