/**
 * Functions for generating Perlin noise. To run the demos, put "grass.png" 
 * and "sand.png" in the executable folder.
 **/

/**
 * Code from devmag.org.za/2009/04/25/perlin-noise by Hermann Tulleken.
 * Modified slightly:
 * - to account for API changes (e.g. no Color.FromArgb anymore)
 * - to wrap around terrain at the poles correctly (not top-to-bottom but "circular"
 *   at the top)
 */

using System;
using System.Collections.Generic;
using UnityEngine;
//using System.Drawing;
//using System.Drawing.Imaging;
using System.Text;

namespace PerlinNoise
{
    class PerlinNoise
    {
        /*#region Feilds
		static System.Random random = new System.Random();
        #endregion

        #region Demo
        private static void DemoImageBlend()
        {
            int octaveCount = 8;

            Color gradientStart1 = Color(0, 255, 255);
            Color gradientEnd1 = Color(0, 255, 0);

            Color[][] image1 = LoadImage("grass.png");
            Color[][] image2 = LoadImage("sand.png");

            int width = image1.Length;
            int height = image1[0].Length;

            float[][] perlinNoise = GeneratePerlinNoise(width, height, octaveCount);
            perlinNoise = AdjustLevels(perlinNoise, 0.2f, 0.8f);

            Color[][] perlinImage = BlendImages(image1, image2, perlinNoise);

            SaveImage(perlinImage, "perlin_noise_blended.png");
        }

        public static void DemoPlantGrowth()
        {
            int frameCount = 10;


            Color[][] image1 = LoadImage("sand.png");
            Color[][] image2 = LoadImage("grass.png");

            Color[][][] animation = AnimateTransition(image1, image2, frameCount);

            for (int i = 0; i < frameCount; i++)
            {
                SaveImage(animation[i], "blend_animation" + i + ".png");
            }
        }

        public static void Main()
        {
            DemoGradientMap();
            DemoImageBlend();
            DemoPlantGrowth();
        }
        #endregion
*/
        #region Reusable Functions

		public static float[][] GenerateWhiteNoise(int width, int height, System.Random random)
        {            
            float[][] noise = GetEmptyArray<float>(width, height);

            for (int i = 0; i < width; i++)
            {
                for (int j = 0; j < height; j++)
                {
                    noise[i][j] = (float)random.NextDouble() % 1;
                }
            }

            return noise;
        }

        public static float Interpolate(float x0, float x1, float alpha)
        {
            return x0 * (1 - alpha) + alpha * x1;
        }

        public static Color Interpolate(Color col0, Color col1, float alpha)
        {
            float beta = 1 - alpha;
            return new Color(
                (int)(col0.r * alpha + col1.r * beta),
                (int)(col0.g * alpha + col1.g * beta),
				(int)(col0.b * alpha + col1.b * beta),
				255);
        }

        public static Color GetColor(Color gradientStart, Color gradientEnd, float t)
        {        
            float u = 1 - t;

            Color color = new Color(
                (int)(gradientStart.r * u + gradientEnd.r * t),
                (int)(gradientStart.g * u + gradientEnd.g * t),
                (int)(gradientStart.b * u + gradientEnd.b * t),
				255);

            return color;
        }

        public static Color[][] MapGradient(Color gradientStart, Color gradientEnd, float[][] perlinNoise)
        {
            int width = perlinNoise.Length;
            int height = perlinNoise[0].Length;

            Color[][] image = GetEmptyArray<Color>(width, height); //an array of colours

            for (int i = 0; i < width; i++)
            {
                for (int j = 0; j < height; j++)
                {
                    image[i][j] = GetColor(gradientStart, gradientEnd, perlinNoise[i][j]);
                }
            }

            return image;
        }

        public static T[][] GetEmptyArray<T>(int width, int height)
        {
            T[][] image = new T[width][];

            for (int i = 0; i < width; i++)
            {
                image[i] = new T[height];
            }

            return image;
        }

