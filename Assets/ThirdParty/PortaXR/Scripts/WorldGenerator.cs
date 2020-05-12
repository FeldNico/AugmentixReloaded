using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.Assertions;
using Npgsql;
using System;

public class WorldGenerator : EditorWindow
{
    [MenuItem("PortaXR/Generate", false, 2000)]
    static void OpenWindow()
    {
        GetWindow<WorldGenerator>(true);
    }
    
    void OnGUI()
    {
        if (GUILayout.Button("Create Terrain"))
        {
            net = new PsmNetwork();

            net.EstablishServerConnection();
            GenerateWorld();
            net.CloseServerConnection();
        }
    }
    
    public class PXR_Model
    {
        public string id = "_00000";
        public Vector3 position = Vector3.zero;
    }

    public class PXR_PolygonModel
    {
        public List<Vector2> polygon;
        public float height;
    }

    private PsmNetwork net = null;

    // map settings
    public static Vector2 worldReference = new Vector2(2546400f, 5513500f); // EPSG31466

    // models
    public Material streetMat;
    public Material buildingMat;
    

    void Start()
    {
        net = new PsmNetwork();

        net.EstablishServerConnection();
        GenerateWorld();
        net.CloseServerConnection();
    }

    private void GenerateWorld()
    {
        Debug.Log("Elevate terrain...");
        GenerateTerrain();

        Debug.Log("Rendering ways...");
        GenerateStreets();

        Debug.Log("Rendering walls...");
        GenerateWalls();

        Debug.Log("Rendering buildings...");
        GenerateBuildings();
    }

    private void GenerateTerrain()
    {
        /*
        Terrain terrain = new Terrain();
        terrain.terrainData = new TerrainData();
        terrain.terrainData.se

        string rawPath = Application.dataPath + "/models/terrain.raw";
        int height = 100;
        int width = 100;
        float[ , ] data = new float[height, width];
        using (var file = System.IO.File.OpenRead(rawPath))
        using (var reader = new System.IO.BinaryReader(file))
        {
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    float v = (float)reader.ReadUInt16() / 0xFFFF;
                    data[y, x] = v;
                }
            }
        }
        terrain.terrainData.SetHeights(0, 0, data);
        */
    }

