using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using static UnityEngine.Networking.UnityWebRequest;

public class UI_KartSelectController : MonoBehaviour
{
    [Header("UI 설정")]
    [SerializeField] int m_visibleSlotsCount;
    [SerializeField] float m_slotsSpacing;
    [SerializeField] float m_kartRotateSpeed;
    [SerializeField, Range(1f, 5f)] float m_horizontalSpeed;
    [SerializeField, Range(1f, 5f)] float m_verticalSpeed;
    

    [Header("카트 설정")]
    //Ride Height
    [SerializeField, Range(0f, 1f)] float m_roadsterTireWidth;
    [SerializeField, Range(0f, 1f)] float m_4x4TireWidth;
    [SerializeField, Range(0f, 1f)] float m_muscleTireWidth;
    [SerializeField] float m_maxTireWidth;
    //Camber
    float m_minCamber;
    float m_maxCamber;

    GameObject[] m_rightLeftWheelArr = new GameObject[4];

    List<RaycastResult> resultList = new List<RaycastResult>();
    List<GameObject> m_bodyPrefabList = new List<GameObject>();
    List<GameObject> m_fwPrefabList = new List<GameObject>();
    List<GameObject> m_rwPrefabList = new List<GameObject>();
    List<Image> m_bodySlotList = new List<Image>();
    List<Image> m_fwSlotList = new List<Image>();
    List<Image> m_rwSlotList = new List<Image>();
    List<Sprite> m_bodyIconList = new List<Sprite>();
    List<Sprite> m_wheelIconList = new List<Sprite>();

    PointerEventData m_ped = new PointerEventData(null);
    GraphicRaycaster m_gr;
    Camera m_mainCam;
    Canvas m_canvas;
    GameObject m_IconSlotPrefab;
    Transform m_kartPreview;
    RectTransform m_selectedSlotRect;
    RectTransform m_bodiesRect;
    RectTransform m_fwRect;
    RectTransform m_rwRect;

    Vector3 m_roadsterFWPos = new Vector3(0f, .3f, .98f);
    Vector3 m_roadsterRWPos = new Vector3(0f, .3f, -.66f);
    Vector3 m_4x4FWPos = new Vector3(0f, .39f, .91f);
    Vector3 m_4x4RWPos = new Vector3(0f, .39f, -.52f);
    Vector3 m_muscleFWPos = new Vector3(0f, .26f, 1.11f);
    Vector3 m_muscleRWPos = new Vector3(0f, .48f, -.97f);
    Vector3 m_mouseStartPos;
    Vector3 m_slotInitPos;
    Vector3 m_dirH;
    Vector3 m_dirV;

    float m_roadsterWheelScale = 1f;
    float m_4x4WheelScale = 1.25f;
    float m_muscleFWScale = .83f;
    float m_muscleRWScale = 1.51f;
    float m_mouseDragValue;
    float m_rotY;
    int m_curBodyCount = 0;
    int m_curFWCount = 0;
    int m_curRWCount = 0;
    bool m_inputEnabled = true;
    bool m_isMouseDrag = false;

    enum BodyType
    {
        None = -1,
        Roadster = 0,
        Tuner = 1,
        Racer = 2,
        Kraken = 3,
        Defender = 4,
        Boxer = 5,
        Beast = 6,
        Monster = 7,
        Cruiser = 8,
        Thrasher = 9,
        Sprinter = 10,
        Phoenix = 11,
        Max
    }

