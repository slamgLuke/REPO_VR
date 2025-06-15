using UnityEditor;
using UnityEngine;
using System.IO;

public class SetupAllProps
{
    [MenuItem("Tools/🛠️ Setup Props (carpeta por carpeta)")]
    public static void SetupPropsFromSubfolders()
    {
        string rootFolder = "Assets/Props/";
        string[] subfolders = Directory.GetDirectories(rootFolder);

        int total = 0;

        foreach (string subfolder in subfolders)
        {
            string[] fbxFiles = Directory.GetFiles(subfolder, "*.fbx", SearchOption.TopDirectoryOnly);
            if (fbxFiles.Length == 0)
                continue;

            string assetPath = fbxFiles[0].Replace("\\", "/");
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
            if (prefab == null)
                continue;

            GameObject instance = PrefabUtility.InstantiatePrefab(prefab) as GameObject;
            instance.name = prefab.name + "_setup";

            if (instance.GetComponent<Rigidbody>() == null)
                instance.AddComponent<Rigidbody>();

            foreach (MeshFilter mf in instance.GetComponentsInChildren<MeshFilter>())
            {
                GameObject go = mf.gameObject;
                if (go.GetComponent<Collider>() == null)
                {
                    var mc = go.AddComponent<MeshCollider>();
                    mc.convex = true;
                }
            }

            if (instance.GetComponent<DestructibleObject>() == null)
                instance.AddComponent<DestructibleObject>();

            total++;
        }

        Debug.Log($"✅ Setup completado para {total} props.");
    }
}
