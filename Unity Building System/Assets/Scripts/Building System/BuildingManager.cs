using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuildingManager : MonoBehaviour
{
    [Header("References")]
    // reference for the current building instance we want to place
    private Building buildingInstnace;
    // the closest pivot point to the camera ray and the closest snapping point to the ray
    private Transform closestPivotPoint = default, closestSnappingPoint = default;
    // if true then the building is snapped to a snapping point if not the building is free to follow the camera ray
    private bool hasSnapped = false;
    // the name of the layer which will be the building instance's layer once he is placed
    private const string buildingLayerName = "Building";
    // reference for the main camera used to shoot a ray from the camera position
    private Camera mainCam;

    [Header("Settings")]
    // the layer mask of all the buildings used for checking if a building is overlaping another
    [SerializeField] LayerMask buildingsLayerMask;
    // a test for the building prefab that will be placed (in future need to be tied with an item)
    [SerializeField] Building buildingPrefabTEST;

    private void Awake()
    {
        // TEST will be deleted but for now it spawns a building instnace in the start so we can test the system
        if (buildingPrefabTEST)
            buildingInstnace = Instantiate(buildingPrefabTEST, transform.position, Quaternion.identity);
        SubscribeToEvents();
    }

    private void Start() => FindPrivateObjects();

    private void Update()
    {
        if (buildingInstnace) HandleSnapping(GetMouse3DPosition(mainCam));
        if (!hasSnapped) SetBuildingTransformBasedOnView();
    }

    private void OnDestroy() => PlayerAttackingManager.OnPlayerAction -= PlaceBuilding;

    /// <summary>
    /// set the building instance position and rotation based on the mouse position and the manager(player) rotation
    /// </summary>
    private void SetBuildingTransformBasedOnView()
    {
        buildingInstnace.transform.position = GetMouse3DPosition(mainCam);
        buildingInstnace.transform.rotation = transform.rotation;
    }


    /// <summary>
    /// method that get called on awake, subscribes methods to events
    /// </summary>
    private void SubscribeToEvents()
    {
        // subscribes the place building when we press the building button (in future we need to tie it with the item action)
        PlayerAttackingManager.OnPlayerAction += PlaceBuilding;
        // subscribes the rotate building preformed action to the rotate building method
        InputManager.instance.playerInputActions.Interaction.RotateBuilding.performed += RotateBuilding;
    }

    /// <summary>
    /// method for finding the private objects
    /// </summary>
    private void FindPrivateObjects()
    {
        // finds the main camera
        mainCam = Camera.main;
    }


    /// <summary>
    /// method that get called when we preform the RotateBuilding input action
    /// </summary>
    private void RotateBuilding(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        // rotates the building instance 90 degrees around the closest pivot point
        buildingInstnace.transform.RotateAround(closestPivotPoint.position, buildingInstnace.transform.up, 90f);
    }

    /// <summary>
    /// method for placing the building instance, TEST: get called when the player action event invokes 
    /// </summary>
    private void PlaceBuilding()
    {
        // if the building instance is null we do nothing
        if (!buildingInstnace) { return; }
        // if the building is overlapping we do nothing
        if (IsBuildingOverLapping(buildingsLayerMask)) { return; }
        // if the building is not overlapping we place the building
        SetPlacedBuilding();
    }

    /// <summary>
    /// this method sets the settigs of a building when it's placed
    /// </summary>
    private void SetPlacedBuilding()
    {
        // set the layer of the building and the building childrens to the building layer
        GameObjectExtentions.SetLayerRecursively(buildingInstnace.transform, LayerMask.NameToLayer(buildingLayerName));
        // enables the collider of the building
        buildingInstnace.GetComponent<Collider>().enabled = true;
        // TEST: makes a new instance of the building so we can place the building again
        buildingInstnace = Instantiate(buildingPrefabTEST, transform.position, Quaternion.identity);
        // set snapped to false
        hasSnapped = false;
    }

    /// <summary>
    /// this methods checks if there is any colliders from the given building mask in the checking radius from the
    /// building instance position
    /// </summary>
    private bool IsBuildingOverLapping(LayerMask buildingMask, float checkingRadius = 1f)
    {
        // get the overlapping colliders in the building layer mask
        Collider[] Colliders = Physics.OverlapSphere
            (buildingInstnace.transform.position, checkingRadius, buildingMask);
        // return if there are overlapping colliders
        return Colliders.Length > 0;
    }

    /// <summary>
    /// this method sets the closest rotation pivot point and the closest snapping point
    /// and snaps the building instance to the closest snapping point and closest rotation pivot
    /// </summary>
    private void HandleSnapping(Vector3 currentPos)
    {
        // create a list of all the near buildings tranfomrs
        List<Transform> buildingsTransforms = CreateNearBuildingsTransforms(currentPos);
        // if there are no building transforms near we going out of snapp and do nothing
        if (buildingsTransforms.Count < 1) { hasSnapped = false; return; }
        // get the closest building out of all the buildingsTransforms
        Building closestBuilding = DetectionHelper.GetClosest
            (buildingsTransforms, currentPos).GetComponent<Building>();
        // loop all the snapping group of the closest buidling
        LoopClosestBuildingSnappingGroups(currentPos, closestBuilding);
        // if we are not snaped we snap the building
        if (!hasSnapped) SnapBuilding();
    }

    /// <summary>
    /// this method loops over all the snapping groups of the closest building to the building instance 
    /// </summary>
    private void LoopClosestBuildingSnappingGroups(Vector3 currentPos, Building closestBuilding)
    {
        foreach (SnappingGroup snappingGroup in closestBuilding.GetSnappingGroups())
        {
            // if the snapping group id is not the same for the snappingGroup
            // and the building instance's snappingGroup we ignore the group
            if (snappingGroup.GetGroupId() != buildingInstnace.GetSnappingGroupId()) { continue; }
            GetClosestSnapping(currentPos, snappingGroup);
        }
    }

    /// <summary>
    /// this method gets the closest snapping point and rotation point from the snapping group it recieves
    /// </summary>
    private void GetClosestSnapping(Vector3 currentPos, SnappingGroup snappingGroup)
    {
        // reference the snapping point before we detect any new snapping points
        Transform oldSnappingPoint = closestSnappingPoint;
        // detect the closest snapping point
        closestSnappingPoint = DetectionHelper.GetClosest(snappingGroup.GetPositionSnappingPoints(), currentPos);
        // detect the closest pivot point
        closestPivotPoint = DetectionHelper.GetClosest(snappingGroup.GetRotatationPivotPoints(), currentPos);
        // if the old snapping point or the new snapping point is null we do nothing
        if (!oldSnappingPoint || !closestSnappingPoint) { return; }
        // if the old snapping point is not the new one we set hasSnapped to false so we can snap to the new point
        if (oldSnappingPoint.position != closestSnappingPoint.position) { hasSnapped = false; }
    }

    /// <summary>
    /// this method snapps the building instance to the closest snapping point position and rotation
    /// </summary>
    private void SnapBuilding()
    {
        // set the position of the building instance to the snapping point position
        buildingInstnace.transform.position = closestSnappingPoint.position;
        // set the rotation of the building instance to the snapping point rotation
        buildingInstnace.transform.rotation = closestSnappingPoint.rotation;
        // set hasSnapped to true
        hasSnapped = true;
    }

    /// <summary>
    /// this method loops all the colliders in the colliders list created with overlap sphere
    /// set to detect the building layer mask and in the radiusBuildingCheck from the current pos
    /// it adds each valid collider's building to the buildingTranforms list
    /// </summary>
    private List<Transform> CreateNearBuildingsTransforms(Vector3 currentPos, float radiusBuildingCheck = 5f)
    {
        // create a list of all the near building transforms
        List<Transform> buildingsTransforms = new List<Transform>();
        // get all the near colliders
        Collider[] Colliders = Physics.OverlapSphere(currentPos, radiusBuildingCheck, buildingsLayerMask);
        // if there are no colliders we are not snapped
        if (Colliders.Length < 1) { hasSnapped = false; }
        // loop all the colliders
        for (int i = 0; i < Colliders.Length; i++)
        {
            // get the current collider
            Collider Collider = Colliders[i];
            // add the collider's transform to the building transform list if the collider is a valid building
            if (Collider.TryGetComponent<Building>(out Building building) && building != buildingInstnace)
                buildingsTransforms.Add(building.transform);
        }

        return buildingsTransforms;
    }

    /// <summary>
    /// this method gets the world position of the mouse from the main camera
    /// </summary>
    private Vector3 GetMouse3DPosition(Camera mainCamera)
    {
        Ray ray = mainCamera.ScreenPointToRay(InputManager.GetMousePosition());
        Physics.Raycast(ray, out RaycastHit hit);
        return hit.point;
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        if (!mainCam) { return; }
        if (GetMouse3DPosition(mainCam) == null) { return; }
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(GetMouse3DPosition(mainCam), 1f);
    }
#endif
}
