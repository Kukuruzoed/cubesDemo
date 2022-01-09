using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

using Photon.Pun;
using Photon.Realtime;

public class DragAndDrop : MonoBehaviourPun, IPunOwnershipCallbacks
{
    [SerializeField]
    private Vector3 velocity = Vector3.zero;
    [SerializeField]
    private float mouseDragSpeed = .1f;
    [SerializeField]
    private GameObject requestUI;
    [SerializeField]
    private InputAction mouseClick;
    [SerializeField]
    private InputAction mouseDown;


    private Camera mainCamera;

    private void Awake() {
        mainCamera = Camera.main;
        PhotonNetwork.AddCallbackTarget(this);
    }

    private void Start() {
    }
    private void OnEnable() {
        mouseDown.Enable();
        mouseDown.performed += MousePressed;
        mouseClick.Enable();
        mouseClick.performed += MouseClicked;
    }
    private void OnDisable() {
        mouseDown.Disable();
        mouseDown.performed -= MousePressed;
        mouseClick.Disable();
        mouseClick.performed -= MouseClicked;
    }

    private void MousePressed(InputAction.CallbackContext context) {
        Ray ray = mainCamera.ScreenPointToRay(Mouse.current.position.ReadValue());
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit))
        {
            if (hit.collider != null)
            {
                PhotonView view = hit.collider.GetComponent<PhotonView>();
                if (view != null)
                {
                    if (view.IsMine)
                    {
                        if (PhotonNetwork.IsConnected == true)
                        {
                            StartCoroutine(DragUpdate(hit.collider.gameObject));
                        }
                    }
                    else
                    {
                        switch (view.OwnershipTransfer)
                        {
                            case OwnershipOption.Takeover:
                                view.TransferOwnership(PhotonNetwork.LocalPlayer);
                                break;
                            case OwnershipOption.Request:
                                view.RequestOwnership();
                                break;
                        }
                    }
                }
            }
        }
    }

    private void MouseClicked(InputAction.CallbackContext context) {
        Ray ray = mainCamera.ScreenPointToRay(Mouse.current.position.ReadValue());
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit)) {
            if (hit.collider != null) {
                PhotonView view = hit.collider.GetComponent<PhotonView>();
                if (view != null)
                {
                    if (view.IsMine)
                    {
                        if (PhotonNetwork.IsConnected == true)
                        {
                            //hit.collider.GetComponent<Renderer>().material.SetColor("_Color", Color.red);
                            Vector3 color = new Vector3(Random.Range(0, 255), Random.Range(0, 255), Random.Range(0, 255));

                            view.RPC("setColor", RpcTarget.All, color);
                        }
                    }
                }
            }
        }
    }

    private IEnumerator DragUpdate(GameObject clickedObject) {
        float initialDistance = Vector3.Distance(clickedObject.transform.position, mainCamera.transform.position);
        while (mouseDown.ReadValue<float>() != 0) {
            Ray ray = mainCamera.ScreenPointToRay(Mouse.current.position.ReadValue());

            clickedObject.transform.position = Vector3.SmoothDamp(clickedObject.transform.position, ray.GetPoint(initialDistance), ref velocity, mouseDragSpeed);
            yield return null; 
        }
    }

    public void ShuffleCubes()
    {
        GameObject[] cubes = GameObject.FindGameObjectsWithTag("Cube");
        foreach (GameObject cube in cubes)
        {
            cube.GetPhotonView().RPC("moveTo", RpcTarget.All, new Vector3(Random.Range(-7, 7), Random.Range(-4, 6), 0));
        }
    }

    public void CloseRequestUI()
    {
        requestUI.SetActive(false);
    }

    public void OnOwnershipRequest(PhotonView targetView, Player requestingPlayer)
    {
        targetView.TransferOwnership(requestingPlayer);
        if (PhotonNetwork.IsMasterClient)
        {
            requestUI.SetActive(true);
        }
    }

    public void OnOwnershipTransfered(PhotonView targetView, Player previousOwner)
    {
        throw new System.NotImplementedException();
    }

    public void OnOwnershipTransferFailed(PhotonView targetView, Player senderOfFailedRequest)
    {
        throw new System.NotImplementedException();
    }
}
