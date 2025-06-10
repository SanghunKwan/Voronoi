using System.Collections.Generic;
using UnityEngine;

public class TestPickker : MonoBehaviour
{
    WorldGenerator _worldG;


    private void Start()
    {
        _worldG = GetComponent<WorldGenerator>();
    }

    private void Update()
    {
        if (_worldG._isEndLoad)
        {
            if (Input.GetMouseButtonDown(1))
            {
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                int lMask = 1 << LayerMask.NameToLayer("ResBlock");
                if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, lMask))
                {
                    //Debug.LogFormat("픽한 위치 : {0}, 오브젝트 위치 {1}", hit.point, hit.transform.position);
                    Vector3 blockPos = hit.transform.position;

                    //맨 아래 블록은 소멸되지 않는다.
                    if (blockPos.y <= 0)
                        return;

                    int x = Mathf.RoundToInt(blockPos.x);
                    int y = Mathf.RoundToInt(blockPos.y);
                    int z = Mathf.RoundToInt(blockPos.z);


                    _worldG.ToggleBlock(_worldG[x, y, z], new Vector3Int(x, y, z));
                }
            }
        }
    }
}
