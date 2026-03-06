using UnityEngine;

public class Tutorial_TimerController : MonoBehaviour
{
    [SerializeField] TMPro.TextMeshProUGUI timerText;

    [SerializeField] Color colorNormal;
    [SerializeField] Color colorFail;
    [SerializeField] Color colorRecord;

    float timerCur = 0f;
    float recordTime = 30f;

    public bool isTimerStart = false;

    string notiFailInTableKey = "Noti_Tutorial_Fail";
    string notiSuccessInTableKey = "Noti_Tutorial_Success";
    string notiRecordInTableKey = "Noti_Tutorial_Record";

    Direction_Notification notiDir;

    private void Start()
    {
        notiDir = GameObject.FindGameObjectWithTag("GameController").GetComponent<Direction_Notification>();
    }

    private void OnEnable()
    {
        recordTime = ES3.Load<float>("TutorialRecord", 30f);

        int minutes = Mathf.FloorToInt(recordTime / 60f);
        int seconds = Mathf.FloorToInt(recordTime % 60f);
        int milliseconds = Mathf.FloorToInt((recordTime * 100) % 100);

        timerText.text = $"{minutes:00} : {seconds:00}.{milliseconds:00}";
    }

    void Update()
    {
        if(isTimerStart)
        {
            timerCur += Time.deltaTime;

            int minutes = Mathf.FloorToInt(timerCur / 60f);
            int seconds = Mathf.FloorToInt(timerCur % 60f);
            int milliseconds = Mathf.FloorToInt((timerCur * 100) % 100);

            timerText.text = $"{minutes:00} : {seconds:00}.{milliseconds:00}";
        }

    }

    public void TutorialStart()
    {
        timerCur = 0f;
        isTimerStart = true;
        timerText.color = colorNormal;
    }

    public void TutorialEnd(bool isClear)
    {
        isTimerStart = false;

        if (isClear)
        {
            string _noti = notiSuccessInTableKey;

            timerText.color = colorNormal;
            if(timerCur <= recordTime)
            {
                timerText.color = colorRecord;
                recordTime = timerCur;
                ES3.Save<float>("TutorialRecord", recordTime);
                _noti = notiRecordInTableKey;
            }

            notiDir.Notification(_noti);
        }
        else
        {
            timerText.color = colorFail;
            notiDir.Notification(notiFailInTableKey);
        }

    }
}
