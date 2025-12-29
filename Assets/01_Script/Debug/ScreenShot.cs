/*


using System;
using System.IO;
using UnityEditor.Recorder;
using UnityEditor.Recorder.Input;
using System.Collections;

namespace UnityEngine.Recorder.Examples
{
    [Serializable]
    public class ScreenShotData
    {
        public string name;
        public int width;
        public int height;
    }

    public class ScreenShot : MonoBehaviour
    {
        RecorderController m_RecorderController;

        [SerializeField]
        ScreenShotData[] ScreenShotDatas;

        private void Setting(string name, int width, int height)
        {
            string currentTime = DateTime.Now.ToString("yyyy-MM-dd HH-mm-ss");
            var settings = ScriptableObject.CreateInstance<RecorderControllerSettings>();
            m_RecorderController = new RecorderController(settings);
            var mediaOutputFolder = Path.Combine(Application.dataPath, "../", "ScreenShot"); // 스크린샷 저장 경로 지정

            var imageRecorder = ScriptableObject.CreateInstance<ImageRecorderSettings>();
            imageRecorder.name = name;
            imageRecorder.Enabled = true;
            imageRecorder.OutputFormat = ImageRecorderSettings.ImageRecorderOutputFormat.PNG;
            imageRecorder.CaptureAlpha = false;

            imageRecorder.OutputFile = Path.Combine(mediaOutputFolder, name + "_" + width + "_" + height);
            imageRecorder.imageInputSettings = new GameViewInputSettings
            {
                OutputWidth = width,
                OutputHeight = height,
            };

            settings.AddRecorderSettings(imageRecorder);
            settings.SetRecordModeToSingleFrame(0);
        }

        private void OnGUI()
        {
            if (Input.GetKeyDown(KeyCode.F2))
            {
                for (int i = 0; i < ScreenShotDatas.Length; i++)
                {
                    StartCoroutine(Capture());
                }
            }
        }

        IEnumerator Capture()
        {
            foreach (ScreenShotData data in ScreenShotDatas)
            {
                Setting(data.name, data.width, data.height);
                m_RecorderController.PrepareRecording();
                m_RecorderController.StartRecording();
                yield return null;
            }
        }
    }
}

*/