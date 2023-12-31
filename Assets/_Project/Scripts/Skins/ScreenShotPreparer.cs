using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScreenShotPreparer : MonoBehaviour
{
    [SerializeField] private Transform _transformToScale;
    [SerializeField] private DownloadFile _fileDownloader;

    public void DoScreenshot()
    {
        _transformToScale.localScale = new Vector3(1.3f, 1.3f, 1f);
        _fileDownloader.DoScreenshot();
        _transformToScale.localScale = new Vector3(1f, 1f, 1f);
    }
}
