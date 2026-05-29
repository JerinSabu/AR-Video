using System;
using System.Collections;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.Video;

public class ARVideoContentLoader : MonoBehaviour
{
    private VideoPlaybackManager playbackManager;
    private AsyncOperationHandle<VideoClip> currentVideoHandle;
    private string currentActiveKey;

    private void OnDestroy()
    {
        ReleaseCurrentVideoAsset();
    }

    
    public void SetPlaybackManager(VideoPlaybackManager manager)
    {
        playbackManager = manager;
        Debug.Log("VideoPlaybackManager instance dynamically linked to Content Loader.");
    }

    
    // Initiates the asynchronous remote download of a VideoClip using its Addressable key
    public void LoadAndPlayRemoteVideo(string addressableKey)
    {
        if (string.IsNullOrEmpty(addressableKey))
        {
            Debug.LogError("The requested Addressable Key is null or empty.");
            return;
        }

        if (addressableKey == currentActiveKey && currentVideoHandle.IsValid())
        {
            Debug.LogWarning($"Video key '{addressableKey}' is already active.");
            return;
        }

        // Unload previous video from memory before switching
        ReleaseCurrentVideoAsset();
        currentActiveKey = addressableKey;

        StartCoroutine(LoadVideoAssetRoutine(addressableKey));
    }

    private IEnumerator LoadVideoAssetRoutine(string key)
    {
        AsyncOperationHandle<VideoClip> loadHandle = Addressables.LoadAssetAsync<VideoClip>(key);
        currentVideoHandle = loadHandle;

        while (!loadHandle.IsDone)
        {
            yield return null;
        }

        if (loadHandle.Status == AsyncOperationStatus.Succeeded)
        {
            
            VideoClip downloadedClip = loadHandle.Result;

            
            if (playbackManager != null && downloadedClip != null)
            {
                playbackManager.PlayVideoFromClip(downloadedClip);
            }
            else
            {
                Debug.LogError("Cannot play video: VideoPlaybackManager has not been linked yet. Tap a wall first!");
            }
        }
        else
        {
            Debug.LogError($"Failed to stream Addressable asset with key: {key}. Check your GitHub URL config.");
            currentActiveKey = string.Empty;
        }
    }

    private void ReleaseCurrentVideoAsset()
    {
        if (currentVideoHandle.IsValid())
        {
            if (playbackManager != null)
            {
                playbackManager.StopVideo();
            }

            Addressables.Release(currentVideoHandle);
            currentActiveKey = string.Empty;
            Debug.Log("Prior Addressable asset cleared from RAM.");
        }
    }
}