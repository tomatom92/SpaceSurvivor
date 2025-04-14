using System.Collections;
using UnityEngine;
using TMPro;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager instance;
    public float score; // Changed to float
    public float highScore; // Changed to float
    public int coins;
    [SerializeField] TextMeshProUGUI scoreText; // Reference to the TextMeshProUGUI component

    // Animation parameters
    [SerializeField] float animationDuration = 0.6f;
    [SerializeField] float impulseStrength = 0.2f;
    [SerializeField] float bounceCount = 3f;
    [SerializeField] float scaleMultiplier = 1.2f;
    [SerializeField] Color scoreAddColor = new Color(1f, 0.8f, 0.2f); // Subtle yellow/gold color

    private Color originalColor;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    void Start()
    {
        originalColor = scoreText.color;
        UpdateScoreText();
    }

    public void AddScore(float scoreValue) // Changed to float
    {
        score += scoreValue * EnemySpawnManager.Instance.currentDifficulty;
        UpdateScoreText();
        StartCoroutine(AnimateScoreImpulse(scoreValue));
    }
    public void ResetScore()
    {
        score = 0;
        UpdateScoreText();// Update UI or other necessary components
    }

    void UpdateScoreText()
    {
        scoreText.text = $"{score}"; // Display score with one decimal place
    }

    IEnumerator AnimateScoreImpulse(float scoreValue) // Changed to float
    {
        // Store original values
        Vector3 originalPosition = scoreText.transform.localPosition;
        Vector3 originalScale = scoreText.transform.localScale;
        Quaternion originalRotation = scoreText.transform.localRotation;

        // Adjust animation variables based on the score value
        float strength = Mathf.Clamp(impulseStrength * (1f + scoreValue * 0.02f), impulseStrength, impulseStrength * 2f);
        float duration = animationDuration;

        // Animate
        float elapsed = 0f;
        while (elapsed < duration)
        {
            float progress = elapsed / duration;

            // Dampened sine wave for side-to-side rocking
            float rockAngle = strength * 30f * Mathf.Sin(progress * bounceCount * Mathf.PI * 2) * (1f - progress);

            // Scale that pulses larger, then returns to normal
            float scaleFactor = 1f + (scaleMultiplier - 1f) * Mathf.Sin(progress * Mathf.PI) * (1f - progress);

            // Color that fades from highlight color back to original
            Color currentColor = Color.Lerp(scoreAddColor, originalColor, progress);

            // Apply transformations
            scoreText.transform.localRotation = Quaternion.Euler(0, 0, rockAngle);
            scoreText.transform.localScale = originalScale * scaleFactor;
            scoreText.color = currentColor;

            // Add a small vertical bounce
            float bounceHeight = strength * 20f * Mathf.Sin(progress * Mathf.PI) * (1f - progress);
            scoreText.transform.localPosition = originalPosition + new Vector3(0, bounceHeight, 0);

            elapsed += Time.deltaTime;
            yield return null;
        }

        // Return to original state
        scoreText.transform.localPosition = originalPosition;
        scoreText.transform.localRotation = originalRotation;
        scoreText.transform.localScale = originalScale;
        scoreText.color = originalColor;
    }
}
