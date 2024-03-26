using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.UI;

/// <summary>
/// 確認モードプログラム
/// </summary>
public class DisplayImage : MonoBehaviour
{
	[SerializeField] Image currentImg;	//表示する画像
	[SerializeField] int totalVerticalImgNum;	//縦の画像枚数
	[SerializeField] int totalHorizontalImgNum;	//横の画像枚数
	[SerializeField] Vector2 angleLimit;	//回転を読み取る角度

	Main main;	//メインスクリプト
	float changeTimeLeft=0; //画像を変更するか判定する時間間隔
	Vector2 angleInterval;  //画像を変更する角度の間隔
	Vector3 currentVRAngle;	//現在のVRの角度

	void Start()
	{
		main = this.GetComponent<Main>();
		LoadCenterImage();
		angleInterval = new Vector2(angleLimit.y / totalHorizontalImgNum, angleLimit.x / totalVerticalImgNum);
	}

	// Update is called once per frame
	void Update()
	{
		//if (main.ProcessPhase == Phase.Load)
		//{
  //          //LoadImage();
  //          main.ProcessPhase = Phase.Checkmode;
		//}
		//else if (main.ProcessPhase == Phase.CheckMode)
		//{
		//	//VRの角度をクォータニオン形式で取得
		//	Quaternion rotation = InputTracking.GetLocalRotation(XRNode.CenterEye);

		//	changeTimeLeft -= Time.deltaTime;
		//	//時間になったら
		//	if (changeTimeLeft <= 0)
		//	{
		//		//オイラー角に変換
		//		currentVRAngle = rotation.eulerAngles;
				
		//		SwichImage();
				
		//		changeTimeLeft = 0.2f; //0.2秒
		//	}
		//}
	}

	///// <summary>
	///// 現在の菌床の画像を読み込み
	///// </summary>
	//void LoadImage()
	//{
	//	main.LoadedImgList = new List<Sprite>();
	//	//全体で画像総数回繰り返し
	//	for (int verticalNum = 0; verticalNum < totalVerticalImgNum; verticalNum++)
	//	{
	//		for (int horizontalNum = 0; horizontalNum < totalHorizontalImgNum; horizontalNum++)
	//		{
	//			//スプライト画像を生成
	//			string imgStr = $@"{main.SystemPath}\StreamingAssets\images\Kinsho_{main.currentKinshoCount+1}\IMG_{((verticalNum) * totalHorizontalImgNum + horizontalNum + 1)}.jpg";
	//			Sprite loadImg = main.CreateSprite(imgStr);
	//			main.LoadedImgList.Add(loadImg);
	//		}
	//	}
	//}

	/// <summary>
	/// 各菌床の正面中央画像の読み込み
	/// </summary>
	void LoadCenterImage()
	{
		main.LoadedCenterImgList = new List<Sprite>();
		string imgDir = $@"{main.SystemPath}\StreamingAssets\images";
		//菌床数
		int imgFolderCount = Directory.GetDirectories(imgDir, "Kinsho_*").Length;	
		
		for(int i = 1; i <= imgFolderCount; i++)
		{
			//正面中央画像のスプライト画像を生成
			string imgStr = $@"{imgDir}\Kinsho_{i}\IMG_9.jpg";
			Sprite centerImg = main.CreateSprite(imgStr);
			main.LoadedCenterImgList.Add(centerImg);
		}
	}

	/// <summary>
	/// 角度に応じて画像を変更
	/// </summary>
	void SwichImage()
	{
		//変更する画像のインデックスを計算する
		//0～360度を-180度から180度に変換
		if (currentVRAngle.x > 180)
		{
			currentVRAngle.x -= 360;
		}
		if (currentVRAngle.y > 180)
		{
			currentVRAngle.y -= 360;
		}
		
		//限界角以上は限界角に設定
		currentVRAngle.x = Mathf.Min(currentVRAngle.x, angleLimit.x - 1f);
		currentVRAngle.x = Mathf.Max(currentVRAngle.x, -(angleLimit.x - 1f));
		currentVRAngle.y = Mathf.Min(currentVRAngle.y, angleLimit.y - 1f);
		currentVRAngle.y = Mathf.Max(currentVRAngle.y, -(angleLimit.y - 1f));
		
		//変更する画像のリストのインデックスを計算
		int firstIndex = ((int)((currentVRAngle.x + angleLimit.x) / (angleInterval.y * 2)));
		int secondIndex = ((int)((currentVRAngle.y + angleLimit.y) / (angleInterval.x * 2)));

		//水平方向の画像は正面から右方向が番号順になるようにインデックスを調整
		secondIndex = secondIndex - totalHorizontalImgNum / 2;
		if (secondIndex < 0)
		{
			secondIndex = totalHorizontalImgNum + secondIndex;
		}

		//算出された画像に切り替え
		currentImg.sprite = main.LoadedImgList[firstIndex * totalHorizontalImgNum + secondIndex];
	}
}
