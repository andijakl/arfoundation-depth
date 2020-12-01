using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

public class DepthImageVisualizerMaterial : MonoBehaviour
{
    /// <summary>
    /// Get or set the <c>ARCameraManager</c>.
    /// </summary>
    public ARCameraManager cameraManager
    {
        get => m_CameraManager;
        set => m_CameraManager = value;
    }

    [SerializeField]
    [Tooltip("The ARCameraManager which will produce camera frame events.")]
    ARCameraManager m_CameraManager;

    /// <summary>
    /// The depth material for rendering depth textures.
    /// </summary>
    public Material depthMaterial
    {
        get => m_DepthMaterial;
        set => m_DepthMaterial = value;
    }

    [SerializeField]
    Material m_DepthMaterial;

    /// <summary>
    /// Name of the display rotation matrix in the shader.
    /// </summary>
    const string k_DisplayRotationPerFrameName = "_DisplayRotationPerFrame";

    /// <summary>
    /// ID of the display rotation matrix in the shader.
    /// </summary>
    static readonly int k_DisplayRotationPerFrameId = Shader.PropertyToID(k_DisplayRotationPerFrameName);


    /// <summary>
    /// Get or set the <c>AROcclusionManager</c>.
    /// </summary>
    public AROcclusionManager occlusionManager
    {
        get => m_OcclusionManager;
        set => m_OcclusionManager = value;
    }

    [SerializeField]
    [Tooltip("The AROcclusionManager which will produce depth textures.")]
    AROcclusionManager m_OcclusionManager;

    /// <summary>
    /// The UI RawImage used to display the image on screen.
    /// </summary>
    public RawImage rawImage
    {
        get => m_RawImage;
        set => m_RawImage = value;
    }

    [SerializeField]
    RawImage m_RawImage;

