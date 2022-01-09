using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class Cube : MonoBehaviourPun
{
    [PunRPC]
    public void setColor(Vector3 color)
    {
        Color bColor = new Color(color.x / 255f, color.y / 255f, color.z / 255f);
        transform.GetComponent<Renderer>().material.color = bColor;
    }

    [PunRPC]
    public void moveTo(Vector3 point)
    {
        transform.position = point;
    }

}
