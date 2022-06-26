using System.Collections.Generic;
using UnityEngine;

public class Building : MonoBehaviour
{
    // snapping groups with the same group id will influence this building
    [SerializeField] int mySnappingGroupId;
    public int GetSnappingGroupId() => mySnappingGroupId;

    // all the snapping group that influance buildings when they close to this building
    [SerializeField] SnappingGroup[] snappingGroups;
    public SnappingGroup[] GetSnappingGroups() => snappingGroups;

#if UNITY_EDITOR
    private void OnDrawGizmosSelected() => LoopSnappingGroups();
#endif

    /// <summary>
    /// these method loops all the snapping group of the building
    /// it will only loop if the length of the array is bigger than 0 
    /// </summary>
    private void LoopSnappingGroups()
    {
        // check if the snapping groups have items in it, if not, do nothing
        if (snappingGroups.Length < 1) { return; }
        // loops each snapping group in the snappingGroups array
        foreach (SnappingGroup snappingGroup in snappingGroups)
        {
            // loop all the snapping points in the snapping group
            LoopSnappingPoints(snappingGroup);
            // loop all the rotation pivot points in the snapping group
            LoopRotationPivotPoints(snappingGroup);
        }
    }

    /// <summary>
    /// these method loops over all the snapping points of a snapping group
    /// </summary>
    private void LoopSnappingPoints(SnappingGroup snappingGroup)
    {
        // checks if the snapping group's snapping points has items in it, if not, do nothing
        if (snappingGroup.GetPositionSnappingPoints().Count < 1) { return; }
        // loop all the snapping points in the positionSnappingPoints list
        foreach (Transform snappingPoint in snappingGroup.GetPositionSnappingPoints())
            // draw point for each snapping point in the snapping group
            DrawPoint(Color.green, snappingPoint);
    }

    /// <summary>
    /// this method loops all the rotation pivot points of a snappingGroup
    /// </summary>
    private void LoopRotationPivotPoints(SnappingGroup snappingGroup)
    {
        // if the count of the rotationPivotPoints is nothing than do nothing
        if (snappingGroup.GetRotatationPivotPoints().Count < 1) { return; }
        // goes over each rotation pivot in the rotationPivotPoints of the snappingGroup
        foreach (Transform rotationPivot in snappingGroup.GetRotatationPivotPoints()) 
            // draws a point for each of the rotation pivot points
            DrawPoint(Color.blue, rotationPivot);
    }

    /// <summary>
    /// this method draws a gizmos sphere with a given color, transform and radius
    /// </summary>
    private void DrawPoint(Color gizmosColor, Transform pointTransform, float radius = 1f)
    {
        // check if the transform is null if it is, do nothing
        if (!pointTransform) { return; }
        // set the color to the given gizmos color
        Gizmos.color = gizmosColor;
        // draws a sphere in the position of the transform with the given radius
        Gizmos.DrawSphere(pointTransform.position, radius);
    }
}

[System.Serializable]
// a snapping group represents a snapping points and rotation pivot that buildings with the same
// group id will snapp to and rotate around when near this building object
public struct SnappingGroup
{
    // position snapping points, all the transform pos which buildings position with the same id group can snap to
    // rotaion pivot points, all the transform points which buildings with the same id group can rotate around
    [SerializeField] List<Transform> positionSnappingPoints, rotationPivotPoints;
    public List<Transform> GetPositionSnappingPoints() => positionSnappingPoints;
    public List<Transform> GetRotatationPivotPoints() => rotationPivotPoints;

    // buildings with the same group id will acknowledge this snapping group 
    [SerializeField] int groupId; 
    public int GetGroupId() => groupId;
}