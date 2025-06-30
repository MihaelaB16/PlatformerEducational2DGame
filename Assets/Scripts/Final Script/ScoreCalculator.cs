using System;
using UnityEngine;

[System.Serializable]
public class ScoreCalculator : MonoBehaviour
{
    [Header("Star Images")]
    public Sprite[] starSprites = new Sprite[5]; // Array pentru imaginile cu 1-5 stele

    // Constante pentru formula de calcul - NOUA FORMULA SPECIFICATĂ
    private const float FIRST_ATTEMPT_WEIGHT = 0.40f;      // 40% pentru răspunsuri corecte din prima
    private const float WRONG_ANSWER_WEIGHT = 0.30f;       // 30% pentru răspunsuri greșite (inversat)
    private const float COINS_WEIGHT = 0.30f;              // 30% pentru monede
    private const float TIME_BONUS_WEIGHT = 0.05f;         // 5% bonus pentru timp < 10 minute

    /// <summary>
    /// Calculează scorul final pentru o scenă pe baza performanței jucătorului cu noua formulă
    /// Noua formulă: 40% răspunsuri corecte din prima + 30% răspunsuri greșite (inversat) + 30% monede + 5% bonus timp
    /// </summary>
    /// <param name="coins">Numărul de monede colectate</param>
    /// <param name="timeSpent">Timpul petrecut în scenă (secunde)</param>
    /// <param name="totalCorrectAnswers">Numărul total de răspunsuri corecte</param>
    /// <param name="firstAttemptCorrect">Răspunsuri corecte din prima încercare</param>
    /// <param name="wrongAnswers">Numărul total de răspunsuri greșite</param>
    /// <returns>Scorul calculat (0-100)</returns>
    public static float CalculateScore(int coins, float timeSpent, int totalCorrectAnswers, int firstAttemptCorrect, int wrongAnswers)
    {
        // 1. Componenta pentru răspunsuri corecte din prima (40% din scor)
        // Presupunem că 12 răspunsuri corecte din prima = scor maxim (6 per level × 2 levels)
        float firstAttemptScore = Mathf.Min(firstAttemptCorrect / 12f, 1f) * (FIRST_ATTEMPT_WEIGHT * 100f);

        // 2. Componenta pentru răspunsuri greșite (30% din scor, inversat)
        // Cu cât mai puține răspunsuri greșite, cu atât scorul este mai mare
        // Presupunem maximum 12 răspunsuri greșite pentru normalizare
        float wrongAnswerPerformance = Mathf.Max(0f, 1f - (wrongAnswers / 12f));
        float wrongAnswerScore = wrongAnswerPerformance * (WRONG_ANSWER_WEIGHT * 100f);

        // 3. Componenta pentru monede (30% din scor)
        // Maxim la 250 monede colectate
        float coinScore = Mathf.Min(coins / 250f, 1f) * (COINS_WEIGHT * 100f);

        // 4. Bonus pentru timp sub 10 minute (5% din scor)
        float timeBonus = 0f;
        if (timeSpent < 600f) // 600 secunde = 10 minute
        {
            // Bonus maxim dacă termină în mai puțin de 10 minute
            timeBonus = TIME_BONUS_WEIGHT * 100f;
        }

        // Calculul final
        float finalScore = firstAttemptScore + wrongAnswerScore + coinScore + timeBonus;

        // Asigurăm că scorul este între 0 și 100
        finalScore = Mathf.Clamp(finalScore, 0f, 100f);

        Debug.Log($"Score Calculation Breakdown (NOVA FORMULA):");
        Debug.Log($"Răspunsuri corecte din prima: {firstAttemptCorrect}/12 -> {firstAttemptScore:F1} points (40% weight)");
        Debug.Log($"Performanță la răspunsuri greșite: {wrongAnswers}/12 -> {wrongAnswerScore:F1} points (30% weight, inversat)");
        Debug.Log($"Monede: {coins}/250 -> {coinScore:F1} points (30% weight)");
        Debug.Log($"Bonus timp (<10min): {timeSpent:F1}s -> {timeBonus:F1} points (5% bonus)");
        Debug.Log($"Final Score: {finalScore:F1}");

        return finalScore;
    }

    /// <summary>
    /// Overload pentru compatibilitate cu codul existent - calculează răspunsurile corecte totale automat
    /// </summary>
    public static float CalculateScore(int coins, float timeSpent, int firstAttemptCorrect, int wrongAnswers)
    {
        // Estimează răspunsurile corecte totale pe baza celor din prima și greșite
        int estimatedTotalCorrect = firstAttemptCorrect + wrongAnswers;
        return CalculateScore(coins, timeSpent, estimatedTotalCorrect, firstAttemptCorrect, wrongAnswers);
    }

    /// <summary>
    /// Convertește scorul în numărul de stele (1-5)
    /// </summary>
    /// <param name="score">Scorul calculat (0-100)</param>
    /// <returns>Numărul de stele (1-5)</returns>
    public static int ScoreToStars(float score)
    {
        if (score >= 85f) return 5;
        if (score >= 70f) return 4;
        if (score >= 55f) return 3;
        if (score >= 35f) return 2;
        return 1; // Minimum 1 stea
    }

    /// <summary>
    /// Obține sprite-ul pentru numărul de stele dat
    /// </summary>
    /// <param name="stars">Numărul de stele (1-5)</param>
    /// <returns>Sprite-ul corespunzător</returns>
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
    /// Calculează și returnează toate datele pentru o scenă
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
/// Structură pentru stocarea datelor calculate ale unei scene
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