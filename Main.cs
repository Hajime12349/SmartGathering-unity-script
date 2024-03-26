using System;
using System.Collections.Generic;
using System.IO;
using System.Diagnostics;
using System.Xml.Serialization;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// スクリプト間の変数・メソッドの共有
/// </summary>
public class Main : MonoBehaviour
{
	[SerializeField] int imgWidth, imgHeight;	
	[SerializeField] int totalImgNum;			
	[SerializeField] Transform bboxCanvas;
	[SerializeField] string systemPath;


	public GameObject targetEffect;

	bool isAscendingOrder = true;	//昇順ソートするか
	KinshoFace loadedFace = KinshoFace.Front;	//菌床面のロードの進捗

	List<List<string>> choosedBboxNameList = new List<List<string>>();
	List<List<string>> sendCoordList = new List<List<string>>();
	List<Dictionary<GameObject, DetectionInfo>> addedBboxDetectionList = new List<Dictionary<GameObject, DetectionInfo>>();
	List<List<string>> removedNameList = new List<List<string>>();
	Dictionary<KinshoFace, string> faceTxtDict = new Dictionary<KinshoFace, string>()
	 {
		  { KinshoFace.Front, "F" },
		  { KinshoFace.Right, "R" },
		  { KinshoFace.Back, "B" },
		  { KinshoFace.Light, "L" }
	 };

	public static bool cloneFlag=false;

	// 以下プロパティ
	
	public int ImgTotal { get { return totalImgNum; } }	//画像総数

	public int currentKinshoCount { get; set; }	//現在作業中のの菌床の番号

	public List<Sprite> LoadedImgList { get; set; }	//読み込んだ現在の菌床の画像リスト

	public List<Sprite> LoadedCenterImgList { get; set; }	//読み込んだ各菌床の正面中央部分の画像

	public string SystemPath { get { return systemPath; } }	//システムディレクトリパス

	public Transform BboxCanvas { get { return bboxCanvas; } }	//BBoxを配置するフォルダ
	public GameObject CurrentChooseBbox { set; get; }	//現在選択されているBBox
	public Dictionary<GameObject, DetectionInfo> AllDetectionDict { set; get; }	//配置したBBoxとその情報

	public Phase ProcessPhase { set; get; } = Phase.Start;	//現在の処理の段階
	public KinshoFace CurrentFace { set; get; } = KinshoFace.Front;	//現在選択している菌床の面
	public string CurrentFaceTxt { get { return faceTxtDict[CurrentFace]; } }  //現在選択している菌床の面を表すテキスト

	public List<GameObject> ChoosedBboxList { set; get; }	//現在選ばれているBBoxのリスト

	public List<string> ChoosedBboxNameList {	//現在選ばれているBBoxの名前のリスト
		set {
			if (choosedBboxNameList.Count == (int)CurrentFace)
			{
				choosedBboxNameList.Add(new List<string>());
			}
			choosedBboxNameList[(int)CurrentFace] = value;
		}
		get {
			if (choosedBboxNameList.Count == (int)CurrentFace)
			{
				return new List<string>();
			}
			return choosedBboxNameList[(int)CurrentFace];
		}
	}
	public List<string> SendCoordinateList {	//送信する座標リスト
		set {
			if (sendCoordList.Count == (int)CurrentFace)
			{
				sendCoordList.Add(new List<string>());
			}
			sendCoordList[(int)CurrentFace] = value;
		}
		get {
			return sendCoordList[(int)CurrentFace];
		}
	}

	public Dictionary<GameObject, DetectionInfo> DetectionLinkedAddedBbox {	//編集モードで追加したBBoxとその情報
		set {
			for (int i = addedBboxDetectionList.Count; i <= (int)CurrentFace; i++)
			{
				addedBboxDetectionList.Add(new Dictionary<GameObject, DetectionInfo>());
			}
			addedBboxDetectionList[(int)CurrentFace] = value;
		}
		get {
			for (int i = addedBboxDetectionList.Count; i <= (int)CurrentFace; i++)
			{
				addedBboxDetectionList.Add(new Dictionary<GameObject, DetectionInfo>());
			}
			return addedBboxDetectionList[(int)CurrentFace];
		}
	}

