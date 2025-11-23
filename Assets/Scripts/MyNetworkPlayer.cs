using Cainos.PixelArtTopDown_Basic;
using Mirror;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

public class MyNetworkPlayer : NetworkBehaviour
{
    [Header("References")]
    [SerializeField] private TMP_Text usernameText;
    [SerializeField] private SpriteRenderer spriteRenderer;

    [Header("Player Info")]
    [SyncVar(hook = nameof(OnPlayerNameChanged)), SerializeField] private string playerName = "Missing Player Name";
    [SyncVar(hook = nameof(OnPlayerColorChanged)), SerializeField] private Color playerColor = Color.black;

    [Header("Local Player Events")]
    [SerializeField] UnityEvent OnLocalPlayer;

    private Coroutine blinkCoroutine;
    #region Server

    [Server]
    public void SetPlayerName(string newName)
    {
        playerName = newName;
        //RpcLogNewPlayerName(newName);
    }

    [Server]
    public void SetPlayerColor (Color newColor)
    {
        playerColor = newColor;
    }

    [Command, ContextMenu("Set Random Name")]
    private void CmdSetNameRandomly()
    {
        string[] randomNames = { "Alpha", "Bravo", "Charlie", "Delta", "Echo" };
        int randomIndex = Random.Range(0, randomNames.Length);

        // Sanitize the name if necessary (e.g., remove unwanted characters)
        if (randomNames[randomIndex].Length > 12)
        {
            randomNames[randomIndex] = randomNames[randomIndex].Substring(0, 12);
        }

        if (string.IsNullOrWhiteSpace(randomNames[randomIndex]))
        {
            randomNames[randomIndex] = "Player " + netId;
        }

        SetPlayerName(randomNames[randomIndex]);
        RpcLerpColorPingPong(3);
    }

    #endregion

    #region Client

    private void OnPlayerNameChanged(string oldName, string newName)
    {
        usernameText.text = newName;
    }

    private void OnPlayerColorChanged(Color oldColor, Color newColor)
    {
        spriteRenderer.color = newColor;
    }

    [ClientRpc]
    private void RpcLogNewPlayerName(string newName)
    {
        Debug.Log($"Player with NetId {netId} changed name to {newName}");
    }


    [ClientRpc]
    public void RpcLerpColorPingPong(float duration)
    {
        if (blinkCoroutine != null)
            StopCoroutine(blinkCoroutine);

        blinkCoroutine = StartCoroutine(LerpColorPingPongCoroutine(duration));
    }

    private IEnumerator LerpColorPingPongCoroutine(float duration)
    {
        float timer = 0f;
        Color originalColor = playerColor;          
        Color flashColor = Color.lightGreen;

        while (timer < duration)
        {
            float time = Mathf.PingPong(Time.time * 5f, 1f);
            spriteRenderer.color = Color.Lerp(originalColor, flashColor, time);

            timer += Time.deltaTime;
            yield return null;
        }

        spriteRenderer.color = originalColor;
        blinkCoroutine = null;
    }

    [TargetRpc]
    public void InvokeIsMineEvents()
    {
        if (isLocalPlayer)
        {
            // Set camera to follow this player
            CameraFollow cameraFollow = FindAnyObjectByType<CameraFollow>();
            if (cameraFollow != null)
            {
                cameraFollow.SetTarget(transform);
            }

            OnLocalPlayer?.Invoke();
        }
    }

    #endregion
}
