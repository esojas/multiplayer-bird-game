using System.Collections;
using Unity.Netcode;
using UnityEngine;

public class NetworkTimerManager : NetworkBehaviour
{
    private NetworkVariable<float> timeRemaining = new NetworkVariable<float>
        (writePerm: NetworkVariableWritePermission.Server);

    public event System.Action OnTimerEnd;
    public event System.Action<float> OnTimerUpdate;

    public void StartTimer(float duration)
    {
        if (!IsServer) return;
        Debug.Log($"StartTimer called with duration: {duration}");
        timeRemaining.Value = duration;
    }

    public override void OnNetworkSpawn()
    {
        Debug.Log($"NetworkTimerManager OnNetworkSpawn — IsServer: {IsServer}, timeRemaining: {timeRemaining.Value}");

        timeRemaining.OnValueChanged += (oldVal, newVal) =>
        {
            //Debug.Log($"OnValueChanged fired — old: {oldVal}, new: {newVal}");
            OnTimerUpdate?.Invoke(newVal);
        };
    }

    public float GetCurrentTime() => timeRemaining.Value;

    void Update()
    {
        if (!IsServer) return;
        if (timeRemaining.Value <= 0) return;

        timeRemaining.Value -= Time.deltaTime;

        if (timeRemaining.Value <= 0)
        {
            timeRemaining.Value = 0;
            OnTimerEnd?.Invoke();
        }
    }
}
