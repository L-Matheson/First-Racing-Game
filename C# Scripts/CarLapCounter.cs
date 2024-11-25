using System;
using UnityEngine;

public class CarLapCounter : MonoBehaviour
{
    int carPosition = 0;
   int passedCheckPointNumber = 0;
   float timeAtLastPassedCheckPoint = 0;
   int numberOfPassedCheckpoints = 0;
   int lapsCompleted = 0;
   const int lapsToComplete = 3;
    bool isRaceCompleted = false;
//Event, can tell other scripts that something happened
public event Action<CarLapCounter> OnPassCheckpoint;

public void SetCarPosition(int position)
{
    carPosition = position;
}

public int GetNumberOfCheckpointsPassed(){
    return numberOfPassedCheckpoints;
}

public float GetTimeAtLastCheckpoint(){
    return timeAtLastPassedCheckPoint;
}
   void OnTriggerEnter2D(Collider2D collider2D){
    // Checks if the tag on the game object is labeled "CheckPoint"
    if (collider2D.CompareTag("CheckPoint"))
    {
        if (isRaceCompleted)
        {
            return;
        }
        CheckPoint checkPoint = collider2D.GetComponent<CheckPoint>();

        //ensures checkpoints are passed in correct order
        if (passedCheckPointNumber + 1 == checkPoint.checkPointNumber)
        {
            numberOfPassedCheckpoints ++;

            passedCheckPointNumber = checkPoint.checkPointNumber;


            timeAtLastPassedCheckPoint = Time.time;

            if(checkPoint.isFinishLine)
            {
                passedCheckPointNumber = 0;
                lapsCompleted++;

                if (lapsCompleted >= lapsToComplete)
                    isRaceCompleted = true;
            }
            // triggers the passed checkpoint event
            OnPassCheckpoint?.Invoke(this);
        }

    }
   }
}
