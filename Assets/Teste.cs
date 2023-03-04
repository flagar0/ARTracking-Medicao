using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json.Linq;
using System.Text;
using System.IO;

public class Teste : MonoBehaviour
{
    public TextAsset jsonFile;
    JObject json;
    StreamWriter arquivo;
    string[] jtokens = {"timestamp","success","translation_x","translation_y","translation_z","rotation_right_x"
    ,"rotation_right_y","rotation_right_z","rotation_up_x","rotation_up_y","rotation_up_z","rotation_forward_x"
    ,"rotation_forward_y","rotation_forward_z"};
    void Start()
    {
        try
        {
            arquivo = new StreamWriter(Application.dataPath + "/Data/" + "coisas.csv");
            json = JObject.Parse(jsonFile.text);
            CriaCSV();
            EscreveCSV();
            arquivo.Flush();
            arquivo.Close();
            Debug.Log("Criado com sucesso");
            Debug.Log(arquivo.Encoding);
        }
        catch (System.Exception e)
        {
            Debug.Log(e);
        }

    }

    void CriaCSV()
    {
        for (int i = 0; i < jtokens.Length; i++)
        {
            arquivo.Write(jtokens[i]);
            if (i < jtokens.Length-1)
            {
                arquivo.Write(",");
            }
            else
            {
                arquivo.WriteLine();
            }
        }
    }

    void EscreveCSV()
    {
        for (int i = 0; i < json.Count; i++)
        {
            arquivo.Write(json[jtokens[i]]);
            if (i < jtokens.Length-1)
            {
                arquivo.Write(",");
            }
            else
            {
                arquivo.WriteLine();
            }
        }
    }

    /*
    {"timestamp": 1677594319.8186967, "success": true, "translation_x": 11.430583295477035, "translation_y": 5.142802280146702, "translation_z": 42.815210412740825, 
    "rotation_right_x": -0.9399653958653161, "rotation_right_y": 0.31731478045920997, "rotation_right_z": -0.12560407906546253, "rotation_up_x": -0.3016497091359976, 
    "rotation_up_y": -0.6003944856751889, "rotation_up_z": 0.7406307545254879, "rotation_forward_x": 0.15960108882438012, "rotation_forward_y": 0.7340557142839697, 
    "rotation_forward_z": 0.6600679516331054}
    */

}