    // TODO: 
    // - care about different street width and types
    private void GenerateStreets()
    {
        List<List<Vector2>> ways = net.QueryLinesWhere("highway = 'residential'");
        GameObject streetRoot = new GameObject("Streets");
      
        foreach (List<Vector2> way in ways)
        {
            // TODO get those values from somewhere meaningfull and acessible...
            float streetThickness = 0.3f;
            float streetWidth = 10f;

            // resample path
            // TODO maybe dynamic?
            float sampleEveryXMeter = 5.0f;
            List<Vector2> resampledWay = new List<Vector2>();
            resampledWay.Add(way[0]);

            for (int i = 0; i + 1 < way.Count; ++i)
            {
                Vector2 v2_diff = way[i+1] - way[i];
                int numOfSamples = Mathf.FloorToInt(v2_diff.magnitude / sampleEveryXMeter) - 1;

                if (numOfSamples > 0)
                {
                    float stepSize = v2_diff.magnitude / numOfSamples;    
                    for(int j = 1 ; j < numOfSamples ; ++j)
                    {
                        resampledWay.Add(way[i] + v2_diff.normalized * stepSize * j);
                    }   
                }
                resampledWay.Add(way[i + 1]);
            }

            // TODO merge meshes? at least for segemnts which were segements before?
            // get terrain hights at path start and end point
            Vector3 v3_startPos = new Vector3(resampledWay[0].x, 0, resampledWay[0].y);
            Vector3 v3_endPos = new Vector3(resampledWay[1].x, 0, resampledWay[1].y);
            Vector3 v3_pathDirection = Vector3.Cross(v3_endPos - v3_startPos, Vector3.up).normalized;

            // Generate path width by seachinng points to the "left" and the "right" of the base point 
            Vector3 v3_startPosLeft = PutPointOnSurface(v3_startPos + v3_pathDirection * streetWidth / 2.0f);
            Vector3 v3_startPosRight = PutPointOnSurface(v3_startPos - v3_pathDirection * streetWidth / 2.0f);

            for (int i = 0; i + 1 < resampledWay.Count; ++i)
            {
                // get terrain hights at path start and end point
                v3_startPos = new Vector3(resampledWay[i].x, 0, resampledWay[i].y);
                v3_endPos = new Vector3(resampledWay[i + 1].x, 0, resampledWay[i + 1].y);
                v3_pathDirection = Vector3.Cross(v3_endPos - v3_startPos, Vector3.up).normalized;

                GameObject pathPiece = new GameObject("generated Path");
                pathPiece.layer = LayerMask.NameToLayer("Paths");
                pathPiece.transform.parent = streetRoot.transform;

                // Generate path width by seachinng points to the "left" and the "right" of the base point 
                Vector3 v3_endPosLeft = PutPointOnSurface(v3_endPos + v3_pathDirection * streetWidth / 2.0f);
                Vector3 v3_endPosRight = PutPointOnSurface(v3_endPos - v3_pathDirection * streetWidth / 2.0f);

                // Create a polygon for the triagulator
                List<Vector3> pathPolygon = new List<Vector3>();
                pathPolygon.Add(v3_startPosLeft);
                pathPolygon.Add(v3_startPosRight);
                pathPolygon.Add(v3_endPosRight);
                pathPolygon.Add(v3_endPosLeft);

                DrawMeshFromPolygon(pathPiece, pathPolygon, buildingMat, streetThickness);

                // make sure that the mesh neighbour parts are connected 
                v3_startPosLeft = v3_endPosLeft;
                v3_startPosRight = v3_endPosRight;
            }
        }
    }

    private void GenerateWalls()
    {
        List<List<Vector2>> walls = net.QueryLinesWhere("barrier = 'city_wall'");
        GameObject wallsRoot = new GameObject("Walls");

        foreach (List<Vector2> wall in walls)
        {
            for (int i = 0; i + 1 < wall.Count; ++i)
            {
                // TODO get those values from somewhere meaningfull and acessible...
                float wallHeight = 15f;
                float wallThinkness = 6f;

                // get terrain hights at wall start and end point
                Vector3 v3_startPos = PutPointOnSurface(wall[i]);
                Vector3 v3_endPos = PutPointOnSurface(wall[i + 1]);
                Vector3 v3_wallDirection = Vector3.Cross(v3_endPos - v3_startPos, Vector3.up).normalized;

                GameObject wallPiece = new GameObject("generated Wall");
                wallPiece.transform.parent = wallsRoot.transform;

                // TODO maybe interpolate and probe terrain high inbetween starting end ending point to cover potential holes in the ground
                List<Vector3> wallPolygon = new List<Vector3>();
                wallPolygon.Add(v3_startPos);
                wallPolygon.Add(v3_startPos - v3_wallDirection * wallThinkness);
                wallPolygon.Add(v3_endPos - v3_wallDirection * wallThinkness);
                wallPolygon.Add(v3_endPos);

                DrawMeshFromPolygon(wallPiece, wallPolygon, buildingMat, wallHeight);

                /*
                // when you wannt do it with modles and want to manipulate them
                // caclculate the slope between start and end point
                // put a standard cube into the scene hieracy, which has to be modified in the follwing
                
                // calculate the wall elemet's mid point as this will be the origin of the generated piece
                Vector2 mid = (wall[i + 1] + wall[i]) / 2;
                Vector2 direction = wall[i + 1] - wall[i];

                GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
                cube.transform.parent = wallRoot.transform;

                Vector3 v3_horizonzal = v3_endPos - v3_startPos;
                v3_horizonzal.y = 0;

                float elementSlope = Vector3.SignedAngle(v3_horizonzal, v3_endPos - v3_startPos, v3_horizonzal);

                cube.transform.position = new Vector3(mid.x, Math.Max(v3_startPos.y, v3_endPos.y) + hight / 2, mid.y);
                cube.transform.localScale = new Vector3(direction.magnitude, hight, 1f);

                cube.transform.rotation = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.y), Vector3.up);
                cube.transform.Rotate(0, 90, 0);
                cube.transform.Rotate(0, 0, -elementSlope);
                */
            }
        }
    }

