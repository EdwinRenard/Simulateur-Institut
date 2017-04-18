using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class aiBonusForVehicle : MonoBehaviour {

    private GameObject BonusChoose;
    public Texture[] aiBonus;
    float currentTime;
    float bonusTime;
    float endBonusSwitch;
    int bonusIndex =0;
    float bonusDeltaTime = 0.2f;
    private bool switchingBonus = false;

    // Use this for initialization
    void Start () {
        BonusChoose = GameObject.Find("aiUI");
    }

    // Update is called once per frame
    void Update()
    {
        currentTime = Time.time;
        if (switchingBonus == true)
        {

            if (currentTime > bonusTime + bonusDeltaTime) //every bonusDeltaTime secondes
            {
                BonusChoose.transform.GetChild(0).GetComponent<RawImage>().texture = aiBonus[bonusIndex];
                bonusIndex += 1;
                bonusTime = currentTime;
                if (bonusIndex == aiBonus.Length) //end of list reached - reset
                {
                    bonusIndex = 0;
                    bonusTime = currentTime;
                }
            }
        }
        if (currentTime > endBonusSwitch + 10)
            {
                switchingBonus = false;
            }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "aiBonus")
        {
            //Save collision occuring time :
            bonusTime = currentTime;
            endBonusSwitch = currentTime;
            switchingBonus = true;
            BonusChoose.transform.GetChild(0).GetComponent<RawImage>().texture = aiBonus[bonusIndex];
            bonusIndex += 1;
        }
    }
}
