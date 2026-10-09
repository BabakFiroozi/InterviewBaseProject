using UnityEngine;
using System.Collections;
namespace UI
{
    public class ShareManager : MonoBehaviour
    {
        private string screenshotPath;

        public void ShareButton()
        {
            StartCoroutine(CaptureAndShare());
        }

        IEnumerator CaptureAndShare()
        {
            yield return new WaitForEndOfFrame(); 
            Texture2D tex = ScreenCapture.CaptureScreenshotAsTexture();
            
            screenshotPath = Application.temporaryCachePath + "/shared_img.png";
            
            System.IO.File.WriteAllBytes(screenshotPath, tex.EncodeToPNG());

            Destroy(tex); 
            
            new NativeShare()
                .AddFile(screenshotPath)
                .SetSubject("Check my score!")
                .SetText("My new high score in the game!")
                .Share();
        }
    }

}