    void Awake()
    {
        if (m_visibleSlotsCount % 2 == 0)
        {
            Debug.LogError("슬롯의 개수는 홀수여야 합니다.");
            return;
        }

        m_mainCam = Camera.main;
        m_canvas = GetComponent<Canvas>();
        m_gr = m_canvas.GetComponent<GraphicRaycaster>();
        m_selectedSlotRect = transform.Find("SelectedSlot").GetComponent<RectTransform>();
        m_kartPreview = transform.Find("KartPreview");

        InitSlots("Select/Bodies/IconSlots", out m_bodiesRect);
        InitSlots("Select/FrontWheels/IconSlots", out m_fwRect);
        InitSlots("Select/RearWheels/IconSlots", out m_rwRect);

        InitPrefabs(m_bodyPrefabList, m_curBodyCount, "Prefabs/Bodies", "KartPreview/Bodies");
        InitPrefabs(m_fwPrefabList, m_curFWCount, "Prefabs/Wheels", "KartPreview/FrontWheels");
        InitPrefabs(m_rwPrefabList, m_curRWCount, "Prefabs/Wheels", "KartPreview/RearWheels");

        InitIcons(m_bodyIconList, "Icons/Bodies");
        InitIcons(m_wheelIconList, "Icons/Wheels");
        InitIconsPos(m_bodySlotList, m_bodyIconList, m_bodiesRect, "Prefabs/BodyIconSlot");
        InitIconsPos(m_fwSlotList, m_wheelIconList, m_fwRect, "Prefabs/WheelIconSlot");
        InitIconsPos(m_rwSlotList, m_wheelIconList, m_rwRect, "Prefabs/WheelIconSlot");

        GetWheels();
        AdjustWheels();

        m_selectedSlotRect.position = m_bodiesRect.position;
    }
    void Update()
    {
        if (InputManager.Instance.InputForward && m_inputEnabled)
        {
            m_dirV = -Vector3.up;
            SelectVerticalSlot();
        }
        if (InputManager.Instance.InputBackward && m_inputEnabled)
        {
            m_dirV = -Vector3.down;
            SelectVerticalSlot();
        }
        if (InputManager.Instance.InputRight && m_inputEnabled)
        {
            m_dirH = Vector3.right;
            SelectHorizontalSlot();
        }
        if (InputManager.Instance.InputLeft && m_inputEnabled)
        {
            m_dirH = Vector3.left;
            SelectHorizontalSlot();
        }
        if (Input.GetMouseButtonDown(0))
        {
            m_ped.position = Input.mousePosition;
            m_gr.Raycast(m_ped, resultList);
            if (resultList.Count > 0)
            { 
                for (int i=0; i<resultList.Count; i++)
                {
                    if (resultList[i].gameObject.CompareTag("Ray"))
                    {
                        m_mouseStartPos = m_mainCam.ScreenToWorldPoint(Input.mousePosition + new Vector3(0f, 0f, 1f));
                        m_isMouseDrag = true;
                        resultList.Clear();
                        break;
                    }
                }
            }
        }
        if (Input.GetMouseButtonUp(0))
        {
            m_isMouseDrag = false;
            m_mouseDragValue = 0f;
        }
        if (m_isMouseDrag)
        {
            var endPos = m_mainCam.ScreenToWorldPoint(Input.mousePosition  + new Vector3(0f, 0f, 1f));
            var resultPos = m_mouseStartPos - endPos;
            m_mouseDragValue = resultPos.x;
            m_mouseStartPos = endPos;
        }
        m_rotY += m_mouseDragValue * m_kartRotateSpeed * Time.deltaTime * 1000f;
        m_kartPreview.localRotation = Quaternion.Euler(0f, m_rotY, 0f);
    }

