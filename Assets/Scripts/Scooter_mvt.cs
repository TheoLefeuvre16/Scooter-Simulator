using System.Collections;
using System.Collections.Generic;
using System.Timers;
using UnityEngine;
using UnityEngine.UI;
using System;
using System.IO;
using System.Net.Sockets;
using UnityEditor;




public class Scooter_mvt : MonoBehaviour
{

    public WheelCollider FrontWheel;
    public WheelCollider BackWheel;
    public Transform FrontWheelMesh;
    public Transform BackWheelMesh;
    public Transform handlebarMesh;
    public Transform AttacheMesh;

    public float torque;
    public float maxSpeed = 200f;
    public float speed;

    public float brake; //freinage
    public float coeffacceleration = 10f;
    public float wheelanglemax = 30f;

    public bool isDebug = false;

    public float arriere=0;
    public GameObject objet;
    float dir_value = 1;
    public string strineg;
    public string testtableau;
    //==============================
    //   Parameters
    //==============================
    String Host = "192.168.0.201";
    Int32 Port = 80;

    public string globas2quat;
    public int global;
    public int acceleration;
    public float accelerationfonction;
    public int frein;
    public int orientation;
    public int klaxon;
    public int rotation;

    float m_lastTimeConnexion = -1.0f;
    internal Boolean socketReady = false;
    TcpClient mySocket;
    NetworkStream theStream;
    StreamWriter theWriter;
    StreamReader theReader;

    public Transform manette_droite;
    public Transform manette_gauche_fixe;
    public int point_fixe_manette = 0;
    public int rotation_manette = 0;
    public int rotation_manette_gauche = 0;
    public int init_rotation = 50;
    public int init_position = 50;
    //==============================


