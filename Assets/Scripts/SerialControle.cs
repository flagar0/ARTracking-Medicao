using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net;
using System.Net.Sockets;
using System.Text;
using Newtonsoft.Json.Linq;
using System.IO;

public class SerialControle : MonoBehaviour
{

    UdpClient clientData;
    int portData = 9000;
    public int receiveBufferSize = 120000;

    public bool showDebug = false;
    IPEndPoint ipEndPointData;
    private object obj = null;
    private System.AsyncCallback AC;
    byte[] receivedBytes;
    /////
    float[] olds = { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };//x ,y, z - right - up - forward
    float[] news = { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };//x ,y, z- right - up - forward
    JObject json;
    bool uma_vez = false;
    [SerializeField] int Sensibilidade = 5;

    public GameObject Cubo;
    bool Movimentar = false;
    string new_timestamp, old_timestamp;


    Vector3 UltimaPos;
    StreamWriter arquivo;
    string[] jtokens = {"timestamp","success","translation_x","translation_y","translation_z","rotation_right_x"
    ,"rotation_right_y","rotation_right_z","rotation_up_x","rotation_up_y","rotation_up_z","rotation_forward_x"
    ,"rotation_forward_y","rotation_forward_z"};

    bool gravar = false;//bool para comecar a gravar
    bool gravou = false;//define se ja comecou a gravar
    public GameObject rec;

