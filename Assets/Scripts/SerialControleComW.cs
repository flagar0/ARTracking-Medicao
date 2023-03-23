using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net;
using System.Net.Sockets;
using System.Text;
using Newtonsoft.Json.Linq;
using System.IO;
using UnityEngine.UI;
using System;

public class SerialControleComW : MonoBehaviour
{


    UdpClient clientData;

    [Header("Server UDP")]
    public int portData = 9000;
    public int receiveBufferSize = 120000;

    IPEndPoint ipEndPointData;
    private object obj = null;
    private System.AsyncCallback AC;
    byte[] receivedBytes;
    ////////////////////////////////////////////
    float[] oldPositions = { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };//[0-2]x ,y, z - [3-5]right - [6-8]up - [9-11]forward
    float[] newPositions = { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };///[0-2]x ,y, z - [3-5]right - [6-8]up - [9-11]forward
    JObject json;//Json aonde ficara salvo os valores vindos do AR - Tracking
    bool uma_vez = false; // Booleana para quando o programa rodar a primeira vez

    [Serializable]
    public class Configuracoes
    {
        [Header("Inverter Direcao")]
        [Range(-1, 1)]
        public int x_inversor = 1;//Inverter direcao que o obj anda na direcao x
        [Range(-1, 1)]
        public int y_inversor = 1;//Inverter direcao que o obj anda na direcao y
        [Range(-1, 1)]
        public int z_inversor = 1; //Inverter direcao que o obj anda na direcao z
        [Header("Ajustes")]
        [SerializeField]
        public float Sensibilidade = 5f;//Sensibilidade da translação
        [Header("Limites")]
        [SerializeField]//Limites de ate onde o obj pode andar
        public float max_x = 4;
        public float min_x = -4;
        public float max_y = 2.5f;
        public float min_y = -2.5f;
        public float max_z = 4;
        public float min_z = -4;


    }



    [Header("Objetos")]

    public GameObject Cubo; // Objeto que sera movimentado
    //bool Movimentar = false; // Booleana para liberar o movimento do objeto
    string new_timestamp, old_timestamp; //Variaveis que guardam o tempo enviado pelo ar tracking


    Vector3 UltimaPos;//Vetor que guarda a ultima posicao do objeto
    StreamWriter arquivo; //Arquivo que serao  armazenados os dados
    string[] jtokens = {"timestamp","success","translation_x","translation_y","translation_z","rotation_x","rotation_y","rotation_z","rotation_w"};//cabecalho do ar track

    bool gravar = false; //bool para comecar a gravar
    public GameObject rec; //UI que avisa se esta gravando
    public Text Infos_debug; //Texto que mostra os dados recebidos do ar tracking
    string receivedString;

