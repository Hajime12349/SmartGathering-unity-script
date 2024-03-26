using UnityEngine;
using UnityEngine.UI;

public class ChangeBB : MonoBehaviour
{
	[SerializeField] GameObject controlar;	//画面上のコントローラー

	Main main;	//メインスクリプト

	int addCount = 1;	//追加した菌床数
	bool isStart = true;	//編集開始時の処理
	bool colorFlag; //選択時のグラフィックを表示するか
	GameObject previewBox;  //編集中に表示するBBoxの元
	GameObject boxClone;	//編集中に表示するBBox
	GameObject bboxAdded;	//追加するBBox
	Vector3 boxSize;	//編集中に表示するBBoxのサイズ

	// Start is called before the first frame update
	void Start()
	{
		main = this.GetComponent<Main>();
		previewBox = Resources.Load<GameObject>("Panel");
		bboxAdded = Resources.Load<GameObject>("added");
	}

	// Update is called once per frame
	void Update()
	{
		if (main.ProcessPhase == Phase.ChangeEditMode || main.ProcessPhase == Phase.AddEditMode)
		{
			if (isStart)    //編集開始時
			{
				colorFlag = false;

				//見た目だけのBBoxを生成 
				boxClone = Instantiate(previewBox, main.BboxCanvas);

				//既存のBBoxを選択した場合
				if (main.ProcessPhase == Phase.ChangeEditMode && main.CurrentChooseBbox != null)
				{
					colorFlag = main.CurrentChooseBbox.transform.GetChild(0).gameObject.activeSelf;
					boxSize = main.CurrentChooseBbox.transform.localScale;
					//選択したBBoxが収穫選択されていた場合、収穫選択リストから削除
					if (colorFlag)
					{
						main.ChoosedBboxList.Remove(main.CurrentChooseBbox);
					}
					//既存のBBoxを削除
					if (main.CurrentChooseBbox.tag == "added")
					{
						main.CurrentChooseBbox.SetActive(false);
					}
					else
					{
						Destroy(main.CurrentChooseBbox);
					}
					main.AllDetectionDict.Remove(main.CurrentChooseBbox);
					main.RemovedNameList.Add(main.CurrentChooseBbox.name);
					main.CurrentChooseBbox = null;
				}
				//BBoxを新規作成する
				else if (main.ProcessPhase == Phase.AddEditMode)
				{
					boxSize = new Vector3(0.2f, 0.2f, 1);
				}
				addCount++;
				isStart = false;
			}
			else
			{
				Vector3 boxPosition = LaserRay();

				//背景のフレーム内にレーザーがあるなら
				if (boxPosition.x > -999)
				{
					//コントローラーのスティックでBBoxのサイズを変更
					Vector3 stick = OVRInput.Get(OVRInput.RawAxis2D.RThumbstick);
					boxSize += stick * 0.005f;
					boxSize = new Vector3(Mathf.Max(0.02f, boxSize.x), Mathf.Max(0.02f, boxSize.y), 1);
					boxSize = new Vector3(Mathf.Min(0.98f, boxSize.x), Mathf.Min(0.98f, boxSize.y), 1);
					boxPosition = new Vector3(Mathf.Min(boxPosition.x, 5 - boxSize.x * 5 - 0.02f), Mathf.Min(boxPosition.y, 5 - boxSize.y * 5 - 0.02f), 0);
					boxPosition = new Vector3(Mathf.Max(boxPosition.x, -5 + boxSize.x * 5 + 0.02f), Mathf.Max(boxPosition.y, -5 + boxSize.y * 5 + 0.02f), 0);
					boxClone.transform.localPosition = boxPosition;
					boxClone.transform.localScale = boxSize;

					//Aボタンで確定
					if ((OVRInput.GetDown(OVRInput.RawButton.A)))
					{
						//BBoxの位置とサイズを算出
						Vector2 addCenter = new Vector2((boxClone.transform.localPosition.x + 5) / 10 * main.ImgWidth, (-(boxClone.transform.localPosition.y) + 5) / 10 * main.ImgHight);
						Vector2 addSize = new Vector2(boxClone.transform.localScale.x * main.ImgWidth, boxClone.transform.localScale.y * main.ImgHight);

						//選択できるBBoxを生成・配置
						GameObject AddBB = Instantiate(bboxAdded, main.BboxCanvas);
						AddBB.GetComponent<Button>().onClick.AddListener(() => main.bboxChoosed(AddBB));
						AddBB.tag = "Added";
						AddBB.name = $"Added{addCount}";
						AddBB.transform.localPosition = boxClone.transform.localPosition;
						AddBB.transform.localScale = boxClone.transform.localScale;
						//選択されていたBBoxを変更した場合は選択処理
						if (colorFlag)
						{
							main.ChoosedBboxList.Add(AddBB);
							AddBB.transform.GetChild(0).gameObject.SetActive(true);
						}
						//見た目だけのBBoxを削除
						Destroy(boxClone);

						//追加したBBoxの情報を登録する
						DetectionInfo det = new DetectionInfo { ClassName = "added", Confidence = 100, XCenter = addCenter.x, YCenter = addCenter.y, Width = addSize.x, Height = addSize.y };
						main.AllDetectionDict.Add(AddBB, det);
						main.DetectionLinkedAddedBbox.Add(AddBB, det);

						isStart = true;
						main.ProcessPhase = Phase.ChooseMode;
					}
					//中止
					else if ((OVRInput.GetDown(OVRInput.RawButton.RHandTrigger)))
					{
						Destroy(boxClone);
						main.ProcessPhase = Phase.ChooseMode;
						isStart = true;
					}
				}
			}
		}
	}

	/// <summary>
	/// コントローラからレイを飛ばして画面上の座標を取得
	/// </summary>
	/// <returns>画面上の座標</returns>
	Vector3 LaserRay()
	{
		RaycastHit hit = new RaycastHit();
		Vector3 origin, direction;
		origin = controlar.transform.position;
		direction = controlar.transform.forward;
		Ray ray = new Ray(origin, direction);
		//レイを飛ばして背景のフレームに当たったら
		if (Physics.Raycast(ray, out hit, 10.0f))
		{
			if (hit.collider.tag == "BackChoose")
			{
				return hit.point;
			}
		}
		//当たらなかったら
		return new Vector3(-999, 0, 0);
	}
}