    void Start()
    {
        InitializeUDPListener();
        CriaCSV();
    }
    void CriaCSV()
    {
        try
        {
            arquivo = new StreamWriter(Application.dataPath + "/Data/" + "coisas.csv");
        }
        catch (System.Exception e)
        {
            Debug.Log(e);

        }
        for (int i = 0; i < jtokens.Length; i++)
        {
            arquivo.Write(jtokens[i]);
            if (i < jtokens.Length - 1)
            {
                arquivo.Write(",");
            }
            else
            {
                arquivo.WriteLine();
            }
        }
    }
    public void InitializeUDPListener()
    {
        ipEndPointData = new IPEndPoint(IPAddress.Loopback, portData);
        clientData = new UdpClient();
        clientData.Client.ReceiveBufferSize = receiveBufferSize;
        clientData.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, optionValue: true);
        clientData.ExclusiveAddressUse = false;
        clientData.EnableBroadcast = true;
        clientData.Client.Bind(ipEndPointData);
        clientData.DontFragment = true;
        if (showDebug) Debug.Log("BufSize: " + clientData.Client.ReceiveBufferSize);
        AC = new System.AsyncCallback(ReceivedUDPPacket);
        clientData.BeginReceive(AC, obj);
        Debug.Log("UDP - Start Receiving..");
    }

    void ReceivedUDPPacket(System.IAsyncResult result)
    {
        receivedBytes = clientData.EndReceive(result, ref ipEndPointData);
        byte[] receiveBytes = clientData.Receive(ref ipEndPointData);
        string receivedString = Encoding.ASCII.GetString(receiveBytes);
        json = JObject.Parse(receivedString);
        if (json["success"].ToString() == "True")
        {
            Movimentar = true;
        }
        else
        {
            Movimentar = false;
        }
        clientData.BeginReceive(AC, obj);


    } // ReceiveCallBack


    private void FixedUpdate()
    {
        rec.SetActive(gravar);
        if (Input.GetKeyDown("r"))
        {
            gravar = !gravar;
            gravou = true;
        }

        try // Movimenta Cubo
        {
            if (Movimentar)
            {
                news[0] = float.Parse(json["translation_x"].ToString()) / Sensibilidade;
                news[1] = float.Parse(json["translation_y"].ToString()) / Sensibilidade;
                news[2] = float.Parse(json["translation_z"].ToString()) / Sensibilidade;
                news[6] = float.Parse(json["rotation_up_x"].ToString());
                news[7] = float.Parse(json["rotation_up_y"].ToString());
                news[8] = float.Parse(json["rotation_up_z"].ToString());
                news[9] = float.Parse(json["rotation_forward_x"].ToString());
                news[10] = float.Parse(json["rotation_forward_y"].ToString());
                news[11] = float.Parse(json["rotation_forward_z"].ToString());
                new_timestamp = json["timestamp"].ToString();


                if (gravar && new_timestamp != old_timestamp)
                {
                    AnotaCSV(true);
                }


                if (uma_vez == false) // executa uma vez para o cubo nao ir longe
                {
                    olds[0] = news[0];
                    olds[1] = news[1];
                    olds[2] = news[2];
                    uma_vez = true;
                    UltimaPos = Cubo.transform.position;
                }


                //Translacao
                Cubo.transform.Translate(new Vector3(-news[0] + olds[0], -news[1] + olds[1], news[2] - olds[2]), Space.World);

                if (Cubo.transform.position.z < -4 || Cubo.transform.position.z > 4)
                {//Limite de posicao z
                    Cubo.transform.position = UltimaPos;
                }
                else if (Cubo.transform.position.x < -4 || Cubo.transform.position.x > 4)
                { //limite x
                    Cubo.transform.position = UltimaPos;
                }
                else if (Cubo.transform.position.y < -2.5 || Cubo.transform.position.y > 2.5)
                { //limite z
                    Cubo.transform.position = UltimaPos;
                }

                //Rotacao 
                Vector3 up = new Vector3(news[6], news[7], news[8]);
                Vector3 forward = new Vector3(news[9], news[10], news[11]);
                Cubo.transform.localRotation = Quaternion.LookRotation(forward, up);


                //Salva posicoes antigas
                Debug.Log(news[0]);
                olds[0] = news[0];
                olds[1] = news[1];
                olds[2] = news[2];
                old_timestamp = new_timestamp;
                UltimaPos = Cubo.transform.position;
            }
            else
            {// == false
                new_timestamp = json["timestamp"].ToString();
                if (gravar && new_timestamp != old_timestamp)
                {
                    AnotaCSV(false);
                }
                old_timestamp = new_timestamp;
            }
        }
        catch (System.Exception e)
        {
            Debug.Log(e);
        }
        if (gravar == false && gravou == true)
        {//comecou a gravar e quer parar
            FechaCSV();
        }
    }


    void AnotaCSV(bool Detectando)
    {

        if (Detectando)
        {//Recebendo informacoes do AR Tracking
            for (int i = 0; i < json.Count; i++)
            {
                arquivo.Write(json[jtokens[i]].ToString().Replace(',', '.'));
                if (i < jtokens.Length - 1)
                {
                    arquivo.Write(",");
                }
                else
                {
                    arquivo.WriteLine();
                }
            }
        }
        else
        { // AR tracking parou de enviar
            for (int i = 0; i <= 2; i++)
            {
                if (i < 2)
                {
                    arquivo.Write(json[jtokens[i]].ToString().Replace(',', '.'));
                }
            

                if (i <=1)
                {
                    arquivo.Write(",");
                }
                else
                {
                    arquivo.WriteLine();
                }
            }
        }
    }


    void FechaCSV()
    {
        arquivo.Flush();
        arquivo.Close();
        Debug.Log("Criado com sucesso");
    }
    void OnDestroy()
    {
        if (clientData != null)
        {
            clientData.Close();
            FechaCSV();
        }

    }

}


/*
{"timestamp": 1677594319.8186967, "success": true, "translation_x": 11.430583295477035, "translation_y": 5.142802280146702, "translation_z": 42.815210412740825, 
"rotation_right_x": -0.9399653958653161, "rotation_right_y": 0.31731478045920997, "rotation_right_z": -0.12560407906546253, "rotation_up_x": -0.3016497091359976, 
"rotation_up_y": -0.6003944856751889, "rotation_up_z": 0.7406307545254879, "rotation_forward_x": 0.15960108882438012, "rotation_forward_y": 0.7340557142839697, 
"rotation_forward_z": 0.6600679516331054}
*/

