using System.Collections;
using TMPro;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;

public class GameTimer : NetworkBehaviour
{
    public static GameTimer Instance { get; private set; }
    [SerializeField] private TextMeshProUGUI timerText;
    [SerializeField] private float gameDuration;
    public event System.Action OnGameOver;
    private bool gameOverFired = false;

    // ✅ NetworkVariable lives HERE not in a separate script
    private NetworkVariable<float> timeRemaining = new NetworkVariable<float>
        (writePerm: NetworkVariableWritePermission.Server);

    private void Awake() => Instance = this;

    private void Start()
    {
        SoundManager.PlayTrack(1);
        SoundManager.PlayAmbience();
    }

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            timeRemaining.Value = gameDuration;
        }

        // ✅ Same pattern as GameOverManager - subscribe on all clients via ClientRpc
        InitTimerClientRpc();
    }

    [ClientRpc]
    private void InitTimerClientRpc()
    {
        // ✅ Subscribe to NetworkVariable changes on all clients
        timeRemaining.OnValueChanged += (old, newVal) => UpdateTimerDisplay(newVal);

        // ✅ Render current value immediately (won't fire OnValueChanged retroactively)
        UpdateTimerDisplay(timeRemaining.Value);
    }

    void Update()
    {
        if (!IsServer) return;

        if (timeRemaining.Value > 0)
        {
            timeRemaining.Value -= Time.deltaTime;
        }
        else if (!gameOverFired)
        {
            gameOverFired = true;
            timeRemaining.Value = 0;
            TriggerGameOverClientRpc();
            OnGameOver?.Invoke();
        }
    }

    [ClientRpc]
    private void TriggerGameOverClientRpc()
    {
        // ✅ Unsubscribe to avoid ghost callbacks
        timeRemaining.OnValueChanged -= (old, newVal) => UpdateTimerDisplay(newVal);
        timerText.text = "00:00";
    }

    public void StopTimer()
    {
        if (!IsServer) return;
        gameOverFired = true;
        timeRemaining.Value = 0;
        StopTimerClientRpc();
    }

    [ClientRpc]
    private void StopTimerClientRpc()
    {
        timerText.text = "00:00";
    }

    private void UpdateTimerDisplay(float time)
    {
        if (timerText == null) return;
        int minutes = Mathf.FloorToInt(time / 60);
        int seconds = Mathf.FloorToInt(time % 60);
        timerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
    }

    public float GetCurrentTime() => timeRemaining.Value;

}