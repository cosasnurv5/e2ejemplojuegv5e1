using UnityEngine;
using UnityEngine.UI;
using Hunter;

namespace UI_Debug
{
    public class StateWorldUI : MonoBehaviour
    {
        [Header("Referencias.")]
        public HunterNPC hunter;
        public Text stateText;
        public Image stateSquare; 
         
        [Header("Configuración.")]
        public Vector3 offset = new Vector3(0, 2.5f, 0);

        private Camera mainCamera;

        private void Start()
        {
            mainCamera = Camera.main;

            if (hunter == null)
            {
                hunter = GetComponentInParent<HunterNPC>();
            }
        }

        private void LateUpdate()
        {
            if (hunter == null || stateText == null) return;

            transform.position = hunter.transform.position + offset;

            if (mainCamera == null) mainCamera = Camera.main;

            if (mainCamera != null)
            {
                transform.rotation = mainCamera.transform.rotation;
            }

            string currentStateName = "Sin Estado";
            Color statusColor = Color.white;

            if (hunter.FSM != null && hunter.FSM.CurrentState != null)
            {
                string rawName = hunter.FSM.CurrentState.GetType().Name;
                currentStateName = rawName.Replace("Hunter", "").Replace("State", "");

                if (rawName.Contains("Patrol")) statusColor = Color.cyan;       
                else if (rawName.Contains("Attack")) statusColor = Color.red;
                else if (rawName.Contains("Rv"))  statusColor = Color.yellow;
                else if (rawName.Contains("Gather")) statusColor = Color.green;
               
            }

         
            stateText.color = statusColor;

            if (stateSquare != null)
            {
                stateSquare.color = statusColor;
            }
            
            string targetInfo = "Ninguno";
            if (hunter.CurrentTarget != null)
            {
                targetInfo = hunter.CurrentTarget.name;
            }
            else if (hunter.DestroyedTarget != null)
            {
                targetInfo = $"{hunter.DestroyedTarget.name} (Destruido)";
            }

            string tbaStatus = hunter.TBATimer <= 0 ? "LISTO" : $"{hunter.TBATimer:F1}s";

            stateText.text = $"[ {currentStateName.ToUpper()} ]\n" +
                             $"Target: {targetInfo}\n" +
                             $"TBA: {tbaStatus}";
        }
    }
}