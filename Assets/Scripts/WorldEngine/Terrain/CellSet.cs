using UnityEngine;
using System.Collections.Generic;
using System.Xml.Serialization;
using System.Linq;

public class CellSet
{
    public HashSet<TerrainCell> Cells;

    public TerrainCell Top;
    public TerrainCell Bottom;
    public TerrainCell Left;
    public TerrainCell Right;

    public int RectArea = 0;
    public int RectWidth = 0;
    public int RectHeight = 0;

    public int Area = 0;

    public bool NeedsUpdate = false;

    private bool _initialized = false;

    public void AddCell(TerrainCell cell)
    {
        if (!Cells.Add(cell)) return; // cell already present

        Area++;

        if (!_initialized)
        {
            Top = cell;
            Bottom = cell;
            Left = cell;
            Right = cell;

            _initialized = true;
            return;
        }

        if (((cell.Longitude - Left.Longitude) == -1) ||
            ((cell.Longitude - Left.Longitude - Manager.WorldWidth) == -1))
        {
            Left = cell;
        }

        if (((cell.Longitude - Right.Longitude) == 1) ||
            ((cell.Longitude - Right.Longitude + Manager.WorldWidth) == 1))
        {
            Right = cell;
        }

        if ((cell.Latitude - Top.Latitude) == -1)
        {
            Top = cell;
        }

        if ((cell.Latitude - Bottom.Latitude) == 1)
        {
            Bottom = cell;
        }

        NeedsUpdate = true;
    }

    public void Update()
    {
        int top = Top.Latitude;
        int bottom = Bottom.Latitude;
        int left = Left.Longitude;
        int right = Right.Longitude;

        // adjust for world wrap
        if (right < left) right += Manager.WorldWidth;

        RectHeight = bottom - top + 1;
        RectWidth = right - left + 1;

        RectArea = RectWidth * RectHeight;

        Area = Cells.Count;

        NeedsUpdate = false;
    }

    public bool IsCellEnclosed(TerrainCell cell)
    {
        int top = Top.Latitude;
        int bottom = Bottom.Latitude;
        int left = Left.Longitude;
        int right = Right.Longitude;

        // adjust for world wrap
        if (right < left) right += Manager.WorldWidth;

        if (!cell.Latitude.IsInsideRange(top, bottom)) return false;

        int longitude = cell.Longitude;

        if (longitude.IsInsideRange(left, right)) return true;

        longitude += Manager.WorldWidth;

        if (longitude.IsInsideRange(left, right)) return true;

        return false;
    }

    public void Merge(CellSet sourceSet)
    {
        Cells.UnionWith(sourceSet.Cells);

        if (Top.Latitude > sourceSet.Top.Latitude)
        {
            Top = sourceSet.Top;
        }

        if (Bottom.Latitude < sourceSet.Bottom.Latitude)
        {
            Bottom = sourceSet.Bottom;
        }

        bool offsetNeeded = false;
        bool rightOffsetDone = false;
        bool sourceSetRightOffsetDone = false;

        int rigthLongitude = Right.Longitude;
        if (Left.Longitude >= Right.Longitude)
        {
            rigthLongitude += Manager.WorldWidth;
            offsetNeeded = true;
            rightOffsetDone = true;
        }

        int sourceSetRigthLongitude = sourceSet.Right.Longitude;
        if (sourceSet.Left.Longitude >= sourceSet.Right.Longitude)
        {
            sourceSetRigthLongitude += Manager.WorldWidth;
            offsetNeeded = true;
            sourceSetRightOffsetDone = true;
        }

        int leftLongitude = Left.Longitude;
        if (offsetNeeded && !rightOffsetDone)
        {
            rigthLongitude += Manager.WorldWidth;
            leftLongitude += Manager.WorldWidth;
        }

        int sourceSetLeftLongitude = sourceSet.Left.Longitude;
        if (offsetNeeded && !sourceSetRightOffsetDone)
        {
            sourceSetRigthLongitude += Manager.WorldWidth;
            sourceSetLeftLongitude += Manager.WorldWidth;
        }

        if (leftLongitude > sourceSetLeftLongitude)
        {
            Left = sourceSet.Left;
        }

        if (rigthLongitude < sourceSetRigthLongitude)
        {
            Right = sourceSet.Right;
        }

        NeedsUpdate = true;
    }

    public static IEnumerable<CellSet> SplitIntoSubsets(
        CellSet set,
        int maxSetSideLength)
    {
        int maxLength = Mathf.Max(set.RectHeight, set.RectWidth);

        if (maxLength <= maxSetSideLength)
        {
            yield return set;
        }
        else if (maxLength == set.RectHeight)
        {
            int middleHeight = (set.Top.Latitude + set.Bottom.Latitude) / 2;

            CellSet topCellSet = new CellSet();
            CellSet bottomCellSet = new CellSet();

            foreach (TerrainCell cell in set.Cells)
            {
                if (cell.Latitude > middleHeight)
                    bottomCellSet.AddCell(cell);
                else
                    topCellSet.AddCell(cell);
            }

            topCellSet.Update();
            bottomCellSet.Update();

            foreach (CellSet subset in SplitIntoSubsets(topCellSet, maxSetSideLength))
            {
                yield return subset;
            }

            foreach (CellSet subset in SplitIntoSubsets(bottomCellSet, maxSetSideLength))
            {
                yield return subset;
            }
        }
        else
        {
            int middleWidth = (set.Left.Longitude + set.Right.Longitude) / 2;

            CellSet leftCellSet = new CellSet();
            CellSet rightCellSet = new CellSet();

            foreach (TerrainCell cell in set.Cells)
            {
                if (cell.Longitude > middleWidth)
                    rightCellSet.AddCell(cell);
                else
                    leftCellSet.AddCell(cell);
            }

            leftCellSet.Update();
            rightCellSet.Update();

            foreach (CellSet subset in SplitIntoSubsets(leftCellSet, maxSetSideLength))
            {
                yield return subset;
            }

            foreach (CellSet subset in SplitIntoSubsets(rightCellSet, maxSetSideLength))
            {
                yield return subset;
            }
        }
    }
}
