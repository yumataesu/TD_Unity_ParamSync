using System.IO;
using System.Collections.Generic;
using System.Text.Json;

using UnityEngine;

namespace Nqmq.Osc
{
    public class TDOscDatInterfaceExporter : MonoBehaviour
    {
        [SerializeField] private string outputDirectory;
        private string _jsonStr;

        public void Export()
        {
            HashSet<string> customPages = new HashSet<string>();
            TDOscDatReceiver[] rcvs = FindObjectsOfType<TDOscDatReceiver>();
            foreach (var rcv in rcvs)
            {
                string top = rcv.Address.Split('/', System.StringSplitOptions.RemoveEmptyEntries)[0];
                customPages.Add(top);
            }

            Dictionary<string, List<InterfaceData>> data = new Dictionary<string, List<InterfaceData>>();
            foreach (string page in customPages)
            {

                List<InterfaceData> interf = new List<InterfaceData>();

                foreach (var rcv in rcvs)
                {
                    string toppath = "/" + page;
                    Debug.Log(rcv.interfaceData.address);
                    Debug.Log(toppath);
                    Debug.Log("--");

                    if (rcv.interfaceData.address.StartsWith(toppath))
                    {
                        interf.Add(rcv.interfaceData);
                    }
                }
                data.Add(page, interf);
            }

            _jsonStr = JsonSerializer.Serialize(data);
            Debug.Log(_jsonStr);


            string path = outputDirectory + "/osc_settings.json";
            StreamWriter writer = new StreamWriter(path, false);
            writer.WriteLine(_jsonStr);
            writer.Close();
        }
    }

#if UNITY_EDITOR
    [UnityEditor.CanEditMultipleObjects, UnityEditor.CustomEditor(typeof(TDOscDatInterfaceExporter))]
    public class TDOscDatInterfaceExporterEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            TDOscDatInterfaceExporter obj = target as TDOscDatInterfaceExporter;
            if (GUILayout.Button("Export Osc Interface Define json")) obj.Export();
        }
    }
#endif
}

