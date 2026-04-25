using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Trainamari.Core;
using Trainamari.Train;
using Trainamari.Input;

namespace Trainamari.UI
{
    /// <summary>
    /// HUD display - PS1 style chunky UI.
    /// Shows speed, brake indicator, score, combo, station approach.
    /// </summary>
    public class TrainHUD : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private TrainController train;
        [SerializeField] private TrainInput trainInput;
        [SerializeField] private ScoreManager scoreManager;
        [SerializeField] private CargoManager cargoManager;
        
        [Header("HUD Elements")]
        [SerializeField] private TextMeshProUGUI speedText;
        [SerializeField] private TextMeshProUGUI scoreText;
        [SerializeField] private TextMeshProUGUI comboText;
        [SerializeField] private RectTransform throttleIndicator;
        [SerializeField] private RectTransform brakeIndicator;
        [SerializeField] private RectTransform speedBar;
        [SerializeField] private Image speedBarFill;
        [SerializeField] private TextMeshProUGUI stationAlert;
        [SerializeField] private TextMeshProUGUI passengerText;
        [SerializeField] private GameObject cargoPanel;
        
        [Header("PS1 Style")]
        [SerializeField] private Font ps1Font;           // chunky pixel font
        [SerializeField] private Color ps1Yellow = new Color(1f, 0.85f, 0f);
        [SerializeField] private Color ps1Green = new Color(0f, 0.9f, 0.3f);
        [SerializeField] private Color ps1Red = new Color(0.9f, 0.1f, 0.1f);
        [SerializeField] private Color ps1White = new Color(1f, 1f, 1f);
        
        [Header("Animation")]
        [SerializeField] private float comboScaleDuration = 0.3f;
        [SerializeField] private float stationAlertDuration = 3f;
        
        private float stationAlertTimer = 0f;
        private int lastCombo = 0;
        
        private void Update()
        {
            if (train == null) return;
            
            UpdateSpeedDisplay();
            UpdateThrottleDisplay();
            UpdateScoreDisplay();
            UpdateComboDisplay();
            UpdatePassengerDisplay();
            UpdateStationAlert();
        }
        
        private void UpdateSpeedDisplay()
        {
            float speed = train.CurrentSpeed;
            float speedRatio = speed / GameConstants.MAX_SPEED;

            if (speedText != null)
            {
                speedText.text = $"{Mathf.RoundToInt(speed)} km/h";
                if (speedRatio < 0.6f) speedText.color = ps1Green;
                else if (speedRatio < 0.85f) speedText.color = ps1Yellow;
                else speedText.color = ps1Red;
            }
            
            // Speed bar fill
            if (speedBarFill != null)
            {
                speedBarFill.fillAmount = speedRatio;
                speedBarFill.color = speedText != null ? speedText.color : ps1White;
            }
        }
        
        private void UpdateThrottleDisplay()
        {
            if (trainInput == null) return;
            
            float throttle = trainInput.Throttle;
            
            if (throttleIndicator != null)
            {
                // Move throttle indicator up/down
                throttleIndicator.anchoredPosition = new Vector2(
                    throttleIndicator.anchoredPosition.x,
                    throttle * 100f
                );
            }
            
            if (brakeIndicator != null)
            {
                // Show brake intensity
                float brakeIntensity = Mathf.Clamp01(-throttle);
                brakeIndicator.localScale = new Vector3(1f, brakeIntensity, 1f);
            }
        }
        
        private void UpdateScoreDisplay()
        {
            if (scoreManager != null && scoreText != null)
            {
                scoreText.text = scoreManager.TotalScore.ToString("N0");
            }
        }
        
        private void UpdateComboDisplay()
        {
            if (scoreManager == null || comboText == null) return;
            
            if (scoreManager.ComboCount > 0)
            {
                comboText.text = $"x{scoreManager.ComboMultiplier:F1}";
                comboText.color = ps1Yellow;
                
                // Pop animation on new combo
                if (scoreManager.ComboCount != lastCombo)
                {
                    lastCombo = scoreManager.ComboCount;
                    // TODO: Tween scale animation
                }
            }
            else
            {
                comboText.text = "";
                lastCombo = 0;
            }
        }
        
        private void UpdatePassengerDisplay()
        {
            if (cargoManager != null && passengerText != null)
            {
                passengerText.text = $"👥 {cargoManager.PassengerCount}";
            }
        }
        
        private void UpdateStationAlert()
        {
            if (stationAlert == null) return;
            
            if (stationAlertTimer > 0f)
            {
                stationAlertTimer -= Time.deltaTime;
                float alpha = stationAlertTimer / stationAlertDuration;
                stationAlert.color = new Color(1f, 1f, 1f, alpha);
            }
            else
            {
                stationAlert.color = new Color(1f, 1f, 1f, 0f);
            }
        }
        
        /// <summary>
        /// Show a station approach alert.
        /// </summary>
        public void ShowStationAlert(string stationName, float distance)
        {
            if (stationAlert != null)
            {
                stationAlert.text = $"APPROACHING\n{stationName}\n{distance:F0}m";
                stationAlertTimer = stationAlertDuration;
            }
        }
        
        /// <summary>
        /// Show a derailment warning.
        /// </summary>
        public void ShowDerailmentWarning()
        {
            if (stationAlert != null)
            {
                stationAlert.text = "⚠️ REDUCE SPEED! ⚠️";
                stationAlert.color = ps1Red;
                stationAlertTimer = 2f;
            }
        }
    }
}