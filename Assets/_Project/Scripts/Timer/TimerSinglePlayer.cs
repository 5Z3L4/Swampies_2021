using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class TimerSinglePlayer : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _timerText;
    private float _startTime;
    private bool _isRunStarted;
    private float _timeInSeconds;

    private List<float> _times = new();

    //Timer display
    private float _minutes, _seconds;

    public float TimeInSeconds => _timeInSeconds;
    public List<float> Times => _times;

    private void Awake()
    {
        //_timerText = GameObject.Find("TimerText").GetComponent<TextMeshProUGUI>();   
    }
    private void OnEnable()
    {
        StartRun.RunStart += On_RunStart;
        FinishSinglePlayer.RunFinish += On_RunFinish;
    }

    private void OnDisable()
    {
        FinishSinglePlayer.RunFinish -= On_RunFinish;
        StartRun.RunStart -= On_RunStart;
    }

    private void Update()
    {
        if (!_isRunStarted) return;
        _timeInSeconds = Time.time - _startTime;
        _minutes = (int)(_timeInSeconds / 60f);
        _seconds = (int)(_timeInSeconds % 60f);
        _timerText.SetText(_minutes.ToString("00") + ":" + _seconds.ToString("00"));
    }

    private void On_RunStart()
    {
        _startTime = Time.time;
        _timerText.gameObject.SetActive(true);
        _isRunStarted = true;
    }

    private void On_RunFinish(float newScore)
    {
        _isRunStarted = false;
        _times.Add(_timeInSeconds);
    }
    public void ChangeRunStartedBool(bool value)
    {
        _isRunStarted = value;
    }
}