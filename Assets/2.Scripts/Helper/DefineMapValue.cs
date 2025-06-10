using UnityEngine;

namespace DefineMapValue
{
    public enum ResourceType
    {
        Copper,
        Iron,
        Gold,
        Diamond,

        Soil,
        Grass,
        Stone,
        Snow,
        Unbreak
    }


    public enum SpecialOreTypes
    {
        Copper,
        Iron,
        Gold,
        Diamond,

        Max
    }

    public enum ClassificationHeight
    {
        Soil = 0,
        Grass = 35,
        Snow = 55,

        MapStandardHeight = 30
    }
    public enum GetResourceRate
    {
        Stone = 35,
        Copper = 25,
        Iron = 20,
        Gold = 10,
        Diamond = 5
    }
    public class ResMapSetting
    {

        //==추가
        public const int _stageOfDividCount = 15;
        //==추가(end)

        static ResMapSetting _uniqueInstance = null;

        public static ResMapSetting _instance
        {
            get
            {
                if (_uniqueInstance == null)
                {
                    _uniqueInstance = new ResMapSetting();
                    _uniqueInstance.InitInst();
                }
                return _uniqueInstance;
            }
        }
        Color[] _SpecialOreColor;
        public Color GetColorPerSpecialOreType(SpecialOreTypes type) => _SpecialOreColor[(int)type];


        void InitInst()
        {
            _uniqueInstance = this;

            _SpecialOreColor = new Color[(int)SpecialOreTypes.Max];
            _SpecialOreColor[(int)SpecialOreTypes.Copper] = new Color(0.2735849f, 0.1199825f, 0.02968139f);
            _SpecialOreColor[(int)SpecialOreTypes.Iron] = new Color(0.7169812f, 0.3191613f, 0.08454966f);
            _SpecialOreColor[(int)SpecialOreTypes.Gold] = new Color(1f, 0.9314751f, 0.3349057f);
            _SpecialOreColor[(int)SpecialOreTypes.Diamond] = new Color(0.7028302f, 0.8469079f, 0.9245283f);
        }

    }
}
