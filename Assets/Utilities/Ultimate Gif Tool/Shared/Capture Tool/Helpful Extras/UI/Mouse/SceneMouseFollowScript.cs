﻿using UnityEngine;

namespace TRS.CaptureTool.Extras
{
    public class SceneMouseFollowScript : MouseFollowScript
    {
        void Update()
        {
            Vector3 screenPoint = new Vector3(FlexibleInput.MousePosition().x, FlexibleInput.MousePosition().y, Camera.main.nearClipPlane + transform.lossyScale.z);
            Vector3 position = Camera.main.ScreenToWorldPoint(screenPoint);
            transform.position = position;
        }
    }
}