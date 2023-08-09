using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;

public class AnotaPosQuest : MonoBehaviour
{

    private StreamWriter arquivo;
    public GameObject AR;
    string[] header = {"timestamp","AR_x","AR_y","AR_z","AR_ang_x","AR_ang_y","AR_ang_z","AR_ang_w","Distancia","HAP_x","HAP_y","HAP_z","HAP_ang_x","HAP_ang_y","HAP_ang_z","HAP_ang_w"};
    string[] data={"0","0","0","0","0","0","0","0","0","0","0","0","0","0","0","0"};
     Vector3 PosicoesAR;
     Quaternion AnguloAR;

    void Start()
    {
        CriaCSV();
    }

     void Update() {
        PosicoesAR = AR.transform.position;
        AnguloAR = AR.transform.rotation;
        SalvaDados();
    }

    void SalvaDados()
    {
        data[1]=PosicoesAR.x.ToString();
        data[2]=PosicoesAR.y.ToString();
        data[3]=PosicoesAR.z.ToString();
        data[4]=AnguloAR.x.ToString();
        data[5]=AnguloAR.y.ToString();
        data[6]=AnguloAR.z.ToString();
        data[7]=AnguloAR.w.ToString();
    }

//Cria o arquivo CSV aonde serao armazenados os dados
void CriaCSV() 
    {
        try
        {
            try{
                arquivo = new StreamWriter(Application.dataPath + "/Data/"
                                            +System.DateTime.Now.Hour.ToString()+"-"
                                            +System.DateTime.Now.Minute.ToString()+"-"
                                            +System.DateTime.Now.Second.ToString()+"/"
                                            +"Pos_Unity.csv");
            }
            
            catch(System.Exception){
                Directory.CreateDirectory(Application.dataPath + "/Data/"
                                            +System.DateTime.Now.Hour.ToString()+"-"
                                            +System.DateTime.Now.Minute.ToString()+"-"
                                            +System.DateTime.Now.Second.ToString()+"/"); 
                arquivo = new StreamWriter(Application.dataPath + "/Data/"
                                            +System.DateTime.Now.Hour.ToString()+"-"
                                            +System.DateTime.Now.Minute.ToString()+"-"
                                            +System.DateTime.Now.Second.ToString()
                                            +"/"+"Pos_Unity.csv");
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
            Debug.Log("Arquivo criado com sucesso!");
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


