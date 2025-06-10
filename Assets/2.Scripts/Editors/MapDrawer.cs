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
        //�ؽ��� �ȼ� �ϳ��ϳ��� ���� ��� �ִ� �迭�� Texture2D�� �ȼ� ������ 1���� �迭�� �Ǿ� �ֽ��ϴ�.
        Rect rect = vo.PlotBounds;
        int width = Mathf.RoundToInt(rect.width);
        int height = Mathf.RoundToInt(rect.height);

        Color[] pixelColors = Enumerable.Repeat(Color.white, width * height).ToArray();
        List<Vector2> siteCoords = vo.SiteCoords();
        //���� �߽��� �׸���
        //foreach (var item in siteCoords)
        //{
        //    int x = Mathf.RoundToInt(item.x);
        //    int y = Mathf.RoundToInt(item.y);

        //    int index = x * width + y;
        //    pixelColors[index] = Color.red;
        //}

        //==�߰�
        Vector2Int size = new Vector2Int(width, height);
        //�𼭸� �׸���
        foreach (var site in vo.Sites)
        {
            //�������� �̿� ��������� ���´�.
            List<Site> neighbors = site.NeighborSites();
            foreach (var neighbor in neighbors)
            {
                //�̿��� ������鿡�Լ� ��ġ�� �����ڸ��� �����س���.
                Edge edge = vo.FindEdgeFromAdjacentPolygons(site, neighbor);

                if (edge.ClippedVertices is null)
                    continue;

                //�����ڸ��� �̷�� �𼭸� ���� 2���� ���´�.
                Vector2 corner1 = edge.ClippedVertices[LR.LEFT];
                Vector2 corner2 = edge.ClippedVertices[LR.RIGHT];


                //1�� �Լ��� �׷����� �׸����� �ؽ��Ŀ� �����ڸ� ������ �׸���.
                Vector2 targetPoint = corner1;
                float delta = 1 / (corner2 - corner1).magnitude;
                float lerpRatio = 0f;

                while ((int)targetPoint.x != (int)corner2.x || (int)targetPoint.y != (int)corner2.y)
                {
                    //���� ������ ���� �� ���� �� ���̸� ������ ���� ���´�(LerpRatio��ŭ ������).
                    targetPoint = Vector2.Lerp(corner1, corner2, lerpRatio);
                    lerpRatio += delta;

                    //�ؽ����� ��ǥ ������ (0 ~ size.x - 1)������ ���γ��� ���̾�׷��� ��ǥ ������ (0 ~ (float) size.x)�̴�.
                    int x = Mathf.Clamp((int)targetPoint.x, 0, size.x - 1);
                    int y = Mathf.Clamp((int)targetPoint.y, 0, size.y - 1);

                    int index = x * size.x + y;
                    pixelColors[index] = Color.black;
                }
            }
            PrintColor(ResMapSetting._instance.GetColorPerSpecialOreType((SpecialOreTypes)Random.Range((int)SpecialOreTypes.Copper, (int)SpecialOreTypes.Max)), Color.white, pixelColors, site.Coord, size);
        }
        //==�߰�(end)
        //�ؽ���ȭ ��Ű�� ��������Ʈ�� ��ȯ 
        return DrawSprite(size, pixelColors);
    }
    static void PrintColor(in Color color, in Color targetColor, in Color[] pixelColors, in Vector2 coordinate, in Vector2Int size)
    {
        Vector2Int startPoint = new Vector2Int((int)coordinate.x, (int)coordinate.y);

        MovePrintColor(color, targetColor, pixelColors, startPoint, size);
    }
    static void MovePrintColor(in Color color, in Color targetColor, in Color[] pixelColors, Vector2Int coordinate, in Vector2Int size)
    {
        //��������� 4���� Ž��. ������ ���� ã��.
        //���� �ε���
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
                //���� �������κ����� �Ÿ��� ���� ���� ����. ���� ����ŷ ������ ����� ����.
                float distFromCenter = Vector2.Distance(center, position) + (radius - maskRadius);
                float colorFactor = distFromCenter / radius;
                //�Ÿ��� �ּ��� ���� 1�� ����������� ���� �ǵ��� ���� �����뿩�� �ϱ⿡ ���� �����Ѵ�.
                colorDatas[index++] = 1 - colorFactor;
            }
        }
        return colorDatas;
    }

    //==�߰�
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
    //==�߰�(end)
}
