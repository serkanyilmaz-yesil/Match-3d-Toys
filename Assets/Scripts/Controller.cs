using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Controller : MonoBehaviour
{
    public static Controller ctrl;

    public float positionX;
    public float moveFactorX;
    public float MoveFactorX => moveFactorX;
    public float swerveSpeed = 0.5f;
    public int childCount;


    [HideInInspector]
    public GameObject selectedObject;


    private void Awake()
    {
        ctrl = this;
    }

    private void Start()
    {
    }


    private void Update()
    {

        float swerveAmount = Time.deltaTime * swerveSpeed * MoveFactorX;
        transform.Translate(new Vector3(swerveAmount, 0 * Time.deltaTime, 0 * Time.deltaTime));

        if (Input.mousePosition.y < 500 && ToysSelect.select.selected == false)
        {
            SwerveControl();

        }

    }

    void FixedUpdate()
    {



        RaycastHit hit;
        if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit))
        {

            //Debug.Log("Ray hit : " + hit.collider.gameObject.name);
            if (Input.GetMouseButton(0) && ToysSelect.select.selected == false)
            {

                if (hit.collider.CompareTag("Hamper"))
                {
                    selectedObject = hit.collider.gameObject;

                }

            }
        }



        Vector3 pos = transform.position;
        pos.x = Mathf.Clamp(pos.x, -7f, 7f);
        transform.position = pos;


    }


    void SwerveControl()
    {
        if (Input.GetMouseButtonDown(0))
        {
            positionX = Input.mousePosition.x;
        }

        else if (Input.GetMouseButton(0))
        {

            moveFactorX = Input.mousePosition.x - positionX;
            positionX = Input.mousePosition.x;

        }
        else if (Input.GetMouseButtonUp(0))
        {
            moveFactorX = 0f;
        }

    }
}