	public List<string> RemovedNameList {	//編集モードで削除したBBoxの名前リスト
		set {
			for (int i = removedNameList.Count; i <= (int)CurrentFace; i++)
			{
				removedNameList.Add(new List<string>());
			}
			removedNameList[(int)CurrentFace] = value;
		}
		get {
			for (int i = removedNameList.Count; i <= (int)CurrentFace; i++)
			{
				removedNameList.Add(new List<string>());
			}
			return removedNameList[(int)CurrentFace];
		}
	}
	public int ImgWidth {	//画像の横のピクセル数
		get { return imgWidth; }
	}

	public int ImgHight {	//画像の縦のピクセル数
		get { return imgHeight; }
	}

	public bool IsSeted { set; get; }   //一度BBoxを設置しているか

	// 以下メソッド

	/// <summary>
	/// 次の菌床面に移動するときの初期化処理
	/// </summary>
	public void NextInit()
	{
		if (CurrentFace == KinshoFace.Light)
			WriteAndReload();
		CleanBbox();
		CurrentFace = CurrentFace + 1;
		if (CurrentFace > loadedFace)
		{
			IsSeted = false;
			loadedFace = CurrentFace;
		}
		ProcessPhase = Phase.SetBBox;
	}

	/// <summary>
	/// 前の菌床面に戻るときの初期化処理 
	/// </summary>
	public void BackInit()
	{
		CleanBbox();
		CurrentFace = CurrentFace - 1;
		ProcessPhase = Phase.SetBBox;
	}

	/// <summary>
	/// バイナリデータからスプライトを生成
	/// </summary>
	/// <param name="path">画像ファイルのパス</param>
	/// <returns>スプライト形式の画像</returns>
	public Sprite CreateSprite(string path)
	{
		byte[] readBinary = ReadFile(path);
		if (readBinary == null)
		{
			return null;
		}
		Texture2D texture = new Texture2D(1, 1);
		texture.LoadImage(readBinary);
		Sprite createdSprite = Sprite.Create(texture, new Rect(0, 0, ImgWidth, ImgHight), new Vector2(0.5f, 0.5f),60f);
		return createdSprite;
	}

	/// <summary>
	/// 画像を読み込んでバイナリデータにする
	/// </summary>
	/// <param name="path">画像ファイルのパス</param>
	/// <returns>画像のバイナリデータ</returns>
	byte[] ReadFile(string path)
	{
		byte[] values = new byte[1];
		try
		{
			FileStream fileStream = new FileStream(path, FileMode.Open, FileAccess.Read);
			BinaryReader bin = new BinaryReader(fileStream);
			values = bin.ReadBytes((int)bin.BaseStream.Length);
			bin.Close();
		}
		catch (FileNotFoundException e)
		{
			return null;
		}

		return values;
	}

	/// <summary>
	/// 現在の菌床のBBox情報を登録
	/// </summary>
	/// <param name="pathList">XMLファイルのパスのリスト</param>
	/// <returns>検出データのリスト</returns>
	public List<DetectionInfo> ExtractDetectionData(List<string> pathList)
	{
		//List<DetectionInfo> detectionList = new List<DetectionInfo>();
　		string xmlPath = pathList[(int)CurrentFace];
		var fsXml = new FileStream(xmlPath, FileMode.Open);
		var serializer = new XmlSerializer(typeof(RootObject));
		var root = (RootObject)serializer.Deserialize(fsXml);
		return root.Detections;
	}

	/// <summary>
	/// コントローラーによるレイヤー順の並び替え
	/// </summary>
	public void SortLayerControl()
	{
		if (OVRInput.GetDown(OVRInput.Button.PrimaryThumbstickDown, OVRInput.Controller.RTouch))
		{
			if (isAscendingOrder)
			{
				foreach (GameObject bbox in AllDetectionDict.Keys)
				{
					bbox.transform.SetAsFirstSibling();
				}
			}
			else
			{
				foreach (GameObject bbox in AllDetectionDict.Keys)
				{
					bbox.transform.SetAsLastSibling();
				}
			}
			isAscendingOrder = !isAscendingOrder;
		}
	}

