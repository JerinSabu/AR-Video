using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems; 
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

[RequireComponent(typeof(ARRaycastManager))]
[RequireComponent(typeof(ARAnchorManager))]
public class ARPlacementManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private ARInputReader inputReader;
    [SerializeField] private ARVideoContentLoader videoContentLoader;

    [Header("Placement Configuration")]
    [Tooltip("The video playback surface prefab that will be spawned on the wall.")]
    [SerializeField] private GameObject videoSurfacePrefab;

    private ARRaycastManager raycastManager;
    private ARAnchorManager anchorManager;

    private ARAnchor currentAnchor;
    private GameObject spawnedSurfaceInstance;

    private static List<ARRaycastHit> hitResults = new List<ARRaycastHit>();

    private void Awake()
    {
        raycastManager = GetComponent<ARRaycastManager>();
        anchorManager = GetComponent<ARAnchorManager>();
    }

    private void OnEnable()
    {
        if (inputReader != null)
        {
            inputReader.OnScreenTapped += HandleScreenTap;
        }
    }

    private void OnDisable()
    {
        if (inputReader != null)
        {
            inputReader.OnScreenTapped -= HandleScreenTap;
        }
    }

    private void HandleScreenTap(Vector2 screenPosition)
    {
        //Check if Finger is iver any UI elements
        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
        {
            Debug.Log("Screen tap intercepted by UI Canvas. Ignoring AR Placement repositioning.");
            return;
        }
        
        // Raycasting and object placement
        if (raycastManager.Raycast(screenPosition, hitResults, TrackableType.PlaneWithinPolygon))
        {
            ARRaycastHit closestHit = hitResults[0];
            ProcessPlacement(closestHit);
        }
    }

    private void ProcessPlacement(ARRaycastHit hitResult)
    {
        if (currentAnchor != null)
        {
            Destroy(currentAnchor.gameObject);
        }

        Vector3 wallPosition = hitResult.pose.position;
        Vector3 rawAngles = hitResult.pose.rotation.eulerAngles;
        Quaternion stableWallRotation = Quaternion.Euler(0f, rawAngles.y, 0f);
        Pose stabilizedPose = new Pose(wallPosition, stableWallRotation);

        ARPlaneManager planeManager = GetComponent<ARPlaneManager>();
        if (planeManager != null)
        {
            ARPlane plane = planeManager.GetPlane(hitResult.trackableId);
            if (plane != null)
            {
                currentAnchor = anchorManager.AttachAnchor(plane, stabilizedPose);
            }
        }

        if (currentAnchor == null)
        {
            GameObject anchorGO = new GameObject("AR_Anchor_Fallback");
            anchorGO.transform.position = stabilizedPose.position;
            anchorGO.transform.rotation = stabilizedPose.rotation;
            currentAnchor = anchorGO.AddComponent<ARAnchor>();
        }

        if (spawnedSurfaceInstance == null)
        {
            spawnedSurfaceInstance = Instantiate(videoSurfacePrefab, currentAnchor.transform);
            spawnedSurfaceInstance.transform.localPosition = Vector3.zero;
            spawnedSurfaceInstance.transform.localRotation = Quaternion.identity;
        }
        else
        {
            spawnedSurfaceInstance.transform.SetParent(currentAnchor.transform, false);
            spawnedSurfaceInstance.transform.localPosition = Vector3.zero;
            spawnedSurfaceInstance.transform.localRotation = Quaternion.identity;
        }

        VideoPlaybackManager activePlaybackManager = spawnedSurfaceInstance.GetComponentInChildren<VideoPlaybackManager>();

        if (activePlaybackManager != null && videoContentLoader != null)
        {
            videoContentLoader.SetPlaybackManager(activePlaybackManager);
        }
        else
        {
            Debug.LogWarning("Failed to establish dynamic system references. Check your prefab children setup!");
        }
    }
}