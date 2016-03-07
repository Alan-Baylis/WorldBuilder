using UnityEngine;
using System.Collections;
using pn = PerlinNoise.PerlinNoise;

public class World : MonoBehaviour 
{

	int width;
	int height;

	public string seed;
	public bool useRandomSeed;
	public bool usePerlinNoise;

	public int perlinOctaves = 6;

	public int step = 16;

	public int [,] heightMap { get; protected set;}

	int heightRange = 256;
	int prevStep;

	/** Index of the first cell SOUTH of the equator. Equator lies between this and equator - 1. */
	int equator_i; 
	int north_15_i, north_30_i, north_45_i, north_60_i, north_75_i;
	int south_15_i, south_30_i, south_45_i, south_60_i, south_75_i;

	System.Random prng;


	static Vector2 NW = new Vector2 ( -2, -2 );
	static Vector2 NNW = new Vector2 ( -1, -2 );
	static Vector2 NORTH = new Vector2 ( 0, -2 );
	static Vector2 NNE = new Vector2 ( 1, -2 );
	static Vector2 NE = new Vector2 ( 2, -2 );
	static Vector2 ENE = new Vector2 ( 2, -1 );
	static Vector2 EAST = new Vector2 ( 2, 0 );
	static Vector2 ESE = new Vector2 ( 2, 1 );
	static Vector2 SE = new Vector2 ( 2, 2 );
	static Vector2 SSE = new Vector2 ( 1, 2 );
	static Vector2 SOUTH = new Vector2 ( 0, 2 );
	static Vector2 SSW = new Vector2 ( -1, 2 );
	static Vector2 SW = new Vector2 ( -2, 2 );
	static Vector2 WSW = new Vector2 ( -2, 1 );
	static Vector2 WEST = new Vector2 ( -2, 0 );
	static Vector2 WNW = new Vector2 ( -2, -1 );
	static Vector2[] compassDirections = {NORTH, NNE, NE, ENE, EAST, ESE, SE, SSE, SOUTH, SSW, SW, WSW, WEST, WNW, NW, NNW };

	public int [,] GenerateHeightMap(int width, int height, int numLevels)
	{
		//Debug.Log ("urs " + useRandomSeed);
		if (useRandomSeed)
		{
			//Debug.Log ("Time " + System.DateTime.Now.ToString());
			seed = System.DateTime.Now.ToString(); //Time.time.ToString ();
			//Debug.Log ("Seed " + seed);
		}
		prng = new System.Random (seed.GetHashCode ());

		this.width = width;
		this.height = height;
		this.heightRange = numLevels;

		equator_i = height / 2;
		north_15_i = equator_i - height / 12;
		north_30_i = equator_i - height / 6;
		north_45_i = equator_i - height / 4;
		north_60_i = equator_i - height / 3;
		north_75_i = equator_i - height / 3 - height / 12;

		//Debug.Log (equator_i + " " + north_15_i + " " + north_30_i + " " + north_45_i + " " + north_60_i + " " + north_75_i);

		south_15_i = equator_i + height / 12;
		south_30_i = equator_i + height / 6;
		south_45_i = equator_i + height / 4;
		south_60_i = equator_i + height / 3;
		south_75_i = equator_i + height / 3 + height / 12;

		heightMap = new int[width, height];

		for (int x = 0; x < width; ++x)
		{
			for (int y = 0; y < height; ++y)
			{
				heightMap [x, y] = -1;
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
					heightMap [x, y] = (int)(perlinMap[x][y] * heightRange);
				}
			}
		} 
		else
		{
			prevStep = step;
			StepFillMap ();
		}

