using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
using TMPro;
using System;
using System.IO;
using Newtonsoft.Json.Linq;

[RequireComponent(typeof(InputData))]
public class DisplayInputData : MonoBehaviour
{
    private StreamWriter arquivo;
    private InputData _inputData;
    string[] header = {"timestamp","left_pos_x","left_pos_y","left_pos_z", "left_rot_x","left_rot_y", "left_rot_z", "left_rot_w",
                                    "right_pos_x","right_pos_y","right_pos_z", "right_rot_x","right_rot_y", "right_rot_z","right_rot_w",
                                    "obj_pos_x", "obj_pos_y", "obj_pos_z", "obj_rot_x", "obj_rot_y", "obj_rot_z", "obj_rot_w"};
    string[] data={"0","0","0","0","0","0","0","0","0","0","0","0","0","0","0","0"};

    private void Start()
    {
        _inputData = GetComponent<InputData>();
        CriaCSV();
    }

    int i = 0;
    // Update is called once per frame
    void Update()
    {
        if (_inputData._leftController.TryGetFeatureValue(CommonUsages.devicePosition, out Vector3 LeftPosition) &&
            _inputData._leftController.TryGetFeatureValue(CommonUsages.deviceRotation, out Quaternion LeftRotation) &&
            _inputData._rightController.TryGetFeatureValue(CommonUsages.devicePosition, out Vector3 RightPosition) &&
            _inputData._rightController.TryGetFeatureValue(CommonUsages.deviceRotation, out Quaternion RightRotation))
        {
            arquivo.Write(i.ToString());
            arquivo.Write(";");
            arquivo.Write(LeftPosition.x.ToString());
            arquivo.Write(";");
            arquivo.Write(LeftPosition.y.ToString());
            arquivo.Write(";");
            arquivo.Write(LeftPosition.z.ToString());
            arquivo.Write(";");
            arquivo.Write(LeftRotation.x.ToString());
            arquivo.Write(";");
            arquivo.Write(LeftRotation.y.ToString());
            arquivo.Write(";");
            arquivo.Write(LeftRotation.z.ToString());
            arquivo.Write(";");
            arquivo.Write(LeftRotation.w.ToString());
            arquivo.Write(";");
            arquivo.Write(RightPosition.x.ToString());
            arquivo.Write(";");
            arquivo.Write(RightPosition.y.ToString());
            arquivo.Write(";");
            arquivo.Write(RightPosition.z.ToString());
            arquivo.Write(";");
            arquivo.Write(RightRotation.x.ToString());
            arquivo.Write(";");
            arquivo.Write(RightRotation.y.ToString());
            arquivo.Write(";");
            arquivo.Write(RightRotation.z.ToString());
            arquivo.Write(";");
            arquivo.Write(RightRotation.w.ToString());
            arquivo.Write(";");
            arquivo.Write(_inputData.transform.position.x.ToString());
            arquivo.Write(";");
            arquivo.Write(_inputData.transform.position.y.ToString());
            arquivo.Write(";");
            arquivo.Write(_inputData.transform.position.z.ToString());
            arquivo.Write(";");
            arquivo.Write(_inputData.transform.rotation.x.ToString());
            arquivo.Write(";");
            arquivo.Write(_inputData.transform.rotation.y.ToString());
            arquivo.Write(";");
            arquivo.Write(_inputData.transform.rotation.z.ToString());
            arquivo.Write(";");
            arquivo.Write(_inputData.transform.rotation.w.ToString());
            arquivo.WriteLine();
        }
        i++;
    }

    //Cria o arquivo CSV aonde serao armazenados os dados
    void CriaCSV() 
    {
        try
        {
            try
            {
                arquivo = new StreamWriter(Application.dataPath + "/Data/Medicao_QUEST_"
                                            +System.DateTime.Now.Day.ToString()+"-"
                                            +System.DateTime.Now.Month.ToString()+"-"
                                            +System.DateTime.Now.Year.ToString()+"-"
                                            +System.DateTime.Now.Hour.ToString()+"-"
                                            +System.DateTime.Now.Minute.ToString()+"-"
                                            +System.DateTime.Now.Second.ToString()+"/"
                                            +"Pos_QUEST.csv");
            }
            
            catch(System.Exception){
                Directory.CreateDirectory(Application.dataPath + "/Data/Medicao_QUEST_"
                                            +System.DateTime.Now.Day.ToString()+"-"
                                            +System.DateTime.Now.Month.ToString()+"-"
                                            +System.DateTime.Now.Year.ToString()+"-"
                                            +System.DateTime.Now.Hour.ToString()+"-"
                                            +System.DateTime.Now.Minute.ToString()+"-"
                                            +System.DateTime.Now.Second.ToString()+"/"); 
                arquivo = new StreamWriter(Application.dataPath + "/Data/Medicao_QUEST_"
                                            +System.DateTime.Now.Day.ToString()+"-"
                                            +System.DateTime.Now.Month.ToString()+"-"
                                            +System.DateTime.Now.Year.ToString()+"-"
                                            +System.DateTime.Now.Hour.ToString()+"-"
                                            +System.DateTime.Now.Minute.ToString()+"-"
                                            +System.DateTime.Now.Second.ToString()
                                            +"/"+"Pos_QUEST.csv");
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
            Debug.Log(e);
        }
    }

    void AnotaCSV(string timestamp)
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

    public static DateTime ConversorTempo( double unixTimeStamp )
    {
        // Unix timestamp is seconds past epoch
        DateTime dateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
        dateTime = dateTime.AddSeconds( unixTimeStamp ).ToLocalTime();
        return dateTime;
    }

}
