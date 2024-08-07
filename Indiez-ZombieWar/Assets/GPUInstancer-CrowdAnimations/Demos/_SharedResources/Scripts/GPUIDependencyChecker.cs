using UnityEngine;
using UnityEngine.UI;

namespace GPUInstancer.CrowdAnimations
{
    public class GPUIDependencyChecker : MonoBehaviour
    {
        private void Start()
        {
#if !GPU_INSTANCER
            if (gameObject.GetComponent<Canvas>() != null)
                DisplayErrorOnCanvas(gameObject.GetComponent<Canvas>());
#endif
        }

        public static void DisplayErrorOnCanvas(Canvas canvas)
        {
            if (canvas != null)
            {
                for (int i = 0; i < canvas.transform.childCount; i++)
                {
                    canvas.transform.GetChild(i).gameObject.SetActive(false);
                }
                GameObject errorTextGO = new GameObject("ErrorText");
                errorTextGO.transform.SetParent(canvas.transform);
                Text errorText = errorTextGO.AddComponent<Text>();
                errorText.text = GPUICrowdConstants.ERROR_GPUI_Dependency;
                errorText.color = Color.red;
                errorText.fontSize = 30;
                errorText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
                errorText.alignment = TextAnchor.MiddleCenter;
                RectTransform rt = errorText.GetComponent<RectTransform>();
                rt.anchoredPosition = Vector2.zero;
                rt.anchorMin = new Vector2(0, 0);
                rt.anchorMax = new Vector2(1, 1);
                rt.pivot = new Vector2(1f, 1f);
                rt.offsetMax = Vector2.zero;
                rt.offsetMin = Vector2.zero;
                rt.localScale = Vector3.one;
            }
        }
        }
    }
