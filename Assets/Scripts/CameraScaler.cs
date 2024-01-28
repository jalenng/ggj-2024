using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraScaler : MonoBehaviour
{
  public float aspectRatio = 1.5f; // 3:2

  private Camera currCamera;

  private int lastScreenWidth = -1;
  private int lastScreenHeight = -1;

  void Start()
  {
    currCamera = GetComponent<Camera>();
  }

  void Update()
  {
    int screenWidth = Screen.width; //px
    int screenHeight = Screen.height; //px

    bool screenSizeChanged = screenWidth != lastScreenWidth || screenHeight != lastScreenHeight;
    if (screenSizeChanged)
    {
      // Resize viewport and center it while maintaining the desired aspect ratio.

      float newX = 0f;
      float newY = 0f;
      float newWidth = 1f;
      float newHeight = 1f;

      // Check whether we need to adjust horizontally or vertically by 
      // comparing the desired aspect ratio and the current aspect ratio
      float currentScreenAspectRatio = (float)screenWidth / screenHeight;

      // If screen is too wide, we center horizontally
      if (currentScreenAspectRatio > aspectRatio)
      {
        int aspectRatioWidth = (int)(screenHeight * aspectRatio);
        newWidth = (float)aspectRatioWidth / screenWidth;
        newX = (1f - newWidth) / 2;
      }
      // else, screen is too tall, so we center vertically
      else
      {
        int aspectRatioHeight = (int)(screenWidth / aspectRatio);
        newHeight = (float)aspectRatioHeight / screenHeight;
        newY = (1f - newHeight) / 2;
      }

      currCamera.rect = new Rect(newX, newY, newWidth, newHeight);

      // Remember width and height to track changes
      lastScreenWidth = screenWidth;
      lastScreenHeight = screenHeight;
    }
  }

  // Because we scale the camera viewport to be smaller than the screen,
  // this creates black bars on the screen which don't get refreshed.
  // So we need to manually clear them.
  void OnPreCull()
  {
    // Remember viewport rect
    Rect oldViewportRect = currCamera.rect;

    // New viewport rect to clear the whole screen
    Rect newViewportRect = new Rect(0, 0, 1, 1);

    // Clear the pixels with black
    currCamera.rect = newViewportRect;
    GL.Clear(true, true, Color.black);

    // Restore the previous viewport rect
    currCamera.rect = oldViewportRect;
  }
}
