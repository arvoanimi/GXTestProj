using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(Collider))]
[RequireComponent(typeof(NetworkIdentity))]
public class MultiPickableItem : NetworkBehaviour
{
    
    public Transform owner;

    //ADD OWNER

    public void TryAddOwner(NetworkIdentity ownerNI, bool isRightHand)
    {
        Debug.Log("trying to send the info");
        CmdAddOwner(ownerNI, isRightHand);
    }

    [Command]
    public void CmdAddOwner(NetworkIdentity ownerNI, bool isRightHand)
    {
        if (isServer)
        {
            Debug.Log("got the info");
        }
        GetComponent<NetworkTransform>().clientAuthority = true;
        RpcAddOwner(ownerNI, isRightHand);
    }

    [ClientRpc]
    public void RpcAddOwner(NetworkIdentity ownerNI, bool isRightHand)
    {
        GetComponent<NetworkTransform>().clientAuthority = true;
        Debug.Log("add the transform on the client");
        Transform ownerHand = isRightHand ? ownerNI.GetComponent<Mirror.Examples.Basic.Player>().rh : ownerNI.GetComponent<Mirror.Examples.Basic.Player>().lh;

        if (owner == null)
        {
            owner = ownerHand;
            //transform.parent = owner;
        }
    }

    //RELEASE
    public void TryReleaseOwner(NetworkIdentity ownerNI, bool isRightHand)
    {
        Debug.Log("Calling Remove");
        CmdReleaseOwner(ownerNI, isRightHand);
    }

    [Command]
    public void CmdReleaseOwner(NetworkIdentity ownerNI, bool isRightHand)
    {
        GetComponent<NetworkIdentity>().RemoveClientAuthority();

        Transform ownerHand = isRightHand ? ownerNI.GetComponent<Mirror.Examples.Basic.Player>().rh : ownerNI.GetComponent<Mirror.Examples.Basic.Player>().lh;

        if (owner == ownerHand)
        {
            
            GetComponent<Rigidbody>().isKinematic = false; 
            GetComponent<NetworkTransform>().clientAuthority = false;
            owner = null;
            //transform.parent = null;
            
            
        }

        RpcRemoveOwner(ownerNI, isRightHand);
        
    }

    [ClientRpc]
    public void RpcRemoveOwner(NetworkIdentity ownerNI, bool isRightHand)
    {
        Transform ownerHand = isRightHand ? ownerNI.GetComponent<Mirror.Examples.Basic.Player>().rh : ownerNI.GetComponent<Mirror.Examples.Basic.Player>().lh;

        if (owner == ownerHand)
        {

            GetComponent<Rigidbody>().isKinematic = false;
            GetComponent<NetworkTransform>().clientAuthority = false;
            owner = null;
            //transform.parent = null;

        }
    }

    private void Update()
    {
        if (hasAuthority)
        {
            Debug.Log("Item has auth");
            if (owner != null)//s.Count > 0)
            {
                GetComponent<Rigidbody>().isKinematic = true;
                //Vector3 initialPos = owners[0].position;

                //if (owners.Count > 1)
                //{
                //    for (int i = 0; i < owners.Count - 1; i++)
                //    {
                //        initialPos = initialPos + (owners[i + 1].position - initialPos) / 2;//getting the avg position of all owners combined
                //    }
                //}

                //transform.position = initialPos;
                transform.position = owner.position;
                
            }
            else
            {
                GetComponent<Rigidbody>().isKinematic = false;
            }
        }
        else
        {
            Debug.Log("don't have auth");
        }
    }

    public void OnTriggerStay(Collider other)
    {
        if(other.transform.root.GetComponent<NetworkIdentity>() != null)
        {
            GetComponent<MeshRenderer>().material.color = Color.green;
        }
    }

    public void OnTriggerExit(Collider other)
    {
        if (other.transform.root.GetComponent<NetworkIdentity>() != null)
        {
            GetComponent<MeshRenderer>().material.color = Color.white;
        }
    }
}
