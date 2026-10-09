using UnityEngine;

namespace OrientationLock
{
    
    public class OrientationLock : MonoBehaviour
    {
        void Start()
        {
            if (Screen.width > Screen.height)
            {
                Screen.orientation = ScreenOrientation.LandscapeLeft;
                Screen.autorotateToLandscapeLeft = true;
                Screen.autorotateToLandscapeRight = true;
                Screen.autorotateToPortrait = false;
                Screen.autorotateToPortraitUpsideDown = false;
            }
            else
            {
                Screen.orientation = ScreenOrientation.Portrait;
                Screen.autorotateToPortrait = true;
                Screen.autorotateToPortraitUpsideDown = true;
                Screen.autorotateToLandscapeLeft = false;
                Screen.autorotateToLandscapeRight = false;
            }
            
            Screen.orientation = ScreenOrientation.AutoRotation;
        }
    }
    
}