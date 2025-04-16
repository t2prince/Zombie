using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class duckMove : MonoBehaviour {
    public Animator duckAni;
    public Animator duckAniShadow;
    //public eventSwimBird birdCon;
    //public control_event eventControl;
    public float moveSpeed = 1f;
    public float rotateSpeed = 1f;
    public float randomStayMinTime = 10f;
    public float randomStayMaxTime = 10f;
    public float arriveDistance = 0.1f;
    public Transform targetPos;
    //Vector3 randomTarget;
    Vector3 newDir;
    //float lerpMoveSpeed = 0;
    //float sTime = 0;
    float height;
    bool isMoving = false;
    bool eventFinish = false;

    // Use this for initialization
    void Start ()
    {
        AnimatorStateInfo state1 = duckAni.GetCurrentAnimatorStateInfo(0);

        float startTime = Random.Range(0f, 1f);
        duckAni.Play(state1.fullPathHash, -1, startTime);
        duckAniShadow.Play(state1.fullPathHash, -1, startTime);

        transform.position = targetPos.position;
        transform.rotation = targetPos.rotation;

        StartCoroutine(eMove());
    }


    // Update is called once per frame
    IEnumerator eMove()
    {
        while (true)
        {

            //sTime += Time.deltaTime;
            //time += Time.deltaTime;
            //lerpMoveSpeed = Mathf.Lerp(lerpMoveSpeed, moveSpeed, Time.deltaTime);
            //Vector3 targetDir = duckCon.transform.position - transform.position;
            Vector3 targetDir = targetPos.position - transform.position;
            Vector3 newDir = Vector3.RotateTowards(transform.forward, targetDir, Time.deltaTime * rotateSpeed, 0.0f);
            transform.rotation = Quaternion.LookRotation(newDir);
            transform.position = Vector3.Lerp(transform.position, targetPos.position, Time.deltaTime*0.8f);
            //}
            
            yield return null;
        }

    }

    public void swim()
    {
        duckAni.SetBool("swim", true);
        duckAniShadow.SetBool("swim", true);
        duckAniShadow.SetFloat("offset", Random.Range(0,2f));

    }

    public void idle()
    {
        duckAni.SetBool("swim", false);
        duckAniShadow.SetBool("swim", false);
        duckAniShadow.SetFloat("offset", Random.Range(0, 30f));

        int random = Random.Range(0, 6);
        switch (random)
        {
            case 0:
                break;

            case 1:
                duckAni.SetTrigger("event1");
                duckAniShadow.SetTrigger("event1");
                break;

            case 2:
                duckAni.SetTrigger("event2");
                duckAniShadow.SetTrigger("event2");
                break;

            case 3:
                duckAni.SetTrigger("event0");
                duckAniShadow.SetTrigger("event0");
                break;

            default:
                break;
        }


    }
}
