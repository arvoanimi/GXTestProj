using UnityEngine;
using UnityEngine.Android;

/*
	Documentation: https://mirror-networking.gitbook.io/docs/components/network-manager
	API Reference: https://mirror-networking.com/docs/api/Mirror.NetworkManager.html
*/

namespace Mirror.Examples.Basic
{
    [AddComponentMenu("")]
    public class BasicNetManager : NetworkManager
    {

        public override void Start()
        {
            base.Start();
#if UNITY_ANDROID
        networkAddress = "72.14.182.130";
        StartClient();
#endif

        }

        /// <summary>
        /// Called on the server when a client adds a new player with NetworkClient.AddPlayer.
        /// <para>The default implementation for this function creates a new player object from the playerPrefab.</para>
        /// </summary>
        /// <param name="conn">Connection from client.</param>
        public override void OnServerAddPlayer(NetworkConnection conn)
        {
            base.OnServerAddPlayer(conn);
            Player.ResetPlayerNumbers();
        }

        /// <summary>
        /// Called on the server when a client disconnects.
        /// <para>This is called on the Server when a Client disconnects from the Server. Use an override to decide what should happen when a disconnection is detected.</para>
        /// </summary>
        /// <param name="conn">Connection from client.</param>
        public override void OnServerDisconnect(NetworkConnection conn)
        {
            base.OnServerDisconnect(conn);
            Player.ResetPlayerNumbers();
        }


        //public GameObject outsideCam;
        //public override void OnServerConnect(NetworkConnection conn)
        //{
        //    base.OnServerConnect(conn);

        //    Camera.main.enabled = false;
        //    outsideCam.SetActive(true);

        //}
    }
}
