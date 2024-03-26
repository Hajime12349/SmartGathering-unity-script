//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;

//public class Modules : MonoBehaviour
//{
//	public void SortLayerControl()
//	{
//		if (OVRInput.GetDown(OVRInput.Button.PrimaryThumbstickDown, OVRInput.Controller.RTouch))
//		{
//			if (isAscendingOrder)
//			{
//				foreach (GameObject bbox in AllDetectionDict.Keys)
//				{
//					bbox.transform.SetAsFirstSibling();
//				}
//			}
//			else
//			{
//				foreach (GameObject bbox in AllDetectionDict.Keys)
//				{
//					bbox.transform.SetAsLastSibling();
//				}
//			}
//			isAscendingOrder = !isAscendingOrder;
//		}
//	}
//}
