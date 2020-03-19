﻿// Jack 16/02/2020 - Abstracted salt system from FirstPersonController
// Jack 23/02/2020 - Added interaction with books in Interact()
// Morgan 03/03/2020 - added door interaction & tag
// Louie 10/02/2020 - Added interaction with rabbit
// Jack 19/03/2020 increased throw force

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Deals with picking up, moving and throwing objects as well as any other item interactions done with a raycast.
/// </summary>
public class Interaction_Jack : MonoBehaviour
{
    public Camera playerCamera;
    public CharacterController characterController;

    // Picking up objects
    private readonly LayerMask moveableObjectsLayer = 1 << 8;
    private const ushort interactDistance = 10;
    private GameObject heldObject = null;
    private Collider heldObjectCollider = null;
    private Rigidbody heldObjectRigidbody = null;
    private const RigidbodyConstraints freezeRotationConstraints = RigidbodyConstraints.FreezeRotation;
    private RigidbodyConstraints heldObjectConstraints = RigidbodyConstraints.None;
    private const float dropObjectDist = 20f;
    private const float objectMoveDeadZone = 0.3f;
    private const float objectMoveSpeed = 8f;
    private readonly Vector3 zeroVector3 = new Vector3(0, 0, 0);

    // Throwing objects
    private const float throwForce = 20f;

    // Interactable objects
    private readonly LayerMask interactableObjectsLayer = 1 << 9;
    private const string tabletSlotTag = "Tablet Slot";
    private const string bookTag = "Book";
    private const string doorTag = "Door";
    private const string alcoholPlacementTag = "Alcohol Placement";
    private const string pianoKeysTag = "Piano Keys";
    private const string rabbitTag = "Rabbit";
    private const string valveTag = "Valve";

    public KeyUIManager_Jack keyManagerScript;

    // Start is called before the first frame update
    void Start()
    {
        if(!playerCamera)
        {
            playerCamera = GetComponent<Camera>();
        }

        if(!characterController)
        {
            characterController = GetComponent<CharacterController>();
        }

        if(!keyManagerScript)
        {
            keyManagerScript = FindObjectOfType<KeyUIManager_Jack>();
        }
    }

    void FixedUpdate()
    {
        // Held object
        if (heldObject)
        {
            Vector3 targetPosition = playerCamera.transform.position + playerCamera.transform.forward * 2f;
            Vector3 distanceToTarget = targetPosition - heldObject.transform.position;
            if (distanceToTarget.sqrMagnitude > dropObjectDist)
            {
                DropObject();
            }
            else if (distanceToTarget.sqrMagnitude > objectMoveDeadZone)
            {
                distanceToTarget.Normalize();
                heldObjectRigidbody.velocity = distanceToTarget * objectMoveSpeed;
            }
            else
            {
                heldObjectRigidbody.velocity = zeroVector3;
            }
        }
    }

    /// Interacts with anything in front of the player.
    /// 
    /// Fires a ray forward from the camera. If it hits an interactable object
    /// it will be activated.
    public void Interact()
    {
        if (Physics.Raycast(playerCamera.transform.position, playerCamera.transform.forward, out RaycastHit hit, interactDistance, interactableObjectsLayer))
        {
            if (hit.transform.CompareTag(tabletSlotTag))
            {
                hit.transform.gameObject.GetComponent<TabletSlot_Jack>().RotateTablet();
            }
            else if (hit.transform.CompareTag(bookTag))
            {
                hit.transform.gameObject.GetComponent<Book_Jack>().PullOutBook();
            }
            else if (hit.transform.CompareTag(alcoholPlacementTag))
            {
                hit.transform.gameObject.GetComponent<BottlePlacementManager_Jack>().Interact();
            }
            else if (hit.transform.CompareTag(pianoKeysTag))
            {
                if (!keyManagerScript.WasClosed())
                {
                    keyManagerScript.Open();
                }
            }
            else if (hit.transform.CompareTag(doorTag))
            {
                hit.transform.gameObject.GetComponent<DoorOpenClose_Dan>().DoorMechanism();
            }
            else if (hit.transform.CompareTag(rabbitTag))
            {
                hit.transform.gameObject.GetComponent<BunnyAI_Louie>().ChangeRabbitState(BunnyState.caught);
            }
            else if (hit.transform.CompareTag(valveTag))
            {
                hit.transform.gameObject.GetComponentInParent<Valve_Jack>().StartTurn();
            }
        }
    }

    /// Picks up an object in front of the player.
    /// 
    /// Fires a ray from the camera in front of the player. If the ray hits an
    /// object that can be held it will be picked up.
    public void PickupObject()
    {
        Debug.DrawRay(playerCamera.transform.position, playerCamera.transform.forward * interactDistance, Color.red, 2f);
        if (Physics.Raycast(playerCamera.transform.position, playerCamera.transform.forward, out RaycastHit hit, interactDistance, moveableObjectsLayer))
        {
            heldObject = hit.transform.gameObject;

            heldObjectCollider = heldObject.GetComponent<Collider>();
            Physics.IgnoreCollision(heldObjectCollider, characterController);

            heldObjectRigidbody = heldObject.GetComponent<Rigidbody>();
            heldObjectRigidbody.useGravity = false;
            heldObjectConstraints = heldObjectRigidbody.constraints;
            heldObjectRigidbody.constraints = freezeRotationConstraints;
        }
    }

    /// Drops the held object if there is one.
    public void DropObject()
    {
        heldObjectRigidbody.constraints = heldObjectConstraints;
        heldObjectRigidbody.useGravity = true;

        heldObjectRigidbody = null;

        Physics.IgnoreCollision(heldObjectCollider, characterController, false);
        heldObjectCollider = null;

        heldObject = null;
    }

    /// <summary>
    /// Throws the currently held object forward relative to where the player is looking.
    /// Check if there is an object held before using this function.
    /// </summary>
    public void ThrowObject()
    {
        heldObjectRigidbody.AddForce(playerCamera.transform.forward * throwForce, ForceMode.Impulse);
        DropObject();
    }

    /// Returns heldObject.
    public GameObject GetHeldObject()
    {
        return heldObject;
    }
}
