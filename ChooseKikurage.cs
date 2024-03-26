using System.Collections.Generic;
using UnityEngine;

public class ChooseKikurage : MonoBehaviour
{

	[SerializeField] float CenterY, CenterZ;	//アームが画像の中心にあるときの座標
	[SerializeField] float realHight, realWidth; //写真に写る範囲におけるアーム座標の高さと幅（㎜）
	[SerializeField] float secondWidth;
	[SerializeField] float realOpenLimit; //アーム先端が最大まで開いた時の両端間距離(mm)
	[SerializeField] GameObject mainCamera, subCamera;	//視点自由のメインカメラ、視点固定のサブカメラ
	[SerializeField] GameObject bboxFolder;	//BBoxを生成するフォルダ
	[SerializeField] GameObject chooseImg;    //選択モード時の画像
	[SerializeField] GameObject resetUI;

	Main main;  //メインスクリプト

	bool wait = false;	//プロセスフェーズ（Main.ProcessPhase）がwaitであるか
	bool backFlag;	//前の菌床面に戻る初期化処理なら

	List<GameObject> choosedBboxList = new List<GameObject>();	//現在の菌床面の選ばれているBBoxのリスト

	void Start()
	{
		main = this.GetComponent<Main>();
	}

	// Update is called once per frame
	void Update()
	{
		//if ((int)main.ProcessPhase == (int)Phase.Start || (int)main.ProcessPhase == (int)Phase.CheckMode)
		//{
		//	//サブカメラに切り替え
		//	mainCamera.SetActive(false);
		//	subCamera.SetActive(true);

		//	if ((OVRInput.GetDown(OVRInput.RawButton.RHandTrigger)) && main.ProcessPhase == Phase.CheckMode)
		//	{
		//		//メインカメラに切り替え
		//		mainCamera.SetActive(true);
		//		subCamera.SetActive(false);

		//		bboxFolder.SetActive(true);
		//		if (main.IsSeted)
		//			//BBoxを表示
		//			main.ProcessPhase = Phase.ChooseMode;
		//		else
		//			//BBoxを配置
		//			main.ProcessPhase = Phase.SetBBox;
		//		return;
		//	}
		//}

		if (main.ProcessPhase == Phase.ChooseMode)
		{
			main.SortLayerControl();

			chooseImg.SetActive(true);
			UpdateChooseList();
			//Aボタンで次の菌床面に進む
			if (OVRInput.GetDown(OVRInput.RawButton.A))
			{
				backFlag = false;
				main.ProcessPhase = Phase.EndUI;
			}
			//Bボタンで前の菌床面に戻る
			else if (OVRInput.GetDown(OVRInput.RawButton.B))
			{
				if (main.CurrentFace == KinshoFace.Front)
				{
					resetUI.SetActive(true);
				}
				else
				{

					backFlag = true;
					main.ProcessPhase = Phase.Record;

				}
			}
			//ハンドトリガーで確認モード
			//else if ((OVRInput.GetDown(OVRInput.RawButton.RHandTrigger)))
			//{
			//	bboxFolder.SetActive(false);
			//	main.ProcessPhase = Phase.CheckMode;
			//	chooseImg.SetActive(false);
			//}
			//スティックミドルボタンで編集モード
			else if ((OVRInput.GetDown(OVRInput.RawButton.RThumbstick)))
			{
				if (!wait)
				{
					main.ProcessPhase = Phase.WaitEditMode;
					return;
				}
				else
				{
					wait = false;
				}
			}
		}
		else if (main.ProcessPhase == Phase.WaitEditMode)
		{
			//既存のBBoxを編集するか新規作成するか
			wait = true;
			main.SortLayerControl();
			if ((OVRInput.GetDown(OVRInput.RawButton.RThumbstick)))
			{
				main.ProcessPhase = Phase.AddEditMode;
			}
		}
		else if (main.ProcessPhase == Phase.Record)
		{
			//BBoxを保存して初期化処理に
			main.SendCoordinateList = CalculateCoordinate();
			if (backFlag)
			{
				main.BackInit();
			}
			else
			{
				main.NextInit();
			}
		}
	}

	/// <summary>
	/// BBox選択時に呼び出され、選択したBBoxを記録する
	/// </summary>
	void UpdateChooseList()
	{
		choosedBboxList = main.ChoosedBboxList;
		if (main.CurrentChooseBbox != null && choosedBboxList.IndexOf(main.CurrentChooseBbox) == -1)
		{
			choosedBboxList.Add(main.CurrentChooseBbox);
			main.CurrentChooseBbox = null;

		}

		if (main.CurrentChooseBbox != null && choosedBboxList.IndexOf(main.CurrentChooseBbox) != -1)
		{
			choosedBboxList.Remove(main.CurrentChooseBbox);
			main.CurrentChooseBbox = null;
		}
	}

	/// <summary>
	/// 選ばれているBBoxの座標を計算して保存する
	/// </summary>
	/// <returns>座標の文字列リスト</returns>
	List<string> CalculateCoordinate()
	{
		var nameList = new List<string>();
		var coordinateList = new List<string>();
		foreach (GameObject choosedBbox in choosedBboxList)
		{
			if (choosedBbox != null)
			{
				//BBoxの情報から座標を計算
				float x, y, z;
				DetectionInfo choosedDetection = main.AllDetectionDict[choosedBbox];
				//if(main.CurrentFaceTxt=="F"|| main.CurrentFaceTxt == "B")
				//{
				//	x = 310;
				//}
				//else
				//{
				//	x = 350;
				//	realHight = secondWidth;
				//	realWidth = secondWidth;
				//}
				x = 370;
				y = CenterY + ((choosedDetection.XCenter / main.ImgWidth - 0.5f) * realWidth)-10;
				z = (CenterZ - ((choosedDetection.YCenter / main.ImgHight - 0.5f) * realHight))+10;
				//縦軸（ｚ）は故障防止のため140～235に制限
				//z = Mathf.Min(Mathf.Max(z, 140), 235);

				float realOpen,sendOpen;
				float closeLimit = 73;  //アーム先端が閉じたときの値
				float openLimit = 10;

				//収穫時のアーム先端の開く距離をBBoxのサイズから計算
				realOpen = choosedDetection.Width / main.ImgWidth * realWidth;
				//実際に送信する値を計算
				sendOpen = closeLimit - realOpen * closeLimit / realOpenLimit;
				sendOpen = Mathf.Max(openLimit, sendOpen);

				//送信するテキストを作成して保存
				string coordinate = $"S X{x} Y{y} Z{z} O{sendOpen};{main.CurrentFaceTxt}";
				coordinateList.Add(coordinate);
				nameList.Add(choosedBbox.name);
			}
		}
		main.ChoosedBboxNameList = nameList;
		choosedBboxList = new List<GameObject>();
		return coordinateList;
	}
}
