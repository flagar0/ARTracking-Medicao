using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class ListenerHap : MonoBehaviour
{
    Vector3 PosicoesHaptico;
    Quaternion AnguloHaptico;
    private StreamWriter arquivo;

    string[] header = {"timestamp","position_x","position_y","position_z","rotation_x","rotation_y","rotation_z","rotation_w"};
    string[] data={"0","0","0","0","0","0","0","0"};

    void Start(){
        CriaCSV();
    }
    void Update()
    {
        PosicoesHaptico = this.GetComponent<HapticPlugin>().proxyPositionRaw;
        AnguloHaptico = this.GetComponent<HapticPlugin>().proxyOrientationRaw;
        SalvaDados();
    }

    void SalvaDados(){
        data[1]=PosicoesHaptico.x.ToString();
        data[2]=PosicoesHaptico.y.ToString();
        data[3]=PosicoesHaptico.z.ToString();
        data[4]=AnguloHaptico.x.ToString();
        data[5]=AnguloHaptico.y.ToString();
        data[6]=AnguloHaptico.z.ToString();
        data[7]=AnguloHaptico.w.ToString();
    }

    void CriaCSV() //Cria o arquivo CSV aonde serao armazenados os dados
    {
        try
        {
            try{
                arquivo = new StreamWriter(Application.dataPath + "/Data/" +System.DateTime.Now.Hour.ToString()+"-"+System.DateTime.Now.Minute.ToString()+"-"+System.DateTime.Now.Second.ToString()+"/"+"Medicao_HAP.csv");
            }catch(System.Exception){
            Directory.CreateDirectory(Application.dataPath + "/Data/" +System.DateTime.Now.Hour.ToString()+"-"+System.DateTime.Now.Minute.ToString()+"-"+System.DateTime.Now.Second.ToString()+"/"); 
            arquivo = new StreamWriter(Application.dataPath + "/Data/" +System.DateTime.Now.Hour.ToString()+"-"+System.DateTime.Now.Minute.ToString()+"-"+System.DateTime.Now.Second.ToString()+"/"+"Medicao_HAP.csv");
            }
            for (int i = 0; i < header.Length; i++)
            {
                arquivo.Write(header[i]);
                
                if (i < header.Length - 1)
                {
                    arquivo.Write(";");
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
           // Debug.Log(e);

        }
}
public void AnotaCSV(string timestamp)
    {
            data[0] = timestamp;
            for (int i = 0; i < header.Length; i++)
            {
                arquivo.Write(data[i].ToString());
                if (i < header.Length-1)
                {
                    
                    arquivo.Write(";");
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
        
        arquivo.Close();
        Debug.Log("Salvo com sucesso");
    }

    void OnDestroy()
    {
                FechaCSV();
        }

    }
