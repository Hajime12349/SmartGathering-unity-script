using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// バウンディングボックス（検出枠）の配置
/// </summary>
public class SetBoundingBox : MonoBehaviour
{
	[SerializeField] int start, coefficient;	//正面の画像の最初の番号、次の正面画像の番号までの間隔
	[SerializeField] Image backImg;	//選択モードの時の菌床画像

	Main main;  //メインスクリプト
	GameObject Bbox;  //バウンディングボックス（検出枠）
	Dictionary<GameObject, DetectionInfo> detectionLinkedBbox = new Dictionary<GameObject, DetectionInfo>(); //BBoxとその情報
	List<List<DetectionInfo>> detectionLists=new List<List<DetectionInfo>>(); //BBox情報の一時保管リスト
	List<Sprite> choosedImg = new List<Sprite>();	//現在の選択モードに使用する菌床の画像
	List<GameObject> choosedBboxList = new List<GameObject>();	//以前に選ばれていたBBoxのリスト
	Dictionary<string, Sprite> BboxColor;  //BBoxのクラス名に対する枠の色
	List<string> pathList;

	// Start is called before the first frame update
	void Start()
	{
		main = GetComponent<Main>();
		Bbox = Resources.Load<GameObject>("BB");	//BBoxのプレハブをロード

		//BBoxのクラスごとに枠の画像をロード
		BboxColor = new Dictionary<string, Sprite>() {
			{ "harvestable", Resources.Load<Sprite>("flameAble") },
			{ "soon", Resources.Load<Sprite>("flameSoon") },
			{ "not_yet", Resources.Load<Sprite>("flameNot") },
			{ "Added", Resources.Load<Sprite>("flameAdded") }
		};
	}

	// Update is called once per frame
	void Update()
	{
		if (main.ProcessPhase == Phase.SetBBox)
		{
			//はじめての実行なら
			if (!main.IsSeted && main.CurrentFace==KinshoFace.Front)
			{
				pathList = new List<string>();
				//選択モードで使用する画像の読み込み
				for (int i = start; i <= main.ImgTotal; i += coefficient)
				{
					//BBoxを表示するための検出データXMLファイルのパスを記憶
					string txtPath = $@"{main.SystemPath}\system\txt_output\Kinsho_{main.currentKinshoCount + 1}\IMG_" + i.ToString() + ".xml";
					pathList.Add(txtPath);

					//使用する画像の記憶
					choosedImg.Add(main.LoadedImgList[i - 1]);
				}
			}

			//現在の菌床面の画像を表示
			Sprite displaySprite = choosedImg[(int)main.CurrentFace];
			backImg.sprite = displaySprite;

			//現在の菌床面のBBoxを配置
			if (!main.IsSeted)
			{
				detectionLists.Add(new List<DetectionInfo>());
				Debug.Log(pathList.Count);
				detectionLists[(int)main.CurrentFace] = main.ExtractDetectionData(pathList);
			}
			SetBbox();

			main.IsSeted = true;
			main.ProcessPhase = Phase.ChooseMode;
		}
	}

	/// <summary>
	/// BBoxの配置
	/// </summary>
	void SetBbox()
	{
		detectionLinkedBbox = new Dictionary<GameObject, DetectionInfo>();
		int BboxCount = 0;

		SetAddedBbox();

		//検出データにしたがってBBoxを生成・配置
		foreach (DetectionInfo det in detectionLists[(int)main.CurrentFace])
		{
			BboxCount++;
			GameObject BboxClone = Instantiate(Bbox, main.BboxCanvas);
			BboxClone.name = $"BB{BboxCount}";
			if (PreviousRemovedProcess(BboxClone))
			{
				Destroy(BboxClone);
				continue;
			}
			BboxClone.GetComponent<Button>().onClick.AddListener(() => main.bboxChoosed(BboxClone));
			BboxClone.transform.GetChild(1).GetComponent<Image>().sprite = BboxColor[det.ClassName];
			BboxClone.tag = "BB";
			BboxClone.transform.localPosition = new Vector3(det.XCenter / main.ImgWidth * 10 - 5, -(det.YCenter / main.ImgHight * 10 - 5), 0);
			BboxClone.transform.localScale = new Vector3(det.Width / main.ImgWidth, det.Height / main.ImgHight, 0);
			detectionLinkedBbox.Add(BboxClone, det);
			PreviousChoosedProcess(BboxClone);
		}

		main.ChoosedBboxList = choosedBboxList;
		main.AllDetectionDict = detectionLinkedBbox;
		choosedBboxList = new List<GameObject>();
	}

	/// <summary>
	/// 以前に追加されたBBoxを表示する
	/// </summary>
	void SetAddedBbox()
	{
		foreach (GameObject added in main.DetectionLinkedAddedBbox.Keys)
		{
			//追加したBBoxが削除されているならスキップ
			if (PreviousRemovedProcess(added))
			{
				continue;
			}
			added.SetActive(true);
			PreviousChoosedProcess(added);
			detectionLinkedBbox.Add(added, main.DetectionLinkedAddedBbox[added]);
		}
	}

	/// <summary>
	/// 以前に選ばれていたBBoxに選択処理する
	/// </summary>
	/// <param name="bbox">BBox</param>
	void PreviousChoosedProcess(GameObject bbox)
	{
		var choosedNameList = main.ChoosedBboxNameList;
		if (choosedNameList.Contains(bbox.name))
		{
			bbox.transform.GetChild(0).gameObject.SetActive(true);
			choosedBboxList.Add(bbox);
		}
	}

	/// <summary>
	/// BBoxが以前に削除されていたか判定
	/// </summary>
	/// <param name="bbox">BBox</param>
	/// <returns>削除されていたか</returns>
	bool PreviousRemovedProcess(GameObject bbox)
	{
		foreach (string removedName in main.RemovedNameList)
		{
			if (bbox.name == removedName)
			{
				return true;
			}
		}
		return false;
	}
}