    void Start()
    {
       GetComponent<Rigidbody>().centerOfMass = new Vector3(0, 0, 0); //I found on internet that it could fixe the problem, it helped a little but not resolved the pb
                                                                      //point_fixe_manette = get_rotation_guidon();
        
    }
    void Update()
    {
        if (init_position > 0)
            init_position -= 1;
        if(init_position == 0)
        {
            objet.transform.eulerAngles = new Vector3(0, (int)manette_gauche_fixe.eulerAngles.y, 0);
            objet.transform.position = new Vector3(manette_gauche_fixe.position.x, manette_gauche_fixe.position.y, manette_gauche_fixe.position.z-1);
            init_position -= 1;
        }
        isDebug = false;
        float l_time;
        l_time = Time.time;
        int[] values = new int[4];

        //deconnection 
        if (Input.GetKey(KeyCode.Escape))
        {

            mySocket.Close();
            Application.Quit();
        }

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
                CSVStringToInt(receivedText);
            }
            //Quaternion rotation = objet.transform.localRotation; //quaternion xyzw
        }
        speed = GetComponent<Rigidbody>().velocity.magnitude * 3.6f;

        

        //acceleration
        CalculateBrake();
        CalculateAcceleration();

        //audio
        Audio();

        //deceleration
        Deceleration();

        //rotation des roues
        CalculateWheelAngle();

        //rotation verticale a envoyer
        rotation = get_rotation();
        rotation_manette = get_rotation_guidon();

        strineg = rotation.ToString() + ";" + rotation_manette.ToString() +"\n";


        writeSocket(strineg);


        //rotation des sprites des roues / guidon
        MeshRotation();
    }


    public int get_rotation()
    {
        int value = (int)objet.transform.eulerAngles.x;
        if (value > 180)
            value = 360 - value;
        else
            value *= -1;

        return value;
    }

    public int get_rotation_guidon()
    {
        if (init_rotation == 0)
        {
            point_fixe_manette = (int)manette_gauche_fixe.eulerAngles.y;
            init_rotation -= 1;
        }
        else if (init_rotation > 0)
            init_rotation -= 1;

        rotation_manette_gauche = (int)manette_gauche_fixe.eulerAngles.y;
        int value = (int)manette_droite.eulerAngles.y;
        value = value - point_fixe_manette ;

        if (value > 180)
            value = -1*(360 - value);
        if(value < -180)
        {
            value = 360 + value;
        }


        return value;
    }
    public float GetWheelAngle()
    {
        float result = 0;

        if (isDebug)
        {
            if (Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.RightArrow))
                result = Input.GetAxis("Horizontal") * wheelanglemax;
        }
        else
            result = rotation_manette * 0.02f * wheelanglemax;

        
        return result;

    }
    public void CalculateWheelAngle()
    {
        float result = 0;
        result = GetWheelAngle();

        FrontWheel.steerAngle =result;
        return;
    }
    public float direction()
    {

        if (isDebug && Input.GetKey(KeyCode.UpArrow))
            dir_value = 1.0f;

        if (isDebug && Input.GetKey(KeyCode.DownArrow))
            dir_value = -1.0f;

        if (!isDebug)
        {
            float value = GetAcceleration();
            if(value > 0 )
                dir_value = 1.0f;
            else
                dir_value = -1.0f;
        }
        return dir_value;
    }
    public float GetAcceleration()
    {
        float acc = 0;

        // just for Debug
        if (isDebug && Input.GetKey(KeyCode.UpArrow))
            acc = 1.0f;

        if (isDebug && Input.GetKey(KeyCode.DownArrow))
            acc = -1.0f;

        if(!isDebug)
        {
            acc = acceleration * 0.01f;

        }
        return acc;
    }
    public void CalculateAcceleration() {

        FrontWheel.motorTorque = GetAcceleration() * torque * coeffacceleration * Time.deltaTime - direction()  * 100 * GetBrake();
        BackWheel.motorTorque = GetAcceleration() * torque * coeffacceleration * Time.deltaTime - direction()  * 100 * GetBrake();
  

    }
    public float GetBrake()
    {
        float brake = 0;
        if (isDebug && Input.GetKey(KeyCode.Space))
            brake = 1.0f;

        if (!isDebug && frein > 0)
            brake = frein * 0.005f;

        return brake;
    }
    public void CalculateBrake()
    {
        
        FrontWheel.brakeTorque = GetBrake();
        BackWheel.brakeTorque = GetBrake();
      

 
    }

    public void Deceleration()
    {
        if ((isDebug && !Input.GetKey(KeyCode.UpArrow) && !Input.GetKey(KeyCode.DownArrow)) || speed > maxSpeed)
        {
            if (GetComponent<Rigidbody>().velocity.y > 1)
            {
                FrontWheel.motorTorque = -50;
                BackWheel.motorTorque = -50;
            }
            else
            {
                FrontWheel.brakeTorque = 1;
                BackWheel.brakeTorque = 1;
            }
        }
        /*if(!isDebug && acceleration == 0)
        {
            if (GetComponent<Rigidbody>().velocity.y > 1)
            {
                FrontWheel.motorTorque = -50;
                BackWheel.motorTorque = -50;
            }
            else
            {
                FrontWheel.brakeTorque = 1;
                BackWheel.brakeTorque = 1;
            }

        }*/
    }
  
    public bool isButtonPressed()
    {
        return false;

    }
    public void MeshRotation()
    {
        //rotation avant roues
        FrontWheelMesh.Rotate(FrontWheel.rpm / 60 * 360 * Time.deltaTime, 0, 0);
        BackWheelMesh.Rotate(FrontWheel.rpm / 60 * 360 * Time.deltaTime, 0, 0);

        //rotation direction roue avant + guidon
        FrontWheelMesh.localEulerAngles = new Vector3(FrontWheelMesh.localEulerAngles.x, FrontWheel.steerAngle - FrontWheelMesh.localEulerAngles.z, FrontWheelMesh.localEulerAngles.z);
        handlebarMesh.localEulerAngles = new Vector3(0, FrontWheel.steerAngle - handlebarMesh.localEulerAngles.z, 0);
        AttacheMesh.localEulerAngles = new Vector3(0, FrontWheel.steerAngle - AttacheMesh.localEulerAngles.z, 0);

    }
    public void Audio()
    {
        GetComponent<AudioSource>().pitch = 1f + speed / maxSpeed;
    }



    //SEND AND RECEIVE INFORMATIONS
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
            {
                return theReader.ReadLine();

            }
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

    public void CSVStringToInt(string receivedText)
    {
        string[] mot = receivedText.Split(';');
        acceleration = int.Parse(mot[0]);
        frein = int.Parse(mot[1]);
       

        return;
    }
}








/*
 
    public float GetBrake()
    {
        brake = 0;
        if (isDebug && Input.GetKey(KeyCode.Space))
        {
            brake = 1.0f;

        }
        return brake;
    }

    public float GetAcceleration()
    {
        float acc = 0;
        if (isDebug && Input.GetKey(KeyCode.UpArrow) && speed < maxSpeed)
        {
            arriere = -1;
            acc = Input.GetAxis("Vertical") * torque * coeffacceleration * Time.deltaTime;

        }
        if (isDebug && Input.GetKey(KeyCode.DownArrow))
        {
            arriere = 1;
            acc = Input.GetAxis("Vertical") * torque * coeffacceleration * Time.deltaTime;

        }
        return acc;
    }


    public float GetWheelAngle()
    {
        float result = 0;
        if(isDebug)
            result = Input.GetAxis("Horizontal") * wheelanglemax; ;

        return result;

    }
    */
