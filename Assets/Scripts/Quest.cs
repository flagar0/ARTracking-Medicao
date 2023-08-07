using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
 
public class MotionRecorder : MonoBehaviour
{
    [System.Serializable]
    private struct MotionFrame
    {
        public int frameIndex;
        public float timeStamp;
        public Matrix4x4 headToWorld;
    }
 
    public GameObject vrcam;
   
    public int ID; // Input the participant ID before running
    public int scenario; // Input the scenario number before running
 
    private StreamWriter writer;
    private bool firstFrameWritten;
 
    // Start is called before the first frame update
    void Start()
    {
        string path = Application.dataPath + "/" + "Data Collection" + "/" + "P" + ID + "_" + "S" + scenario + ".json";
        writer = new StreamWriter(path, false);
        writer.AutoFlush = false;
        writer.WriteLine("{");
        writer.WriteLine("\"frames\":[");
    }
 
    // Update is called once per frame
    void LateUpdate()
    {
        MotionFrame frame;
        frame.frameIndex = Time.frameCount;
        frame.timeStamp = Time.time;
        frame.headToWorld = vrcam.transform.localToWorldMatrix;
 
        string jsonFrame = JsonUtility.ToJson(frame, true);
        if (firstFrameWritten)
        {
            writer.WriteLine(",");
        }
        writer.Write(jsonFrame);
        writer.FlushAsync();
        firstFrameWritten = true;
    }
 
    void OnApplicationQuit()
    {
        writer.WriteLine("]");
        writer.WriteLine("}");
        writer.Close();
    }
}