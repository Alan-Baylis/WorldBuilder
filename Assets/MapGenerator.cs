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

	int [,] map;

	public int step = 12;
	int prevStep;
	//bool initial = true;

	void Start()
	{
		prevStep = step;
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
		//RandomFillMap ();
		StepFillMap ();
		/*for (int i = 0; i < 5; ++i)
		{
			SmoothMap ();
		}*/
	}

	void StepFillMap()
	{
		if (step <= 0)
			return;

		if (useRandomSeed)
		{
			seed = Time.time.ToString ();
		}

		System.Random prng = new System.Random (seed.GetHashCode ());

		for (int x = 0; x < width; x += step)
		{
			for (int y = 0; y < height; y += step)
			{
				if (step == prevStep) // We are on the initial run
				{
					if (x % step == 0 && y % step == 0)
						map [x, y] = prng.Next (0, 100);
					else
						map [x, y] = 0;
				} 
				else
				{
					if (x % prevStep == 0 && y % prevStep == 0) // Done on the previous iteration
						continue;
					else if (x % prevStep == 0) // On a major column
					{
						//int next = y + step >= height ? y + step - height : y + step;
						//int prev = y - step;
						/*int next = y + step;
						if (next >= height)
						{
							next -= height;
							Debug.Log ("Next in x " + next);
						}
						int prev = y - step;
						map [x, y] = (map [x, next] + map [x, prev]) / 2;*/
					} else if (y % prevStep == 0) // On a major row
					{
						//int next = x + step >= width ? x + step - width : x + step;
						//int prev = x - step;
						/*int next = x + step;
						if (next >= width)
						{
							next -= width;
							Debug.Log ("Next in y " + next);
						}
						int prev = x - step;
						map [x, y] = (map [next, y] + map [prev, y]) / 2;*/
					} 
					else // On a crossing diagonal
					{
						int nextY = y + step;
						if (nextY >= height)
						{
							nextY -= height;
						}
						int prevY = y - step;
						int nextX = x + step;
						if (nextX >= width)
						{
							nextX -= width;
						}
						int prevX = x - step;
						map [x, y] = (map [nextX, nextY] + map [nextX, prevY] + map [prevX, prevY] + map [prevX, nextY]) / 4;
					}
				}
			}
		}

		prevStep = step;
		step /= 2;
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
					float scale = 1f - (map [x, y] / 100f);
					Gizmos.color = new Color (scale, scale, scale); //map [x, y] == 1 ? Color.black : Color.white;
					Vector3 pos = new Vector3 (-width / 2 + x + .5f, 0, -height / 2 + y + .5f);
					Gizmos.DrawCube (pos, Vector3.one);
				}
			}
		}
	}
}
