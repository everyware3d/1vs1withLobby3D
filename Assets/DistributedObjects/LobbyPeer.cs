using System;
using System.Collections.Generic;
using P2PPlugin.Network;
using UnityEngine;

public class LobbyPeer : P2PNetworkObject
{

    public LobbyPeer()
    {
    }
    [P2PDistributed]
    public long timeCreated = DateTimeOffset.Now.ToUnixTimeMilliseconds(); //only set for source, then distributed
    [P2PDistributed]
    public long requestedFromPeerComputerID = -1;


    static private Dictionary<int, Action<bool, LobbyPeer>> LobbyPeerChanged = new Dictionary<int, Action<bool, LobbyPeer>>();
    static private int maxKeyForPeers = 0;
    static public void fireLobbyPeerChanged(bool addOrRemoved, LobbyPeer LobbyPeer)
    {
        foreach (Action<bool, LobbyPeer> callback in LobbyPeerChanged.Values)
        {
            callback(addOrRemoved, LobbyPeer);
        }
    }
    static public int addP2PChangeListener(Action<bool, LobbyPeer> callback)
    {
        int key = ++maxKeyForPeers;
        LobbyPeerChanged.Add(key, callback);
        return key;
    }
    static public int removeP2PChangeListener(int key)
    {
        LobbyPeerChanged.Remove(key);
        return key;
    }

    public void AfterInsertRemote()
    {
        fireLobbyPeerChanged(true, this);
    }
    public void AfterDeleteRemote()
    {
        fireLobbyPeerChanged(false, this);
    }
}