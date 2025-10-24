using System.Collections.Generic;
using P2PPlugin.Network;
using TMPro;
using UnityEngine;

public class PeerLobbyScript : MonoBehaviour, P2PObject.PeerComputerUpdates
{
    public enum PeerLobbyStateEnum
    {
        CreationTime = 0,
        //        PeerID
    }

    public GameObject localPeerNameGO;
    public GameObject lobbyPeerObjectPrefab;
    public GameObject lobbyPeerParent;

    static LobbyPeer localLobbyPeerInstance = null;
    static private Dictionary<long, GameObject> RemoteLobbyPeers = new Dictionary<long, GameObject>();


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
    public void updated(PeerComputerInfo peerInfo)
    {
        GameObject LobbyPeerGO;
        if (peerInfo.isLocal){
            updateLocalPeerName(peerInfo.name);
        } else if (RemoteLobbyPeers.TryGetValue(peerInfo.computerID, out LobbyPeerGO))
        {
            PeerLobbyEntryComponents peerLobbyEntryComponents = LobbyPeerGO.GetComponent<PeerLobbyEntryComponents>();
            if (peerLobbyEntryComponents != null)
            {
                TextMeshProUGUI peerNameText = peerLobbyEntryComponents.PeerNameTextGO.GetComponent<TextMeshProUGUI>();
                if (peerNameText != null)
                {
                    peerNameText.text = peerInfo.name;
                }
            }
        }
    }
    

    void Start()
    {
        LobbyPeer.addP2PChangeListener((addOrRemove, p2pIns) =>
        {
            if (addOrRemove)
            {
                GameObject lobbyPeerGO = Instantiate(lobbyPeerObjectPrefab);
                lobbyPeerGO.name = "LobbyPeer_" + p2pIns.sourceComputerID;
                RemoteLobbyPeers.Add(p2pIns.sourceComputerID, lobbyPeerGO);
                lobbyPeerGO.transform.SetParent(lobbyPeerParent.transform);
                RectTransform rt = lobbyPeerGO.GetComponent<RectTransform>();
                rt.anchoredPosition = new Vector2(0, 0);
                rt.localPosition = new Vector3(0, 0, 0);
                rt.localScale = new Vector3(1, 1, 1);
                object peerObj;
                if (P2PObject.allPeerComputerIDs.TryGetValue(p2pIns.sourceComputerID, out peerObj))
                {
                    PeerComputerInfo peerInfo = peerObj as PeerComputerInfo;
                    PeerLobbyEntryComponents peerLobbyEntryComponents = lobbyPeerGO.GetComponent<PeerLobbyEntryComponents>();
                    if (peerLobbyEntryComponents != null)
                    {
                        TextMeshProUGUI peerNameText = peerLobbyEntryComponents.PeerNameTextGO.GetComponent<TextMeshProUGUI>();
                        if (peerNameText != null)
                        {
                            peerNameText.text = peerInfo.name;
                        }
                    }
                }
            }
            else
            {
                if (RemoteLobbyPeers.ContainsKey(p2pIns.sourceComputerID))
                {
                    GameObject lobbyPeerGO = RemoteLobbyPeers[p2pIns.sourceComputerID];
                    RemoteLobbyPeers.Remove(p2pIns.sourceComputerID);
                    Destroy(lobbyPeerGO);
                }
            }
        });
    }

    // Update is called once per frame
    void Update()
    {
        if (P2PObject.instantiated)
        {
            if (localLobbyPeerInstance == null)
            {
                P2PObject.addPeerChangeListener(this);
                localLobbyPeerInstance = new LobbyPeer();

                object peerObj;
                if (P2PObject.allPeerComputerIDs.TryGetValue(P2PObject.peerComputerID, out peerObj))
                {
                    PeerComputerInfo peerInfo = peerObj as PeerComputerInfo;
                    updateLocalPeerName(peerInfo.name);
                }
                Debug.Log("Created LobbyPeer instance for this computer with Peer ID: " + localLobbyPeerInstance.sourceComputerID + " and Creation Time: " + localLobbyPeerInstance.timeCreated + " isLocal: " + localLobbyPeerInstance.getIsLocal()); 
            }
            if (!localLobbyPeerInstance.getInserted())
            {
                localLobbyPeerInstance.Insert();
                Debug.Log("Inserted Local LobbyPeer for this computer with Peer ID: " + localLobbyPeerInstance.sourceComputerID + " and Creation Time: " + localLobbyPeerInstance.timeCreated + " isLocal: " + localLobbyPeerInstance.getIsLocal());
            }
        }        
    }
}
