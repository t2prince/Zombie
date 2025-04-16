using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class control_event : MonoBehaviour
{
    public bool EnvEvent = true;
    public GameObject eventObject;
    public float eventRatio = 10f;   //이벤트 일어날 확률

    public float eventStartMinTime = 100f;      //이벤트 일어나는 시간 랜덤시작값
    public float eventStartMaxTime = 200f;      //이벤트 일어나는 시간 랜덤 끝값
    public float eventEndMinTime = 100f;       //이벤트 끝나는 시간 랜덤시작값
    public float eventEndMaxTime = 200f;       //이벤트 끝나는 시간 랜덤 끝값
    public float evnetRestartWaitTime = 200f;   //이벤트 끝나고 재시작 기다리는 시간;
    public bool eventOnStart = false;       //게임시작할때 바로 이벤트 일어나는지
    public bool eventAutoReplay = false;    //이벤트 끝나고 자동 재생되는지
    [HideInInspector]
    public bool eventFinished = false;

    public FlockController birdEvent;
    public int birdEventRatio = 50;
    public int birdMinAmount = 0;
    public int birdMaxAmount = 3;


    void Start()
    {

        if(birdEvent != null)
        {
            int bird = Random.Range(birdMinAmount, birdMaxAmount);

            if (bird != 0)
            {
                birdEvent.gameObject.SetActive(true);
                birdEvent._childAmount = bird+1;
                birdEvent.UpdateChildAmount();
            }
            
        }
        if (EnvEvent)
        {
            if (Random.Range(0, 100) < eventRatio)
            {
                if (eventOnStart)
                {
                    eventObject.SendMessage("eventStart");

                }
                else
                    StartCoroutine(eventStart());
            }
        }
        


    }

    IEnumerator eventStart()
    {
        yield return new WaitForSeconds(Random.Range(eventStartMinTime, eventStartMaxTime));
        eventObject.SendMessage("eventStart");

        yield return new WaitForSeconds(Random.Range(eventEndMinTime, eventEndMaxTime));
        eventObject.SendMessage("eventEnd");

        if (!eventAutoReplay)
        {
            while (!eventFinished)
            {
                yield return new WaitForSeconds(1f);
            }
            eventFinished = false;

        }

        yield return new WaitForSeconds(evnetRestartWaitTime);

        StartCoroutine(eventStart());

    }
}