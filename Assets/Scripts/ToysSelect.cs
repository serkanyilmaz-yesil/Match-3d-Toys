using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using Tabtale.TTPlugins;


public class ToysSelect : MonoBehaviour
{
    public static ToysSelect select;

    [HideInInspector]
    public GameObject selectedObject;
    private Vector3 selectObjStartPos;
    public float dist;
    public bool selected, fail;
    public bool effect;
    public int currentChildCount;
    public int targetChildCount;
    public int sceneControl;
    public GameObject nextButton, finishEffects,placedEffect,failed, restartButton, hand;

    public TextMeshProUGUI cattleT, computerT, carT, droneT, shipT;
    private int cattle, computer, car, drone, ship;

    private AudioSource source;
    private AudioClip bip,win;


    private void Awake()
    {
        TTPCore.Setup();
        select = this;
        //ball = GameObject.Find("ball");
    }

    private void Start()
    {
        selectedObject = null;
        selected = false;
        effect = false;
        nextButton.SetActive(false);
        source = GetComponent<AudioSource>();
        bip = Resources.Load<AudioClip>("bip");
        win = Resources.Load<AudioClip>("Win");
        Load();
        fail = false;
        failed.SetActive(false);
        restartButton.SetActive(false);
        hand.SetActive(false);
    }

    public void NextButton()
    {
        sceneControl++;

        if (sceneControl == 3)
        {
            sceneControl = 0;
            SceneManager.LoadScene(0);
        }
        else
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);

    }

    public void RestartButton()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }


    private void Update()
    {
        if (currentChildCount == targetChildCount)
        {
            nextButton.SetActive(true);
            if (!effect)
            {
                Instantiate(finishEffects, new Vector3(0, -3.5f, 0), Quaternion.identity);
                source.PlayOneShot(win, 1);
                effect = true;
            }
            hand.SetActive(false);

        }

        ScoreTextControl();
        Save();
    }

    void FixedUpdate()
    {

        if (selectedObject != null)
        {
            dist = Vector3.Distance(selectedObject.transform.position, Controller.ctrl.selectedObject.transform.position);

        }

        if (Input.GetMouseButton(0))
        {

            hand.SetActive(true);
            RaycastHit hit;

            if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit))
            {

                if (Controller.ctrl.selectedObject != null)
                {
                    hand.transform.position = new Vector3(hit.point.x - .3f, hit.point.y, hand.transform.position.z);

                    if (selectedObject != null)
                    {
                        selected = true;
                        selectedObject.transform.position = new Vector3(hit.point.x, hit.point.y, Controller.ctrl.selectedObject.transform.position.z);



                        if (dist < 0.5f)
                        {
                            if (selectedObject.gameObject.name == Controller.ctrl.selectedObject.name)
                            {
                                float zPos = Random.Range(-.4f, .4f);
                                float yPos = Random.Range(0.1f, 0.5f);
                                selectedObject.transform.position = new Vector3(selectedObject.transform.position.x, Controller.ctrl.selectedObject.transform.position.y + yPos, Controller.ctrl.selectedObject.transform.position.z + zPos);

                                selectedObject.transform.parent = Controller.ctrl.selectedObject.transform;
                                source.PlayOneShot(bip,1);
                                Instantiate(placedEffect, Controller.ctrl.selectedObject.transform.position, Quaternion.identity);
                                ScoreControl();
                                selectedObject.gameObject.tag = "Untagged";
                                selectedObject = null;
                                currentChildCount++;

                            }

                        }

                    }

                    if (hit.collider.gameObject.CompareTag("Toys"))
                    {
                        if (selectedObject == null)
                        {
                            selectedObject = hit.collider.gameObject;
                            selectObjStartPos = selectedObject.transform.position;


                        }

                    }

                }


            }


        }
        if (Input.GetMouseButtonUp(0))
        {

            if (selectedObject != null)
            {
                if (dist < 0.5f && selectedObject.gameObject.name != Controller.ctrl.selectedObject.name)
                {
                    fail = true;
                    Instantiate(placedEffect, Controller.ctrl.selectedObject.transform.position, Quaternion.identity);
                    failed.SetActive(true);
                    restartButton.SetActive(true);

                }

                if (!fail)
                {
                    selectedObject.transform.position = selectObjStartPos;

                }


            }
            selectedObject = null;
            selected = false;
        }

    }


    void ScoreTextControl()
    {

        cattleT.text = cattle.ToString() + " / 3";
        computerT.text = computer.ToString() + " / 3";
        carT.text = car.ToString() + " / 3";
        droneT.text = drone.ToString() + " / 3";
        shipT.text = ship.ToString() + " / 3";
    }

    void ScoreControl()
    {
        if (selectedObject.gameObject.name == "cattle")
        {
            cattle++;
        }
        if (selectedObject.gameObject.name == "computer")
        {
            computer++;
        }
        if (selectedObject.gameObject.name == "car")
        {
            car++;
        }
        if (selectedObject.gameObject.name == "drone")
        {
            drone++;
        }
        if (selectedObject.gameObject.name == "ship")
        {
            ship++;
        }


    }

    public void Save()
    {
        PlayerPrefs.SetInt("scene", sceneControl);
    }


    public void Load()
    {
        if (PlayerPrefs.HasKey("scene"))
        {
            sceneControl = PlayerPrefs.GetInt("scene");
        }

    }
}
