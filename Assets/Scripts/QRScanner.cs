using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using ZXing;

public class QRScanner : MonoBehaviour
{
    [SerializeField]
    private RawImage _rawImageBack;
    [SerializeField]
    private AspectRatioFitter _aspectRatioFitter;
    [SerializeField]
    private TextMeshProUGUI _textOut;
    [SerializeField]
    private RectTransform _scanZone;

    private bool _isCamAvailable;
    private WebCamTexture _cameratexture;

    // Start is called before the first frame update
    void Start()
    {
        if (Application.platform == RuntimePlatform.Android)
        {
            if (!UnityEngine.Android.Permission.HasUserAuthorizedPermission(UnityEngine.Android.Permission.Camera))
            {
                UnityEngine.Android.Permission.RequestUserPermission(UnityEngine.Android.Permission.Camera);
            }
        }

        SetUpCamera();
    }

    // Update is called once per frame
    void Update()
    {
        UpdateCameraRender();
        if (_isCamAvailable)
        {
            Scan();
        }
    }

    private void SetUpCamera()
    {
        WebCamDevice[] devices = WebCamTexture.devices;
        if (devices.Length == 0)
        {
            Debug.LogWarning("No camera devices found");
            _isCamAvailable = false;
            return;
        }

        for (int i = 0; i < devices.Length; i++)
        {
            if (devices[i].isFrontFacing == false)
            {
                _cameratexture = new WebCamTexture(devices[i].name, (int)_scanZone.rect.width, (int)_scanZone.rect.height);
                break;
            }
        }

        if (_cameratexture == null)
        {
            Debug.LogWarning("No suitable camera found");
            _isCamAvailable = false;
            return;
        }

        _cameratexture.Play();
        _rawImageBack.texture = _cameratexture;
        _isCamAvailable = true;
        Debug.Log("Camera setup complete");
    }

    private void UpdateCameraRender()
    {
        if (!_isCamAvailable)
        {
            return;
        }

        float ratio = (float)_cameratexture.width / (float)_cameratexture.height;
        _aspectRatioFitter.aspectRatio = ratio;

        int orientation = -_cameratexture.videoRotationAngle;
        _rawImageBack.rectTransform.localEulerAngles = new Vector3(0, 0, orientation);
    }

    private void Scan()
    {
        try
        {
            IBarcodeReader barcodeReader = new BarcodeReader();
            Result result = barcodeReader.Decode(_cameratexture.GetPixels32(), _cameratexture.width, _cameratexture.height);
            if (result != null)
            {
                _textOut.text = result.Text;
            }
        }
        catch
        {
            _textOut.text = "Failed in try";
        }
    }
}
