using UnityEngine;
using csDelaunay;
using System.Collections.Generic;
using DefineMapValue;

public class MapEditor : MonoBehaviour
{
    [Header("보르노이 맵 설정")]
    [SerializeField] Vector2Int _size;
    [SerializeField] int _nodeAmount = 0;
    [SerializeField] int _lloydIteratCount = 0;

    [Header("펄린노이즈맵 설정")]
    [SerializeField, Range(0f, 0.4f)] float _noiseFrequency = 0;
    [SerializeField] int _noise0ctave = 0;
    [SerializeField] int _seed = 0;
    [SerializeField, Range(0f, 0.5f)] float _landNoiseThreshold = 0;
    [SerializeField] int _noiseMaskRadius = 0;
    [SerializeField, Range(0f, 0.05f)] float _offsetLandHeight = 0;
    //==추가
    //==추가(end)

    [Header("뷰어 설정 설정")]
    [SerializeField] SpriteRenderer _voronoiMapRecoder;
    [SerializeField] SpriteRenderer _noiseMapRenderer;

    private void Awake()
    {
        //==수정
        Voronoi vo = GenerateVoronoi(_size, _nodeAmount, _lloydIteratCount);
        //==수정(end)
        _voronoiMapRecoder.sprite = MapDrawer.DrawVoronoiToSprite(vo);
    }
    //==추가
    private void Update()
    {
        GenerateNoiseMap();
    }
    //==추가(end)

    //==수정
    Voronoi GenerateVoronoi(Vector2Int size, int nodeAmount, int lloydCount)
    //==수정(end)
    {
        List<Vector2> centroids = new List<Vector2>();

        for (int i = 0; i < nodeAmount; i++)
        {
            int rx = Random.Range(0, size.x);
            int ry = Random.Range(0, size.y);

            centroids.Add(new Vector2(rx, ry));
        }

        Rect rt = new Rect(0, 0, size.x, size.y);
        //==수정
        return new Voronoi(centroids, rt, lloydCount);
        //==수정(end)
    }
    //==추가
    float[] CreateMapShape(Vector2Int size, float frequency, int octave)
    {
        //인스펙터 창에서 seed값이 0일 때 seed 값을 임의로 책정해야 한다.
        //같이 있다면 해당하는 값을 사용해야 한다.
        int seed = (_seed == 0) ? Random.Range(1, int.MaxValue) : _seed;

        FastNoiseLite noise = new FastNoiseLite();
        //펄린 노이즈를 생성하도록 설정.
        noise.SetNoiseType(FastNoiseLite.NoiseType.Perlin);
        //기본적인 프렉탈 노이즈 타입으로 생성(Fracional Brownian motion)
        noise.SetFractalType(FastNoiseLite.FractalType.FBm);
        noise.SetFrequency(frequency);
        noise.SetFractalOctaves(octave);
        noise.SetSeed(seed);

        //색은 0~1 범위이며 0이면 검은색, 1이면 흰색.
        float[] colorDatas = new float[size.x * size.y];
        //==추가
        float[] mask = MapDrawer.GetRadialGradientMask(size, _noiseMaskRadius);
        //==추가(end)

        int divisionCount = ResMapSetting._stageOfDividCount;
        int index = 0;
        for (int y = 0; y < size.y; ++y)
        {
            for (int x = 0; x < size.x; ++x)
            {
                float noiseColorFactor = Mathf.Round(noise.GetNoise(x, y) * divisionCount) / divisionCount;
                // 노이즈는 -1~1이기 때문에, 0~1 범위로 변환.
                noiseColorFactor = (noiseColorFactor + 1) * 0.5f;
                //==수정
                noiseColorFactor *= mask[index];
                float color = noiseColorFactor > _landNoiseThreshold ? noiseColorFactor : 0f;
                //==추가
                color += _offsetLandHeight;
                //==추가(end)
                colorDatas[index] = color;
                //colorDatas[index] = noiseColorFactor;
                //==수정(end)
                index++;
            }

        }
        return colorDatas;
    }

    public void GenerateNoiseMap()
    {
        float[] noiseColors = CreateMapShape(_size, _noiseFrequency, _noise0ctave);
        Color[] colors = new Color[noiseColors.Length];

        for (int i = 0; i < colors.Length; i++)
        {
            byte[] color = System.BitConverter.GetBytes(noiseColors[i]);
            float r = noiseColors[i];//color[0] / 255.0f;
            float g = noiseColors[i];//color[1] / 255.0f;
            float b = noiseColors[i];//color[2] / 255.0f;
            float a = 1;
            colors[i] = new Color(r, g, b, a);
        }
        _noiseMapRenderer.sprite = MapDrawer.DrawSprite(_size, colors);
    }
    private void OnGUI()
    {
        string fullPath = Application.dataPath;
        if (GUI.Button(new Rect(0, 0, 200, 30), "Create Vortonoi"))
        {
            fullPath += "/4.Images/VortonoiMap.png";
            MapDrawer.CreateImageToFile(fullPath, _voronoiMapRecoder.sprite);
        }

        if (GUI.Button(new Rect(0, 40, 200, 30), "Create Noise Map"))
        {
            fullPath += "/4.Images/NoiseMap.png";
            MapDrawer.CreateImageToFile(fullPath, _noiseMapRenderer.sprite);
        }

    }
    //==추가(end)
}

//프렉탈 노이즈 : 여러 노이즈가 합쳐져 있는 그래프
//헤이트 맵 : 소리를 높낮이에 따라 색깔로 나타냄. 1 : 흰색 0 : 검은색
//펄린 노이즈 : 