using DG.Tweening;
using System.Collections;
using UnityEngine;

public class Respawn : MonoBehaviour 
{
    [SerializeField] private Transform _respawnPos;
    private Transform _startPos;
    public Transform StartPos => _startPos;
    private float _timeStartedPenalty;
    private CheckPoint _lastCheckPoint;
    private bool _speedrunModeEnabled;


    private void Start()
    {
        FinishSinglePlayer.RunFinish += EndRun;
        StartRun.RunStart += RunStart;
        _startPos = _respawnPos;
        ToggleSpeedrunMode.SpeedrunModeToggle += OnSpeedrunModeToggle;
        if (PlayerPrefs.HasKey("SpeedrunMode"))
        {
            _speedrunModeEnabled = PlayerPrefs.GetInt("SpeedrunMode") == 1;
        }
    }
    private void Update()
    {
        if (_speedrunModeEnabled)
        {
            if (Input.GetKeyDown(KeyCode.R))
            {
                FinishPanelManagement.RestartPlayer();
            }
        }
    }

    private void OnDisable()
    {
        FinishSinglePlayer.RunFinish -= EndRun;
        StartRun.RunStart -= RunStart;
    }

    public void ChangeSpawnPos(Transform newPos, CheckPoint checkPoint)
    {
        if (_lastCheckPoint is not null)
        {
            _lastCheckPoint.ResetCheckPoint();
        }
        _respawnPos = newPos;
        _lastCheckPoint = checkPoint;
    }
    private void EndRun(float newScore) => _respawnPos = _startPos;
    private void RunStart() => _respawnPos = _startPos;

    public IEnumerator RespawnPlayer(Transform player, float penaltyTime = 0) 
    {
        yield return new WaitForSecondsRealtime(penaltyTime);
        
        player.DOMove(_respawnPos.position, 0).OnComplete(() => { player.GetComponent<IPawnController>().RespawnPlayer(); });
    }


    private void OnDrawGizmosSelected() 
    {
        Gizmos.color = Color.green;
        Gizmos.DrawSphere(_respawnPos.position, 0.5f);
    }
    private void OnSpeedrunModeToggle(bool isOn)
    {
        _speedrunModeEnabled = isOn;
    }
}
