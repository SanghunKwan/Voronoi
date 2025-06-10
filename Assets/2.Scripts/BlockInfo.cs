using DefineMapValue;
using UnityEngine;

public class BlockInfo
{
    public ResourceType _resType { get; set; }
    public bool _isVisible { get; set; }
    public GameObject _obj { get; set; }

    public BlockInfo(ResourceType type, bool visible, GameObject block = null)
    {
        _resType = type;
        _isVisible = visible;
        _obj = block;
    }




}
