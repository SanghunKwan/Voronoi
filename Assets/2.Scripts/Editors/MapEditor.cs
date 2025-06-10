using UnityEngine;
using csDelaunay;
using System.Collections.Generic;
using DefineMapValue;

public class MapEditor : MonoBehaviour
{
    [Header("�������� �� ����")]
    [SerializeField] Vector2Int _size;
    [SerializeField] int _nodeAmount = 0;
    [SerializeField] int _lloydIteratCount = 0;

    [Header("�޸�������� ����")]
    [SerializeField, Range(0f, 0.4f)] float _noiseFrequency = 0;
    [SerializeField] int _noise0ctave = 0;
    [SerializeField] int _seed = 0;
    [SerializeField, Range(0f, 0.5f)] float _landNoiseThreshold = 0;
    [SerializeField] int _noiseMaskRadius = 0;
    [SerializeField, Range(0f, 0.05f)] float _offsetLandHeight = 0;
    //==�߰�
    //==�߰�(end)

    [Header("��� ���� ����")]
    [SerializeField] SpriteRenderer _voronoiMapRecoder;
    [SerializeField] SpriteRenderer _noiseMapRenderer;

    private void Awake()
    {
        //==����
        Voronoi vo = GenerateVoronoi(_size, _nodeAmount, _lloydIteratCount);
        //==����(end)
        _voronoiMapRecoder.sprite = MapDrawer.DrawVoronoiToSprite(vo);
    }
    //==�߰�
    private void Update()
    {
        GenerateNoiseMap();
    }
    //==�߰�(end)

    //==����
    Voronoi GenerateVoronoi(Vector2Int size, int nodeAmount, int lloydCount)
    //==����(end)
    {
        List<Vector2> centroids = new List<Vector2>();

        for (int i = 0; i < nodeAmount; i++)
        {
            int rx = Random.Range(0, size.x);
            int ry = Random.Range(0, size.y);

            centroids.Add(new Vector2(rx, ry));
        }

        Rect rt = new Rect(0, 0, size.x, size.y);
        //==����
        return new Voronoi(centroids, rt, lloydCount);
        //==����(end)
    }
    //==�߰�
    float[] CreateMapShape(Vector2Int size, float frequency, int octave)
    {
        //�ν����� â���� seed���� 0�� �� seed ���� ���Ƿ� å���ؾ� �Ѵ�.
        //���� �ִٸ� �ش��ϴ� ���� ����ؾ� �Ѵ�.
        int seed = (_seed == 0) ? Random.Range(1, int.MaxValue) : _seed;

        FastNoiseLite noise = new FastNoiseLite();
        //�޸� ����� �����ϵ��� ����.
        noise.SetNoiseType(FastNoiseLite.NoiseType.Perlin);
        //�⺻���� ����Ż ������ Ÿ������ ����(Fracional Brownian motion)
        noise.SetFractalType(FastNoiseLite.FractalType.FBm);
        noise.SetFrequency(frequency);
        noise.SetFractalOctaves(octave);
        noise.SetSeed(seed);

        //���� 0~1 �����̸� 0�̸� ������, 1�̸� ���.
        float[] colorDatas = new float[size.x * size.y];
        //==�߰�
        float[] mask = MapDrawer.GetRadialGradientMask(size, _noiseMaskRadius);
        //==�߰�(end)

        int divisionCount = ResMapSetting._stageOfDividCount;
        int index = 0;
        for (int y = 0; y < size.y; ++y)
        {
            for (int x = 0; x < size.x; ++x)
            {
                float noiseColorFactor = Mathf.Round(noise.GetNoise(x, y) * divisionCount) / divisionCount;
                // ������� -1~1�̱� ������, 0~1 ������ ��ȯ.
                noiseColorFactor = (noiseColorFactor + 1) * 0.5f;
                //==����
                noiseColorFactor *= mask[index];
                float color = noiseColorFactor > _landNoiseThreshold ? noiseColorFactor : 0f;
                //==�߰�
                color += _offsetLandHeight;
                //==�߰�(end)
                colorDatas[index] = color;
                //colorDatas[index] = noiseColorFactor;
                //==����(end)
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
    //==�߰�(end)
}

//����Ż ������ : ���� ����� ������ �ִ� �׷���
//����Ʈ �� : �Ҹ��� �����̿� ���� ����� ��Ÿ��. 1 : ��� 0 : ������
//�޸� ������ : 