    void InitSlots(string str, out RectTransform slots)
    {
        slots = transform.Find(str).GetComponent<RectTransform>();
        m_slotInitPos = slots.anchoredPosition;
    }
    void InitPrefabs(List<GameObject> prefabList, int curCount, string prefab, string preview)
    {
        var prefabs = Resources.LoadAll<GameObject>(prefab);
        for (int i = 0; i < prefabs.Length; i++)
        {
            var obj = Instantiate(prefabs[i]);
            prefabList.Add(obj);
            obj.transform.SetParent(transform.Find(preview));
            obj.transform.localPosition = Vector3.zero;
            obj.transform.localRotation = Quaternion.identity;
            obj.SetActive(false);
        }
        prefabList[curCount].SetActive(true);
    }
    void InitIcons(List<Sprite> iconList, string str)
    {
        var icons = Resources.LoadAll<Sprite>(str);
        for (int i = 0; i < icons.Length; i++)
        {
            iconList.Add(icons[i]);
        }
    }
    void InitIconsPos(List<Image> slotList, List<Sprite> iconList, RectTransform slotRect, string str)
    {
        m_IconSlotPrefab = Resources.Load<GameObject>(str);
        int val = (int)((m_visibleSlotsCount + 1) * .5f);
        for (int i = 0; i < m_visibleSlotsCount + 2; i++)
        {
            int idx = i < val ? iconList.Count + i - val : m_curBodyCount + i - val;
            var obj = Instantiate(m_IconSlotPrefab);
            obj.transform.SetParent(slotRect);
            slotList.Add(obj.GetComponent<Image>());
            slotList[i].GetComponent<RectTransform>().anchoredPosition = new Vector3(0f, (val - i) * m_slotsSpacing);
            slotList[i].sprite = iconList[idx];
        }
        slotList[0].gameObject.SetActive(false);
        slotList[slotList.Count - 1].gameObject.SetActive(false);
    }

    void SelectVerticalSlot()
    {
        if (m_selectedSlotRect.position.x == m_bodiesRect.position.x)
        {
            CoroutineVertical(m_bodyPrefabList, m_bodySlotList, m_bodyIconList, m_bodiesRect, m_curBodyCount, out m_curBodyCount);
            AdjustWheels();
        }
        else if (m_selectedSlotRect.position.x == m_fwRect.position.x)
        {
            CoroutineVertical(m_fwPrefabList, m_fwSlotList, m_wheelIconList, m_fwRect, m_curFWCount, out m_curFWCount);
            AdjustWheels();
        }
        else
        {
            CoroutineVertical(m_rwPrefabList, m_rwSlotList, m_wheelIconList, m_rwRect, m_curRWCount, out m_curRWCount);
            AdjustWheels();
        }
    }
    void CoroutineVertical(List<GameObject> prefabList, List<Image> slotList, List<Sprite> iconList, RectTransform rect, int curCountPrev, out int curCount)
    {
        int plusMinus = (m_dirV == Vector3.up) ? 1 : -1;
        int val = (m_dirV == Vector3.up) ? 0 : slotList.Count - 1;
        prefabList[curCountPrev].SetActive(false);
        slotList[val].gameObject.SetActive(true);
        curCountPrev += plusMinus;
        StartCoroutine(VerticalSlotAsync(slotList, iconList, rect, m_verticalSpeed, curCountPrev));
        if (curCountPrev < 0) curCountPrev = iconList.Count - 1;
        if (curCountPrev >= iconList.Count) curCountPrev = 0;
        prefabList[curCountPrev].SetActive(true);
        curCount = curCountPrev;
    }
    void SelectHorizontalSlot()
    {
        if (m_dirH == Vector3.right && m_selectedSlotRect.position.x != m_rwRect.position.x)
        {
            StartCoroutine(HorizontalSlotAsync(m_rwRect, m_horizontalSpeed));
        }
        else if (m_dirH == Vector3.left && m_selectedSlotRect.position.x != m_bodiesRect.position.x)
        {
            StartCoroutine(HorizontalSlotAsync(m_bodiesRect, m_horizontalSpeed));
        }
    }

