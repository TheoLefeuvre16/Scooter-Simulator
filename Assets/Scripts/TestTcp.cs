using UnityEngine;
using System.Collections;
using System;
using System.IO;
using System.Net.Sockets;


public class TestTcp : MonoBehaviour
{
    //==============================
    //   Parameters
    //==============================
    String Host = "localhost";
    Int32 Port = 5333;
    //==============================

    public int global;
    public int acceleration;
    public int brake;
    public int orientation;
    public int klaxon;

    void Update()
    {
        float l_time; 
        l_time = Time.time;

        //Auto-connection to TCP server
        if (!socketReady)
        {
            //2s re-connection timer
            if ((l_time - m_lastTimeConnexion) >= 2.0)
            {
                openSocket();
                m_lastTimeConnexion = l_time;
            }
        }

        if (socketReady)
        {
            string receivedText = readSocket();
            if (receivedText != "")
            {
                global = int.Parse(receivedText);
                string[] mot = receivedText.Split(';');
                acceleration =  int.Parse(mot[0]);
                brake =         int.Parse(mot[1]);
                orientation =   int.Parse(mot[2]);
                klaxon =        int.Parse(mot[3]);
            }
        }
    }

   

    void OnApplicationFocus(bool hasFocus)
    {
        if (hasFocus == true)
        {
            if (socketReady)
            {
                writeSocket("R");
            }
        }
        else //loses focus
        {
            if (socketReady)
            {
                writeSocket("S");
           }
        }
    }

    void OnApplicationPause(bool pauseStatus)
    {
        if (pauseStatus == true) 
        {
            if (socketReady)
            {
                writeSocket("S");
            }
        }
        else
        {
            if (socketReady)
            {
                writeSocket("R");
            }
        }
    }


    void OnGUI()
    {
        if (!socketReady)
        {
            GUI.contentColor = Color.black;
            GUI.Label(new Rect(10, 10, 1000, 400), "Not yet connected to server...");
        }

        if (socketReady)
        {

            if (GUILayout.Button("Close"))
            {
                closeSocket();
            }

            if (GUILayout.Button("Init. Pose"))
            {
  
            }
        }
    }

    void OnApplicationQuit()
    {
        closeSocket();
    }

    public void openSocket()
    {
        Debug.Log("Trying to connect to server");
        try
        {
            mySocket = new TcpClient(Host, Port);
            theStream = mySocket.GetStream();
            theWriter = new StreamWriter(theStream);
            theReader = new StreamReader(theStream);
            socketReady = true;
            Debug.Log("Connection established");
        }
        catch (Exception e)
        {
            Debug.Log("Socket error: " + e);
        }
    }

    public void writeSocket(string theLine)
    {
        if (!socketReady)
            return;
        try
        {
            theWriter.Write(theLine);
            theWriter.Flush();
        }
        catch (Exception e)
        {
            socketReady = false;
            Debug.Log("Socket error: " + e);
        }
    }

    public String readSocket()
    {
        if (!socketReady)
            return "";
        try
        {
           if (theStream.DataAvailable)
                return theReader.ReadLine();
        }
        catch (Exception e)
        {
            socketReady = false;
            Debug.Log("Socket error: " + e);
        }
        return "";
    }

    public void closeSocket()
    {
        if (!socketReady)
            return;
        theWriter.Close();
        theReader.Close();
        mySocket.Close();
        socketReady = false;
    }


    float m_lastTimeConnexion = -1.0f;
    private Transform[] sphere;
    public Quaternion rot = Quaternion.identity;
    internal Boolean socketReady = false;
    TcpClient mySocket;
    NetworkStream theStream;
    StreamWriter theWriter;
    StreamReader theReader;
}

