using UnityEngine;
using UnityEngine.UI;


public class ScoreManager : MonoBehaviour
{
    public Text scoreText;
    public float scoreMultiplier = 1f;

    private float score = 0f;

    public float GetScore()
    {
        return score;
    }

    private bool isCounting = true;
    
    void Update()
    {
        if (isCounting)
        {
            score += Time.deltaTime * scoreMultiplier;
            scoreText.text = "SCORE: " + Mathf.FloorToInt(score).ToString();
        }
    }
    
    public void StopCounting()
    {
        isCounting = false;
    }
}
