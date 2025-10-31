using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using P2PPlugin.Network;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

[DisallowMultipleComponent]
public class PeerLobbyScript : MonoBehaviour, P2PObject.PeerComputerUpdates
{
    static public PeerLobbyScript Instance;
    PeerLobbyScript() {
        Instance = this;
    }
    public GameObject localPeerNameGO;
    public GameObject lobbyPeerObjectPrefab;
    public GameObject lobbyPeerParent;

    public GameObject wantToPlayGO;
    public GameObject wantToPlayTextGO;

    public GameObject lobbyMessageGO;

    static LobbyPeer localLobbyPeerInstance = null;
    static public bool isPlaying = false;
    static private Dictionary<long, GameObject> RemoteLobbyPeersGO = new Dictionary<long, GameObject>();


    // Callbacks from P2PObject.PeerComputerUpdates
    public void changed(bool addOrRemoved, PeerComputerInfo peerInfo)
    {
        // already listen to LobbyPeer changes, so do nothing here
    }
    public void updateLocalPeerName(string newName)
    {

        TextMeshPro localPeerNameText = localPeerNameGO.GetComponent<TextMeshPro>();
        if (localPeerNameText != null)
        {
            localPeerNameText.text = newName;
        }
    }
    public string getPeerName(long peerID)
    {
        object peerObj;
        if (P2PObject.allPeerComputerIDs.TryGetValue(peerID, out peerObj))
        {
            PeerComputerInfo peerInfo = peerObj as PeerComputerInfo;
            return peerInfo.name;
        }
        return "";
    }
    public void updateRemotePeerName(long peerID, string name)
    {
        GameObject LobbyPeerGO;
        if (RemoteLobbyPeersGO.TryGetValue(peerID, out LobbyPeerGO))
        {
            PeerLobbyEntry peerLobbyEntry = LobbyPeerGO.GetComponent<PeerLobbyEntry>();
            if (peerLobbyEntry != null)
            {
                TextMeshProUGUI peerNameText = peerLobbyEntry.PeerNameTextGO.GetComponent<TextMeshProUGUI>();
                if (peerNameText != null)
                {
                    peerNameText.text = name;
                }
            }
        }
    }
    public void updateRemotePeerButtonText(long peerID, string name)
    {
        GameObject LobbyPeerGO;
        if (RemoteLobbyPeersGO.TryGetValue(peerID, out LobbyPeerGO))
        {
            if (LobbyPeerGO.IsDestroyed())
            {
                Debug.Log("ERROR: updateRemotePeerButtonText: Object is already destroyed: peerID: " + peerID);
                return;
            }
            PeerLobbyEntry peerLobbyEntry = LobbyPeerGO.GetComponent<PeerLobbyEntry>();
            if (peerLobbyEntry != null)
            {
                TextMeshProUGUI peerButton = peerLobbyEntry.RequestOrJoinButtonTextGO.GetComponent<TextMeshProUGUI>();
                if (peerButton != null)
                {
                    peerButton.text = name;
                }
            }
        }
    }

    public void showHideRemotePeerButton(long peerID, bool show)
    {
        GameObject LobbyPeerGO;
        if (RemoteLobbyPeersGO.TryGetValue(peerID, out LobbyPeerGO))
        {
            if (LobbyPeerGO.IsDestroyed())
            {
                Debug.Log("ERROR: showHideRemotePeerButton: Object is already destroyed: peerID: " + peerID);
                return;
            }
            PeerLobbyEntry peerLobbyEntry = LobbyPeerGO.GetComponent<PeerLobbyEntry>();
            if (peerLobbyEntry != null)
            {
                peerLobbyEntry.RequestOrJoinButtonGO.SetActive(show);
            }
        }
    }
    public void updated(PeerComputerInfo peerInfo)
    {

        if (peerInfo.isLocal)
        {
            updateLocalPeerName(peerInfo.name);
        }
        else
        {
            updateRemotePeerName(peerInfo.computerID, peerInfo.name);
        }
    }

    private int addP2PChangeKey = 0;
    private int peerChangeKey = 0;

