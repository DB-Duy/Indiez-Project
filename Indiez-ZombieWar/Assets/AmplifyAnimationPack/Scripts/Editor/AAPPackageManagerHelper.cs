// Amplify Animation Pack - Third-Person Character Controller
// Copyright (c) Amplify Creations, Lda <info@amplify.pt>
#if (UNITY_EDITOR) 
using System;
using UnityEditor;
using UnityEditor.EventSystems;
using UnityEditor.PackageManager;
using UnityEditor.PackageManager.Requests;
using UnityEngine;


namespace AmplifyAnimationPack
{
    public static class AAPPackageManagerHelper
    {
        public delegate void FinishedEventType();
        public static FinishedEventType FinishedEvent;
        private static AddRequest m_request;
        public static void ImportPackage(string packageId)
        {
            if (m_request == null)
            {
                m_request = Client.Add(packageId);
                EditorApplication.update += Progress;
            }
        }

        static void Progress()
        {
            if (m_request.IsCompleted)
            {
                if (m_request.Status == StatusCode.Success)
                    Debug.Log("Installed: " + m_request.Result.packageId);
                else if (m_request.Status >= StatusCode.Failure)
                    Debug.Log(m_request.Error.message);

                EditorApplication.update -= Progress;

                m_request = null;
                if (FinishedEvent != null)
                    FinishedEvent.Invoke();
            }
        }

        public static bool CanStartImport { get { return m_request == null; } }
    }
}
#endif