using UnityEngine;

public class Direction_Ending : MonoBehaviour
{

    [SerializeField] Direction_EndingBG bg;

    [SerializeField] Direction_EndingStoryText storyText;
    [SerializeField] CanvasGroup storyTextCanvas;
    [SerializeField] AnimationCurve storyTextCurve;
    [SerializeField] Direction_EndingCredit credit;

    [SerializeField] Direction_SceneChanger sceneChanger;


    private void OnEnable()
    {
        Ending();
    }

    public void Ending()
    {
        bg.PlayBG();
    }

    public void EndingBGAppear()
    {
        storyText.EndingPlay();
    }

    public void EndingStoryPrinted()
    {
        StartCoroutine(storyText.FadeOut(storyTextCanvas, 2.5f, storyTextCurve));
        credit.PlayCredit();
    }

    public void CreditPrintComplete()
    {
        sceneChanger.ToMainMenu();
    }






}
