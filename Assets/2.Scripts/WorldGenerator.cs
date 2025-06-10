using UnityEngine;
using DefineMapValue;
using System.Collections;
using System.Collections.Generic;

public class WorldGenerator : MonoBehaviour
{
    //== ����
    [SerializeField] GameObject[] _resourcePrefabs;
    //== ����(end)
    //== �߰�
    [Header("�� ����")]
    [SerializeField] Texture2D _noiseMapInfo;
    [SerializeField] Texture2D _biomMapInfo;
    //���� ���� ���� �ڿ� ������ ����.
    //== �߰�(end)

    public Vector3Int _mapSize { get; private set; }
    Transform _rootMap;

    //== �߰�
    BlockInfo[,,] _worldBlocks;

    public BlockInfo this[int x, int y, int z]
    {
        get => _worldBlocks[x, y, z];
        set => _worldBlocks[x, y, z] = value;
    }
    public BlockInfo this[Vector3Int vector]
    {
        get => _worldBlocks[vector.x, vector.y, vector.z];
        set => _worldBlocks[vector.x, vector.y, vector.z] = value;
    }

    //== �߰�(end)
    public bool _isEndLoad { get; private set; }

    Dictionary<SpecialOreTypes, int> specialType2ChangeRate;

    readonly Vector3Int[] neighborOffset = new Vector3Int[]
    {
        new Vector3Int(-1, -1, -1), new Vector3Int(0, -1, -1), new Vector3Int(1, -1, -1),
        new Vector3Int(-1,  0, -1), new Vector3Int(0,  0, -1), new Vector3Int(1,  0, -1),
        new Vector3Int(-1,  1, -1), new Vector3Int(0,  1, -1), new Vector3Int(1,  1, -1),

        new Vector3Int(-1, -1, 0), new Vector3Int(0, -1, 0), new Vector3Int(1, -1, 0),
        new Vector3Int(-1,  0, 0),                        /* skip center */     new Vector3Int(1,  0, 0),
        new Vector3Int(-1,  1, 0), new Vector3Int(0,  1, 0), new Vector3Int(1,  1, 0),

        new Vector3Int(-1, -1, 1), new Vector3Int(0, -1, 1), new Vector3Int(1, -1, 1),
        new Vector3Int(-1,  0, 1), new Vector3Int(0,  0, 1), new Vector3Int(1,  0, 1),
        new Vector3Int(-1,  1, 1), new Vector3Int(0,  1, 1), new Vector3Int(1,  1, 1)
    };


    private void Start()
    {
        _mapSize = new Vector3Int(128, 128, 128);
        _rootMap = GameObject.FindGameObjectWithTag("MapRoot").transform;
        _worldBlocks = new BlockInfo[_mapSize.x, _mapSize.y, _mapSize.z];
        specialType2ChangeRate = new Dictionary<SpecialOreTypes, int>();
        int length = (int)SpecialOreTypes.Max;
        SpecialOreTypes tempType;
        for (int i = 0; i < length; i++)
        {
            tempType = (SpecialOreTypes)i;
            specialType2ChangeRate.Add(tempType, (int)System.Enum.Parse<GetResourceRate>(tempType.ToString()));
        }


        //== ����
        StartCoroutine(GenerateTerrainAll(_mapSize));

        //== ����(end)
    }

