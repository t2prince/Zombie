using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class eventSwimBird : MonoBehaviour {
    public GameObject birdMoveDummy;

    public GameObject[] birdObj;
    public GameObject[] birdbabyObj;

    public float AdultRatio = 50f;
    public float BabyRatio = 30f;

    public float minLimitX = 1f;
    public float maxLimitX = 100f;
    public float minLimitZ = 1f;
    public float maxLimitZ = 1f;
    public float moveSpeed = 1f;
    public float rotateSpeed = 1f;
    public float randomStayMinTime = 10f;
    public float randomStayMaxTime = 10f;
    public float arriveDistance = 0.1f;
    Vector3 randomTarget;
    Vector3 newDir;
    float lerpMoveSpeed = 0;
    float height;
    //[HideInInspector]
    //public bool isMoving = false;
    bool eventFinish = false;

  
    // Use this for initialization
    void Awake () {
        birdMoveDummy.SetActive(false);
        for(int i =0; i < birdObj.Length; i++)
        {
            birdObj[i].SetActive(false);
        }

        for (int i = 0; i < birdbabyObj.Length; i++)
        {
            birdbabyObj[i].SetActive(false);
        }
    }

    void Start()
    {
        height = transform.position.y;
        birdMoveDummy.transform.position = new Vector3(Random.Range(minLimitX, maxLimitX), height, Random.Range(minLimitZ, maxLimitZ));
        //birdMoveDummy.transform.position = transform.position;
        birdMoveDummy.transform.rotation = Quaternion.Euler(new Vector3(0, Random.Range(0, 360), 0));
        for (int i = 0; i < birdObj.Length; i++)
        {
            if (birdObj[i].activeSelf)
            {
                birdObj[i].SendMessage("idle");

            }
        }

        if (birdbabyObj != null)
        {

            for (int i = 0; i < birdbabyObj.Length; i++)
            {
                if(birdbabyObj[i].activeSelf)
                {
                    birdbabyObj[i].SendMessage("idle");

                }
            }
        }
        StartCoroutine(randomMove());
    }

    // Update is called once per frame

    public void eventStart()
{
    birdMoveDummy.SetActive(true);
    birdObj[0].SetActive(true);

    if (Random.Range(0, 100) < AdultRatio)
        birdObj[1].SetActive(true);

    if (birdbabyObj != null)
    {
        for (int i = 0; i < birdbabyObj.Length; i++)
        {
            if (Random.Range(0, 100) < BabyRatio)
                birdbabyObj[i].SetActive(true);
        }
    }
}


    IEnumerator randomMove()
    {
        //float time = 0;
        float StayTime = Random.Range(randomStayMinTime, randomStayMaxTime);
        yield return new WaitForSeconds(StayTime);

        randomTarget = new Vector3(Random.Range(minLimitX, maxLimitX), height, Random.Range(minLimitZ, maxLimitZ));
        float distance = Vector3.Distance(randomTarget, birdMoveDummy.transform.position);

        for (int i = 0; i < birdObj.Length; i++)
        {

            if (birdObj[i].activeSelf)
            {
                yield return new WaitForSeconds(Random.Range(0, 1f));
                birdObj[i].SendMessage("swim");

            }
        }

        for (int i = 0; i < birdbabyObj.Length; i++)
        {

            if (birdbabyObj[i].activeSelf)
            {
                yield return new WaitForSeconds(Random.Range(0, 1f));
                birdbabyObj[i].SendMessage("swim");

            }
        }

        while (distance > arriveDistance)
        {
            lerpMoveSpeed = Mathf.Lerp(lerpMoveSpeed, moveSpeed, Time.deltaTime);
            Vector3 targetDir = randomTarget - birdMoveDummy.transform.position;
            Vector3 newDir = Vector3.RotateTowards(birdMoveDummy.transform.forward, targetDir, Time.deltaTime * rotateSpeed, 0.0f);
            birdMoveDummy.transform.rotation = Quaternion.LookRotation(newDir);
            birdMoveDummy.transform.position += birdMoveDummy.transform.forward * lerpMoveSpeed * Time.deltaTime;

            distance = Vector3.Distance(randomTarget, birdMoveDummy.transform.position);

            yield return null;
        }
        while (lerpMoveSpeed > 0)
        {
            lerpMoveSpeed -= Time.deltaTime;
            birdMoveDummy.transform.position += transform.forward * lerpMoveSpeed * Time.deltaTime;

            yield return null;
        }

        lerpMoveSpeed = 0;
        //isMoving = false;

        for (int i = 0; i < birdObj.Length; i++)
        {

            if (birdObj[i].activeSelf)
            {
                yield return new WaitForSeconds(Random.Range(0, 1f));
                birdObj[i].SendMessage("idle");

            }
        }

        if (birdbabyObj != null)
        {

            for (int i = 0; i < birdbabyObj.Length; i++)
            {
                if (birdbabyObj[i].activeSelf)
                {
                    yield return new WaitForSeconds(Random.Range(0, 1f));
                    birdbabyObj[i].SendMessage("idle");

                }
            }
        }

        StartCoroutine(randomMove());

    }
#if UNITY_EDITOR_WIN
    void OnDrawGizmosSelected()
    {
        // Draw a semitransparent red cube at the transforms position
        Gizmos.color = new Color(1, 0, 0, 0.5f);
        Gizmos.DrawCube(
            new Vector3(
                (maxLimitX + minLimitX) / 2,
                transform.position.y,
                (maxLimitZ + minLimitZ) / 2
            )
            , new Vector3(
                maxLimitX - minLimitX,
                transform.position.y,
                maxLimitZ - minLimitZ));
    }
#endif


}
