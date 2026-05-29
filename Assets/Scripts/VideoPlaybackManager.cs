using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Video;

[RequireComponent(typeof(VideoPlayer))]
public class VideoPlaybackManager : MonoBehaviour
{
    
    public event Action OnVideoPreparationStarted;
    public event Action OnVideoPreparationComplete;

    private VideoPlayer videoPlayer;
    private Coroutine aspectAdjustmentCoroutine;

    private void Awake()
    {
        videoPlayer = GetComponent<VideoPlayer>();
    }

    private void OnEnable()
    {
        
        videoPlayer.prepareCompleted += HandlePrepareComplete;
    }

    private void OnDisable()
    {
        videoPlayer.prepareCompleted -= HandlePrepareComplete;
    }

    
    // Instructs the player to load and prepare a video from a direct URL/Path.
    // Supports both local and remote streaming endpoints seamlessly.
    
    public void PlayVideoFromUrl(string videoUrl)
    {
        if (string.IsNullOrEmpty(videoUrl))
        {
            Debug.LogError("Playback URL supplied to VideoPlaybackManager is null or empty.");
            return;
        }

        // Safely halt any ongoing playback cycles before shifting tracks
        StopVideo();

        OnVideoPreparationStarted?.Invoke();

        // Assign the cloud Addressable path directly to the Video Player engine
        videoPlayer.source = VideoSource.Url;
        videoPlayer.url = videoUrl;

        // Initialize the asynchronous background hardware decoding thread
        videoPlayer.Prepare();
    }

    
    /// Fallback VideoClip references via Addressables.
    
    public void PlayVideoFromClip(VideoClip clip)
    {
        if (clip == null)
        {
            Debug.LogError("VideoClip reference supplied to VideoPlaybackManager is null.");
            return;
        }

        StopVideo();
        OnVideoPreparationStarted?.Invoke();

        videoPlayer.source = VideoSource.VideoClip;
        videoPlayer.clip = clip;

        
        if (videoPlayer.audioTrackCount > 0)
        {
            videoPlayer.EnableAudioTrack(0, true);
        }
        

        videoPlayer.Prepare();
    }

    public void StopVideo()
    {
        if (aspectAdjustmentCoroutine != null)
        {
            StopCoroutine(aspectAdjustmentCoroutine);
            aspectAdjustmentCoroutine = null;
        }

        if (videoPlayer.isPlaying || videoPlayer.isPrepared)
        {
            videoPlayer.Stop();
        }
    }

    private void HandlePrepareComplete(VideoPlayer source)
    {
        OnVideoPreparationComplete?.Invoke();

        
        source.Play();

        
        if (source.audioOutputMode == VideoAudioOutputMode.AudioSource)
        {
            AudioSource attachedSource = source.GetTargetAudioSource(0);
            if (attachedSource != null && !attachedSource.isPlaying)
            {
                attachedSource.Play();
                Debug.Log($"Explicitly started audio hardware track for: {source.name}");
            }
        }
        

        // Adjust aspect ratio dynamically 
        if (aspectAdjustmentCoroutine != null) StopCoroutine(aspectAdjustmentCoroutine);
        aspectAdjustmentCoroutine = StartCoroutine(AdjustMeshAspectRatioRoutine());
    }

    //adjusting Transform to reduce stretch
    private IEnumerator AdjustMeshAspectRatioRoutine()
    {
        // Wait until the hardware decoder reports valid texture sizes
        while (videoPlayer.texture == null || videoPlayer.texture.width <= 0)
        {
            yield return null;
        }

        float videoWidth = videoPlayer.texture.width;
        float videoHeight = videoPlayer.texture.height;

        
        float targetAspectRatio = videoWidth / videoHeight;

        
        Vector3 newLocalScale = new Vector3(targetAspectRatio, 1f, 1f);
        transform.localScale = newLocalScale;

        Debug.Log($"Video aspect ratio adjusted seamlessly: {videoWidth}x{videoHeight} (Aspect: {targetAspectRatio})");
    }
}