    public Configuracoes config;
    void Start()
    {
        ConectarUDP();
    }
    void CriaCSV() //Cria o arquivo CSV aonde serao armazenados os dados
    {
        try
        {
            arquivo = new StreamWriter(Application.dataPath + "/Data/" + "Medicao_AR"+System.DateTime.Now.Minute.ToString()+"-"+System.DateTime.Now.Second.ToString()+".csv");
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
            Debug.Log("Arquivo criando com sucesso!");
        }
        catch (System.Exception e)
        {
            Debug.Log(e);

        }

    }
    public void ConectarUDP() //Conecta no servidor  do AR TRacking
    {
        ipEndPointData = new IPEndPoint(IPAddress.Loopback, portData);
        clientData = new UdpClient();
        clientData.Client.ReceiveBufferSize = receiveBufferSize;
        clientData.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, optionValue: true);
        clientData.Client.Bind(ipEndPointData);
        Debug.Log("BufSize: " + clientData.Client.ReceiveBufferSize);
        AC = new System.AsyncCallback(LeitorUDP);
        clientData.BeginReceive(AC, obj);
        Debug.Log("Conectando com AR Tracking");
    }

    void LeitorUDP(System.IAsyncResult result) // Le os dados enviados pelo AR Tracking
    {
        receivedBytes = clientData.EndReceive(result, ref ipEndPointData);
        byte[] receiveBytes = clientData.Receive(ref ipEndPointData);
        receivedString = Encoding.ASCII.GetString(receiveBytes);
        json = JObject.Parse(receivedString);//Salva os dados recebidos do AR no json
        clientData.BeginReceive(AC, obj);
    }


    private void Update()
    {
        if (Input.GetKeyDown("r")) //Botao de comecar a gravar
        {
            if (gravar == false)
            {
                gravar = true;

                rec.SetActive(true);
                CriaCSV();
            }
            else
            {
                gravar = false;
                rec.SetActive(false);
                FechaCSV();
            }

        }

        MovimentaCubo();

    }


    void MovimentaCubo()
    {
        try // Movimenta Cubo
        {
            if (json["success"].ToString() == "True")
            {
                Infos_debug.text = receivedString;
                SalvaDadosJson();//Salva os dados recebidos


                


                if (uma_vez == false) // executa uma vez para o cubo nao ir longe
                {
                    oldPositions[0] = newPositions[0];
                    oldPositions[1] = newPositions[1];
                    oldPositions[2] = newPositions[2];
                    uma_vez = true;
                    UltimaPos = Cubo.transform.position;
                }


                //Translacao
                Cubo.transform.Translate(new Vector3((newPositions[0] - oldPositions[0]) * config.x_inversor, (-newPositions[1] + oldPositions[1]) * config.y_inversor, (-newPositions[2] + oldPositions[2]) * config.z_inversor), Space.World);

                //LimitesCubo();//Limites  de translacao do cubo

                //Rotacao 
                Vector3 up = new Vector3(newPositions[6], newPositions[7], newPositions[8]);
                Vector3 forward = new Vector3(newPositions[9], newPositions[10], newPositions[11]);
                Cubo.transform.localRotation = Quaternion.LookRotation(forward, up);

                if (gravar && new_timestamp != old_timestamp) //Nao deixa repetir os valores no mesmo tempo
                {
                    AnotaCSV(true); //csv ar
               
                }

                //Salva posicoes antigas
                //Debug.Log(news[0]);
                oldPositions[0] = newPositions[0];
                oldPositions[1] = newPositions[1];
                oldPositions[2] = newPositions[2];
                old_timestamp = new_timestamp;
                UltimaPos = Cubo.transform.position;
            }
            else
            {//Caso o cubo nao esteja sendoreconhecido
                new_timestamp = json["timestamp"].ToString();
                Infos_debug.text = "Nao Conectado " + new_timestamp;
                /* TIMESPAN do sistema
                TimeSpan tempo = System.DateTime.Now.Subtract(System.DateTime.UnixEpoch);
                Debug.Log(tempo.TotalMilliseconds / 1000 + " aaaa");
                Debug.Log(new_timestamp + " bbbb");
                */
                if (gravar && new_timestamp != old_timestamp)
                {
                    AnotaCSV(false);
                }
                old_timestamp = new_timestamp;
            }
        }
        catch (System.Exception e)
        {
            //Debug.Log(e);
        }
    }

    void LimitesCubo()
    {
        if (Cubo.transform.position.z < config.min_z || Cubo.transform.position.z > config.max_z)
        {//Limite de posicao z
            Cubo.transform.position = UltimaPos;
        }
        else if (Cubo.transform.position.x < config.min_x || Cubo.transform.position.x > config.max_x)
        { //limite x
            Cubo.transform.position = UltimaPos;
        }
        else if (Cubo.transform.position.y < config.min_y || Cubo.transform.position.y > config.max_y)
        { //limite z
            Cubo.transform.position = UltimaPos;
        }
    }
    void SalvaDadosJson()
    {
		//Definição dos novos valores de translação
        newPositions[0] = float.Parse(json["translation_x"].ToString()) / config.Sensibilidade;
        newPositions[1] = float.Parse(json["translation_y"].ToString()) / config.Sensibilidade;
        newPositions[2] = float.Parse(json["translation_z"].ToString()) / config.Sensibilidade;
		
		//Definição dos novos valores de rotação (up)
        newPositions[6] = float.Parse(json["rotation_up_x"].ToString());
        newPositions[7] = float.Parse(json["rotation_up_y"].ToString());
        newPositions[8] = float.Parse(json["rotation_up_z"].ToString());
		
		//Definição dos novos valores de rotação (forward)
        newPositions[9] = float.Parse(json["rotation_forward_x"].ToString());
        newPositions[10] = float.Parse(json["rotation_forward_y"].ToString());
        newPositions[11] = float.Parse(json["rotation_forward_z"].ToString());
        new_timestamp = json["timestamp"].ToString();
    }

    void AnotaCSV(bool Detectando)
    {
         GameObject.Find("EventSystem").GetComponent<ListenerHap>().AnotaCSV(new_timestamp); //csv hap

        if (Detectando)
        {//Recebendo informacoes do AR Tracking
            
                arquivo.Write(json[jtokens[0]].ToString().Replace(',', '.'));
                arquivo.Write(",");
                arquivo.Write(json[jtokens[1]].ToString().Replace(',', '.'));
                arquivo.Write(",");
                arquivo.Write(json[jtokens[2]].ToString().Replace(',', '.'));
                arquivo.Write(",");
                arquivo.Write(json[jtokens[3]].ToString().Replace(',', '.'));
                arquivo.Write(",");
                arquivo.Write(json[jtokens[4]].ToString().Replace(',', '.'));
                arquivo.Write(",");
                arquivo.Write(Cubo.transform.localRotation.x.ToString().Replace(',', '.'));
                arquivo.Write(",");
                arquivo.Write(Cubo.transform.localRotation.y.ToString().Replace(',', '.'));
                arquivo.Write(",");
                arquivo.Write(Cubo.transform.localRotation.z.ToString().Replace(',', '.'));
                arquivo.Write(",");
                arquivo.Write(Cubo.transform.localRotation.w.ToString().Replace(',', '.'));
                arquivo.WriteLine();
        }
        else
        { // AR tracking parou de enviar armazena so o tempo e o sucess
            for (int i = 0; i < 2; i++)
            {
                arquivo.Write(json[jtokens[i]].ToString().Replace(',', '.'));

                if (i ==0)
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
        Debug.Log("Salvo com sucesso");
    }
    void OnDestroy()//Desliga a leitura do ar tracking quando o jogo fecha
    {
        if (clientData != null)
        {
            clientData.Close();
            if (gravar)
            {
                FechaCSV();
            }
        }

    }

}


/* RETORNO do AR TRACKING
{"timestamp": 1677594319.8186967, "success": true, "translation_x": 11.430583295477035, "translation_y": 5.142802280146702, "translation_z": 42.815210412740825, 
"rotation_right_x": -0.9399653958653161, "rotation_right_y": 0.31731478045920997, "rotation_right_z": -0.12560407906546253, "rotation_up_x": -0.3016497091359976, 
"rotation_up_y": -0.6003944856751889, "rotation_up_z": 0.7406307545254879, "rotation_forward_x": 0.15960108882438012, "rotation_forward_y": 0.7340557142839697, 
"rotation_forward_z": 0.6600679516331054}
*/

