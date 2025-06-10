using UnityEngine;
using DefineMapValue;
using System.Collections;
using System.Collections.Generic;

public class WorldGenerator : MonoBehaviour
{
    //== 수정
    [SerializeField] GameObject[] _resourcePrefabs;
    //== 수정(end)
    //== 추가
    [Header("맵 정보")]
    [SerializeField] Texture2D _noiseMapInfo;
    [SerializeField] Texture2D _biomMapInfo;
    //각각 색깔에 따른 자원 함유량 계산용.
    //== 추가(end)

    public Vector3Int _mapSize { get; private set; }
    Transform _rootMap;

    //== 추가
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

    //== 추가(end)
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


        //== 수정
        StartCoroutine(GenerateTerrainAll(_mapSize));

        //== 수정(end)
    }

    IEnumerator GenerateTerrain(Vector3Int size)
    {
        //땅의 좌우, 상하 크기까지 반복하며 블록을 배치.
        for (int x = 0; x < size.x; x++)
        {
            for (int z = 0; z < size.z; z++)
            {
                //== 추가
                Color noisePixel = _noiseMapInfo.GetPixel(x, z);
                Color biomPixel = _biomMapInfo.GetPixel(x, z);
                int height = (int)(noisePixel.r * ResMapSetting._stageOfDividCount) + (int)ClassificationHeight.MapStandardHeight;

                _worldBlocks[x, height, z] = CreateBlock(x, z, height, true, biomPixel);

                while (height-- > 0)
                    _worldBlocks[x, height, z] = CreateBlock(x, z, height, false, biomPixel);
                //== 추가(end)
                //== 수정
                //Instantiate(_prefabGrass, new Vector3(x, height, z), Quaternion.identity, _rootMap);
                //== 수정(end)
            }
            yield return null;
        }
        yield return null;
    }
    //== 추가
    BlockInfo CreateBlock(int x, int z, int height, bool isVisible, Color pixelColor)
    {
        //block 생성 시 
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
        //color로 근사값 계산을 통해 specialType을 구함.

        ResMapSetting tempSetting = ResMapSetting._instance;
        if (!ApproximatelyEqualColor(color, Color.black))
        {
            SpecialOreTypes i = SpecialOreTypes.Copper;
            while (!ApproximatelyEqualColor(color, tempSetting.GetColorPerSpecialOreType(i)))
            {
                i++;
            }
            //specialType을 구한 후 specialType2ChangeRate에 넣어서 확률을 구함.
            //구한 확률보다 랜덤값이 작으면 변경해서 리턴
            if (changeRate < specialType2ChangeRate[i])
                return (ResourceType)i;
        }

        //아니면 stone 계산
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
        //아니면 return nowType;
        return nowType;
    }
    bool ApproximatelyEqualColor(Color a, Color b, float epsilon = 0.01f)
    {
        //vector4로 변환해서 거리 비교할 수도 있음.
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
        //전체를 순회하지 말고 껍데기에서 더 작은 방향으로만 순회.
        //해당 좌표(x,z)의 껍데기만 가져오는 함수.

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
    //== 추가(end)
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
    //    //X,Z 좌표를 이용해 가장 큰 y 좌표 얻기.
    //    int tempHeight = 1;

    //    while (_worldBlocks[xzCoordinate.x, tempHeight, xzCoordinate.y]._obj == null)
    //    {
    //        tempHeight++;
    //    }

    //    return tempHeight;
    //}
}