        public static float[][] GenerateSmoothNoise(float[][] baseNoise, int octave)
        {
            int width = baseNoise.Length;
            int height = baseNoise[0].Length;

            float[][] smoothNoise = GetEmptyArray<float>(width, height);

            int samplePeriod = 1 << octave; // calculates 2 ^ k
            float sampleFrequency = 1.0f / samplePeriod;

            for (int i = 0; i < width; i++)
            {
				int i0_j0, i0_j1, i1_j0, i1_j1;

                //calculate the horizontal sampling indices
                int sample_i0 = (i / samplePeriod) * samplePeriod;
                int sample_i1 = (sample_i0 + samplePeriod) % width; //wrap around

				i0_j0 = i0_j1 = sample_i0;
				i1_j0 = i1_j1 = sample_i1;
                float horizontal_blend = (i - sample_i0) * sampleFrequency;

                for (int j = 0; j < height; j++)
                {
                    //calculate the vertical sampling indices
                    int sample_j0 = (j / samplePeriod) * samplePeriod;
					int sample_j1 = (sample_j0 + samplePeriod);

                    //int sample_j1 = (sample_j0 + samplePeriod) % height; //wrap around
					// The height wrapping is now polar. E.g. on a 256 wide by 128 high map, the 
					// top left 0,0 would wrap vertically to 256,0 instead of 0,128

					// Correctly wrap vertically
					if (sample_j1 >= height)
					{
						// How much the vertical wraps around
						int diff = sample_j1 % height;
						sample_j1 = height - (diff + 1);

						// Wrap the horizontal points around the centre
						diff = sample_i0 - width / 2;
						diff = diff < 0 ? diff + 1 : diff - 1; // Reduce difference by 1
						i0_j1 = width / 2 + (-diff);

						diff = sample_i1 - width / 2;
						diff = diff < 0 ? diff + 1 : diff - 1; // Reduce difference by 1
						i1_j1 = width / 2 + (-diff);
					}
                    float vertical_blend = (j - sample_j0) * sampleFrequency;

                    //blend the top two corners
                    float top = Interpolate(baseNoise[sample_i0][sample_j0],
                        baseNoise[sample_i1][sample_j0], horizontal_blend);

                    //blend the bottom two corners
                    float bottom = Interpolate(baseNoise[i0_j1][sample_j1],
                        baseNoise[i1_j1][sample_j1], horizontal_blend);

                    //final blend
                    smoothNoise[i][j] = Interpolate(top, bottom, vertical_blend);                    
                }
            }
            
            return smoothNoise;
        }

        public static float[][] GeneratePerlinNoise(float[][] baseNoise, int octaveCount)
        {
            int width = baseNoise.Length;
            int height = baseNoise[0].Length;

            float[][][] smoothNoise = new float[octaveCount][][]; //an array of 2D arrays containing

            float persistance = 0.7f;

            //generate smooth noise
            for (int i = 0; i < octaveCount; i++)
            {
                smoothNoise[i] = GenerateSmoothNoise(baseNoise, i);
            }

            float[][] perlinNoise = GetEmptyArray<float>(width, height); //an array of floats initialised to 0

            float amplitude = 1.0f;
            float totalAmplitude = 0.0f;
            
            //blend noise together
            for (int octave = octaveCount - 1; octave >= 0; octave--)
            {
                amplitude *= persistance;
                totalAmplitude += amplitude;

                for (int i = 0; i < width; i++)
                {
                    for (int j = 0; j < height; j++)
                    {
                        perlinNoise[i][j] += smoothNoise[octave][i][j] * amplitude;
                    }
                }
            }

            //normalisation
            for (int i = 0; i < width; i++)
            {
                for (int j = 0; j < height; j++)
                {
                    perlinNoise[i][j] /= totalAmplitude;
                }
            }        

            return perlinNoise;
        }

		public static float[][] GeneratePerlinNoise(int width, int height, int octaveCount, System.Random random)
        {
            float[][] baseNoise = GenerateWhiteNoise(width, height, random);

            return GeneratePerlinNoise(baseNoise, octaveCount);
        }