    void AdjustWheels()
    {
        if (m_curBodyCount >= (int)BodyType.Roadster && m_curBodyCount <= (int)BodyType.Kraken)
        {
            SetWheels(m_roadsterFWPos, m_roadsterRWPos, m_roadsterWheelScale, m_roadsterWheelScale, m_roadsterTireWidth);
        }
        else if (m_curBodyCount >= (int)BodyType.Defender && m_curBodyCount <= (int)BodyType.Monster)
        {
            SetWheels(m_4x4FWPos, m_4x4RWPos, m_4x4WheelScale, m_4x4WheelScale, m_4x4TireWidth);
        }
        else if (m_curBodyCount >= (int)BodyType.Cruiser && m_curBodyCount <= (int)BodyType.Phoenix)
        {
            SetWheels(m_muscleFWPos, m_muscleRWPos, m_muscleFWScale, m_muscleRWScale, m_muscleTireWidth);
        }
    }
    void GetWheels()
    {
        for (int i = 0; i < 2; i++)
        {
            m_rightLeftWheelArr[i] = m_fwPrefabList[m_curFWCount].transform.GetChild(i).gameObject;
        }
        for (int i = 2; i < 4; i++)
        {
            m_rightLeftWheelArr[i] = m_rwPrefabList[m_curRWCount].transform.GetChild(i - 2).gameObject;
        }
    }
    void SetWheels(Vector3 frontWheelPos, Vector3 rearWheelPos, float frontWheelScale, float rearWheelScale, float tireWidth)
    {
        m_fwPrefabList[m_curFWCount].transform.localPosition = frontWheelPos;
        m_rwPrefabList[m_curRWCount].transform.localPosition = rearWheelPos;
        m_fwPrefabList[m_curFWCount].transform.localScale = new Vector3(1f, frontWheelScale, frontWheelScale);
        m_rwPrefabList[m_curRWCount].transform.localScale = new Vector3(1f, rearWheelScale, rearWheelScale);
        GetWheels();
        for (int i = 0; i < m_rightLeftWheelArr.Length; i++)
        {
            float plusMinus = 1f;
            if (i == 1 || i == 3) plusMinus = -1f;
            m_rightLeftWheelArr[i].transform.localPosition = new Vector3(plusMinus * tireWidth, 0f, transform.localPosition.z);
        }
    }

    IEnumerator VerticalSlotAsync(List<Image> slotList, List<Sprite> iconList, RectTransform rect, float moveSpeed, int curCount)
    {
        m_inputEnabled = false;
        var curPos = rect.anchoredPosition.y;
        float plusMinus = (m_dirV == Vector3.down) ? 1f : -1f;
        while (plusMinus * rect.anchoredPosition.y > plusMinus * (curPos - plusMinus * m_slotsSpacing))
        {
            rect.transform.Translate(m_dirV * moveSpeed * Time.deltaTime * 1000f);
            yield return new WaitForEndOfFrame();
        }
        for (int i = 0; i < slotList.Count; i++)
        {
            int val = (int)((slotList.Count - 1) * .5f);
            int idx = curCount + i - val;
            if (idx < 0)
            {
                slotList[i].sprite = iconList[iconList.Count + i + curCount - val];
            }
            else if (idx > iconList.Count - 1)
            {
                slotList[i].sprite = iconList[idx - iconList.Count];
            }
            else
            {
                slotList[i].sprite = iconList[curCount + i - val];
            }
        }
        rect.anchoredPosition = m_slotInitPos;
        slotList[0].gameObject.SetActive(false);
        slotList[slotList.Count - 1].gameObject.SetActive(false);
        m_inputEnabled = true;
    }
    IEnumerator HorizontalSlotAsync(RectTransform rect, float moveSpeed)
    {
        m_inputEnabled = false;
        var ArrivePosX = (m_selectedSlotRect.position.x == m_fwRect.position.x) ? rect.position.x : m_fwRect.position.x;
        var plusMinus = (m_dirH == Vector3.left) ? 1f : -1f;
        while (plusMinus * m_selectedSlotRect.position.x > plusMinus * ArrivePosX)
        {
            m_selectedSlotRect.transform.Translate(m_dirH * moveSpeed * Time.deltaTime * 1000f);
            yield return new WaitForEndOfFrame();
        }
        m_selectedSlotRect.position = new Vector3(ArrivePosX, transform.position.y);
        m_inputEnabled = true;
    }
}