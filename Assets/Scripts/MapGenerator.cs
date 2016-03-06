using UnityEngine;
using System.Collections;
using pn = PerlinNoise.PerlinNoise;

public class MapGenerator : MonoBehaviour 
{

	int width;
	int height;

	public string seed;
	public bool useRandomSeed;
	public bool usePerlinNoise;

	public int perlinOctaves = 6;
	//[Range(0,100)]
	//public int randomFillPercent;
	public int step = 16;


	//public GameObject mapTile;

	int [,] map;
	//GameObject[,] tiles;

	int numLevels = 128;
	int prevStep;
	//public int prevWaterLevel;

	System.Random prng;

	void Start()
	{
		/*prevStep = step;
		if (useRandomSeed)
		{
			seed = Time.time.ToString ();
		}
		prng = new System.Random (seed.GetHashCode ());

		map = new int[width, height];
		//tiles = new GameObject[width, height];

		prevWaterLevel = waterLevel;

		GenerateMap ();*/
		//Instantiate(mapTile, new Vector3(0, 0, 0), Quaternion.Euler(90f, 0f, 0f));
	}

	/*void Update ()
	{
		if (waterLevel != prevWaterLevel)
		{
			for (int x = 0; x < width; ++x)
			{
				for (int y = 0; y < height; ++y)
				{
					if (map [x, y] <= waterLevel)
						tiles [x, y].GetComponent<Renderer> ().materials [0].color = Color.blue;
					else
						setColorFromHeight (map [x, y], tiles [x, y]);
				}
			}
		}
	}*/


	public int [,] GenerateMap(int width, int height, int numLevels)
	{
		if (useRandomSeed)
		{
			seed = Time.time.ToString ();
		}
		prng = new System.Random (seed.GetHashCode ());

		this.width = width;
		this.height = height;
		this.numLevels = numLevels;
		map = new int[width, height];

		for (int x = 0; x < width; ++x)
		{
			for (int y = 0; y < height; ++y)
			{
				map [x, y] = -1;
			}
		}


		if (usePerlinNoise)
		{
			float[][] perlinMap = pn.GeneratePerlinNoise (
				                  pn.GenerateWhiteNoise (width, height, prng),
				                  perlinOctaves);

			for (int x = 0; x < width; ++x)
			{
				for (int y = 0; y < height; ++y)
				{
					map [x, y] = (int)(perlinMap[x][y] * numLevels);
				}
			}
		} 
		else
		{
			prevStep = step;
			StepFillMap ();
		}

		return map;
	}

	void InitialStepFillMap()
	{
		// Random fill the initial square grid
		for (int x = 0; x < width; x += step)
		{
			for (int y = 0; y < height; y += step)
			{
				//if (x % step == 0 && y % step == 0)
				map [x, y] = prng.Next (0, numLevels);
				//else
				//	map [x, y] = 0;
			}
		}
			
		// Fill the square centres
		for (int x = step/2; x < width; x += step)
		{
			for (int y = step/2; y < height; y += step)
			{
				//if (x % step == 0 && y % step == 0)
				map [x, y] = prng.Next (0, numLevels);
				//else
				//	map [x, y] = 0;
			}
		}

		prevStep = step;
		step /= 2;
	}

	void StepFillMap()
	{
		// Initial steps
		// Random fill the initial square grid
		for (int x = 0; x < width; x += step)
		{
			for (int y = 0; y < height; y += step)
			{
				map [x, y] = prng.Next (0, numLevels);
				//setColorFromHeight (map [x, y], tiles [x, y]);
				//Material mat = tiles [x, y].GetComponent<Material>();
				//mat.color = colorFromHeight(map [x, y]);
			}
		}

		// Fill the square centres
		for (int x = step/2; x < width; x += step)
		{
			for (int y = step/2; y < height; y += step)
			{
				map [x, y] = prng.Next (0, numLevels);
				//setColorFromHeight (map [x, y], tiles [x, y]);
			}
		}

		//prevStep = step;
		//step /= 2;


		while (step > 0)
		{
			//if (step <= 0)
			//	return;

			for (int x = 0; x < width; x += step)
			{
				for (int y = 0; y < height; y += step)
				{
					
					if (x % prevStep == 0 && y % prevStep == 0) // Done on the previous iteration
						continue;
					else if (x % prevStep == 0 || y % prevStep == 0) // On a major column or major row
					{
						if (map [x, y] == -1)
						{
							// Interpolate from the compass points
							int nextY = y + step;
							if (nextY >= height)
								nextY -= height;

							int prevY = y - step;
							if (prevY < 0)
								prevY += height;
					
							int nextX = x + step;
							if (nextX >= width)
								nextX -= width;
					
							int prevX = x - step;
							if (prevX < 0)
								prevX += width;
					
							map [x, y] = (map [nextX, y] + map [x, nextY] + map [prevX, y] + map [x, prevY]) / 4;
							//setColorFromHeight (map [x, y], tiles [x, y]);
						}
					} else // On a crossing diagonal
					{
					
					
					}
				}
			}

			prevStep = step;
			step /= 2;

			// Fill the square centres
			for (int x = step; x < width; x += prevStep)
			{
				for (int y = step; y < height; y += prevStep)
				{
					if (map [x, y] == -1)
					{
						//Debug.Log ("Diag " + x + " " + y + ": " + map [x, y]);
						// Interpolate from the four corner points
						int nextY = y + step;
						if (nextY >= height)
							nextY -= height;

						int prevY = y - step;
						if (prevY < 0)
							prevY += height;

						int nextX = x + step;
						if (nextX >= width)
							nextX -= width;

						int prevX = x - step;
						if (prevX < 0)
							prevX += width;
					
						map [x, y] = (map [nextX, nextY] + map [nextX, prevY] + map [prevX, prevY] + map [prevX, nextY]) / 4;
						//setColorFromHeight (map [x, y], tiles [x, y]);
					}
				}
			}
		}
	}

