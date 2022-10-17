using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Scale : MonoBehaviour
{

    void Update()
    {



        if (Controller.ctrl.selectedObject != null && ToysSelect.select.selectedObject == null)
        {
            if (Controller.ctrl.selectedObject.name == gameObject.name)
            {
                transform.localScale = new Vector3(1.2f, 1.2f, 1.2f);
                /*
                foreach (Transform t in transform)
                {
                    t.gameObject.tag = "placed";

                }*/

            }
            else
            {
                transform.localScale = new Vector3(1, 1, 1);
                /*
                foreach (Transform t in transform)
                {
                    t.gameObject.tag = "dishes";
                }
                */
            }

        }





    }
}
