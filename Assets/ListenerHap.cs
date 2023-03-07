using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class ListenerHap : MonoBehaviour
{
    Vector3 PosicoesHaptico;
    Quaternion AnguloHaptico;
    private StreamWriter arquivo;

    string[] header = {"position_x","position_y","position_z","rotation_x","rotation_y","rotation_z"};
    string[] data={"0","0","0","0","0","0"};

    void Start(){
        CriaCSV();
    }
    void FixedUpdate()
    {
        PosicoesHaptico = this.GetComponent<HapticPlugin>().proxyPositionRaw;
        AnguloHaptico = this.GetComponent<HapticPlugin>().proxyOrientationRaw;
        SalvaDados();
        AnotaCSV();
    }

    void SalvaDados(){
        data[0]=PosicoesHaptico.x.ToString();
        data[1]=PosicoesHaptico.y.ToString();
        data[2]=PosicoesHaptico.z.ToString();
        data[3]=AnguloHaptico.x.ToString();
        data[4]=AnguloHaptico.y.ToString();
        data[5]=AnguloHaptico.z.ToString();
    }

    void CriaCSV() //Cria o arquivo CSV aonde serao armazenados os dados
    {
        try
        {
            arquivo = new StreamWriter(Application.dataPath + "/Data/" + "coisas.csv");
            for (int i = 0; i < header.Length; i++)
            {
                arquivo.Write(header[i]);
                if (i < header.Length - 1)
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
void AnotaCSV()
    {
            for (int i = 0; i < 6; i++)
            {
                arquivo.Write(data[i].ToString().Replace(',', '.'));
                if (i < 5)
                {
                    arquivo.Write(",");
                }
                else
                {
                    arquivo.WriteLine();
                }
            }
            Debug.Log(AnguloHaptico.eulerAngles);
        }
    void FechaCSV()
    {
        arquivo.Flush();
        arquivo.Close();
        Debug.Log("Salvo com sucesso");
    }

    void OnDestroy()
    {
                FechaCSV();
        }

    }