        public static Color[][] MapToGrey(float[][] greyValues)
        {
            int width = greyValues.Length;
            int height = greyValues[0].Length;

            Color[][] image = GetEmptyArray<Color>(width, height);

            for (int i = 0; i < width; i++)
            {
                for (int j = 0; j < height; j++)
                {
                    int grey = (int)(255 * greyValues[i][j]);
					Color color = new Color(grey, grey, grey, 255);

                    image[i][j] = color;
                }
            }

            return image;
        }

        /*public static void SaveImage(Color[][] image, string fileName)
        {
            int width = image.Length;
            int height = image[0].Length;

            Bitmap bitmap = new Bitmap(width, height, PixelFormat.Format32bppArgb);

            for (int i = 0; i < width; i++)
            {
                for (int j = 0; j < height; j++)
                {
                    bitmap.SetPixel(i, j, image[i][j]);
                }
            }

            bitmap.Save(fileName);
        }

        public static Color[][] LoadImage(string fileName)
        {
            Bitmap bitmap = new Bitmap(fileName);
            
            int width = bitmap.Width;
            int height = bitmap.Height;

            Color[][] image = GetEmptyArray<Color>(width, height);

            for (int i = 0; i < width; i++)
            {
                for (int j = 0; j < height; j++)
                {
                    image[i][j] = bitmap.GetPixel(i, j);
                }
            }

            return image;
        }*/

        public static Color[][] BlendImages(Color[][] image1, Color[][] image2, float[][] perlinNoise)
        {
            int width = image1.Length;
            int height = image1[0].Length;

	        Color[][] image = GetEmptyArray<Color>(width, height); //an array of colours for the new image
            
            for (int i = 0; i < width; i++)
            {
                for (int j = 0; j < height; j++)
                {
                    image[i][j] = Interpolate(image1[i][j], image2[i][j], perlinNoise[i][j]);
                }
            }

	        return image;
        }
        
        /*public static void DemoGradientMap()
        {
            int width = 256;
            int height = 256;
            int octaveCount = 8;

            Color gradientStart = Color.FromArgb(255, 0, 0);
            Color gradientEnd = Color.FromArgb(255, 0, 255);

            float[][] perlinNoise = GeneratePerlinNoise(width, height, octaveCount);
            Color[][] perlinImage = MapGradient(gradientStart, gradientEnd, perlinNoise);
            SaveImage(perlinImage, "perlin_noise.png");
        }*/

        public static float[][] AdjustLevels(float[][] image, float low, float high)
        {
            int width = image.Length;
            int height = image[0].Length;

            float[][] newImage = GetEmptyArray<float>(width, height);

            for (int i = 0; i < width; i++)
            {
                for(int j = 0; j < height; j++)
                {
                    float col = image[i][j];

                    if (col <= low)
                    {
                        newImage[i][j] = 0;
                    }
                    else if (col >= high)
                    {
                        newImage[i][j] = 1;
                    }
                    else
                    {
                        newImage[i][j] = (col - low) / (high - low);
                    }
                }
            }

            return newImage;
        }

        /*private static Color[][][] AnimateTransition(Color[][] image1, Color[][] image2, int frameCount)
        {  
            Color[][][] animation = new Color[frameCount][][];

            float low = 0;
            float increment = 1.0f / frameCount; 
            float high = increment;
            
            float[][] perlinNoise = AdjustLevels(
                GeneratePerlinNoise(image1.Length, image1[0].Length, 9),
                0.2f, 0.8f);

            for (int i = 0; i < frameCount; i++)
            {
                AdjustLevels(perlinNoise, low, high);
                float[][] blendMask = AdjustLevels(perlinNoise, low, high);
                animation[i] = BlendImages(image1, image2, blendMask);
                //SaveImage(animation[i], "blend_animation" + i + ".png");
                SaveImage(MapToGrey(blendMask), "blend_mask" + i + ".png");
                low = high;
                high += increment;
            }

            return animation;
        }*/

        #endregion
    }
}