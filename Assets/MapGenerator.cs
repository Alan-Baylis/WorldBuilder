using UnityEngine;
using System.Collections;

public class MapGenerator : MonoBehaviour 
{

	public int width;
	public int height;

	public string seed;
	public bool useRandomSeed;

	[Range(0,100)]
	public int randomFillPercent;

	[Range(0,100)]
	public int waterLevel;

	int [,] map;

	public int step = 12;
	int prevStep;
	//bool initial = true;

	System.Random prng;

	void Start()
	{
		prevStep = step;
		if (useRandomSeed)
		{
			seed = Time.time.ToString ();
		}
		prng = new System.Random (seed.GetHashCode ());
		GenerateMap ();
	}

	void Update ()
	{
		if (Input.GetMouseButtonDown (0))
		{
			//GenerateMap ();
			//SmoothMap ();
			StepFillMap ();
		}
	}

	void GenerateMap()
	{
		map = new int[width, height];
		for (int x = 0; x < width; ++x)
		{
			for (int y = 0; y < height; ++y)
			{
				map [x, y] = -1;
			}
		}
		//RandomFillMap ();
		//InitialStepFillMap();
		StepFillMap ();
		/*for (int i = 0; i < 5; ++i)
		{
			SmoothMap ();
		}*/
	}

	void InitialStepFillMap()
	{
		// Random fill the initial square grid
		for (int x = 0; x < width; x += step)
		{
			for (int y = 0; y < height; y += step)
			{
				//if (x % step == 0 && y % step == 0)
					map [x, y] = prng.Next (0, 100);
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
					map [x, y] = prng.Next (0, 100);
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
				map [x, y] = prng.Next (0, 100);
			}
		}

		// Fill the square centres
		for (int x = step/2; x < width; x += step)
		{
			for (int y = step/2; y < height; y += step)
			{
				map [x, y] = prng.Next (0, 100);
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
					}
				}
			}
		}
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
				map [x, y] = prng.Next (0, 100);// < randomFillPercent ? 1 : 0;
			}
		}
	}


	void SmoothMap ()
	{
		for (int x = 0; x < width; ++x)
		{
			for (int y = 0; y < height; ++y)
			{
				map [x, y] = AverageOfSurrounding (x, y);
				/*int neighboursHigher = NumberSurroundingHigher (x, y);

				if (neighboursHigher > 4)
					map [x, y] = Mathf.Min(map[x,y] + 10, 100);
				else if (neighboursHigher < 4)
					map [x, y] = Mathf.Max(map[x,y] - 10, 0);
				*/
			}
		}
	}

	int AverageOfSurrounding (int gridX, int gridY)
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
	}


	int NumberSurroundingHigher(int gridX, int gridY)
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
	}

	int GetSurroundingWallCount (int gridX, int gridY)
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
	}

	void OnDrawGizmos ()
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
	}
}
