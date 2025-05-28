using System;
using UnityEngine;

[System.Serializable]
public class ScoreCalculator : MonoBehaviour
{
    [Header("Star Images")]
    public Sprite[] starSprites = new Sprite[5]; // Array pentru imaginile cu 1-5 stele

    // Constante pentru formula de calcul - ACTUALIZATE FINAL
    private const float OPTIMAL_TIME = 720f;           // 720 secunde (12 minute)
    private const float MAX_COINS_WEIGHT = 0.40f;      // 40% pentru monede
    private const float TIME_WEIGHT = 0.15f;           // 15% pentru timp
    private const float CORRECT_ANSWERS_WEIGHT = 0.30f; // 30% pentru r?spunsuri corecte totale
    private const float FIRST_ATTEMPT_WEIGHT = 0.15f;  // 15% pentru r?spunsuri corecte din prima
    private const float WRONG_ANSWER_PENALTY = 0.05f;  // pân? la 5% penalizare

    /// <summary>
    /// Calculeaz? scorul final pentru o scen? pe baza performan?ei juc?torului
    /// </summary>
    /// <param name="coins">Num?rul de monede colectate</param>
    /// <param name="timeSpent">Timpul petrecut în scen? (secunde)</param>
    /// <param name="totalCorrectAnswers">Num?rul total de r?spunsuri corecte</param>
    /// <param name="firstAttemptCorrect">R?spunsuri corecte din prima încercare</param>
    /// <param name="wrongAnswers">Num?rul total de r?spunsuri gre?ite</param>
    /// <returns>Scorul calculat (0-100)</returns>
    public static float CalculateScore(int coins, float timeSpent, int totalCorrectAnswers, int firstAttemptCorrect, int wrongAnswers)
    {
        // Componenta pentru monede (0-40 puncte) - ACTUALIZAT
        // Maxim la 300 monede colectate
        float coinScore = Mathf.Min(coins / 300f, 1f) * (MAX_COINS_WEIGHT * 100f);

        // Componenta pentru timp (0-15 puncte) - NESCHIMBAT
        // Scorul maxim se ob?ine la timpul optim (720s = 12 minute)
        // Bonus pentru timp foarte bun (sub 480s = 8 minute)
        float timeScore;
        if (timeSpent <= 480f) // 8 minute
        {
            // Bonus pentru timp excelent
            timeScore = TIME_WEIGHT * 100f * 1.1f; // 110% pentru timp sub 8 minute
        }
        else if (timeSpent <= OPTIMAL_TIME) // 12 minute
        {
            timeScore = TIME_WEIGHT * 100f; // Scor maxim normal
        }
        else
        {
            // Penalizare progresiv? - mai blând? decât înainte
            float timePenalty = (timeSpent - OPTIMAL_TIME) / OPTIMAL_TIME;
            timeScore = Mathf.Max(0f, TIME_WEIGHT * 100f * (1f - timePenalty * 0.3f));
        }

        // Componenta pentru r?spunsuri corecte totale (0-30 puncte) - NOU
        // Presupunem c? 12 r?spunsuri corecte totale = scor maxim (6 per level × 2 levels)
        float correctAnswersScore = Mathf.Min(totalCorrectAnswers / 12f, 1f) * (CORRECT_ANSWERS_WEIGHT * 100f);

        // Componenta pentru r?spunsuri corecte din prima (0-15 puncte) - ACTUALIZAT
        // Presupunem c? 12 r?spunsuri corecte din prima = scor maxim
        float firstAttemptScore = Mathf.Min(firstAttemptCorrect / 12f, 1f) * (FIRST_ATTEMPT_WEIGHT * 100f);

        // Penalizare pentru r?spunsuri gre?ite (pân? la -5 puncte) - NESCHIMBAT
        float wrongAnswerPenalty = Mathf.Min(wrongAnswers * 1f, WRONG_ANSWER_PENALTY * 100f);

        // Calculul final
        float finalScore = coinScore + timeScore + correctAnswersScore + firstAttemptScore - wrongAnswerPenalty;

        // Asigur?m c? scorul este între 15 ?i 100 (minimul pentru 1 stea)
        finalScore = Mathf.Clamp(finalScore, 15f, 100f);

        Debug.Log($"Score Calculation Breakdown (FINAL FORMULA):");
        Debug.Log($"Coins: {coins} -> {coinScore:F1} points (40% weight, max at 300 coins)");
        Debug.Log($"Time: {timeSpent:F1}s -> {timeScore:F1} points (15% weight, optimal at 720s, bonus under 480s)");
        Debug.Log($"Total Correct Answers: {totalCorrectAnswers} -> {correctAnswersScore:F1} points (30% weight, max at 12)");
        Debug.Log($"First Attempt Correct: {firstAttemptCorrect} -> {firstAttemptScore:F1} points (15% weight, max at 12)");
        Debug.Log($"Wrong Answer Penalty: {wrongAnswers} -> -{wrongAnswerPenalty:F1} points");
        Debug.Log($"Final Score: {finalScore:F1}");

        return finalScore;
    }

    /// <summary>
    /// Overload pentru compatibilitate cu codul existent - calculeaz? r?spunsurile corecte totale automat
    /// </summary>
    public static float CalculateScore(int coins, float timeSpent, int firstAttemptCorrect, int wrongAnswers)
    {
        // Estimeaz? r?spunsurile corecte totale pe baza celor din prima ?i gre?ite
        int estimatedTotalCorrect = firstAttemptCorrect + wrongAnswers;
        return CalculateScore(coins, timeSpent, estimatedTotalCorrect, firstAttemptCorrect, wrongAnswers);
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
        int totalCorrectAnswers = sceneData.Level1.rightAnswer + sceneData.Level2.rightAnswer;
        int totalFirstAttempt = sceneData.Level1.firstAttemptRightAnswer + sceneData.Level2.firstAttemptRightAnswer;
        int totalWrongAnswers = sceneData.Level1.wrongAnswer + sceneData.Level2.wrongAnswer;

        float score = CalculateScore(sceneData.Coins, sceneData.Time, totalCorrectAnswers, totalFirstAttempt, totalWrongAnswers);
        int stars = ScoreToStars(score);

        return new SceneScoreData
        {
            Score = score,
            Stars = stars,
            Coins = sceneData.Coins,
            Time = sceneData.Time,
            TotalCorrectAnswers = totalCorrectAnswers,
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
    public int TotalCorrectAnswers;
    public int FirstAttemptCorrect;
    public int WrongAnswers;
}