    void Start()
    {
        Action<bool, LobbyPeer> changeListener = (addOrRemove, p2pIns) =>
        {
            if (addOrRemove)
            {
                GameObject lobbyPeerGO = Instantiate(lobbyPeerObjectPrefab);
                lobbyPeerGO.name = "LobbyPeer_" + p2pIns.sourceComputerID;

                RemoteLobbyPeersGO.Add(p2pIns.sourceComputerID, lobbyPeerGO);
                lobbyPeerGO.transform.SetParent(lobbyPeerParent.transform);
                RectTransform rt = lobbyPeerGO.GetComponent<RectTransform>();
                rt.anchoredPosition = new Vector2(0, 0);
                rt.localPosition = new Vector3(0, 0, 0);
                rt.localScale = new Vector3(1, 1, 1);
                object peerObj;
                if (P2PObject.allPeerComputerIDs.TryGetValue(p2pIns.sourceComputerID, out peerObj))
                {
                    PeerComputerInfo peerInfo = peerObj as PeerComputerInfo;
                    PeerLobbyEntry peerLobbyEntry = lobbyPeerGO.GetComponent<PeerLobbyEntry>();
                    if (peerLobbyEntry != null)
                    {
                        peerLobbyEntry.peerComputerID = p2pIns.sourceComputerID;
                        TextMeshProUGUI peerNameText = peerLobbyEntry.PeerNameTextGO.GetComponent<TextMeshProUGUI>();
                        if (peerNameText != null)
                        {
                            peerNameText.text = peerInfo.name;
                        }
                    }
                }
            }
            else
            {
                RemoveLobbyPeerGO(p2pIns.sourceComputerID);
            }
        };
        if (addP2PChangeKey == 0)
        {
            addP2PChangeKey = LobbyPeer.addP2PChangeListener(changeListener);        
        } else
        {
            Debug.Log("PeerLobbyScript called with addP2PChangeKey already set: " + addP2PChangeKey);
        }
        foreach (LobbyPeer lobbyPeer in LobbyPeer.RemoteLobbyPeers.Values)
        {
            changeListener(true, lobbyPeer);
        }
    }
    private void RemoveLobbyPeerGO(long peerID)
    {
        GameObject lobbyPeerObj;
        if (RemoteLobbyPeersGO.TryGetValue(peerID, out lobbyPeerObj))
        {
            RemoteLobbyPeersGO.Remove(peerID);
            Destroy(lobbyPeerObj);
        }
    }
    private void RemoveAllLobbyPeersGO()
    {
        if (addP2PChangeKey != 0)
        {
            LobbyPeer.removeP2PChangeListener(addP2PChangeKey);
            addP2PChangeKey = 0;
        }
        foreach (long peerID in RemoteLobbyPeersGO.Keys.ToArray<long>())
        {
            RemoveLobbyPeerGO(peerID);
        }
    }
    // Update is called once per frame
    void Update()
    {
        if (P2PObject.instantiated)
        {
            if (localLobbyPeerInstance == null)
            {
                localLobbyPeerInstance = new LobbyPeer();
                // Debug.Log("Created LobbyPeer instance for this computer with Peer ID: " + localLobbyPeerInstance.sourceComputerID + " and Creation Time: " + localLobbyPeerInstance.timeCreated + " isLocal: " + localLobbyPeerInstance.getIsLocal());
            }
            if (!localLobbyPeerInstance.getInserted() && !isPlaying)
            {
                if (peerChangeKey == 0)
                {
                    peerChangeKey = P2PObject.addPeerChangeListener(this);                
                }
                updateLocalPeerName(getPeerName(P2PObject.peerComputerID));
                localLobbyPeerInstance.Insert();
                // Debug.Log("Inserted Local LobbyPeer for this computer with Peer ID: " + localLobbyPeerInstance.sourceComputerID + " and Creation Time: " + localLobbyPeerInstance.timeCreated + " isLocal: " + localLobbyPeerInstance.getIsLocal());
            }
        }
    }
    IEnumerator CallLater(float delay, Action action)
    {
        yield return new WaitForSeconds(delay);
        action?.Invoke();
    }
    public void TurnOffLobbyMessage()
    {
        StartCoroutine(CallLater(3.0f, () =>
        {
            lobbyMessageGO.SetActive(false);
            Debug.Log("Called after 5 seconds on main thread!");
        }));
    }
    public void TurnOffLobbyMessageAndPlay(bool isPrimaryLeftPlayer = false, long fromPeerID = 0)
    {
        StartCoroutine(CallLater(3.0f, () =>
        {
            isPlaying = true;
            localLobbyPeerInstance.Delete();
            localLobbyPeerInstance.RequestedToPeer = 0;
            lobbyMessageGO.SetActive(false);
            RemoveAllLobbyPeersGO();
            if (peerChangeKey != 0)
            {
                P2PObject.removePeerChangeListener(peerChangeKey);
                peerChangeKey = 0;
            }
            SceneManager.LoadScene("Play");
            if (isPrimaryLeftPlayer)
            {
                PongGamePlayer.CreateLocalPongGamePlayer(fromPeerID);
            }
        }));
    }

