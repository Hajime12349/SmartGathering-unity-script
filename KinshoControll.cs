using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KinshoControll : MonoBehaviour
{
    [SerializeField] GameObject kinsho;
    [SerializeField] GameObject rightHand,leftHand;
    [SerializeField] GameObject ChooseUI;

    float rotateSensitivity =12;

    Main main;
    OVRHand rightHandGesture,leftHandGesture;
    bool isStartPinchRight = true;
    bool isStartPinchLeft = true;
    float handPosX,handPosDiff;
    float loadTime = 2.32f;
    
    // Start is called before the first frame update
    void Start()
    {
        main = this.GetComponent<Main>();
        rightHandGesture = rightHand.transform.Find("RightOVRHand").GetComponent<OVRHand>();
        leftHandGesture = leftHand.transform.Find("LeftOVRHand").GetComponent<OVRHand>();
    }

    // Update is called once per frame
    void Update()
    {
        if (main.ProcessPhase == Phase.Load&&loadTime<0)
        {
            main.ProcessPhase = Phase.Kinsho;
            kinsho.SetActive(true);
            ChooseUI.SetActive(true);
            
        }
        else if (main.ProcessPhase == Phase.Kinsho)
        {
            if (rightHandGesture.GetFingerIsPinching(OVRHand.HandFinger.Thumb)&&isStartPinchRight)
            {
                handPosX = rightHand.transform.position.x;
                isStartPinchRight = false;
            }
            else if (rightHandGesture.GetFingerIsPinching(OVRHand.HandFinger.Thumb))
            {
                handPosDiff = handPosX - rightHand.transform.position.x;
                kinsho.transform.Rotate(0, handPosDiff*rotateSensitivity, 0);
                //Debug.Log(handPosDiff);
            }
            else if (!rightHandGesture.GetFingerIsPinching(OVRHand.HandFinger.Thumb)&&!isStartPinchRight)
            {
                isStartPinchRight = true;
            }

            if (leftHandGesture.GetFingerIsPinching(OVRHand.HandFinger.Thumb) && isStartPinchLeft)
            {
                handPosX = leftHand.transform.position.x;
                isStartPinchLeft = false;
            }
            else if (leftHandGesture.GetFingerIsPinching(OVRHand.HandFinger.Thumb))
            {
                handPosDiff = handPosX - leftHand.transform.position.x;
                kinsho.transform.Rotate(0, handPosDiff * rotateSensitivity, 0);
                //Debug.Log(handPosDiff);
            }
            else if (!leftHandGesture.GetFingerIsPinching(OVRHand.HandFinger.Thumb) && !isStartPinchLeft)
            {
                isStartPinchLeft = true;
            }
            //Debug.Log(rightHand.GetFingerIsPinching(OVRHand.HandFinger.Thumb));
        }
        else if (main.ProcessPhase==Phase.Load)
        {
            loadTime -= Time.deltaTime;
        }
    }
}
