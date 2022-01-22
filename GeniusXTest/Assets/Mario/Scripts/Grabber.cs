using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using UnityEngine.InputSystem;
using UnityEngine.XR.OpenXR.Samples.ControllerSample;

[RequireComponent(typeof(Rigidbody))]

public class Grabber : MonoBehaviour
{
    public bool isRightHand = true;
    public bool canPick = false;
    public bool isPicking = false;
    MultiPickableItem myMultiPickable;

    public void Start()
    {
        canPick = false;
        isPicking = false;
    }

    public void OnTriggerStay(Collider other)
    {
        if(other.GetComponent<MultiPickableItem>() && !isPicking)
        {
            myMultiPickable = other.GetComponent<MultiPickableItem>();

            canPick = true;
        }
    }

    public void OnTriggerExit(Collider other)
    {
        
        if (!isPicking)
        {
            myMultiPickable = null;
            canPick = true;
        }
    }

    [SerializeField]
    private InputActionReference m_ActionReference;
    public InputActionReference actionReference { get => m_ActionReference; set => m_ActionReference = value; }

    public void TryPick()
    {
        GetComponent<MeshRenderer>().material.color = Color.green;
        if (!myMultiPickable.GetComponent<NetworkIdentity>().hasAuthority)//connectionToClient != transform.parent.GetComponent<NetworkIdentity>().connectionToClient )//belongs to the server
        {
            transform.parent.GetComponent<Mirror.Examples.Basic.Player>().RequestAuth(myMultiPickable.GetComponent<NetworkIdentity>());//request it for my player
            Debug.Log("Requesting Auth");
        }
        else 
        {
            Debug.Log("MINE");
            if (canPick)
            {
                Debug.Log("Trying to pick");
                myMultiPickable.TryAddOwner(transform.parent.GetComponent<NetworkIdentity>(), isRightHand);
                isPicking = true;
                canPick = false;
            }
        }
    }

   

    public void TryRelease()
    {
        GetComponent<MeshRenderer>().material.color = Color.gray;
        if (isPicking)
        {
            myMultiPickable.TryReleaseOwner(transform.parent.GetComponent<NetworkIdentity>(), isRightHand);

            myMultiPickable = null;
            isPicking = false;
            canPick = true;

        }
    }

    public void Update()
    {
        if (actionReference != null && actionReference.action != null )
        {

            if(actionReference.action.ReadValue<float>() > 0)//if the grip is pressed
            {
                Debug.Log("Try Pick");
                TryPick();
            }
            else
            {
                //Debug.Log("Try release");
                TryRelease();
            }
        }

    }
}
