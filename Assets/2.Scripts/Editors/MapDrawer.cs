using csDelaunay;
using UnityEngine;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using DefineMapValue;

public static class MapDrawer
{
    public static Sprite DrawSprite(Vector2Int size, Color[] colorDatas)
    {
        Texture2D texture = new Texture2D(size.x, size.y);
        texture.filterMode = FilterMode.Point;
        texture.SetPixels(colorDatas);
        texture.Apply();

        Rect rect = new Rect(0, 0, size.x, size.y);
        return Sprite.Create(texture, rect, Vector2.one * 0.5f);
    }

    public static Sprite DrawVoronoiToSprite(Voronoi vo)
    {
        //텍스쳐 픽셀 하나하나의 색을 담고 있는 배열로 Texture2D의 픽셀 정보는 1차원 배열로 되어 있습니다.
        Rect rect = vo.PlotBounds;
        int width = Mathf.RoundToInt(rect.width);
        int height = Mathf.RoundToInt(rect.height);

        Color[] pixelColors = Enumerable.Repeat(Color.white, width * height).ToArray();
        List<Vector2> siteCoords = vo.SiteCoords();
        //무게 중심점 그리기
        //foreach (var item in siteCoords)
        //{
        //    int x = Mathf.RoundToInt(item.x);
        //    int y = Mathf.RoundToInt(item.y);

        //    int index = x * width + y;
        //    pixelColors[index] = Color.red;
        //}

        //==추가
        Vector2Int size = new Vector2Int(width, height);
        //모서리 그리기
        foreach (var site in vo.Sites)
        {
            //폴리곤의 이웃 폴리곤들을 얻어온다.
            List<Site> neighbors = site.NeighborSites();
            foreach (var neighbor in neighbors)
            {
                //이웃한 폴리곤들에게서 겹치는 가장자리를 유도해낸다.
                Edge edge = vo.FindEdgeFromAdjacentPolygons(site, neighbor);

                if (edge.ClippedVertices is null)
                    continue;

                //가장자리를 이루는 모서리 정점 2개를 얻어온다.
                Vector2 corner1 = edge.ClippedVertices[LR.LEFT];
                Vector2 corner2 = edge.ClippedVertices[LR.RIGHT];


                //1차 함수의 그래프를 그리듯이 텍스쳐에 가장자리 선분을 그린다.
                Vector2 targetPoint = corner1;
                float delta = 1 / (corner2 - corner1).magnitude;
                float lerpRatio = 0f;

                while ((int)targetPoint.x != (int)corner2.x || (int)targetPoint.y != (int)corner2.y)
                {
                    //선형 보간을 통해 두 개의 점 사이를 나누는 점을 얻어온다(LerpRatio만큼 나누는).
                    targetPoint = Vector2.Lerp(corner1, corner2, lerpRatio);
                    lerpRatio += delta;

                    //텍스쳐의 좌표 영역은 (0 ~ size.x - 1)이지만 보로노이 다이어그램의 좌표 영역은 (0 ~ (float) size.x)이다.
                    int x = Mathf.Clamp((int)targetPoint.x, 0, size.x - 1);
                    int y = Mathf.Clamp((int)targetPoint.y, 0, size.y - 1);

                    int index = x * size.x + y;
                    pixelColors[index] = Color.black;
                }
            }
            PrintColor(ResMapSetting._instance.GetColorPerSpecialOreType((SpecialOreTypes)Random.Range((int)SpecialOreTypes.Copper, (int)SpecialOreTypes.Max)), Color.white, pixelColors, site.Coord, size);
        }
        //==추가(end)
        //텍스쳐화 시키고 스프라이트로 변환 
        return DrawSprite(size, pixelColors);
    }
    static void PrintColor(in Color color, in Color targetColor, in Color[] pixelColors, in Vector2 coordinate, in Vector2Int size)
    {
        Vector2Int startPoint = new Vector2Int((int)coordinate.x, (int)coordinate.y);

        MovePrintColor(color, targetColor, pixelColors, startPoint, size);
    }
    static void MovePrintColor(in Color color, in Color targetColor, in Color[] pixelColors, Vector2Int coordinate, in Vector2Int size)
    {
        //재귀적으로 4방향 탐색. 지정된 색만 찾기.
        //현재 인덱스
        int index = coordinate.x * size.x + coordinate.y;

        if (index < 0 || index >= size.x * size.y || pixelColors[index] != targetColor)
            return;

        pixelColors[index] = color;

        if (coordinate.x != 0)
            MovePrintColor(color, targetColor, pixelColors, coordinate + Vector2Int.left, size);
        if (coordinate.y != size.y)
            MovePrintColor(color, targetColor, pixelColors, coordinate + Vector2Int.up, size);
        if (coordinate.x != size.x)
            MovePrintColor(color, targetColor, pixelColors, coordinate + Vector2Int.right, size);
        if (coordinate.y != 0)
            MovePrintColor(color, targetColor, pixelColors, coordinate + Vector2Int.down, size);
    }


    public static float[] GetRadialGradientMask(Vector2Int size, int maskRadius)
    {
        float[] colorDatas = new float[size.x * size.y];

        Vector2Int center = size / 2;
        float radius = center.y;
        int index = 0;
        for (int y = 0; y < size.y; ++y)
        {
            for (int x = 0; x < size.x; ++x)
            {
                Vector2Int position = new Vector2Int(x, y);
                //맵의 중점으로부터의 거리에 따라 색을 결정. 물론 마스킹 범위를 고려한 연산.
                float distFromCenter = Vector2.Distance(center, position) + (radius - maskRadius);
                float colorFactor = distFromCenter / radius;
                //거리가 멀수록 색은 1에 가까워지지만 원래 의도는 땅이 고지대여야 하기에 색을 반전한다.
                colorDatas[index++] = 1 - colorFactor;
            }
        }
        return colorDatas;
    }

    //==추가
    public static void CreateImageToFile(string fileName, Sprite image)
    {
        Texture2D img = new Texture2D(image.texture.width, image.texture.height);
        for (int y = 0; y < img.height; ++y)
        {
            for (int x = 0; x < img.height; ++x)
            {
                img.SetPixel(x, y, image.texture.GetPixel(x, y));
            }
        }
        img.Apply();
        byte[] by = img.EncodeToPNG();
        FileStream fs = new FileStream(fileName, FileMode.Create, FileAccess.Write);
        BinaryWriter bw = new BinaryWriter(fs);
        bw.Write(by);
        bw.Close();
        fs.Close();
    }
    //==추가(end)
}
