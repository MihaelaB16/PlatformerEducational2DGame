using System;
using UnityEngine;

[System.Serializable]
public class ScoreCalculator : MonoBehaviour
{
    [Header("Star Images")]
    public Sprite[] starSprites = new Sprite[5]; // Array pentru imaginile cu 1-5 stele

    // Constante pentru formula de calcul
    private const float OPTIMAL_TIME = 150f;
    private const float MAX_COINS_WEIGHT = 0.35f;
    private const float TIME_WEIGHT = 0.3f;
    private const float FIRST_ATTEMPT_WEIGHT = 0.3f;
    private const float WRONG_ANSWER_PENALTY = 0.05f;

    /// <summary>
    /// Calculeaz? scorul final pentru o scen? pe baza performan?ei juc?torului
    /// </summary>
    /// <param name="coins">Num?rul de monede colectate</param>
    /// <param name="timeSpent">Timpul petrecut în scen? (secunde)</param>
    /// <param name="firstAttemptCorrect">R?spunsuri corecte din prima încercare</param>
    /// <param name="wrongAnswers">Num?rul total de r?spunsuri gre?ite</param>
    /// <returns>Scorul calculat (0-100)</returns>
    public static float CalculateScore(int coins, float timeSpent, int firstAttemptCorrect, int wrongAnswers)
    {
        // Componenta pentru monede (0-35 puncte)
        // Presupunem c? 150+ monede = scor maxim pentru aceast? component?
        float coinScore = Mathf.Min(coins / 150f, 1f) * (MAX_COINS_WEIGHT * 100f);

        // Componenta pentru timp (0-30 puncte)
        // Scorul maxim se ob?ine la timpul optim (150s)
        // Bonus pentru timp foarte bun (sub 100s)
        float timeScore;
        if (timeSpent <= 100f)
        {
            // Bonus pentru timp excelent
            timeScore = TIME_WEIGHT * 100f * 1.1f; // 110% pentru timp sub 100s
        }
        else if (timeSpent <= OPTIMAL_TIME)
        {
            timeScore = TIME_WEIGHT * 100f; // Scor maxim normal
        }
        else
        {
            // Penalizare progresiv? - mai blând? decât înainte
            float timePenalty = (timeSpent - OPTIMAL_TIME) / OPTIMAL_TIME;
            timeScore = Mathf.Max(0f, TIME_WEIGHT * 100f * (1f - timePenalty * 0.3f));
        }

        // Componenta pentru r?spunsuri corecte din prima (0-30 puncte)
        // Presupunem c? 10 r?spunsuri corecte din prima = scor maxim
        float firstAttemptScore = Mathf.Min(firstAttemptCorrect / 10f, 1f) * (FIRST_ATTEMPT_WEIGHT * 100f);

        // Penalizare pentru r?spunsuri gre?ite (pân? la -5 puncte) - redus? semnificativ
        float wrongAnswerPenalty = Mathf.Min(wrongAnswers * 1f, WRONG_ANSWER_PENALTY * 100f);

        // Calculul final
        float finalScore = coinScore + timeScore + firstAttemptScore - wrongAnswerPenalty;

        // Asigur?m c? scorul este între 15 ?i 100 (minimul pentru 1 stea)
        finalScore = Mathf.Clamp(finalScore, 15f, 100f);

        Debug.Log($"Score Calculation Breakdown:");
        Debug.Log($"Coins: {coins} -> {coinScore:F1} points (max at 150 coins)");
        Debug.Log($"Time: {timeSpent:F1}s -> {timeScore:F1} points (bonus under 100s)");
        Debug.Log($"First Attempt Correct: {firstAttemptCorrect} -> {firstAttemptScore:F1} points (max at 10)");
        Debug.Log($"Wrong Answer Penalty: {wrongAnswers} -> -{wrongAnswerPenalty:F1} points");
        Debug.Log($"Final Score: {finalScore:F1}");

        return finalScore;
    }

    /// <summary>
    /// Converte?te scorul în num?rul de stele (1-5)
    /// </summary>
    /// <param name="score">Scorul calculat (0-100)</param>
    /// <returns>Num?rul de stele (1-5)</returns>
    public static int ScoreToStars(float score)
    {
        if (score >= 85f) return 5;
        if (score >= 70f) return 4;
        if (score >= 55f) return 3;
        if (score >= 35f) return 2;
        return 1; // Minimum 1 stea
    }

    /// <summary>
    /// Ob?ine sprite-ul pentru num?rul de stele dat
    /// </summary>
    /// <param name="stars">Num?rul de stele (1-5)</param>
    /// <returns>Sprite-ul corespunz?tor</returns>
    public Sprite GetStarSprite(int stars)
    {
        if (stars < 1 || stars > 5 || starSprites == null || starSprites.Length < 5)
        {
            Debug.LogWarning($"Invalid star count {stars} or missing star sprites!");
            return null;
        }

        return starSprites[stars - 1]; // Array indexat de la 0
    }

    /// <summary>
    /// Calculeaz? ?i returneaz? toate datele pentru o scen?
    /// </summary>
    /// <param name="sceneData">Datele scenei</param>
    /// <returns>Structura cu toate calculele</returns>
    public static SceneScoreData CalculateSceneScore(SceneData sceneData)
    {
        int totalFirstAttempt = sceneData.Level1.firstAttemptRightAnswer + sceneData.Level2.firstAttemptRightAnswer;
        int totalWrongAnswers = sceneData.Level1.wrongAnswer + sceneData.Level2.wrongAnswer;

        float score = CalculateScore(sceneData.Coins, sceneData.Time, totalFirstAttempt, totalWrongAnswers);
        int stars = ScoreToStars(score);

        return new SceneScoreData
        {
            Score = score,
            Stars = stars,
            Coins = sceneData.Coins,
            Time = sceneData.Time,
            FirstAttemptCorrect = totalFirstAttempt,
            WrongAnswers = totalWrongAnswers
        };
    }
}

/// <summary>
/// Structur? pentru stocarea datelor calculate ale unei scene
/// </summary>
[System.Serializable]
public struct SceneScoreData
{
    public float Score;
    public int Stars;
    public int Coins;
    public float Time;
    public int FirstAttemptCorrect;
    public int WrongAnswers;
}