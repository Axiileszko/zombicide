using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Controller
{
    public class AnimationController : MonoBehaviour
    {
        public static AnimationController Instance { get; private set; }
        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
            }
        }
        public void ShowPopupSearch(Vector3 position)
        {
            GameObject canvas = GameObject.FindGameObjectWithTag("GameUI");
            RectTransform canvasRect = canvas.GetComponent<RectTransform>();
            Vector2 screenPoint = RectTransformUtility.WorldToScreenPoint(Camera.main, position);
            Vector2 anchoredPosition;
            Vector3 newPosition;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                canvasRect,
                screenPoint,
                Camera.main,
                out anchoredPosition
            );
            newPosition.x = anchoredPosition.x; newPosition.y = anchoredPosition.y; newPosition.z = -9f;
            GameObject popupPrefab = Resources.Load<GameObject>($"Prefabs/PopupSearch");
            GameObject popup = Instantiate(popupPrefab,canvas.transform);
            popup.transform.localPosition = newPosition;
            Destroy(popup,2f);
        }
        public void ShowPopupSkull(Vector3 position)
        {
            GameObject canvas = GameObject.FindGameObjectWithTag("GameUI");
            RectTransform canvasRect = canvas.GetComponent<RectTransform>();
            Vector2 screenPoint = RectTransformUtility.WorldToScreenPoint(Camera.main, position);
            Vector2 anchoredPosition;
            Vector3 newPosition;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                canvasRect,
                screenPoint,
                Camera.main,
                out anchoredPosition
            );
            newPosition.x = anchoredPosition.x; newPosition.y = anchoredPosition.y; newPosition.z = -9f;
            GameObject popupPrefab = Resources.Load<GameObject>($"Prefabs/PopupSkull");
            GameObject popup = Instantiate(popupPrefab,canvas.transform);
            popup.transform.localPosition = newPosition;
            Destroy(popup, 2f);
        }
    }
}
