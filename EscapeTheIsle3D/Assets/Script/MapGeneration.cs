using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapGeneration : MonoBehaviour
{

    public static readonly Color COLOR_GRASS = new Color(0.470f, 0.666f, 0.074f);
    public static readonly Color COLOR_WATER = new Color(0.349f, 0.760f, 0.850f);
    public static readonly Color COLOR_DEEPWATER = new Color(0.047f, 0.223f, 0.392f);
    public static readonly Color COLOR_SAND = new Color(1, 0.862f, 0.078f);
    public static readonly Color COLOR_STONE = new Color(0.325f, 0.301f, 0.254f);
    private Renderer rend;
    private Texture2D noiseTex;
    private Texture2D terrainMap;


    FastNoiseLite noise;

    public int textureSize = 1000;
    public int borderThickness = 11;

    [SerializeField] private GameObject grassBlock;
    [SerializeField] private GameObject waterBlock;
    [SerializeField] private GameObject deepWaterBlock;
    [SerializeField] private GameObject sandBlock;
    [SerializeField] private GameObject stoneBlock;


    void Start()
    {
        noise = new FastNoiseLite(Random.Range(0, 8000));
        noise.SetNoiseType(FastNoiseLite.NoiseType.OpenSimplex2);

        noise.SetFractalOctaves(7);
        noise.SetFractalLacunarity(1.7f);
        noise.SetFractalGain(0.5f);
        noise.SetFrequency(0.007f);
        noise.SetFractalWeightedStrength(0.6f);

        noise.SetFractalType(FastNoiseLite.FractalType.FBm);
        rend = this.GetComponent<Renderer>();
        noiseTex = new Texture2D(textureSize, textureSize, TextureFormat.RGB24, false);
        noiseTex.filterMode = FilterMode.Point;
        GenerateMap();
      

        

    }

    // Update is called once per frame
    void Update()
    {
    }

    void GenerateMap()
    {
        CalcNoise();
        CalcBorder();
        byte[] bytes = noiseTex.EncodeToPNG();
        var dirPath = Application.dataPath + "/../SaveImages/";
        if (!System.IO.Directory.Exists(dirPath))
        {
            System.IO.Directory.CreateDirectory(dirPath);
        }
        System.IO.File.WriteAllBytes(dirPath + "Image" + ".png", bytes);
        AssignateTerrains();
    }

    void AssignateTerrains()
    {
        //CreateTerrainTextureFromNoise();
        rend.material.SetTexture("_MainTex", noiseTex);
        terrainMap.Apply();
    }

    void CreateTerrainTextureFromNoise()
    {
        terrainMap = new Texture2D(textureSize, textureSize);
        Color[] pixels = new Color[terrainMap.width * terrainMap.height];
        int index = 0;
        foreach(Color c in noiseTex.GetPixels())
        {
            if(c.r <= 0.0002f)
            {
                pixels[index] = COLOR_DEEPWATER;
            }
            else if(c.r <= 0.12f)
            {
                pixels[index] = COLOR_WATER;
            }
            else if(c.r <= 0.2f)
            {
                pixels[index] = COLOR_SAND;
            }
            else if (c.r <= 0.74f)
            {
                pixels[index] = COLOR_GRASS;
            }
            else
            {
                pixels[index] = COLOR_STONE; //error color
            }
            index++;
        }
        terrainMap.SetPixels(pixels);
        terrainMap.Apply();
        
    }
    void CalcNoise()
    {
        float[] pix;
        pix = new float[noiseTex.width * noiseTex.height];
        int index = 0;
        for (int y = 0; y < noiseTex.width; y++)
        {
            for (int x = 0; x < noiseTex.height; x++)
            {
                
                pix[index++] = noise.GetNoise(x, y);
            }
        }

        Color[] colorPix = new Color[noiseTex.width * noiseTex.height];
        for (int i = 0; i < noiseTex.width * noiseTex.height; i++)
        {
            colorPix[i] = new Color(pix[i], pix[i], pix[i]);
        }

        // Copy the pixel data to the texture and load it into the GPU.
        noiseTex.SetPixels(colorPix);
        noiseTex.Apply();
    }
    void CalcBorder() 
    {
        Debug.Log(textureSize);
        Debug.Log(borderThickness);
        for (int i = 0; i < textureSize; i++)
        {
            for (int j = 0; j < textureSize; j++)
            {
                if (
                    (i <= borderThickness) || 
                    (
                        (i > borderThickness) && (i <= textureSize - borderThickness) &&
                        (
                            (j < borderThickness) || (j > textureSize - borderThickness)
                        )
                      
                    ) || 
                    (i > textureSize - borderThickness)
                    )
                {
                    //Debug.Log(i +' '+ j);
                    noiseTex.SetPixel(j, i, new Color(0, 0, 0));
                }
            }
                
        }
        noiseTex.Apply();
    }

    float ApplyNormal(float x, float min, float max)
    {
        return ((x - min) / (max - min));
    }
}
