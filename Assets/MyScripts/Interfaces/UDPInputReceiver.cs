
using UnityEngine;
using System;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Collections.Concurrent;

public class UDPInputReceiver : MonoBehaviour
{
    Thread receiveThread;
    UdpClient client;
    ConcurrentQueue<string> messageQueue = new ConcurrentQueue<string>();
    public int port = 5005;

    // These variables will hold the values received from Python
    public float calcX, calcY, yaw, pitch, CursorX,CursorY;
    // public long sentTimeStamp;
    public int CurrentState = 0;
    public string Shape = "";
    private object lockObject = new object();

    void Start()
    {
        receiveThread = new Thread(new ThreadStart(ReceiveData));
        receiveThread.IsBackground = true;
        receiveThread.Start();
        long receiveTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        Debug.Log(receiveTime);
    }

    private void ReceiveData()
    {
        client = new UdpClient(port);
        while (true)
        {
            try
            {
                IPEndPoint anyIP = new IPEndPoint(IPAddress.Any, 0);
                byte[] dataByte = client.Receive(ref anyIP);
                string text = Encoding.UTF8.GetString(dataByte);
                messageQueue.Enqueue(text);

            }
            catch (Exception e) { Debug.Log(e.ToString()); }
        }
    }

    void Update()
    {
        while (messageQueue.TryDequeue(out string message))
        {
            string[] values = message.Split(',');
            if (values.Length == 9)
            {
                lock (lockObject)
                {
                    calcX = float.Parse(values[0]);
                    calcY = float.Parse(values[1]);
                    yaw = float.Parse(values[2]);
                    pitch = float.Parse(values[3]);
                    CurrentState = int.Parse(values[4]);
                    CursorX = float.Parse(values[5])/1000;
                    CursorY = float.Parse(values[6])/1000;
                    Shape = values[7];
                    // sentTimeStamp = long.Parse(values[8]);
                }
            }
        }
    }

    // Ensure thread stops when game stops
    void OnApplicationQuit()
    {
        if (receiveThread != null) receiveThread.Abort();
        if(client!=null) client.Close();
    }
}