    public void CancelRequestToPeerToPlay(long fromPeerID)
    {
        lobbyMessageGO.GetComponent<TextMeshProUGUI>().text = "Your request to " + getPeerName(fromPeerID) + " was declined.";
        lobbyMessageGO.SetActive(true);
        TurnOffLobbyMessage();

        UpdateRemotePeerButtons();
    }
    public void TurnOffAllLobbyButtons()
    {
        foreach (long peerID in RemoteLobbyPeersGO.Keys)
        {
            showHideRemotePeerButton(peerID, false);
        }
    }
    public void AcceptedRequestToPeerToPlay(long fromPeerID)
    {
        lobbyMessageGO.GetComponent<TextMeshProUGUI>().text = "Your request to " + getPeerName(fromPeerID) + " was accepted, get ready to play!";
        lobbyMessageGO.SetActive(true);
        TurnOffLobbyMessageAndPlay(true, fromPeerID);

        TurnOffAllLobbyButtons();
    }
    public void CancelRequestFromPeerToPlay()
    {
        requestingFromPeerID = 0;
        UpdateRemotePeerButtons();
        wantToPlayGO.SetActive(false);
    }
    public void RequestFromPeerToPlay(long peerComputerID)
    {
        requestingFromPeerID = peerComputerID;
        UpdateRemotePeerButtons();
        lobbyMessageGO.SetActive(false);
        wantToPlayTextGO.GetComponent<TextMeshProUGUI>().text = getPeerName(peerComputerID) + " is requesting, do you want to play?";
        wantToPlayGO.SetActive(true);
    }

    public void RequestToPlayWithPeer(long peerComputerID)
    {

        if (localLobbyPeerInstance.RequestedToPeer == peerComputerID)
        {
            // unrequest
            localLobbyPeerInstance.RequestedToPeer = 0;
        }
        else
        {
            localLobbyPeerInstance.RequestedToPeer = peerComputerID;
        }
        UpdateRemotePeerButtons();
    }

    long requestingFromPeerID = 0;
    public void LocalPeerAcceptedRequest()
    {
        wantToPlayGO.SetActive(false);
        lobbyMessageGO.GetComponent<TextMeshProUGUI>().text = "Your accepted the request to play with " + getPeerName(requestingFromPeerID) + ", get ready to play!";
        lobbyMessageGO.SetActive(true);
        TurnOffLobbyMessageAndPlay(false);
        foreach (long peerID in RemoteLobbyPeersGO.Keys)
        {
            showHideRemotePeerButton(peerID, false);
        }
        LobbyPeer lobbyPeer;
        if (LobbyPeer.RemoteLobbyPeers.TryGetValue(requestingFromPeerID, out lobbyPeer))
        {
            lobbyPeer.AcceptedRequestInvoke(P2PObject.peerComputerID);
        }
        requestingFromPeerID = 0;
    }
    public void LocalPeerDeclinedRequest()
    {
        wantToPlayGO.SetActive(false);
        LobbyPeer lobbyPeer;
        if (LobbyPeer.RemoteLobbyPeers.TryGetValue(requestingFromPeerID, out lobbyPeer))
        {
            lobbyPeer.DeclineRequestInvoke(P2PObject.peerComputerID);
        }
        requestingFromPeerID = 0;
        UpdateRemotePeerButtons();
    }
    public void UpdateRemotePeerButtons()
    {
        if (requestingFromPeerID != 0){
            // if a peer is requesting, then turn all buttons off
            foreach (long peerID in RemoteLobbyPeersGO.Keys){
                showHideRemotePeerButton(peerID, false);
            }
        } else if (localLobbyPeerInstance.RequestedToPeer != 0){
            // if this peer is requesting to play, then turn all off except the requested and set text to "Requested"
            foreach (long peerID in RemoteLobbyPeersGO.Keys){
                bool requesting = peerID == localLobbyPeerInstance.RequestedToPeer;
                if (requesting){
                    // set text to "Requested"
                    updateRemotePeerButtonText(peerID, "Requested");
                    showHideRemotePeerButton(peerID, true);
                } else {
                    showHideRemotePeerButton(peerID, false);
                }
            }
        } else
        {
            // all remote peers that aren't being requested or requesting should be shown
            HashSet<long> peersThatCantBeRequested = new HashSet<long>();
            foreach (LobbyPeer lobbyPeer in LobbyPeer.RemoteLobbyPeers.Values)
            {
                if (lobbyPeer.RequestedToPeer != 0)
                {
                    peersThatCantBeRequested.Add(lobbyPeer.RequestedToPeer);
                    peersThatCantBeRequested.Add(lobbyPeer.sourceComputerID);
                }
            }
            foreach (long peerID in RemoteLobbyPeersGO.Keys){
                if (peersThatCantBeRequested.Contains(peerID)) {
                    // set text to "Requested"
                    showHideRemotePeerButton(peerID, false);
                } else {
                    updateRemotePeerButtonText(peerID, "Request");
                    showHideRemotePeerButton(peerID, true);
                }
            }
        }
    }
}