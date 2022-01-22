using System.Collections.Generic;
using UnityEngine;

namespace Mirror.Examples.Basic
{
    public class Player : NetworkBehaviour
    {
        // Events that the PlayerUI will subscribe to
        public event System.Action<byte> OnPlayerNumberChanged;
        public event System.Action<Color32> OnPlayerColorChanged;
        public event System.Action<ushort> OnPlayerDataChanged;

        // Players List to manage playerNumber
        static readonly List<Player> playersList = new List<Player>();

        [Header("Player UI")]
        public GameObject playerUIPrefab;
        GameObject playerUIObject;

        #region SyncVars

        [Header("SyncVars")]

        /// <summary>
        /// This is appended to the player name text, e.g. "Player 01"
        /// </summary>
        [SyncVar(hook = nameof(PlayerNumberChanged))]
        public byte playerNumber = 0;

        /// <summary>
        /// Random color for the playerData text, assigned in OnStartServer
        /// </summary>
        [SyncVar(hook = nameof(PlayerColorChanged))]
        public Color32 playerColor = Color.white;

        /// <summary>
        /// This is updated by UpdateData which is called from OnStartServer via InvokeRepeating
        /// </summary>
        [SyncVar(hook = nameof(PlayerDataChanged))]
        public ushort playerData = 0;

        // This is called by the hook of playerNumber SyncVar above
        void PlayerNumberChanged(byte _, byte newPlayerNumber)
        {
            OnPlayerNumberChanged?.Invoke(newPlayerNumber);
        }

        // This is called by the hook of playerColor SyncVar above
        void PlayerColorChanged(Color32 _, Color32 newPlayerColor)
        {
            OnPlayerColorChanged?.Invoke(newPlayerColor);
        }

        // This is called by the hook of playerData SyncVar above
        void PlayerDataChanged(ushort _, ushort newPlayerData)
        {
            OnPlayerDataChanged?.Invoke(newPlayerData);
        }

        #endregion

        #region Server

        /// <summary>
        /// This is invoked for NetworkBehaviour objects when they become active on the server.
        /// <para>This could be triggered by NetworkServer.Listen() for objects in the scene, or by NetworkServer.Spawn() for objects that are dynamically created.</para>
        /// <para>This will be called for objects on a "host" as well as for object on a dedicated server.</para>
        /// </summary>
        public override void OnStartServer()
        {
            base.OnStartServer();

            // Add this to the static Players List
            playersList.Add(this);

            // set the Player Color SyncVar
            playerColor = Random.ColorHSV(0f, 1f, 0.9f, 0.9f, 1f, 1f);

            // set the initial player data
            playerData = (ushort)Random.Range(100, 1000);

            // Start generating updates
            InvokeRepeating(nameof(UpdateData), 1, 1);
        }

        // This is called from BasicNetManager OnServerAddPlayer and OnServerDisconnect
        // Player numbers are reset whenever a player joins / leaves
        [ServerCallback]
        internal static void ResetPlayerNumbers()
        {
            byte playerNumber = 0;
            foreach (Player player in playersList)
                player.playerNumber = playerNumber++;
        }

        // This only runs on the server, called from OnStartServer via InvokeRepeating
        [ServerCallback]
        void UpdateData()
        {
            playerData = (ushort)Random.Range(100, 1000);
        }

        /// <summary>
        /// Invoked on the server when the object is unspawned
        /// <para>Useful for saving object data in persistent storage</para>
        /// </summary>
        public override void OnStopServer()
        {
            CancelInvoke();
            playersList.Remove(this);
        }

        #endregion

        #region Client

        /// <summary>
        /// Called when the local player object has been set up.
        /// <para>This happens after OnStartClient(), as it is triggered by an ownership message from the server. This is an appropriate place to activate components or functionality that should only be active for the local player, such as cameras and input.</para>
        /// </summary>
        public override void OnStartLocalPlayer()
        {
            // Activate the main panel
            CanvasUI.instance.mainPanel.gameObject.SetActive(true);
        }

        /// <summary>
        /// Called on every NetworkBehaviour when it is activated on a client.
        /// <para>Objects on the host have this function called, as there is a local client on the host. The values of SyncVars on object are guaranteed to be initialized correctly with the latest state from the server when this function is called on the client.</para>
        /// </summary>
        public override void OnStartClient()
        {
            // Instantiate the player UI as child of the Players Panel
            playerUIObject = Instantiate(playerUIPrefab, CanvasUI.instance.playersPanel);
            PlayerUI playerUI = playerUIObject.GetComponent<PlayerUI>();

            // wire up all events to handlers in PlayerUI
            OnPlayerNumberChanged = playerUI.OnPlayerNumberChanged;
            OnPlayerColorChanged = playerUI.OnPlayerColorChanged;
            OnPlayerDataChanged = playerUI.OnPlayerDataChanged;

            // Invoke all event handlers with the initial data from spawn payload
            OnPlayerNumberChanged.Invoke(playerNumber);
            OnPlayerColorChanged.Invoke(playerColor);
            OnPlayerDataChanged.Invoke(playerData);

            // Set isLocalPlayer for this Player in UI for background shading
            if (isLocalPlayer)
                playerUI.SetLocalPlayer();

            if (isLocalPlayer)
            {
                hTarget = GameObject.Find("Head").transform;
                lhTarget = GameObject.Find("AimLH").transform;
                rhTarget = GameObject.Find("AimRH").transform;
            }
        }

        /// <summary>
        /// This is invoked on clients when the server has caused this object to be destroyed.
        /// <para>This can be used as a hook to invoke effects or do client specific cleanup.</para>
        /// </summary>
        public override void OnStopClient()
        {
            // disconnect event handlers
            OnPlayerNumberChanged = null;
            OnPlayerColorChanged = null;
            OnPlayerDataChanged = null;

            // Remove this player's UI object
            Destroy(playerUIObject);

            // Disable the main panel for local player
            if (isLocalPlayer)
                CanvasUI.instance.mainPanel.gameObject.SetActive(false);
        }

        #endregion

        public Transform head, lh, rh;
        public Transform hTarget, lhTarget, rhTarget;

        public void Start()
        {
            //if(isLocalPlayer)
            //{
            //    hTarget = GameObject.Find("Head").transform;
            //    lhTarget = GameObject.Find("AimLH").transform;
            //    rhTarget = GameObject.Find("AimRH").transform;
            //}
        }

        public void Update()
        {
            if (isLocalPlayer)
            {
                transform.position = hTarget.position;
                transform.rotation = hTarget.rotation;

                lh.position = lhTarget.position;
                lh.rotation = lhTarget.rotation;

                rh.position = rhTarget.position;
                rh.rotation = rhTarget.rotation;
            }
        }

        //REQUEST AUTH for an Item
        public void RequestAuth(NetworkIdentity itemID)
        {
            CmdSetAuth(itemID);
        }

        [Command]
        public void CmdSetAuth(NetworkIdentity item)
        {
            NetworkIdentity myID = GetComponent<NetworkIdentity>();

            if (myID.connectionToClient == item.connectionToClient)
            {
                return;
            }
            else
            {
                if (item.connectionToClient != null)
                {
                    item.RemoveClientAuthority();
                }
                item.AssignClientAuthority(myID.connectionToClient);
            }
        }
    }
}