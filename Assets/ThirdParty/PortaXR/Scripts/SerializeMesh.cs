using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class SerializeMesh : MonoBehaviour
{
    [HideInInspector] [SerializeField] Vector2[] uv;
    [HideInInspector] [SerializeField] Vector3[] verticies;
    [HideInInspector] [SerializeField] int[] triangles;
    [HideInInspector] [SerializeField]bool serialized = false;
    // Use this for initialization
 
    void Awake()
    {
        if (serialized)
        {
            GetComponent<MeshFilter>().mesh = Rebuild();
        }
    }
 
    void Start()
    {
        if (serialized) return;
 
        Serialize();
    }
 
    public void Serialize()
    {
        var mesh = GetComponent<MeshFilter>().sharedMesh;
 
        uv = mesh.uv;
        verticies = mesh.vertices;
        triangles = mesh.triangles;
 
        serialized = true;
    }
 
    public Mesh Rebuild()
    {
        Mesh mesh = new Mesh();
        mesh.vertices = verticies;
        mesh.triangles = triangles;
        mesh.uv = uv;
           
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
 
        return mesh;
    }
    
#if UNITY_EDITOR
    [CustomEditor(typeof(SerializeMesh)),CanEditMultipleObjects]
    class SerializeMeshEditor : Editor
    {
        SerializeMesh obj;
 
        void OnSceneGUI()
        {
            obj = (SerializeMesh)target;
        }
 
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
 
            if (GUILayout.Button("Rebuild"))
            {
                foreach (var o in targets)
                {
                    obj = (SerializeMesh)o;
                    if (obj)
                    {
                        obj.gameObject.GetComponent<MeshFilter>().sharedMesh = obj.Rebuild();
                    }
                }
            }
 
            if (GUILayout.Button("Serialize"))
            {
                foreach (var o in targets)
                {
                    obj = (SerializeMesh)o;
                    if (obj)
                    {
                        obj.Serialize();
                    }
                }
            }
        }
    }
#endif
}