		return heightMap;
	}

	void InitialStepFillMap()
	{
		// Random fill the initial square grid
		for (int x = 0; x < width; x += step)
		{
			for (int y = 0; y < height; y += step)
			{
				//if (x % step == 0 && y % step == 0)
				heightMap [x, y] = prng.Next (0, heightRange);
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
				heightMap [x, y] = prng.Next (0, heightRange);
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
				heightMap [x, y] = prng.Next (0, heightRange);
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
				heightMap [x, y] = prng.Next (0, heightRange);
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
						if (heightMap [x, y] == -1)
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
					
							heightMap [x, y] = (heightMap [nextX, y] + heightMap [x, nextY] 
								+ heightMap [prevX, y] + heightMap [x, prevY]) / 4;
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
					if (heightMap [x, y] == -1)
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
					
						heightMap [x, y] = (heightMap [nextX, nextY] + heightMap [nextX, prevY] 
							+ heightMap [prevX, prevY] + heightMap [prevX, nextY]) / 4;
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
		float greyscale = 1f - (height / (float)heightRange);
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
				heightMap [x, y] = prng.Next (0, heightRange);// < randomFillPercent ? 1 : 0;
			}
		}
	}


	public void SmoothMap (int waterLevel)
	{
		int[,] smoothedMap = new int[width, height];
		for (int x = 0; x < width; ++x)
		{
			for (int y = 0; y < height; ++y)
			{
				smoothedMap [x, y] = heightMap [x, y];
			}
		}

		for (int x = 0; x < width; ++x)
		{
			for (int y = 0; y < height; ++y)
			{
				int surroundingAboveWater = NumberSurroundingAboveWater (x, y, this.heightMap, waterLevel);

				if (surroundingAboveWater < 4 && heightMap [x, y] > waterLevel)
				{
					smoothedMap [x, y] = waterLevel - prng.Next (0, 10);
					//Debug.Log ("Lowered " + x + ", " + y);
				} 
				else if (surroundingAboveWater > 4 && heightMap [x, y] <= waterLevel)
				{
					smoothedMap [x, y] = waterLevel + prng.Next (0, 10);
					//Debug.Log ("Raised " + x + ", " + y);
				}
			}
		}

		this.heightMap = smoothedMap;
	}

	int NumberSurroundingAboveWater(int gridX, int gridY, int [,] theMap, int waterLevel)
	{
		int numAboveWater = 0;

		for (int offsetX = - 1; offsetX <= + 1; ++offsetX)
		{
			for (int offsetY = - 1; offsetY <= + 1; ++offsetY)
			{
				if (offsetX == 0 && offsetY == 0)
					continue;
				
				int indX = gridX + offsetX;
				if (indX < 0)
					indX += width;
				if (indX >= width)
					indX -= width;

				int indY = gridY + offsetY;
				if (indY < 0)
					indY += height;
				if (indY >= height)
					indY -= height;
				
				if (theMap [indX, indY] > waterLevel)
				{
					//Debug.Log (indX + " " + indY);
					++numAboveWater;
				}
			}
		}

		return numAboveWater;
	}


	public Color biomeFromLatitude(int x, int y, int waterLevel)
	{
		if (heightMap [x, y] <= waterLevel)
			return Color.blue;
		else if (y >= north_15_i && y < south_15_i)
			return Color.green; // tropics
		else if (y >= north_30_i && y < south_30_i)
			return Color.yellow; // subtropics
		else if (y >= north_60_i && y < south_60_i)
			return Color.cyan; // temperate
		else if (y >= north_75_i && y < south_75_i)
			return Color.magenta; // tundra
		else
			return Color.white; // polar
	}

	/**
	 * Turn indivdual mountain peaks into ranges by redistributing their height
	 * to "stretch" them out.
	 * 
	 */
	public void CreateMountainRanges(int waterLevel)
	{
		//Debug.Log ("Mountains");
		// Find mountain peaks
		// TODO: Use the full range, just doing this so that I don't have to deal with
		// wrapping around yet.
		int avgAbove = averageHeightAboveWater(waterLevel);
		int threshold = Mathf.RoundToInt ((float)(heightRange + waterLevel) / 2);

		for (int x = 10; x < width - 10; ++x)
		{
			for (int y = 10; y < height - 10; ++y)
			{
				if (heightMap[x, y] > threshold && isPeakAt (x, y, 2))
				{
					Debug.Log ("Peak at " + x + ", " + y + " of " + heightMap [x, y]);
				}
			}
		}





		//// Find points with height above some value
		/*int threshold = 240;
		// TODO: Use the full range, just doing this so that I don't have to deal with
		// wrapping around yet.
		for (int x = 10; x < width - 10; ++x)
		{
			for (int y = 10; y < height - 10; ++y)
			{
				if (heightMap [x, y] > threshold)
				{
					// Find the direction that the mountain "falls" the least
					/// Starting at 2 squares in all directions find the highest point
					/// If it isn't at least 20% different from the others, expand the search square
					int[] heightInDirection = new int[compassDirections.Length];
					int maxHeight = -1;
					int maxIndex = 0;
					for (int d = 0; d < compassDirections.Length; ++d)
					{
						Vector2 dir = compassDirections [d];
						heightInDirection [d] = heightMap [x + (int)dir.x, y + (int)dir.y];
						if (heightInDirection [d] > maxHeight)
						{
							maxHeight = heightInDirection [d];
							maxIndex = d;
						}
					}
					Vector2 direction = compassDirections [maxIndex];
					Debug.Log ("Mountain at " + x + ", " + y + " max dir " + maxIndex);
				}
			}
		}*/
	}

	bool isPeakAt(int x, int y, int range)
	{
		for (int offsetX = -range; offsetX <= +range; ++offsetX)
		{
			for (int offsetY = -range; offsetY <= +range; ++offsetY)
			{
				if (offsetX == 0 && offsetY == 0)
					continue;
				
				int indX = x + offsetX;
				int indY = y + offsetY;

				if (heightMap [indX, indY] > heightMap [x, y])
					return false;
			}
		}
		return true;
	}


	public ArrayList getMountain(int x, int y, int waterLevel)
	{
        ArrayList extent = new ArrayList();
        int range = 1;
        int avgAboveWater = averageHeightAboveWater(waterLevel);

        while(true)
        {
            bool atLeastOne = false;
            for (int offsetX = -range; offsetX <= +range; ++offsetX)
            {
                for (int offsetY = -range; offsetY <= +range; ++offsetY)
                {
                    if (offsetX != -range || offsetY != +range)
                        continue;

                    int indX = x + offsetX;
                    int indY = y + offsetY;

                    if (heightMap[indX, indY] > avgAboveWater)
                    {
                        extent.Add(new Vector2(x, y));
                        atLeastOne = true;
                    }
                }
            }

            if (atLeastOne == false)
                break;
        }

        return extent;
	}


	public int averageHeightAboveWater(int waterLevel)
	{
		int total = 0;
		int aboveWater = 0;
		int avg;
		for (int x = 0; x < width; ++x)
		{
			for (int y = 0; y < height; ++y)
			{
				if (heightMap [x, y] > waterLevel)
				{
					total += heightMap [x, y];
					++aboveWater;
				}
			}
		}

		avg = Mathf.RoundToInt ((float)total / aboveWater);
		//Debug.Log ("Water " + waterLevel + ", avg " + avg);
		return avg;
	}
}
