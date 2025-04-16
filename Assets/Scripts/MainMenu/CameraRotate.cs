using UnityEngine;

public class CameraRotate : MonoBehaviour
{
    [SerializeField]
    private Camera cam;


    public float Rotationing = 1;
    public float Positioning = 0;

    void Update()
    {
        cam.transform.localEulerAngles = new Vector3(-30, cam.transform.localEulerAngles.y + 2 * Time.deltaTime, 0);
    }
}
