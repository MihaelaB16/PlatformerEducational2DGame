using System;
using UnityEngine;

// Calculator pentru scorul final si sistemul de stele
[System.Serializable]
public class ScoreCalculator : MonoBehaviour
{
    [Header("Star Images")]
    public Sprite[] starSprites = new Sprite[5];

    // Constante pentru formula de calcul
    private const float FIRST_ATTEMPT_WEIGHT = 0.40f;      // 40% pentru raspunsuri corecte din prima
    private const float WRONG_ANSWER_WEIGHT = 0.30f;       // 30% pentru raspunsuri gresite (inversat)
    private const float COINS_WEIGHT = 0.30f;              // 30% pentru monede
    private const float TIME_BONUS_WEIGHT = 0.05f;         // 5% bonus pentru timp < 10 minute

    // Calculeaza scorul final pe baza performantei jucatorului
    public static float CalculateScore(int coins, float timeSpent, int totalCorrectAnswers, int firstAttemptCorrect, int wrongAnswers)
    {
        // Componenta pentru raspunsuri corecte din prima (40% din scor)
        // Maxim 12 raspunsuri corecte din prima (6 per nivel × 2 niveluri)
        float firstAttemptScore = Mathf.Min(firstAttemptCorrect / 12f, 1f) * (FIRST_ATTEMPT_WEIGHT * 100f);

        // Componenta pentru raspunsuri gresite (30% din scor, inversat)
        // Cu cat mai putine raspunsuri gresite, cu atat scorul este mai mare
        float wrongAnswerPerformance = Mathf.Max(0f, 1f - (wrongAnswers / 12f));
        float wrongAnswerScore = wrongAnswerPerformance * (WRONG_ANSWER_WEIGHT * 100f);

        // Componenta pentru monede (30% din scor)
        // Maxim la 250 monede colectate
        float coinScore = Mathf.Min(coins / 250f, 1f) * (COINS_WEIGHT * 100f);

        // Bonus pentru timp sub 10 minute (5% din scor)
        float timeBonus = 0f;
        if (timeSpent < 600f) // 600 secunde = 10 minute
        {
            timeBonus = TIME_BONUS_WEIGHT * 100f;
        }

        // Calculul final
        float finalScore = firstAttemptScore + wrongAnswerScore + coinScore + timeBonus;

        // Asigura ca scorul este intre 0 si 100
        finalScore = Mathf.Clamp(finalScore, 0f, 100f);

        return finalScore;
    }

    // Overload pentru compatibilitate - calculeaza raspunsurile corecte totale automat
    public static float CalculateScore(int coins, float timeSpent, int firstAttemptCorrect, int wrongAnswers)
    {
        int estimatedTotalCorrect = firstAttemptCorrect + wrongAnswers;
        return CalculateScore(coins, timeSpent, estimatedTotalCorrect, firstAttemptCorrect, wrongAnswers);
    }

    // Converteste scorul in numarul de stele (1-5)
    public static int ScoreToStars(float score)
    {
        if (score >= 85f) return 5;
        if (score >= 70f) return 4;
        if (score >= 55f) return 3;
        if (score >= 35f) return 2;
        return 1; // Minimum 1 stea
    }

    // Obtine sprite-ul pentru numarul de stele dat
    public Sprite GetStarSprite(int stars)
    {
        if (stars < 1 || stars > 5 || starSprites == null || starSprites.Length < 5)
        {
            return null;
        }

        return starSprites[stars - 1]; // Array indexat de la 0
    }

    // Calculeaza si returneaza toate datele pentru o scena
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

// Structura pentru stocarea datelor calculate ale unei scene
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