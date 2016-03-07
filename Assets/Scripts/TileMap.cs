/**
 * Created by quill18. MIT Licence.
 * 
 */

using UnityEngine;
using System.Collections;

[ExecuteInEditMode]
[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(MeshCollider))]
public class TileMap : MonoBehaviour {
	
	public int size_x = 16;
	public int size_z = 16;
	public float tileSize = 1.0f;

	int heightRange = 256;
	[Range(0,256)]
	public int waterLevel = 150;
	int prevWaterLevel;

	World world;
    bool mount = false;

	//int [,] map;
	
	// Use this for initialization
	void Start () {

		world = GetComponent<World> ();
		//map = 
		world.GenerateHeightMap (size_x, size_z, heightRange);
		BuildMesh();
	}

	void Update ()
	{
		if (waterLevel != prevWaterLevel)
		{
			prevWaterLevel = waterLevel;
			BuildTexture ();
		}

		if (Input.GetMouseButtonDown(0))
		{
			//Debug.Log ("Smoothing map");
			world.SmoothMap (waterLevel);
			//world.CreateMountainRanges (waterLevel);
            mount = true;
			BuildTexture ();

		}
	}

	void BuildTexture()
	{
		int texWidth = size_x;
		int texHeight = size_z;
		Texture2D texture = new Texture2D (texWidth, texHeight);

		for (int y = 0; y < texHeight; ++y)
		{
			for (int x = 0; x < texWidth; ++x)
			{
				Color c;
				//Color c = new Color (Random.Range (0f, 1f), Random.Range (0f, 1f), Random.Range (0f, 1f));
				//if (map [x, y] <= waterLevel)
				//	c = Color.blue;
				//else
				c = colorFromHeight (world.heightMap [x, y]);
				//c = world.biomeFromLatitude (x, y, waterLevel);
				
				texture.SetPixel (x, y, c);
				//Debug.Log ("Tile " + x + ", " + y + ": " + map [x, y]);

			}
		}

        if (mount)
        {
            Debug.Log("Coloring mountain");
            ArrayList mountain = world.getMountain(64, 32, waterLevel);
            foreach (Vector2 loc in mountain)
            {
                texture.SetPixel((int)loc.x, (int)loc.y, Color.magenta);
            } 
        }


		texture.filterMode = FilterMode.Point;
		texture.Apply ();

		MeshRenderer mesh_renderer = GetComponent<MeshRenderer> ();
		mesh_renderer.sharedMaterials [0].mainTexture = texture;

		world.averageHeightAboveWater (waterLevel);
	}
	
	public void BuildMesh() {
		
		int numTiles = size_x * size_z;
		int numTris = numTiles * 2;
		
		int vsize_x = size_x + 1;
		int vsize_z = size_z + 1;
		int numVerts = vsize_x * vsize_z;
		
		// Generate the mesh data
		Vector3[] vertices = new Vector3[ numVerts ];
		Vector3[] normals = new Vector3[numVerts];
		Vector2[] uv = new Vector2[numVerts];
		
		int[] triangles = new int[ numTris * 3 ];

		int x, z;
		for(z=0; z < vsize_z; z++) {
			for(x=0; x < vsize_x; x++) {
				vertices[ z * vsize_x + x ] = new Vector3( x*tileSize, 0, z*tileSize );
				normals[ z * vsize_x + x ] = Vector3.up;
				uv[ z * vsize_x + x ] = new Vector2( (float)x / vsize_x, (float)z / vsize_z );
			}
		}
		//Debug.Log ("Done Verts!");
		
		for(z=0; z < size_z; z++) {
			for(x=0; x < size_x; x++) {
				int squareIndex = z * size_x + x;
				int triOffset = squareIndex * 6;
				triangles[triOffset + 0] = z * vsize_x + x + 		   0;
				triangles[triOffset + 1] = z * vsize_x + x + vsize_x + 0;
				triangles[triOffset + 2] = z * vsize_x + x + vsize_x + 1;
				
				triangles[triOffset + 3] = z * vsize_x + x + 		   0;
				triangles[triOffset + 4] = z * vsize_x + x + vsize_x + 1;
				triangles[triOffset + 5] = z * vsize_x + x + 		   1;
			}
		}
		
		//Debug.Log ("Done Triangles!");
		
		// Create a new Mesh and populate with the data
		Mesh mesh = new Mesh();
		mesh.vertices = vertices;
		mesh.triangles = triangles;
		mesh.normals = normals;
		mesh.uv = uv;
		
		// Assign our mesh to our filter/renderer/collider
		MeshFilter mesh_filter = GetComponent<MeshFilter>();
		MeshRenderer mesh_renderer = GetComponent<MeshRenderer>();
		MeshCollider mesh_collider = GetComponent<MeshCollider>();
		
		mesh_filter.mesh = mesh;
		mesh_collider.sharedMesh = mesh;
		//Debug.Log ("Done Mesh!");

		BuildTexture ();
	}
	
	Color colorFromHeight(int height)
	{
		Color c;
		if (height <= waterLevel)
		{
			c = new Color (0, 0, (float)height / waterLevel);
			//Debug.Log (height + " " + height / 100f * 255f);
		} 
		else
		{
			float g = 1f - (height / (float)heightRange);
			c = new Color (g, g, g);
		}
		return c;
	}
}
