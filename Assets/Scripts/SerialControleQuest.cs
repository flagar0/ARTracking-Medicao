using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net;
using System.Net.Sockets;
using System.Text;
using Newtonsoft.Json.Linq;
using System.IO;
using UnityEngine.UI;
using UnityEngine.XR;
using System;

[RequireComponent(typeof(InputData))]
public class SerialControleQuest : MonoBehaviour
{
    private InputData _inputData;
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
    string[] jtokens = { "timestamp", "success", "translation_x", "translation_y", "translation_z", "rotation_x", "rotation_y", "rotation_z", "rotation_w" };//cabecalho do ar track

    bool gravar = false; //bool para comecar a gravar
    public GameObject rec; //UI que avisa se esta gravando
    public Text Infos_debug; //Texto que mostra os dados recebidos do ar tracking
    string receivedString;
    public bool DeadReckoing;
    List<Vector3> lastPosition = new List<Vector3>(60);

    public Configuracoes config;
    void Start()
    {
        CriaCSV();
        GameObject.Find("dead").GetComponent<Text>().text = DeadReckoing.ToString();
        _inputData = GetComponent<InputData>();
    }

    void CriaCSV() //Cria o arquivo CSV aonde serao armazenados os dados
    {
        try
        {
            Directory.CreateDirectory(Application.dataPath
                                        + "/Data/" + System.DateTime.Now.Hour.ToString()
                                        + "-" + System.DateTime.Now.Minute.ToString()
                                        + "-" + System.DateTime.Now.Second.ToString() + "/");
            arquivo = new StreamWriter(Application.dataPath
                                        + "/Data/" + System.DateTime.Now.Hour.ToString() + "-"
                                        + System.DateTime.Now.Minute.ToString() + "-"
                                        + System.DateTime.Now.Second.ToString() + "/" +
                                        "Medicao_QUEST.csv");
            for (int i = 0; i < jtokens.Length; i++)
            {
                arquivo.Write(jtokens[i]);
                if (i < jtokens.Length - 1)
                {
                    arquivo.Write(";");
                }
                else
                {
                    arquivo.WriteLine();
                }
            }
            Debug.Log("Arquivo criado com sucesso!");
        }
        catch (System.Exception e)
        {
            Debug.Log(e);
        }

    }

