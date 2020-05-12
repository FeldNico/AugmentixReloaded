using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using UnityEngine;
using UnityEditor;

public class Object2Terrain : EditorWindow
{
    [MenuItem("Terrain/Object to Terrain", false, 2000)]
    static void OpenWindow()
    {
        EditorWindow.GetWindow<Object2Terrain>(true);
    }

    private int resolution = 512;
    private int subdivisions = 4;
    int bottomTopRadioSelected = 0;
    static string[] bottomTopRadio = new string[] {"Bottom Up", "Top Down"};
    private float shiftHeight = 0f;

    void OnGUI()
    {
        resolution = EditorGUILayout.IntField("Resolution", resolution);
        subdivisions = EditorGUILayout.IntField("Subdivisions", subdivisions);
        shiftHeight = EditorGUILayout.Slider("Shift height", shiftHeight, -1f, 1f);
        bottomTopRadioSelected = GUILayout.SelectionGrid(bottomTopRadioSelected, bottomTopRadio, bottomTopRadio.Length,
            EditorStyles.radioButton);

        if (GUILayout.Button("Create Terrain"))
        {
            if (Selection.activeGameObject == null)
            {
                EditorUtility.DisplayDialog("No object selected", "Please select an object.", "Ok");
                return;
            }

            else
            {
                CreateTerrain();
            }
        }
    }

    delegate void CleanUp();

    void CreateTerrain()
    {
        //fire up the progress bar
        ShowProgressBar(1, 100);


        CleanUp cleanUp = null;
        
        var selection = Selection.activeGameObject.GetComponentInChildren<MeshFilter>().gameObject;

        Bounds bounds = new Bounds();
        foreach (var filter in Selection.activeGameObject.GetComponentsInChildren<MeshFilter>())
        {
            var collider = filter.GetComponent<MeshCollider>();
            if (!collider)
            {
                collider = filter.gameObject.AddComponent<MeshCollider>();
            }
            bounds.Encapsulate(collider.bounds);
        }

        /*
        MeshCollider collider = selection.GetComponent<MeshCollider>();
        

        //Add a collider to our source object if it does not exist.
        //Otherwise raycasting doesn't work.
        if (!collider)
        {
            collider = selection.AddComponent<MeshCollider>();
            cleanUp = () => DestroyImmediate(collider);
        }
        */

        //Bounds bounds = collider.bounds;

        for (int i = 0; i < subdivisions; i++)
        {
            for (int j = 0; j < subdivisions; j++)
            {
                TerrainData terrain = new TerrainData();
                terrain.heightmapResolution = resolution;
                GameObject terrainObject = Terrain.CreateTerrainGameObject(terrain);

                terrain.size = new Vector3(bounds.size.x / subdivisions, bounds.size.y,
                    bounds.size.z / subdivisions);

                // Do raycasting samples over the object to see what terrain heights should be
                float[,] heights = new float[terrain.heightmapResolution, terrain.heightmapResolution];
                Ray ray = new Ray(
                    new Vector3(bounds.min.x + (i * terrain.size.x), bounds.max.y + bounds.size.y,
                        bounds.min.z + (j * terrain.size.z)), -Vector3.up);
                RaycastHit hit = new RaycastHit();
                float meshHeightInverse = 1 / bounds.size.y;
                Vector3 rayOrigin = ray.origin;

                int maxHeight = heights.GetLength(0);
                int maxLength = heights.GetLength(1);

                Vector2 stepXZ = new Vector2(bounds.size.x / (subdivisions * maxLength),
                    bounds.size.z / (subdivisions * maxHeight));

                for (int zCount = 0; zCount < maxHeight; zCount++)
                {
                    ShowProgressBar(zCount, maxHeight);

                    for (int xCount = 0; xCount < maxLength; xCount++)
                    {
                        float height = 0.0f;

                        if (Physics.Raycast(ray, out hit, bounds.size.y * 3))
                        {
                            height = (hit.point.y - bounds.min.y) * meshHeightInverse;
                            height += shiftHeight;

                            //clamp
                            if (height < 0)
                            {
                                height = 0;
                            }
                        }

                        heights[zCount, xCount] = height;
                        rayOrigin.x += stepXZ[0];
                        ray.origin = rayOrigin;
                    }

                    rayOrigin.z += stepXZ[1];
                    rayOrigin.x = bounds.min.x + (i * terrain.size.x);
                    ray.origin = rayOrigin;
                }


                terrain.SetHeights(0, 0, heights);

                var offset = selection.transform.position - bounds.center;

                terrainObject.transform.position = terrainObject.transform.position - bounds.extents - offset +
                                                   new Vector3((i * terrain.size.x), 0, (j * terrain.size.z));
            }
        }

        EditorUtility.ClearProgressBar();

        if (cleanUp != null)
        {
            cleanUp();
        }
    }

    void ShowProgressBar(float progress, float maxProgress)
    {
        float p = progress / maxProgress;
        EditorUtility.DisplayProgressBar("Creating Terrain...", Mathf.RoundToInt(p * 100f) + " %", p);
    }
}