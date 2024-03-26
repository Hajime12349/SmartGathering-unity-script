using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class UIController : MonoBehaviour
{
	[SerializeField] GameObject chooseEndUI,resetUI, startUI, explainUI,kinshoUI;	//選択終了UI、メニューUI、説明UI、菌床一覧UI
	[SerializeField] GameObject detectedInfoUI, modeUI;	//BBox情報UI,モード通知UI
	[SerializeField] GameObject dummyButton;	//フォーカス指定用のダミーボタン
	[SerializeField] Button startButton, expButton,endButton;	//メニューにあるボタン（菌床をみる、使い方、終了）
	[SerializeField] Transform kinshoList;	//菌床一覧画面の菌床ボタンリスト
	[SerializeField] List<Text> bboxCountTxtList;   //BBox情報UIの各BBoxのテキスト
	[SerializeField] GameObject LoadUIList;
	[SerializeField] GameObject maskEvent;
	[SerializeField] GameObject iconGirl;
	[SerializeField] GameObject iconBoy;
    [SerializeField] GameObject kikurageFolder;

    Main main;  //メインスクリプト
	Sprite viewMode, chooseMode, editMode;	//モード通知UIの画像（確認モード、選択モード、編集モード）
	Image modeImg; //現在のモードUI

	Dictionary<string, int> BboxCount = new Dictionary<string, int>() {	//BBox情報UIの各BBoxの数
		{ "harvestable", 0},
		{ "soon", 0 },
		{ "not_yet", 0 },
		{ "added", 0 }
	};
	Vector3 uiPosition;

	// Start is called before the first frame update
	void Start()
	{
		uiPosition=LoadUIList.transform.position;
		LoadUIList.transform.position = new Vector3(100, 0, 0);
		main = this.GetComponent<Main>();

		viewMode = Resources.Load<Sprite>("ViewMode");
		chooseMode = Resources.Load<Sprite>("ChooseMode");
		editMode = Resources.Load<Sprite>("EditMode");

		modeImg = modeUI.GetComponent<Image>();

		//スクリーンの解像度を変更
		Screen.SetResolution(1000, 1000, true);

        LoadCenterImage();

    }

	
	// Update is called once per frame
	void Update()
	{
		if (screenChange.isLoadUIDestoroy)
		{
			LoadUIList.SetActive(false);
		}
		//ダミーボタンを常にフォーカス
		//GameObject selectedButton = EventSystem.current.currentSelectedGameObject;
		//if (selectedButton != null)
		//{
		//	EventSystem.current.SetSelectedGameObject(dummyButton);
		//}

		//選択終了時UIの表示
		if (main.ProcessPhase == Phase.EndUI)
		{
			//if (main.CurrentFace == KinshoFace.Light)
			//{
			//	chooseEndUI.SetActive(true);
			//}
			//else
			//{
			//	main.ProcessPhase = Phase.Record;
			//}
		}
		else if (chooseEndUI.activeSelf)
		{
			chooseEndUI.SetActive(false);
		}

		//モードUIを編集モードに
		if (main.ProcessPhase == Phase.WaitEditMode && modeImg.sprite != editMode)
		{
			modeImg.sprite = editMode;
		}

		////モードUIを確認モードに
		//if (main.ProcessPhase == Phase.CheckMode && (modeImg.sprite != viewMode || !modeUI.activeSelf))
		//{
		//	modeUI.SetActive(true);
		//	modeImg.sprite = viewMode;
		//}
		//if (main.ProcessPhase == Phase.ChooseMode && !detectedInfoUI.activeSelf)
		//{
		//	//モードUIを選択モード
		//	modeImg.sprite = chooseMode;

		//	//検出の種類ごとのBBをカウントしてBBox情報UIに表示
		//	foreach (DetectionInfo det in main.AllDetectionDict.Values)
		//	{
		//		string className = det.ClassName;
		//		BboxCount[className]++;
		//	}
		//	bboxCountTxtList[0].text = BboxCount["harvestable"].ToString();
		//	bboxCountTxtList[1].text = BboxCount["soon"].ToString();
		//	bboxCountTxtList[2].text = BboxCount["not_yet"].ToString();
		//	bboxCountTxtList[3].text = BboxCount["added"].ToString();
		//	detectedInfoUI.SetActive(true);
		//}
		//else if (main.ProcessPhase != Phase.ChooseMode && detectedInfoUI.activeSelf)
		//{
		//	//BBox情報UIを非表示にして初期化
		//	var BBkeys = new List<string>(BboxCount.Keys);
		//	foreach (string className in BBkeys)
		//	{
		//		BboxCount[className] = 0;
		//	}
		//	detectedInfoUI.SetActive(false);
		//}
	}

    void LoadCenterImage()
    {
        main.LoadedCenterImgList = new List<Sprite>();
        string imgDir = $@"{main.SystemPath}\StreamingAssets\images";
        //菌床数
        int imgFolderCount = Directory.GetDirectories(imgDir, "Kinsho_*").Length;

        for (int i = 1; i <= imgFolderCount; i++)
        {
            //正面中央画像のスプライト画像を生成
            string imgStr = $@"{imgDir}\Kinsho_{i}\IMG_9.jpg";
            Sprite centerImg = main.CreateSprite(imgStr);
            main.LoadedCenterImgList.Add(centerImg);
        }
    }


    //菌床一覧UIに菌床を配置
    void SetListKinsho()
	{
		int count= 0;
		foreach (Sprite centerImg in main.LoadedCenterImgList)
		{
			Transform kinsho=kinshoList.GetChild(count);
			//kinsho.GetComponent<Button>().interactable = true;
			kinsho.GetComponent<SpriteRenderer>().sprite = centerImg;
			//kinsho.GetChild(0).gameObject.SetActive(true);
			kinsho.GetChild(0).GetChild(0).GetComponent<Text>().text = $"菌床：{count+1}";
			count++;
		}
	}

	//メニューのボタンを無効化・有効化
	void SwitchStartButton()
	{
		startButton.interactable = !startButton.interactable;
		expButton.interactable = !expButton.interactable;
		endButton.interactable = !endButton.interactable;
	}

	//選択終了UIの”はい”ボタンが押されたら
	public void OnClickEndChoose()
	{
		main.ProcessPhase = Phase.Record;
		chooseEndUI.SetActive(false);
	}

	public void OnClickToMenu()
	{
		main.SendCoordinate();
		screenChange.isLoadUIDestoroy = false;
		SceneManager.LoadScene(SceneManager.GetActiveScene().name);
	}

	public void OnClickToMenuNo()
	{
		resetUI.SetActive(false);
	}

	//選択終了UIの”いいえ”ボタンが押されたら
	public void OnClickContinue()
	{
		main.ProcessPhase = Phase.ChooseMode;
        foreach (Transform child in kikurageFolder.transform)
        {
            child.GetComponent<BoxCollider>().enabled = true;
        }
        chooseEndUI.SetActive(false);
	}

	//メニューの”菌床を見る”ボタンが押されたら
	public void OnClickLook()
	{
		kinshoUI.SetActive(true);
		SetListKinsho();
		SwitchStartButton();
	}

	//菌床一覧UIの”メニューに戻る”ボタンが押されたら
	public void OnClickEndLook()
	{
		kinshoUI.SetActive(false);
		SwitchStartButton();
	}

	//菌床一覧UIの菌床が押されたら
	public void OnClickKinsho(int number)
	{
		LoadUIList.SetActive(true);
		LoadUIList.transform.position = uiPosition;
		maskEvent.GetComponent<screenChange>().setTrriger(-1);
		iconGirl.GetComponent<changeIcon>().setTrriger(true);
		iconBoy.GetComponent<changeIcon>().setTrriger(true);
		main.ProcessPhase = Phase.Load;
		main.currentKinshoCount = number;
		kinshoUI.SetActive(false);
		startUI.SetActive(false);
	}



	//メニューの”使い方”ボタンが押されたら
	public void OnClickStartExplain()
	{
		explainUI.SetActive(true);
		SwitchStartButton();

	}

	//説明UIのメニューに戻るボタンが押されたら
	public void OnClickEndExplain()
	{
		explainUI.SetActive(false);
		SwitchStartButton();
	}

	//メニューの終了ボタンが押されたら
	public void OnClickEnd()
	{
		Application.Quit();
	}

    public void EndChooseButton()
    {
        chooseEndUI.SetActive(true);
        foreach(Transform child in kikurageFolder.transform)
        {
            child.GetComponent<BoxCollider>().enabled = false;
        }
        main.ProcessPhase = Phase.EndUI;
    }

}
