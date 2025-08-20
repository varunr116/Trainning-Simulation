using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class QuizManager : MonoBehaviour
{
    public static QuizManager Instance;
    
    [Header("Quiz UI")]
    public GameObject quizPanel;
    public TextMeshProUGUI questionText;
    public Button[] answerButtons = new Button[3];
    public TextMeshProUGUI resultText;
    public Button nextButton;
    public Button retryButton;
    public Button finishButton;
    
    [Header("Quiz Data")]
    public QuizData quizData;
    
    private int currentQuestionIndex = 0;
    private int correctAnswers = 0;
    private int totalQuestions = 3;
    private int passingScore = 2;
    private bool quizCompleted = false;
    
    [System.Serializable]
    public class Question
    {
        public string questionText;
        public string[] answers = new string[3];
        public int correctAnswerIndex;
    }
    
    [CreateAssetMenu(fileName = "QuizData", menuName = "Training/Quiz Data")]
    public class QuizData : ScriptableObject
    {
        public Question[] questions = new Question[3];
    }
    
    void Awake()
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
    
    void Start()
    {
        if (quizPanel != null)
            quizPanel.SetActive(false);
            
        SetupButtons();
    }
    
    void SetupButtons()
    {
        // Setup answer buttons
        for (int i = 0; i < answerButtons.Length; i++)
        {
            int answerIndex = i; // Capture for closure
            answerButtons[i].onClick.AddListener(() => OnAnswerSelected(answerIndex));
        }
        
        // Setup control buttons
        if (nextButton != null)
            nextButton.onClick.AddListener(NextQuestion);
            
        if (retryButton != null)
            retryButton.onClick.AddListener(RetryQuiz);
            
        if (finishButton != null)
            finishButton.onClick.AddListener(FinishQuiz);
    }
    
    public void StartQuiz()
    {
        if (quizData == null || quizData.questions.Length < totalQuestions)
        {
            Debug.LogError("Quiz data not properly configured!");
            return;
        }
        
        currentQuestionIndex = 0;
        correctAnswers = 0;
        quizCompleted = false;
        
        // Show quiz UI
        if (quizPanel != null)
            quizPanel.SetActive(true);
            
        // Disable player movement
        FirstPersonController playerController = FindObjectOfType<FirstPersonController>();
        if (playerController != null)
            playerController.SetMovementEnabled(false);
            
        // Play quiz intro audio
        AudioManager.Instance.PlayNarration(GetAudioClip("13_Quiz_Introduction"));
        
        ShowQuestion();
    }
    
    void ShowQuestion()
    {
        if (currentQuestionIndex >= quizData.questions.Length)
        {
            ShowResults();
            return;
        }
        
        Question currentQuestion = quizData.questions[currentQuestionIndex];
        
        // Update question text
        if (questionText != null)
            questionText.text = $"Question {currentQuestionIndex + 1}/3: {currentQuestion.questionText}";
            
        // Update answer buttons
        for (int i = 0; i < answerButtons.Length; i++)
        {
            if (i < currentQuestion.answers.Length)
            {
                answerButtons[i].gameObject.SetActive(true);
                TextMeshProUGUI buttonText = answerButtons[i].GetComponentInChildren<TextMeshProUGUI>();
                if (buttonText != null)
                    buttonText.text = currentQuestion.answers[i];
                    
                // Reset button colors
                ColorBlock colors = answerButtons[i].colors;
                colors.normalColor = Color.white;
                answerButtons[i].colors = colors;
                answerButtons[i].interactable = true;
            }
            else
            {
                answerButtons[i].gameObject.SetActive(false);
            }
        }
        
        // Hide control buttons
        SetControlButtonsVisible(false, false, false);
        
        // Clear result text
        if (resultText != null)
            resultText.text = "";
    }
    
    void OnAnswerSelected(int answerIndex)
    {
        Question currentQuestion = quizData.questions[currentQuestionIndex];
        bool isCorrect = (answerIndex == currentQuestion.correctAnswerIndex);
        
        if (isCorrect)
        {
            correctAnswers++;
            AudioManager.Instance.PlaySFX("correct_answer");
            if (resultText != null)
                resultText.text = "Correct!";
                
            // Highlight correct answer in green
            ColorBlock colors = answerButtons[answerIndex].colors;
            colors.normalColor = Color.green;
            answerButtons[answerIndex].colors = colors;
        }
        else
        {
            AudioManager.Instance.PlaySFX("wrong_answer");
            if (resultText != null)
                resultText.text = "Incorrect. The correct answer is highlighted.";
                
            // Highlight wrong answer in red
            ColorBlock wrongColors = answerButtons[answerIndex].colors;
            wrongColors.normalColor = Color.red;
            answerButtons[answerIndex].colors = wrongColors;
            
            // Highlight correct answer in green
            ColorBlock correctColors = answerButtons[currentQuestion.correctAnswerIndex].colors;
            correctColors.normalColor = Color.green;
            answerButtons[currentQuestion.correctAnswerIndex].colors = correctColors;
        }
        
        // Disable all buttons
        foreach (Button button in answerButtons)
        {
            button.interactable = false;
        }
        
        // Show next button or results
        if (currentQuestionIndex < totalQuestions - 1)
        {
            SetControlButtonsVisible(true, false, false); // Show Next
        }
        else
        {
            // Last question, show results after delay
            Invoke("ShowResults", 2f);
        }
    }
    
    void NextQuestion()
    {
        currentQuestionIndex++;
        ShowQuestion();
    }
    
    void ShowResults()
    {
        bool passed = correctAnswers >= passingScore;
        
        string resultMessage = $"Quiz Complete!\nYou scored {correctAnswers}/{totalQuestions}\n";
        resultMessage += passed ? "Congratulations! You passed!" : "You need at least 2 correct answers to pass.";
        
        if (questionText != null)
            questionText.text = resultMessage;
            
        // Hide answer buttons
        foreach (Button button in answerButtons)
        {
            button.gameObject.SetActive(false);
        }
        
        if (passed)
        {
            // Show finish button
            SetControlButtonsVisible(false, false, true);
            AudioManager.Instance.PlayNarration(GetAudioClip("14_Quiz_Pass"));
        }
        else
        {
            // Show retry button
            SetControlButtonsVisible(false, true, false);
            AudioManager.Instance.PlayNarration(GetAudioClip("15_Quiz_Fail"));
        }
        
        quizCompleted = true;
        
        // Update progress
        ProgressService.Instance.SetQuizScore(correctAnswers);
    }
    
    void SetControlButtonsVisible(bool next, bool retry, bool finish)
    {
        if (nextButton != null) nextButton.gameObject.SetActive(next);
        if (retryButton != null) retryButton.gameObject.SetActive(retry);
        if (finishButton != null) finishButton.gameObject.SetActive(finish);
    }
    
    void RetryQuiz()
    {
        StartQuiz(); // Restart the quiz
    }
    
    void FinishQuiz()
    {
        // Hide quiz UI
        if (quizPanel != null)
            quizPanel.SetActive(false);
            
        // Re-enable player movement
        FirstPersonController playerController = FindObjectOfType<FirstPersonController>();
        if (playerController != null)
            playerController.SetMovementEnabled(true);
            
        // Show certificate
        CertificateUI.Instance.ShowCertificate();
    }
    
    AudioClip GetAudioClip(string clipName)
    {
        // Implement with your actual audio clips
        return null;
    }
    
    public bool IsQuizCompleted() => quizCompleted;
    public int GetScore() => correctAnswers;
    public bool HasPassed() => correctAnswers >= passingScore;
}