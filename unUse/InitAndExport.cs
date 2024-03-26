//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;
//using System;
//using System.IO;

//public class ExportChosen : MonoBehaviour
//{
//   Main main;
//   string path = @".\Assets\Export\";
//   int count = 0;
//   string datas="";
//   List<string> direction = new List<string>(){ "R", "B", "L","F" };
//   // Start is called before the first frame update
   
//   void Start()
//   {
//      main = this.GetComponent<Main>();
//   }

//   // Update is called once per frame
   
//   public void Export()
//	{
//      string coord = "";
//      main.KinshoDirection = direction[count];
//      count++;
//      if (main.SendCoordinateList != null && main.SendCoordinateList.Count != 0)
//      {
//         Debug.Log(main.KinshoDirection);
//         var data = main.SendCoordinateList;
//         Debug.Log(data.Count);
//         //File.WriteAllText(path + $"txt{count + 1}.txt", data[0]);
//         coord += String.Join("\n", data);
//         coord += "\n";
//         File.WriteAllText(path + "Choosed.txt", coord);
         
//         //   Debug.Log(sendCoordinate);
//         //foreach (string sendCoordinate in main.SendCoordinate)
//         //{
//         //   File.WriteAllText(path + $"txt{count + 1}.txt", sendCoordinate);
//         //   Debug.Log(sendCoordinate);
//         //}
//      }
//      main.EndAndReload();
//      main.ChoosePhase = 1;
//      main.SendCoordinateList = new List<string>();
//      foreach (GameObject clone in main.Detect.Keys)
//      {
//         //Debug.Log($"Destoroy:{clone}");
//         Destroy(clone);
//      }
//      if (count == 4)
//      {
//         main.EndAndReload();
//      }
//   }
//   //void Update()
//   //{
//   //   if (main.ChoosePhase == 5)
//   //   {
//   //      main.KinshoDirection = direction[count];
//   //      count++;
//   //      if (main.SendCoordinateList != null && main.SendCoordinateList.Count != 0)
//   //      {
//   //         Debug.Log(main.KinshoDirection);
//   //         var data = main.SendCoordinateList;
//   //         Debug.Log(data.Count);
//   //         //File.WriteAllText(path + $"txt{count + 1}.txt", data[0]);
//   //         string coord=String.Join("\n",data);
//   //         coord += "\n";
//   //         if (count == 1)
//   //         {
//   //            File.WriteAllText(path + "Choosed.txt", coord);
//   //         }
//   //         else
//   //         {
//   //            File.AppendAllText(path + "Choosed.txt", coord);
//   //         }
//   //         //   Debug.Log(sendCoordinate);
//   //         //foreach (string sendCoordinate in main.SendCoordinate)
//   //         //{
//   //         //   File.WriteAllText(path + $"txt{count + 1}.txt", sendCoordinate);
//   //         //   Debug.Log(sendCoordinate);
//   //         //}
//   //      }
//   //      else
//   //      {
//   //         if (count == 1)
//   //         {
//   //            File.WriteAllText(path + "Choosed.txt", "");
//   //         }
//   //         else
//   //         {
//   //            File.AppendAllText(path + "Choosed.txt", "");
//   //         }
//   //      }
//   //      main.ChoosePhase = 1;
//   //      main.SendCoordinateList = new List<string>();
//   //      foreach(GameObject clone in main.Detect.Keys){
//   //         //Debug.Log($"Destoroy:{clone}");
//   //         Destroy(clone);
//			//}
//   //      if (count == 4)
//			//{
//   //         main.EndAndReload();
//			//}
//   //   }
//   //}


  
//}