    void Awake()
    {
#if UNITY_ANDROID
        k_AndroidFlipYMatrix[1, 1] = -1.0f;
        k_AndroidFlipYMatrix[2, 1] = 1.0f;
#endif // UNITY_ANDROID
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    static void UpdateRawImage(RawImage rawImage, XRCpuImage cpuImage)
    {
        // Get the texture associated with the UI.RawImage that we wish to display on screen.
        var texture = rawImage.texture as Texture2D;

        // If the texture hasn't yet been created, or if its dimensions have changed, (re)create the texture.
        // Note: Although texture dimensions do not normally change frame-to-frame, they can change in response to
        //    a change in the camera resolution (for camera images) or changes to the quality of the human depth
        //    and human stencil buffers.
        if (texture == null || texture.width != cpuImage.width || texture.height != cpuImage.height)
        {
            texture = new Texture2D(cpuImage.width, cpuImage.height, cpuImage.format.AsTextureFormat(), false);
            rawImage.texture = texture;
        }

        // For display, we need to mirror about the vertical access.
        var conversionParams = new XRCpuImage.ConversionParams(cpuImage, cpuImage.format.AsTextureFormat(), XRCpuImage.Transformation.MirrorY);

        //Debug.Log("Texture format: " + cpuImage.format.AsTextureFormat()); -> RFloat

        // Get the Texture2D's underlying pixel buffer.
        var rawTextureData = texture.GetRawTextureData<byte>();

        // Make sure the destination buffer is large enough to hold the converted data (they should be the same size)
        Debug.Assert(rawTextureData.Length == cpuImage.GetConvertedDataSize(conversionParams.outputDimensions, conversionParams.outputFormat),
            "The Texture2D is not the same size as the converted data.");

        // Perform the conversion.
        cpuImage.Convert(conversionParams, rawTextureData);

        // "Apply" the new pixel data to the Texture2D.
        texture.Apply();


        // Get the aspect ratio for the current texture.
        float textureAspectRatio = (float)texture.width / (float)texture.height;

        // Determine the raw imge rectSize preserving the texture aspect ratio, matching the screen orientation,
        // and keeping a minimum dimension size.
        float minDimension = 480.0f;
        float maxDimension = Mathf.Round(minDimension * textureAspectRatio);
        var rectSize = new Vector2(maxDimension, minDimension);
        //var rectSize = new Vector2(minDimension, maxDimension);   //Portrait
        rawImage.rectTransform.sizeDelta = rectSize;


        // Make sure it's enabled.
        rawImage.enabled = true;
    }

    // Update is called once per frame
    void Update()
    {
        //m_RawImage.texture = null;
        //if (!Mathf.Approximately(m_TextureAspectRatio, k_DefaultTextureAspectRadio))
        //{
        //    m_TextureAspectRatio = k_DefaultTextureAspectRadio;
        //    UpdateRawImage();
        //}
        if (occlusionManager.TryAcquireEnvironmentDepthCpuImage(out XRCpuImage image))
        {
            using (image)
            {
                // Use the texture.
                UpdateRawImage(m_RawImage, image);
            }
        }


        return;
        Texture2D envDepth = m_OcclusionManager.environmentDepthTexture;
        var displayTexture = envDepth;
        m_RawImage.texture = displayTexture;
        // Get the aspect ratio for the current texture.
        float textureAspectRatio = (displayTexture == null) ? 1.0f : ((float)displayTexture.width / (float)displayTexture.height);

        // Determine the raw imge rectSize preserving the texture aspect ratio, matching the screen orientation,
        // and keeping a minimum dimension size.
        float minDimension = 480.0f;
        float maxDimension = Mathf.Round(minDimension * textureAspectRatio);
        // For portrait mode
        var rectSize = new Vector2(minDimension, maxDimension);
        m_RawImage.rectTransform.sizeDelta = rectSize;
        //Material material;
        //material = m_DepthMaterial;
        //m_DepthMaterial.SetFloat(k_MaxDistanceId, maxDistance);
        //m_DepthMaterial.SetMatrix(k_DisplayRotationPerFrameId, m_DisplayRotationMatrix);
        //m_RawImage.material = m_DepthMaterial;

        // If the raw image needs to be updated because of a device orientation change or because of a texture
        // aspect ratio difference, then update the raw image with the new values.
        //if ((m_CurrentScreenOrientation != Screen.orientation)
        //    || !Mathf.Approximately(m_TextureAspectRatio, textureAspectRatio))
        //{
        //    m_CurrentScreenOrientation = Screen.orientation;
        //    m_TextureAspectRatio = textureAspectRatio;
        //    UpdateRawImage();
        //}
    }

    /// <summary>
    /// The display rotation matrix for the shader.
    /// </summary.
    Matrix4x4 m_DisplayRotationMatrix = Matrix4x4.identity;

    void OnEnable()
    {
        // Subscribe to the camera frame received event, and initialize the display rotation matrix.
        //Debug.Assert(m_CameraManager != null, "no camera manager");
        //m_CameraManager.frameReceived += OnCameraFrameEventReceived;
    }

    void OnDisable()
    {
        // Unsubscribe to the camera frame received event, and initialize the display rotation matrix.
        //Debug.Assert(m_CameraManager != null, "no camera manager");
        //m_CameraManager.frameReceived -= OnCameraFrameEventReceived;
    }


#if UNITY_ANDROID
    /// <summary>
    /// A matrix to flip the Y coordinate for the Android platform.
    /// </summary>
    Matrix4x4 k_AndroidFlipYMatrix = Matrix4x4.identity;
#endif // UNITY_ANDROID

    /// <summary>
    /// When the camera frame event is raised, capture the display rotation matrix.
    /// </summary>
    /// <param name="cameraFrameEventArgs">The arguments when a camera frame event is raised.</param>
    void OnCameraFrameEventReceived(ARCameraFrameEventArgs cameraFrameEventArgs)
    {
        Debug.Assert(m_RawImage != null, "no raw image");
        if (m_RawImage.material != null)
        {
            // Copy the display rotation matrix from the camera.
            Matrix4x4 cameraMatrix = cameraFrameEventArgs.displayMatrix ?? Matrix4x4.identity;

            Vector2 affineBasisX = new Vector2(1.0f, 0.0f);
            Vector2 affineBasisY = new Vector2(0.0f, 1.0f);
            Vector2 affineTranslation = new Vector2(0.0f, 0.0f);
            affineBasisX = new Vector2(cameraMatrix[0, 0], cameraMatrix[0, 1]);
            affineBasisY = new Vector2(cameraMatrix[1, 0], cameraMatrix[1, 1]);
            affineTranslation = new Vector2(cameraMatrix[0, 2], cameraMatrix[1, 2]);

            // The camera display matrix includes scaling and offsets to fit the aspect ratio of the device. In most
            // cases, the camera display matrix should be used directly without modification when applying depth to
            // the scene because that will line up the depth image with the camera image. However, for this demo,
            // we want to show the full depth image as a picture-in-picture, so we remove these scaling and offset
            // factors while preserving the orientation.
            affineBasisX = affineBasisX.normalized;
            affineBasisY = affineBasisY.normalized;
            m_DisplayRotationMatrix = Matrix4x4.identity;
            m_DisplayRotationMatrix[0, 0] = affineBasisX.x;
            m_DisplayRotationMatrix[0, 1] = affineBasisY.x;
            m_DisplayRotationMatrix[1, 0] = affineBasisX.y;
            m_DisplayRotationMatrix[1, 1] = affineBasisY.y;
            m_DisplayRotationMatrix[2, 0] = Mathf.Round(affineTranslation.x);
            m_DisplayRotationMatrix[2, 1] = Mathf.Round(affineTranslation.y);

#if UNITY_ANDROID
            m_DisplayRotationMatrix = k_AndroidFlipYMatrix * m_DisplayRotationMatrix;
#endif // UNITY_ANDROID

            // Set the matrix to the raw image material.
            m_RawImage.material.SetMatrix(k_DisplayRotationPerFrameId, m_DisplayRotationMatrix);
        }
    }
}