    private void Update()
    {
        if (Input.GetKeyDown("r")) //Botao de comecar a gravar
        {
            if (gravar == false)
            {
                gravar = true;

                rec.SetActive(true);
            }
            else
            {
                gravar = false;
                rec.SetActive(false);
                FechaCSV();
            }

        }
        GameObject.Find("CuboAR").GetComponent<AnotaPosQuest>().AnotaCSV(ConversorTempo((double)json[jtokens[0]]).ToString("HH:mm:ss.fff")); //csv hap

        if (_inputData._leftController.TryGetFeatureValue(CommonUsages.devicePosition, out Vector3 position))
        {
            arquivo.Write(ConversorTempo((double)json[jtokens[0]]).ToString("HH:mm:ss.fff"));
            arquivo.Write(";");
            arquivo.Write(json[jtokens[1]].ToString());
            arquivo.Write(";");
            arquivo.Write(json[jtokens[2]].ToString());
            arquivo.Write(";");
            arquivo.Write(json[jtokens[3]].ToString());
            arquivo.Write(";");
            arquivo.Write(json[jtokens[4]].ToString());
            arquivo.Write(";");
            arquivo.Write(position.x.ToString());
            arquivo.Write(";");
            arquivo.Write(position.y.ToString());
            arquivo.Write(";");
            arquivo.Write(position.z.ToString());
            arquivo.WriteLine();

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


                if (DeadReckoing)
                {
                    Transladar();
                    Rotacionar(false);
                }
                else
                {
                    Cubo.transform.Translate(new Vector3((newPositions[0] - oldPositions[0]) * config.x_inversor,
                                                            (-newPositions[1] + oldPositions[1]) * config.y_inversor,
                                                            (-newPositions[2] + oldPositions[2]) * config.z_inversor),
                                                            Space.World);

                    //Rotacao 
                    Vector3 up = new Vector3(newPositions[6], newPositions[7], newPositions[8]);
                    Vector3 forward = new Vector3(newPositions[9], newPositions[10], newPositions[11]);
                    Cubo.transform.localRotation = Quaternion.LookRotation(forward, up);
                }

                AnotaCSV(true);

                oldPositions[0] = newPositions[0];
                oldPositions[1] = newPositions[1];
                oldPositions[2] = newPositions[2];
                old_timestamp = new_timestamp;
                UltimaPos = Cubo.transform.position;
            }
            else
            {//Caso o cubo nao esteja sendoreconhecido
                new_timestamp = json["timestamp"].ToString();
                uma_vez = false;
                /* TIMESPAN do sistema
                TimeSpan tempo = System.DateTime.Now.Subtract(System.DateTime.UnixEpoch);
                Debug.Log(tempo.TotalMilliseconds / 1000 + " aaaa");
                Debug.Log(new_timestamp + " bbbb");
                */
                if (gravar && new_timestamp != old_timestamp)
                {
                    AnotaCSV(false);
                    old_timestamp = new_timestamp;
                }
            }
        }
        catch (System.Exception e)
        {
            Debug.Log(e);
        }
    }

    void Rotacionar(bool first)
    {

        float rot_min = 0.8f;
        float rot_max = 500f;
        Vector3 up = new Vector3(newPositions[6], newPositions[7], newPositions[8]);
        Vector3 forward = new Vector3(newPositions[9], newPositions[10], newPositions[11]);

        var variacao = Vector3.Distance(Cubo.transform.localRotation.eulerAngles, Quaternion.LookRotation(forward, up).eulerAngles);

        //Debug.Log("ROT:"+variacao);
        if (first)
        {//primeira  vez
            Cubo.transform.localRotation = Quaternion.LookRotation(forward, up);
        }
        else if (variacao > rot_min && variacao < rot_max)
        {

            Cubo.transform.localRotation = Quaternion.LookRotation(forward, up);
            //Cubo.transform.localRotation = Quaternion.LookRotation(up, forward);
        }


    }

    void Transladar()
    {
        float Dis_min = 0.05f;
        float Dis_max = 1f;

        Vector3 NextMov = new Vector3((newPositions[0] - oldPositions[0]) * config.x_inversor, (-newPositions[1] + oldPositions[1]) * config.y_inversor, (-newPositions[2] + oldPositions[2]) * config.z_inversor);

        Vector3 NewCubo = Cubo.transform.position + NextMov;
        var distancia = Vector3.Distance(Cubo.transform.position, NewCubo);

        if (distancia != 0)
        {
            //Debug.Log("Trans:"+distancia.ToString());
        }

        //Debug.Log(Cubo.transform.forward);
        if (distancia >= Dis_min && distancia <= Dis_max)
        {
            Cubo.transform.Translate(NextMov, Space.World);
            Debug.Log("X: " + meanPositions(lastPosition, "x") + " Y: " + meanPositions(lastPosition, "y") + " Z: " + meanPositions(lastPosition, "z"));
            AddPositionsInList(new Vector3(newPositions[0], newPositions[1], newPositions[2]));
            //Cubo.transform.position=new Vector3(newPositions[0]* config.x_inversor,newPositions[1]* config.y_inversor,(newPositions[2]* config.z_inversor)-72f);
        }
        else if (distancia > Dis_max)
        {
            Cubo.transform.Translate(new Vector3(meanPositions(lastPosition, "x"), meanPositions(lastPosition, "y"), meanPositions(lastPosition, "z")), Space.World);
            //AddPositionsInList(lastPosition[lastPosition.Count]);
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
        GameObject.Find("CuboAR").GetComponent<AnotaPosQuest>().AnotaCSV(ConversorTempo((double)json[jtokens[0]]).ToString("HH:mm:ss.fff")); //csv hap

        if (Detectando)
        {//Recebendo informacoes do AR Tracking

            arquivo.Write(ConversorTempo((double)json[jtokens[0]]).ToString("HH:mm:ss.fff"));
            arquivo.Write(";");
            arquivo.Write(json[jtokens[1]].ToString());
            arquivo.Write(";");
            arquivo.Write(json[jtokens[2]].ToString());
            arquivo.Write(";");
            arquivo.Write(json[jtokens[3]].ToString());
            arquivo.Write(";");
            arquivo.Write(json[jtokens[4]].ToString());
            arquivo.Write(";");
            arquivo.Write(Cubo.transform.localRotation.x.ToString());
            arquivo.Write(";");
            arquivo.Write(Cubo.transform.localRotation.y.ToString());
            arquivo.Write(";");
            arquivo.Write(Cubo.transform.localRotation.z.ToString());
            arquivo.Write(";");
            arquivo.Write(Cubo.transform.localRotation.w.ToString());
            arquivo.WriteLine();
        }
        else
        { // AR tracking parou de enviar armazena so o tempo e o sucess
            for (int i = 0; i < 2; i++)
            {


                if (i == 0)
                {
                    arquivo.Write(ConversorTempo((double)json[jtokens[i]]).ToString("HH:mm:ss.fff"));
                    arquivo.Write(";");
                }
                else
                {
                    arquivo.Write(json[jtokens[i]].ToString());
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

    public static DateTime ConversorTempo(double unixTimeStamp)
    {
        // Unix timestamp is seconds past epoch
        DateTime dateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
        dateTime = dateTime.AddSeconds(unixTimeStamp).ToLocalTime();
        return dateTime;
    }

    ///<summary>Classe <c>meanPositions</c> retorna a média das distancias entre cada posicao armazenada
    ///</summary>
    private float meanPositions(List<Vector3> positions, string tipo)
    {//x,y,z,distancia
        if (positions.Count < 1) return 0;
        float mean = 0;
        float sum = 0;
        switch (tipo)
        {
            case "x":
                for (int i = 0; i < positions.Count - 1; i++)
                {// de 0 a te 58 medindo a distancia entre um e o proximo a ele
                    var distance = positions[i].x - positions[i + 1].x;
                    sum += distance;
                }

                mean = sum / (positions.Count - 1);
                break;
            case "y":
                for (int i = 0; i < positions.Count - 1; i++)
                {// de 0 a te 58 medindo a distancia entre um e o proximo a ele
                    var distance = positions[i].y - positions[i + 1].y;
                    sum += distance;
                }

                mean = sum / (positions.Count - 1);
                break;
            case "z":
                for (int i = 0; i < positions.Count - 1; i++)
                {// de 0 a te 58 medindo a distancia entre um e o proximo a ele
                    var distance = positions[i].z - positions[i + 1].z;
                    sum += distance;
                }

                mean = sum / (positions.Count - 1);

                break;
            case "distancia":
                for (int i = 0; i < positions.Count - 1; i++)
                {// de 0 a te 58 medindo a distancia entre um e o proximo a ele
                    var distance = Vector3.Distance(positions[i], positions[i + 1]);
                    sum += distance;
                }

                mean = sum / (positions.Count - 1);
                break;
        }

        return mean;
    }


    ///<summary>Classe <c>AddPositionsInList</c> guarda os valores de rotação atuais em listas correspondentes.
    ///</summary>
    private void AddPositionsInList(Vector3 vetor)
    {
        // Se já tem mais de 60 frames guardados,
        // apaga o mais antigo, de index 0
        if (lastPosition.Count >= 60)
        {
            lastPosition.RemoveAt(0);
        }

        lastPosition.Add(vetor);
    }
}


/* RETORNO do AR TRACKING
{"timestamp": 1677594319.8186967, "success": true, "translation_x": 11.430583295477035, "translation_y": 5.142802280146702, "translation_z": 42.815210412740825, 
"rotation_right_x": -0.9399653958653161, "rotation_right_y": 0.31731478045920997, "rotation_right_z": -0.12560407906546253, "rotation_up_x": -0.3016497091359976, 
"rotation_up_y": -0.6003944856751889, "rotation_up_z": 0.7406307545254879, "rotation_forward_x": 0.15960108882438012, "rotation_forward_y": 0.7340557142839697, 
"rotation_forward_z": 0.6600679516331054}
*/

