using System;
using UnityEngine;
using PassthroughCameraSamples;

public class CaptureManager : MonoBehaviour
{
    public static CaptureManager Instance { get; private set; }

    [SerializeField] private SmallCamera m_smallCamera;
    public SmallCamera _smallCamera => m_smallCamera;
    [SerializeField] private CameraToQuad m_cameraToQuad;
    public CameraToQuad _cameraToQuad => m_cameraToQuad;
    [SerializeField] private QuadToWorld m_quadToWorld;
    public QuadToWorld _quadToWorld => m_quadToWorld;
    [SerializeField] private WebCamTextureManager m_webCamTextureManager;
    public WebCamTextureManager _webCamTextureManager => m_webCamTextureManager;
    public PassthroughCameraEye _cameraEye => m_webCamTextureManager.Eye;

    private bool m_isSnapshotTaken = false;
    public Action<Texture2D> _onSnapshotTaken;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    private void Update()
    {
        if(OVRInput.GetDown(OVRInput.Button.One))
        {
            m_isSnapshotTaken = !m_isSnapshotTaken;
            MakeCameraSnapshot(m_isSnapshotTaken);

            if(m_isSnapshotTaken)
            {
                //m_cameraToQuad.UpdateDebugText("snapshot taken");
            }
            else
            {
                m_cameraToQuad.UpdateDebugText("snapshot released");
            }
        }
    }

    public void MakeCameraSnapshot(bool isTaken)
    {
        m_quadToWorld.SnapshotTaken(isTaken);
        m_smallCamera.OnSnap(isTaken);
    }
}
