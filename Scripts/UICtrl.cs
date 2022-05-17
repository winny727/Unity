using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UICtrl : MonoBehaviour
{
    private InputField[,] mInputFields;
    private List<InputField> mPool;

    private int mCurLen;
    private int mGridLen;
    private Transform mGridParent;
    private Transform[,] mInputParents;

    private GameObject mInputFieldPrefab;
    private GameObject mGridPrefab;

    private int[,] mCurArr;
    private int mSolveIndex;
    private bool mIsCurError = false;

    private Slider mSlider;
    private Button mBtn_Last;
    private Button mBtn_Next;
    private Button mBtn_Clear;
    private Button mBtn_ClearInput;
    private Button mBtn_Solve;
    private Text mText_Log;

    private void Awake()
    {
        mGridParent = transform.Find("InputArea");
        mPool = new List<InputField>();
        mCurLen = SudokuSolve.Len;
        mGridLen = (int)Mathf.Sqrt(mCurLen);

        mInputFieldPrefab = Resources.Load<GameObject>("InputField");
        mGridPrefab = Resources.Load<GameObject>("Grid");
    }

    private void Start()
    {
        InitButton();
        InitText();
        InitSlider();
        CreateGrid();
        CreateInputFields(mCurLen);
        UpdateUI();
    }

    private void InitButton()
    {
        mBtn_Last = transform.Find("Button_Last").GetComponent<Button>();
        mBtn_Next = transform.Find("Button_Next").GetComponent<Button>();
        mBtn_Clear = transform.Find("Button_Clear").GetComponent<Button>();
        mBtn_ClearInput = transform.Find("Button_ClearInput").GetComponent<Button>();
        mBtn_Solve = transform.Find("Button_Solve").GetComponent<Button>();

        mBtn_Last.onClick.AddListener(ShowLast);
        mBtn_Next.onClick.AddListener(ShowNext);
        mBtn_Clear.onClick.AddListener(Clear);
        mBtn_ClearInput.onClick.AddListener(ClearInput);
        mBtn_Solve.onClick.AddListener(Solve);
    }

    private void InitText()
    {
        mText_Log = transform.Find("Text_Log").GetComponent<Text>();
    }

    private void InitSlider()
    {
        Transform sliderTrans = transform.Find("Slider_MaxSolveCount");
        Transform inputTrans = sliderTrans.Find("InputField");
        mSlider = sliderTrans.GetComponent<Slider>();
        InputField inputField = inputTrans.GetComponent<InputField>();

        mSlider.onValueChanged.AddListener((value) =>
        {
            value = (int)value;
            string valueStr = value.ToString();
            if (inputField.text != valueStr)
            {
                inputField.text = valueStr;
            }
        });

        inputField.onValueChanged.AddListener((value) =>
        {
            if (value == "" || value.StartsWith("0"))
            {
                string minStr = mSlider.minValue.ToString();
                inputField.text = minStr;
                value = minStr;
            }

            int valueInt = int.Parse(value);
            if (mSlider.value != valueInt)
            {
                mSlider.value = valueInt;
            }
        });
    }

    private void CreateGrid()
    {
        mInputParents = new Transform[mGridLen, mGridLen];

        for (int i = 0; i < mGridLen; i++)
        {
            for (int j = 0; j < mGridLen; j++)
            {
                GameObject go = Instantiate(mGridPrefab);
                go.name = "Grid_" + i + "_" + j;
                Transform trans = go.transform;
                trans.SetParent(mGridParent);
                trans.localScale = Vector3.one;
                mInputParents[i, j] = trans;
            }
        }
    }

    private void CreateInputFields(int len)
    {
        if (mInputFields != null)
        {
            for (int i = 0; i < mCurLen; i++)
            {
                for (int j = 0; j < mCurLen; j++)
                {
                    InputField inputField = mInputFields[i, j];
                    if (inputField != null)
                    {
                        Recycle(inputField);
                        mInputFields[i, j] = null;
                    }
                }
            }
        }

        mCurLen = len;
        mInputFields = new InputField[len, len];
        for (int i = 0; i < len; i++)
        {
            for (int j = 0; j < len; j++)
            {
                InputField inputField = Allocate();
                SetInputField(inputField, i, j);
                mInputFields[i, j] = inputField;
            }
        }
    }

    private InputField Allocate()
    {
        if (mPool.Count == 0)
        {
            return CreateInputField();
        }
        else
        {
            InputField inputField = mPool[mPool.Count - 1];
            inputField.gameObject.SetActive(true);
            mPool.RemoveAt(mPool.Count - 1);
            return inputField;
        }
    }

    private void Recycle(InputField inputField)
    {
        if (inputField != null && !mPool.Contains(inputField))
        {
            inputField.gameObject.SetActive(false);
            mPool.Add(inputField);
        }
        else
        {
            Debug.LogError("InputField Error: " + inputField);
        }
    }

    private InputField CreateInputField()
    {
        GameObject go = Instantiate(mInputFieldPrefab);
        go.transform.SetParent(mGridParent);
        go.transform.localScale = Vector3.one;
        InputField inputField = go.GetComponent<InputField>();
        inputField.onValueChanged.AddListener((val) =>
        {
            if (mIsCurError)
            {
                mIsCurError = false;
                ClearColor();
            }

            if (inputField.text == "0")
            {
                inputField.text = "";
            }
        });
        return inputField;
    }

    private void SetInputField(InputField inputField, int row, int col)
    {
        int x = row / mGridLen;
        int y = col / mGridLen;
        inputField.transform.SetParent(mInputParents[x, y]);
        inputField.gameObject.name = "Input_" + row + "_" + col;
    }

    private void ShowLast()
    {
        if (mSolveIndex > 0)
        {
            DrawResult(--mSolveIndex);
        }
        UpdateUI();
    }

    private void ShowNext()
    {
        if (mSolveIndex < SudokuSolve.Results.Count)
        {
            DrawResult(++mSolveIndex);
        }
        UpdateUI();
    }

    private void Clear()
    {
        SudokuSolve.Clear();

        for (int i = 0; i < mCurLen; i++)
        {
            for (int j = 0; j < mCurLen; j++)
            {
                InputField inputField = mInputFields[i, j];
                int value = mCurArr[i, j];

                inputField.interactable = true;
                if (value != 0)
                {
                    inputField.GetComponent<Image>().color = Color.white;
                    inputField.text = value.ToString();
                }
                else
                {
                    inputField.text = "";
                }

            }
        }
        UpdateUI();
    }

    private void ClearInput()
    {
        if (mIsCurError)
        {
            mIsCurError = false;
            ClearColor();
        }

        for (int i = 0; i < mCurLen; i++)
        {
            for (int j = 0; j < mCurLen; j++)
            {
                mInputFields[i, j].text = "";
            }
        }
    }

    private void Solve()
    {
        if (mIsCurError)
        {
            mIsCurError = false;
            ClearColor();
        }

        int[,] arr = new int[mCurLen, mCurLen];
        for (int i = 0; i < mCurLen; i++)
        {
            for (int j = 0; j < mCurLen; j++)
            {
                if (mInputFields[i, j].text == "")
                {
                    arr[i, j] = 0;
                }
                else
                {
                    int value = int.Parse(mInputFields[i, j].text);
                    arr[i, j] = value;
                }
            }
        }

        if (!CheckValidAndShow(arr))
        {
            mIsCurError = true;
            LogError();
            return;
        }

        for (int i = 0; i < mCurLen; i++)
        {
            for (int j = 0; j < mCurLen; j++)
            {
                if (mInputFields[i, j].text != "")
                {
                    mInputFields[i, j].GetComponent<Image>().color = new Color(0.8f, 0.8f, 0.8f, 1);
                }
            }
        }

        mCurArr = arr;
        int maxResult = (int)mSlider.value;
        SudokuSolve.SolveSuduku(arr, maxResult);
        mSolveIndex = 0;
        DrawResult(mSolveIndex);
        UpdateUI();
    }

    private bool CheckValidAndShow(int[,] arr)
    {
        bool isValid = true;
        for (int i = 0; i < mCurLen; i++)
        {
            for (int j = 0; j < mCurLen; j++)
            {
                if (!SudokuSolve.CheckValid(arr, i, j))
                {
                    isValid = false;
                    mInputFields[i, j].GetComponent<Image>().color = new Color(1, 0.5f, 0.5f, 1);
                }
                else
                {
                    mInputFields[i, j].GetComponent<Image>().color = Color.white;
                }
            }
        }

        return isValid;
    }

    private void UpdateLog()
    {
        mText_Log.text = $"求解完成，耗时：{SudokuSolve.SolveTime} ms\n共解出 {SudokuSolve.Results.Count} 个结果\n";
        if (SudokuSolve.Results.Count > 1)
        {
            mText_Log.text += $"当前为第 {mSolveIndex + 1} 个解";
        }
    }

    private void LogError()
    {
        mText_Log.text = "输入有误，请重新输入";
    }

    private void DrawResult(int index)
    {
        UpdateLog();
        if (SudokuSolve.Results == null || index < 0 || index >= SudokuSolve.Results.Count)
        {
            for (int i = 0; i < mCurLen; i++)
            {
                for (int j = 0; j < mCurLen; j++)
                {
                    mInputFields[i, j].interactable = false;
                }
            }
            return;
        }

        int[,] result = SudokuSolve.Results[index];
        for (int i = 0; i < mCurLen; i++)
        {
            for (int j = 0; j < mCurLen; j++)
            {
                mInputFields[i, j].text = result[i, j].ToString();
            }
        }
    }

    private void ClearColor()
    {
        for (int i = 0; i < mCurLen; i++)
        {
            for (int j = 0; j < mCurLen; j++)
            {
                mInputFields[i, j].GetComponent<Image>().color = Color.white;
            }
        }
    }

    private void UpdateUI()
    {
        if (SudokuSolve.IsSolve)
        {
            mBtn_Last.gameObject.SetActive(true);
            mBtn_Next.gameObject.SetActive(true);
            mBtn_Clear.gameObject.SetActive(true);


            mBtn_Next.interactable = mSolveIndex < SudokuSolve.Results.Count - 1;
            mBtn_Last.interactable = mSolveIndex > 0 && SudokuSolve.Results.Count > 0;

            mSlider.gameObject.SetActive(false);
            mBtn_ClearInput.gameObject.SetActive(false);
            mBtn_Solve.gameObject.SetActive(false);
        }
        else
        {
            mBtn_Last.gameObject.SetActive(false);
            mBtn_Next.gameObject.SetActive(false);
            mBtn_Clear.gameObject.SetActive(false);
            mText_Log.text = "";

            mSlider.gameObject.SetActive(true);
            mBtn_ClearInput.gameObject.SetActive(true);
            mBtn_Solve.gameObject.SetActive(true);
        }
    }
}