	void setColorFromHeight(int height, GameObject tile)
	{
		Renderer rend = tile.GetComponent<Renderer> ();
		//Debug.Log (rend.ToString ());

		//Material mat = rend.GetComponent<Material>();
		//Debug.Log (mat.ToString ());
		rend.materials[0].color = colorFromHeight(height);
	}

	Color colorFromHeight(int height)
	{
		float greyscale = 1f - (height / (float)numLevels);
		return new Color (greyscale, greyscale, greyscale);
	}




	void RandomFillMap ()
	{
		if (useRandomSeed)
		{
			seed = Time.time.ToString ();
		}

		System.Random prng = new System.Random (seed.GetHashCode ());

		for (int x = 0; x < width; ++x)
		{
			for (int y = 0; y < height; ++y)
			{
				map [x, y] = prng.Next (0, numLevels);// < randomFillPercent ? 1 : 0;
			}
		}
	}


	/*void SmoothMap ()
	{
		for (int x = 0; x < width; ++x)
		{
			for (int y = 0; y < height; ++y)
			{
				map [x, y] = AverageOfSurrounding (x, y);

				
			}
		}
	}*/

	/*int AverageOfSurrounding (int gridX, int gridY)
	{
		int average = 0;

		for (int neighbourX = gridX - 1; neighbourX <= gridX + 1; ++neighbourX)
		{
			for (int neighbourY = gridY - 1; neighbourY <= gridY + 1; ++neighbourY)
			{
				if (neighbourX >= 0 && neighbourX < width && neighbourY >= 0 && neighbourY < height)
				{
					if (neighbourX != gridX || neighbourY != gridY)
					{
						average += map [gridX, gridY];
					}
				}
			}
		}

		return average / 9;
	}*/


	/*int NumberSurroundingHigher(int gridX, int gridY)
	{
		int numHigher = 0;

		for (int neighbourX = gridX - 1; neighbourX <= gridX + 1; ++neighbourX)
		{
			for (int neighbourY = gridY - 1; neighbourY <= gridY + 1; ++neighbourY)
			{
				if (neighbourX >= 0 && neighbourX < width && neighbourY >= 0 && neighbourY < height)
				{
					if (neighbourX != gridX || neighbourY != gridY)
					{
						if (map [neighbourX, neighbourY] > map [gridX, gridY])
						{
							++numHigher;
						}
					}
				}
			}
		}

		return numHigher;
	}*/

	/*int GetSurroundingWallCount (int gridX, int gridY)
	{
		int wallCount = 0;
		for (int neighbourX = gridX - 1; neighbourX <= gridX + 1; ++neighbourX)
		{
			for (int neighbourY = gridY - 1; neighbourY <= gridY + 1; ++neighbourY)
			{
				if (neighbourX >= 0 && neighbourX < width && neighbourY >= 0 && neighbourY < height)
				{
					if (neighbourX != gridX || neighbourY != gridY)
					{
						wallCount += map [neighbourX, neighbourY];
					}
				} 
				else
				{
					wallCount++;
				}
			}
		}

		return wallCount;
	}*/

	/*void OnDrawGizmos ()
	{
		if (map != null)
		{
			for (int x = 0; x < width; ++x)
			{
				for (int y = 0; y < height; ++y)
				{
					if (map [x, y] <= waterLevel)
						Gizmos.color = Color.blue;
					else
					{
						float scale = 1f - (map [x, y] / 100f);
						Gizmos.color = new Color (scale, scale, scale); //map [x, y] == 1 ? Color.black : Color.white;
					}
					Vector3 pos = new Vector3 (-width / 2 + x + .5f, 0, -height / 2 + y + .5f);
					Gizmos.DrawCube (pos, Vector3.one);
				}
			}
		}
	}*/
}
