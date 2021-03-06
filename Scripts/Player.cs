﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Player : MonoBehaviour
{
    bool isAtStair = false;
    bool taMorreno = false;
    bool closingHole = false;
    bool flushing = false;
    bool walking = false;
    bool climbing = false;
    bool isAtDoorTrigger = false;
    public bool isUnderWater;

    public float redutor;
    public float speed;
    public float stairSpeed;
    public float fixRate;
    public float folego;
    public float flushTimer;
    float flushCounter;

    Rigidbody myBigidbody;
    Hole hole;

    public AnimController animCtrl;

    Door hoverDoor;
    public GameObject walk;

    public Door[] portas;

    public AudioClip step;
    public AudioClip waterStep;

    [Header("Callbacks")]
    [SerializeField] UnityEvent onPlayerDead;

    bool isDying = false;

    void Start()
    {
        myBigidbody = GetComponent<Rigidbody>();
        //Time.timeScale = 10f;
        //StartCoroutine(GoToMenu());
    }

    void FixedUpdate()
    {
        if (isDying) { return; }

        transform.position = new Vector3(transform.position.x, transform.position.y, 8.85f);
        float speedY = 0;
        if (isAtStair && !walking)
            speedY = Input.GetAxis("Vertical") * stairSpeed;
        if(speedY != 0)
            myBigidbody.velocity = new Vector3(0, speedY, 0);
        else
            myBigidbody.velocity = new Vector3(Input.GetAxisRaw("Horizontal") * (speed - redutor), myBigidbody.velocity.y, 0);

        if (myBigidbody.velocity.x != 0)
            walking = true;
        else walking = false;

        if (isAtStair && !walking)
            climbing = true;
        else climbing = false;

        if (taMorreno)
            animCtrl.PlayAnim(4);
        else if (closingHole)
            animCtrl.PlayAnim(3);
        else if (flushing)
        {
            animCtrl.PlayAnim(2);
        }
        else if (walking)
        {
            animCtrl.PlayAnim(1);
            walk.transform.eulerAngles = new Vector3(0, -90f * Input.GetAxis("Horizontal") - 90, 0);
        }
        else if (climbing)
        {
            animCtrl.PlayAnim(5);
            walk.transform.eulerAngles = new Vector3(0, 0, 0);
        }
        else
        {
            animCtrl.PlayAnim(0);
            walk.transform.eulerAngles = new Vector3(0, 0, 0);
        }

        if (!isUnderWater && folego <= 10)
            folego += Time.deltaTime * 2;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == 8)
        {
            myBigidbody.velocity = new Vector3(myBigidbody.velocity.x, 0, 0);
            isAtStair = true;
            myBigidbody.useGravity = false;
        }

        if (other.gameObject.layer == 9)
        {
            hole = other.gameObject.GetComponent<Hole>();
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.gameObject.layer == 9)
        {
            if (Input.GetKey(KeyCode.Space) && other.GetComponent<Hole>().isOpen)
            {
                closingHole = true;
                hole.LoseHp(fixRate);
            }
            else
            {
                closingHole = false;
                hole.RezetHP();
            }
        }
        if (other.gameObject.layer == 10)
        {
            isUnderWater = true;
            if (other.transform.parent.GetComponent<Water>() && other.transform.parent.GetComponent<Water>().fillAmount >= 30)
            {
                redutor = speed * .4f;
                transform.GetComponentInChildren<AnimController>().anims[1].gameObjects.GetComponent<AudioSource>().clip = waterStep;
            }
            else transform.GetComponentInChildren<AnimController>().anims[1].gameObjects.GetComponent<AudioSource>().clip = step;

            if (other.transform.parent.GetComponent<Water>() && other.transform.parent.GetComponent<Water>().fillAmount >= 80)
            { 
                folego -= Time.deltaTime;
                if (folego <= 0)
                {
                    taMorreno = true;
                    StartCoroutine(GoToMenu());
                }
            }
        }
        if (other.gameObject.layer == 11)
        {
            if (Input.GetKey(KeyCode.Space))
            {
                flushing = true;
                flushCounter += Time.deltaTime;
                if (flushCounter >= flushTimer && other.GetComponent<Flush>())
                {
                    other.GetComponent<Flush>().FlushAct();
                }
            }
            else flushing = false;
        }
        /*
        if(other.gameObject.layer == 12)
        {
            if (Input.GetKeyDown(KeyCode.Space) && other.GetComponent<Door>())
            {
               Door dorComp = other.GetComponent<Door>();
                if (dorComp.isOpen)
                    dorComp.isOpen = false;
                else dorComp.isOpen = true;
                dorComp.col.enabled = dorComp.isOpen;
            }
        }
        */
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.layer == 8)
        {
            myBigidbody.velocity = new Vector3(myBigidbody.velocity.x, 0, 0);
            myBigidbody.useGravity = true;
            isAtStair = false;
        }
        if (other.gameObject.layer == 9)
        {
            hole.RezetHP();
        }
        if (other.gameObject.layer == 10)
        {
            redutor = 0;
            isUnderWater = false;
        }
        //if (other.gameObject.layer == 12)
        //{
        //    hoverDoor = null;
        //    isAtDoorTrigger = true;
        //}
    }

    public IEnumerator GoToMenu()
    {
        if (isDying)
        {
            yield break;
        }

        isDying = true;
        FindObjectOfType<CanvasController>().SaveRecordIfBigger();
        FindObjectOfType<CanvasController>().RefreshRecord() ;
        speed = 0;
        stairSpeed = 0;
        Time.timeScale = 1;
        yield return new WaitForSeconds(5);
        onPlayerDead?.Invoke();
        FindObjectOfType<ScenesManager>().LoadMainMenu();
        //UnityEngine.SceneManagement.SceneManager.LoadScene(0);
    }
}