	/// <summary>
	/// BBoxが選択された時に呼び出される
	/// </summary>
	/// <param name="bbox">BBox</param>
	public void bboxChoosed(GameObject bbox)
	{
		UnityEngine.Debug.Log("pressed");
		if (ProcessPhase == Phase.ChooseMode)
		{
			cloneFlag = true;
			targetEffect.GetComponent<lockOn>().setTrriger(true, bbox.transform.localPosition);
			GameObject self = bbox.transform.GetChild(0).gameObject;
			self.SetActive(!self.activeSelf);	//BBoxをハイライトする・ハイライト解除する
		}
		else if (ProcessPhase == Phase.WaitEditMode)
		{
			ProcessPhase = Phase.ChangeEditMode;
		}
		CurrentChooseBbox = bbox;
	}

	/// <summary>
	/// BBoxを破棄もしくは非表示にする
	/// </summary>
	void CleanBbox()
	{
		foreach (GameObject bbox in AllDetectionDict.Keys)
		{
			if (bbox.tag == "Added")
			{
				bbox.SetActive(false);
			}
			else
			{
				Destroy(bbox);
			}

		}
	}

	/// <summary>
	/// 送信する座標をファイルに書き出してシステムをリセット
	/// </summary>
	public void WriteAndReload()
	{
		//ファイルに書き出し
		string Coordinate = "";
		string path = $@"{SystemPath}\Export\";	
		File.WriteAllText(path + "Chose.txt", Coordinate);
		for (int i = 0; i < 4; i++)
		{
			CurrentFace = (KinshoFace)i;
			if (SendCoordinateList.Count > 0)
			{
				Coordinate = String.Join("\n", SendCoordinateList);
				Coordinate += "\n";
				File.AppendAllText(path + "Chose.txt", Coordinate);
			}
		}

		SendCoordinate();
		screenChange.isLoadUIDestoroy=false;
		//システムリセット
		SceneManager.LoadScene(SceneManager.GetActiveScene().name);
	}

	/// <summary>
	/// Pythonプログラムを起動して座標を送信
	/// </summary>
	public void SendCoordinate()
	{
		var sendStartInfo = new ProcessStartInfo("cmd.exe", $@"/k cd {SystemPath}\system & py send_coord.py");
		sendStartInfo.CreateNoWindow = true;
		sendStartInfo.UseShellExecute = false;
		Process.Start(sendStartInfo);
	}

}

// 以下クラス及び列挙体

/// <summary>
/// 検出データ
/// </summary>
public class DetectionInfo
{
	[XmlElement("className")]
	public string ClassName { get; set; }  //判別した成長度の名前
	[XmlElement("confidence")]
	public float Confidence { get; set; }  //判別の信頼度
	[XmlElement("xCenter")]
	public float XCenter { get; set; }  //BBoxの中心のX座標
	[XmlElement("yCenter")]
	public float YCenter { get; set; }  //BBoxの中心のＹ座標
	[XmlElement("width")]
	public float Width { get; set; } //BBoxの幅
	[XmlElement("height")]
	public float Height { get; set; }	//BBoxの高さ
	[XmlElement("angle")]
	public int Angle { get; set; }	//収穫時の角度
}

/// <summary>
/// XMLから読み込むリスト
/// </summary>
[XmlRoot("detections")]
public class RootObject
{
	[XmlElement("detection")]
	public List<DetectionInfo> Detections { get; set; }
}

///// <summary>
///// 本システムの処理の段階
///// </summary>
//public enum Phase	
//{
//	Start, Load, CheckMode, SetBBox, ChooseMode, WaitEditMode, AddEditMode, ChangeEditMode, EndUI, Record
//}

/// <summary>
/// 本システムの処理の段階
/// </summary>
public enum Phase
{
    Start, Load, Kinsho, SetBBox, ChooseMode, WaitEditMode, AddEditMode, ChangeEditMode, EndUI, Record
}

/// <summary>
/// 現在の菌床の面
/// </summary>
public enum KinshoFace	
{
	Front, Right, Back, Light
}