    private Vector3 PutPointOnSurface(Vector3 point)
    {
        return new Vector3(point.x, GetTerrainHeight(point), point.z);
    }

    private Vector3 PutPointOnSurface(Vector2 point)
    {     
        return new Vector3(point.x, GetTerrainHeight(point), point.y);
    }

    private float GetTerrainHeight(Vector2 point)
    {
        Vector3 point3D = new Vector3(point.x, 0, point.y);
        return GetTerrainHeight(point3D);
    }

    private float GetTerrainHeight(Vector3 point)
    {
        // raycast hits only terrain
        LayerMask layermask = LayerMask.GetMask("Terrain");
        RaycastHit hit;
        // TODO starting at a arbitrary hight (here 1000m) above NN is maybe not the best solution, however do not see a simple other one now 
        Physics.Raycast(point + Vector3.up * 1000, -Vector3.up, out hit, Mathf.Infinity, layermask);
        return hit.point.y;
    }

    private List<Vector3> Put2DPoligonOnSurface(List<Vector2> polygon)
    {
        List<Vector3> polygons3D = new List<Vector3>();
       
        float lowestLevel = Mathf.Infinity;

        foreach (Vector2 elem in polygon)
        {
            float height = GetTerrainHeight(elem);
            if (height < lowestLevel)
            {
                lowestLevel = height;
            }
        }

        foreach (Vector2 elem in polygon)
        {
            Vector3 point = new Vector3(elem.x, lowestLevel, elem.y);
            polygons3D.Add(point);
        }

        return polygons3D;
    }

    private void DrawMeshFromPolygon(GameObject go, List<Vector3> polygon, Material mat, float height)
    {
        MeshFilter meshFilter = go.AddComponent<MeshFilter>();
        meshFilter.mesh = Triangulator.CreateExtrudedMeshFromPolygon(polygon, height);
        Renderer rend = go.AddComponent<MeshRenderer>();
        rend.material = mat;
        go.AddComponent<SerializeMesh>().Serialize();
    }

    private void GenerateBuildings()
    {
        // Get buildings with models
        List<PXR_Model> buildings = net.QueryBuildingsWithModels();
        GameObject buildingsRoot = new GameObject("Buildings");

        foreach (PXR_Model building in buildings)
        {
            string[] GUIDs = AssetDatabase.FindAssets(building.id, new[] { "Assets/ThirdParty/PortaXR/Models" });

            if (GUIDs.Length != 0)
            {
                // there is a problem, if more then one object found to the unique ID
                Assert.IsTrue(GUIDs.Length == 1);
                GameObject go = (GameObject)AssetDatabase.LoadAssetAtPath(AssetDatabase.GUIDToAssetPath(GUIDs[0]), typeof(GameObject));
                Instantiate(go, PutPointOnSurface(building.position), Quaternion.Euler(0, 180, 0), buildingsRoot.transform);
            }
            // TODO:
            // else if (in central shared model container) load it
            // else error
        }

        // generate models for buildings with just polygons
        List<PXR_PolygonModel> polygons = net.QueryBuildingsWithPolygons();
       
        foreach(PXR_PolygonModel item in polygons)
        {
            GameObject building = new GameObject("Generated Building");
            building.transform.parent = buildingsRoot.transform;

           item.polygon.RemoveAt(item.polygon.Count - 1);
           DrawMeshFromPolygon(building, Put2DPoligonOnSurface(item.polygon), buildingMat, item.height);
        }       
    }
}