    IEnumerator GenerateTerrain(Vector3Int size)
    {
        //���� �¿�, ���� ũ����� �ݺ��ϸ� ����� ��ġ.
        for (int x = 0; x < size.x; x++)
        {
            for (int z = 0; z < size.z; z++)
            {
                //== �߰�
                Color noisePixel = _noiseMapInfo.GetPixel(x, z);
                Color biomPixel = _biomMapInfo.GetPixel(x, z);
                int height = (int)(noisePixel.r * ResMapSetting._stageOfDividCount) + (int)ClassificationHeight.MapStandardHeight;

                _worldBlocks[x, height, z] = CreateBlock(x, z, height, true, biomPixel);

                while (height-- > 0)
                    _worldBlocks[x, height, z] = CreateBlock(x, z, height, false, biomPixel);
                //== �߰�(end)
                //== ����
                //Instantiate(_prefabGrass, new Vector3(x, height, z), Quaternion.identity, _rootMap);
                //== ����(end)
            }
            yield return null;
        }
        yield return null;
    }
    //== �߰�
    BlockInfo CreateBlock(int x, int z, int height, bool isVisible, Color pixelColor)
    {
        //block ���� �� 
        BlockInfo info = null;

        ResourceType blockyType = ResourceType.Soil;
        if (height > (int)ClassificationHeight.Snow)
            blockyType = ResourceType.Snow;
        else if (height > (int)ClassificationHeight.Grass)
            blockyType = ResourceType.Grass;
        else if (height > (int)ClassificationHeight.Soil)
            blockyType = ResourceType.Soil;
        else
            blockyType = ResourceType.Unbreak;

        blockyType = ChangeBlockType(blockyType, pixelColor);

        GameObject obj = null;

        if (isVisible || blockyType == ResourceType.Unbreak)
        {
            obj = Instantiate(_resourcePrefabs[(int)blockyType], new Vector3(x, height, z), Quaternion.identity, _rootMap);
            info = new BlockInfo(blockyType, true, obj);
        }
        else
            info = new BlockInfo(blockyType, false, obj);

        return info;
    }
    ResourceType ChangeBlockType(ResourceType nowType, in Color color)
    {
        if (nowType == ResourceType.Unbreak)
            return nowType;

        int changeRate = Random.Range(0, 100);
        //color�� �ٻ簪 ����� ���� specialType�� ����.

        ResMapSetting tempSetting = ResMapSetting._instance;
        if (!ApproximatelyEqualColor(color, Color.black))
        {
            SpecialOreTypes i = SpecialOreTypes.Copper;
            while (!ApproximatelyEqualColor(color, tempSetting.GetColorPerSpecialOreType(i)))
            {
                i++;
            }
            //specialType�� ���� �� specialType2ChangeRate�� �־ Ȯ���� ����.
            //���� Ȯ������ �������� ������ �����ؼ� ����
            if (changeRate < specialType2ChangeRate[i])
                return (ResourceType)i;
        }

        //�ƴϸ� stone ���
        changeRate = Random.Range(0, 100);
        if (changeRate < (int)GetResourceRate.Stone)
            return ResourceType.Stone;



        //int length = (int)SpecialOreTypes.Max;
        //for (int i = 0; i < length; i++)
        //{
        //    changeRate = Random.Range(0, 100);
        //    if (changeRate > specialType2ChangeRate[(SpecialOreTypes)i])
        //        return (ResourceType)i;
        //}
        //�ƴϸ� return nowType;
        return nowType;
    }
    bool ApproximatelyEqualColor(Color a, Color b, float epsilon = 0.01f)
    {
        //vector4�� ��ȯ�ؼ� �Ÿ� ���� ���� ����.
        return
            Mathf.Abs(a.r - b.r) < epsilon &&
            Mathf.Abs(a.g - b.g) < epsilon &&
            Mathf.Abs(a.b - b.b) < epsilon &&
            Mathf.Abs(a.a - b.a) < epsilon;
    }
    IEnumerator GenerateTerrainAll(Vector3Int size)
    {
        yield return GenerateTerrain(size);
        yield return CheckCrust(size);
        _isEndLoad = true;
    }
    IEnumerator CheckCrust(Vector3Int size)
    {
        //��ü�� ��ȸ���� ���� �����⿡�� �� ���� �������θ� ��ȸ.
        //�ش� ��ǥ(x,z)�� �����⸸ �������� �Լ�.

        for (int i = 0; i < size.x; i++)
        {
            for (int y = 0; y < size.y; y++)
            {
                for (int j = 0; j < size.z; j++)
                {
                    if (_worldBlocks[i, y, j] == null || _worldBlocks[i, y, j]._obj != null) continue;

                    if (IsCrust(new Vector3Int(i, y, j)))
                    {
                        ToggleBlock(_worldBlocks[i, y, j], new Vector3Int(i, y, j));
                    }
                }
            }
            yield return null;
        }
    }
    public void ToggleBlock(BlockInfo info, Vector3Int vec)
    {
        if (info._isVisible)
        {
            Destroy(info._obj);
            this[vec] = null;


            BlockInfo tempInfo;
            Vector3Int tempVec;
            foreach (var item in neighborOffset)
            {
                tempVec = vec + item;
                if (IsOutOfArray(tempVec) || !IsCrust(tempVec)) continue;
                tempInfo = this[tempVec];
                if (tempInfo == null || tempInfo._isVisible) continue;

                ToggleBlock(tempInfo, tempVec);
            }
        }
        else
        {
            if (info._obj == null)
            {
                info._obj = Instantiate(_resourcePrefabs[(int)info._resType], vec, Quaternion.identity, _rootMap);
                info._isVisible = true;
            }
        }
    }
    //== �߰�(end)
    bool IsCrust(Vector3Int blockCoordinate)
    {
        return
            (blockCoordinate.x == 0 ? false : _worldBlocks[blockCoordinate.x - 1, blockCoordinate.y, blockCoordinate.z] == null) ||
            (blockCoordinate.y == 0 ? false : _worldBlocks[blockCoordinate.x, blockCoordinate.y - 1, blockCoordinate.z] == null) ||
            (blockCoordinate.z == 0 ? false : _worldBlocks[blockCoordinate.x, blockCoordinate.y, blockCoordinate.z - 1] == null) ||

            (blockCoordinate.x == _mapSize.x - 1 ? false : _worldBlocks[blockCoordinate.x + 1, blockCoordinate.y, blockCoordinate.z] == null) ||
            (blockCoordinate.y == _mapSize.y - 1 ? false : _worldBlocks[blockCoordinate.x, blockCoordinate.y + 1, blockCoordinate.z] == null) ||
            (blockCoordinate.z == _mapSize.z - 1 ? false : _worldBlocks[blockCoordinate.x, blockCoordinate.y, blockCoordinate.z + 1] == null);
    }
    bool IsOutOfArray(Vector3Int blockCoordinate)
    {
        return
            blockCoordinate.x < 0 || blockCoordinate.y < 0 || blockCoordinate.z < 0 ||
            blockCoordinate.x >= _mapSize.x || blockCoordinate.y >= _mapSize.y || blockCoordinate.z >= _mapSize.z;
    }
    //int GetCrustHeight(Vector2Int xzCoordinate)
    //{
    //    //X,Z ��ǥ�� �̿��� ���� ū y ��ǥ ���.
    //    int tempHeight = 1;

    //    while (_worldBlocks[xzCoordinate.x, tempHeight, xzCoordinate.y]._obj == null)
    //    {
    //        tempHeight++;
    //    }

    //    return tempHeight;
    //}
}
