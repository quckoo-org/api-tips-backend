using System.Collections.Concurrent;
using System.Net.Sockets;
using System.Text;
using System.Windows.Forms;

namespace ApiTips.BrainTester;

public class BrainConnection
{
    private readonly string _host;
    private readonly int _port;
    private bool _exit = true;

    private Thread _threadRecv;
    private Thread _threadPing;
    private Thread _threadSend;
    private TcpClient? _client;
    private NetworkStream? _stream;

    private readonly List<byte> _bytesBuffRead = new List<byte>();
    private readonly byte[] _buffer = new byte[1024];

    private readonly BlockingCollection<string> _sendQueue = new BlockingCollection<string>();

    private volatile bool _isConnected;
    public bool IsConnected => _isConnected;

    public event EventHandler<string> Connected;
    public event EventHandler<string> Disconnected;
    public event EventHandler<string> MessageReceived;
    public event EventHandler<string> MessageSended;

    public BrainConnection(string host, int port)
    {
        var connId = Guid.NewGuid();

        _host = host;
        _port = port;
    }

    public bool Start()
    {
        Connect();

        if (_exit)
        {
            _exit = false;

            _threadRecv = new Thread(RecvThread);
            _threadRecv.Name = $"{GetType().Name}-Recv-{_threadRecv.ManagedThreadId}";
            _threadRecv.IsBackground = true;

            _threadPing = new Thread(PingThread);
            _threadPing.Name = $"{GetType().Name}-Ping-{_threadPing.ManagedThreadId}";
            _threadPing.IsBackground = true;

            _threadSend = new Thread(SendThread);
            _threadSend.Name = $"{GetType().Name}-Send-{_threadSend.ManagedThreadId}";
            _threadSend.IsBackground = true;

            if (_port != 0)
            {
                _threadRecv.Start();
                _threadPing.Start();
                _threadSend.Start();
            }
        }

        return true;
    }

    public void Stop()
    {
        _exit = true;
        _client?.Close();
    }

    private void Connect()
    {
        if (IsConnected) return;
        try
        {
            _client = new TcpClient();
            _client.Connect(_host, _port);
            _stream = _client.GetStream();
            _bytesBuffRead.Clear();
            _isConnected = true;

            Connected?.Invoke(this, null);
        }
        catch (Exception e)
        {
            Disconnect("Cannot reach endpoint");
        }
    }

    private void Disconnect(string reason)
    {
        _isConnected = false;
        _client?.Close();
        _stream?.Close();
        _stream = null;
        _client = null;

        EmitDisconnected(reason);
    }

    public void PostString(string str)
    {
        if (IsConnected)
        {
            _sendQueue.Add(str);
        }
        else
        {
            MessageBox.Show($"Неудачная попытка отправить {str}, т.к. соединение не установлено");
        }
    }

    private void SendThread()
    {
        while (!_exit)
        {
            if (IsConnected)
            {
                var text = _sendQueue.Take();

                SendString(text);
            }
            else
            {
                Thread.Sleep(1);
            }
        }
    }

    private void RecvThread()
    {
        while (!_exit)
        {
            if (IsConnected)
            {
                var str = ReadString();

                if (str.Length > 0)
                {
                    OnRecv(str);
                }
            }
            else
            {
                Thread.Sleep(1);
            }
        }
    }

    private void DoDisconnect(string text)
    {
        var disconnectType = _exit ? "Normal" : "ConnectionLost";

        Disconnect(disconnectType);
    }

    private string ReadString()
    {
        if (_stream == null) return "";

        var arrSize = ReadBuf(4);
        if (arrSize.Length != 4)
        {
            DoDisconnect("if (arrSize.Length != 4)");
            return "";
        }
        Array.Reverse(arrSize);
        var nSize = BitConverter.ToInt32(arrSize, 0);
        byte[] arrData = ReadBuf(nSize);
        if (arrData.Length != nSize)
        {
            DoDisconnect($"if (arrData.Length({arrData.Length}) != nSize({nSize} {arrSize[0]} {arrSize[1]} {arrSize[2]} {arrSize[3]})");
            return "";
        }

        var str = Encoding.UTF8.GetString(arrData);
        return str;
    }

    private byte[] ReadBuf(int nCount)
    {
        try
        {
            while (_bytesBuffRead.Count < nCount)
            {
                var nBytes = _stream.Read(_buffer, 0, _buffer.Length);
                if (nBytes == 0)
                    return new byte[0];
                for (var g = 0; g < nBytes; g++) _bytesBuffRead.Add(_buffer[g]);
            }

            byte[] outb = new byte[nCount];
            for (var g = 0; g < nCount; g++)
            {
                outb[g] = _bytesBuffRead[g];
            }

            _bytesBuffRead.RemoveRange(0, nCount);
            return outb;
        }
        catch (Exception ex)
        {
            MessageBox.Show("Exception in ReadBuf() " + ex.Message);
            return new byte[0];
        }
    }

    private void OnRecv(string str)
    {
        MessageReceived?.Invoke(this, str);
        //MessageBox.Show("Получили сообщение от мозга - " + str);
    }

    private void OnSend(string str)
    {
        MessageSended?.Invoke(this, str);
    }

    private void SendString(string str)
    {
        try
        {
            if (_stream == null) throw new Exception("stream == null");

            var xmlBytes = Encoding.UTF8.GetBytes(str + "\r\n");
            var lengthPrefix = BitConverter.GetBytes(xmlBytes.Length);
            // big-endian
            Array.Reverse(lengthPrefix);

            var packet = new byte[lengthPrefix.Length + xmlBytes.Length];
            Array.Copy(lengthPrefix, 0, packet, 0, lengthPrefix.Length);
            Array.Copy(xmlBytes, 0, packet, lengthPrefix.Length, xmlBytes.Length);

            _stream.Write(packet, 0, packet.Length);
            _stream.Flush();

            OnSend(str);
        }
        catch (Exception e)
        {
            // ignored
            MessageBox.Show($"Ошибка при отправке сообщения {str} - " + e.Message);
        }
    }

    private void PingThread()
    {
        while (!_exit)
        {
            if (IsConnected)
            {
                SendString("<Ping/>");
            }
            Thread.Sleep(10000);
        }
    }

    private void EmitDisconnected(string reason)
    {
        Disconnected?.Invoke(this, reason);
        MessageBox.Show("Дисконнект - " + reason);
    }
}
