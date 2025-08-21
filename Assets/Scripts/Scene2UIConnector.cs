using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class Scene2UIConnector : MonoBehaviour
{
    [Header("Scene 2 UI References")]
    public Slider progressBar;
    public TextMeshProUGUI progressText;
    public TextMeshProUGUI timerText;
    public GameObject timerPanel;
    public TextMeshProUGUI sceneLabel;

    [Header("Quiz UI References")]
    public GameObject quizPanel;
    public TextMeshProUGUI questionText;
    public Button[] answerButtons = new Button[3];
    public TextMeshProUGUI resultText;
    public Button nextButton;
    public Button retryButton;
    public Button finishButton;

    [Header("Certificate UI References")]
    public GameObject certificatePanel;
    public TextMeshProUGUI learnerNameText;
    public TextMeshProUGUI completionDateText;
    public TextMeshProUGUI courseNameText;
    public TextMeshProUGUI scoreText;
    public Button closeCertificateButton;

    [Header("Quiz Data")]
    public QuizData quizData;

    [Header("Pickup UI References")]
    public GameObject pickupPrompt;
    public GameObject dropPrompt;
    public TextMeshProUGUI pickupText;
    public TextMeshProUGUI dropText;

    private int currentQuestionIndex = 0;
    private int correctAnswers = 0;
    private int totalQuestions = 3;
    private int passingScore = 2;
    private bool quizCompleted = false;

    void Start()
    {
        StartCoroutine(InitializeScene2Systems());
    }

    IEnumerator InitializeScene2Systems()
    {
        yield return new WaitForSeconds(0.2f);

        ConnectToUIManager();
        TryConnectToQuizManager();
        ConnectToItemPickupController();
        SetupLocalQuizSystem();
        SetupCertificateSystem();
        UpdateProgressFromScene1();
        InitializeAndStartTimer();
        InitializeScene2Audio();
        ConfigureItemsForScene2();
    }

    void ConnectToUIManager()
    {
        if (UIManager.Instance != null)
        {
            UIManager.Instance.progressBar = progressBar;
            UIManager.Instance.progressText = progressText;
            UIManager.Instance.timerText = timerText;
            UIManager.Instance.timerPanel = timerPanel;
            UIManager.Instance.sceneLabel = sceneLabel;

            UIManager.Instance.SetSceneLabel("Scene 2: Warehouse Simulation");
            UIManager.Instance.ShowTimer(true);
        }
    }

    void TryConnectToQuizManager()
    {
        if (QuizManager.Instance != null)
        {
            QuizManager.Instance.quizPanel = quizPanel;
            QuizManager.Instance.questionText = questionText;
            QuizManager.Instance.answerButtons = answerButtons;
            QuizManager.Instance.resultText = resultText;
            QuizManager.Instance.nextButton = nextButton;
            QuizManager.Instance.retryButton = retryButton;
            QuizManager.Instance.finishButton = finishButton;
        }
    }

    void SetupLocalQuizSystem()
    {
        if (quizPanel != null)
        {
            quizPanel.SetActive(false);

            for (int i = 0; i < answerButtons.Length; i++)
            {
                int answerIndex = i;
                answerButtons[i].onClick.AddListener(() => OnAnswerSelected(answerIndex));
            }

            if (nextButton != null)
                nextButton.onClick.AddListener(NextQuestion);

            if (retryButton != null)
                retryButton.onClick.AddListener(RetryQuiz);

            if (finishButton != null)
                finishButton.onClick.AddListener(FinishQuiz);
        }
    }

    void SetupCertificateSystem()
    {
        if (certificatePanel != null)
        {
            certificatePanel.SetActive(false);

            if (closeCertificateButton != null)
                closeCertificateButton.onClick.AddListener(CloseCertificate);
        }

        if (CertificateUI.Instance != null)
        {
            CertificateUI.Instance.certificatePanel = certificatePanel;
            CertificateUI.Instance.learnerNameText = learnerNameText;
            CertificateUI.Instance.completionDateText = completionDateText;
            CertificateUI.Instance.courseNameText = courseNameText;
            CertificateUI.Instance.scoreText = scoreText;
            CertificateUI.Instance.closeButton = closeCertificateButton;
        }
    }

    void ConnectToItemPickupController()
    {
        ItemPickupController pickupController = FindObjectOfType<ItemPickupController>();
        if (pickupController != null)
        {
            pickupController.pickupPrompt = pickupPrompt;
            pickupController.dropPrompt = dropPrompt;
            pickupController.pickupText = pickupText;
            pickupController.dropText = dropText;
        }
    }

    void UpdateProgressFromScene1()
    {
        if (ProgressService.Instance != null && UIManager.Instance != null)
        {
            float currentProgress = ProgressService.Instance.GetProgress();
            UIManager.Instance.UpdateProgressBar(currentProgress);
        }
    }

    void InitializeAndStartTimer()
    {
        if (TimerSystem.Instance != null)
        {
            TimerSystem.Instance.StartTimer(300f);
        }
    }

    void InitializeScene2Audio()
    {
        Scene2AudioManager audioManager = FindObjectOfType<Scene2AudioManager>();
    }

    void ConfigureItemsForScene2()
    {
        InteractableItem[] items = FindObjectsOfType<InteractableItem>();

        foreach (InteractableItem item in items)
        {
            item.isInspectable = false;
            item.isCollectable = true;

            if (item.GetComponent<InteractableItem>())
            {
                item.SendMessage("SetMouseInteractionsEnabled", false, SendMessageOptions.DontRequireReceiver);
            }
        }
    }

    public void StartQuizDirectly()
    {
        if (quizData == null || quizData.questions.Length < totalQuestions)
        {
            return;
        }

        currentQuestionIndex = 0;
        correctAnswers = 0;
        quizCompleted = false;

        if (quizPanel != null)
            quizPanel.SetActive(true);

        CursorCameraController playerController = FindObjectOfType<CursorCameraController>();
        if (playerController != null)
            playerController.SetMovementEnabled(false);

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

        if (questionText != null)
            questionText.text = $"Question {currentQuestionIndex + 1}/3: {currentQuestion.questionText}";

        for (int i = 0; i < answerButtons.Length; i++)
        {
            if (i < currentQuestion.answers.Length)
            {
                answerButtons[i].gameObject.SetActive(true);
                TextMeshProUGUI buttonText = answerButtons[i].GetComponentInChildren<TextMeshProUGUI>();
                if (buttonText != null)
                    buttonText.text = currentQuestion.answers[i];

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

        SetControlButtonsVisible(false, false, false);

        if (resultText != null)
            resultText.text = "";
    }

    void OnAnswerSelected(int answerIndex)
    {
        Question currentQuestion = quizData.questions[currentQuestionIndex];
        bool isCorrect = (answerIndex == currentQuestion.correctAnswerIndex);

        if (ProgressService.Instance != null)
        {
            ProgressService.Instance.RecordQuizAnswer(currentQuestionIndex, answerIndex, isCorrect);
        }
        if (isCorrect)
        {
            correctAnswers++;
            if (AudioManager.Instance != null)
                AudioManager.Instance.PlaySFX("correct_answer");
            if (resultText != null)
                resultText.text = "Correct!";

            ColorBlock colors = answerButtons[answerIndex].colors;
            colors.normalColor = Color.green;
            answerButtons[answerIndex].colors = colors;
        }
        else
        {
            if (AudioManager.Instance != null)
                AudioManager.Instance.PlaySFX("wrong_answer");
            if (resultText != null)
                resultText.text = "Incorrect.";

            ColorBlock wrongColors = answerButtons[answerIndex].colors;
            wrongColors.normalColor = Color.red;
            answerButtons[answerIndex].colors = wrongColors;

            ColorBlock correctColors = answerButtons[currentQuestion.correctAnswerIndex].colors;
            correctColors.normalColor = Color.green;
            answerButtons[currentQuestion.correctAnswerIndex].colors = correctColors;
        }

        foreach (Button button in answerButtons)
        {
            button.interactable = false;
        }

        if (currentQuestionIndex < totalQuestions - 1)
        {
            SetControlButtonsVisible(true, false, false);
        }
        else
        {
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

        foreach (Button button in answerButtons)
        {
            button.gameObject.SetActive(false);
        }

        if (passed)
        {
            SetControlButtonsVisible(false, false, true);
        }
        else
        {
            SetControlButtonsVisible(false, true, false);
        }

        quizCompleted = true;

        if (ProgressService.Instance != null)
        {
            ProgressService.Instance.SetQuizScore(correctAnswers);
        }
    }

    void SetControlButtonsVisible(bool next, bool retry, bool finish)
    {
        if (nextButton != null) nextButton.gameObject.SetActive(next);
        if (retryButton != null) retryButton.gameObject.SetActive(retry);
        if (finishButton != null) finishButton.gameObject.SetActive(finish);
    }

    void RetryQuiz()
    {
        StartQuizDirectly();
    }

    void FinishQuiz()
    {
        if (quizPanel != null)
            quizPanel.SetActive(false);

        CursorCameraController playerController = FindObjectOfType<CursorCameraController>();
        if (playerController != null)
            playerController.SetMovementEnabled(true);

        ShowCertificate();
    }

   void ShowCertificate()
{
    
    
    if (certificatePanel != null)
    {
        certificatePanel.SetActive(true);
        UpdateCertificateData();
       
    }
    else
    {
        Debug.LogError("Certificate panel is null in Scene2UIConnector!");
    }
}
  void UpdateCertificateData()
{
    if (learnerNameText != null)
        learnerNameText.text = "Trainee";
        
    if (completionDateText != null)
        completionDateText.text = System.DateTime.Now.ToString("MMMM dd, yyyy");
        
    if (courseNameText != null)
        courseNameText.text = "Warehouse Safety Training";
        
    if (scoreText != null)
    {
        // CHANGE THIS - Show overall percentage instead of just quiz score
        if (ProgressService.Instance != null)
        {
            float overallProgress = ProgressService.Instance.GetProgress();
            int overallPercent = Mathf.RoundToInt(overallProgress * 100);
            scoreText.text = $"Final Score: {overallPercent}%";
        }
        else
        {
            scoreText.text = $"Final Score: {correctAnswers}/3 (Quiz Only)";
        }
    }
}

    void CloseCertificate()
    {
        if (certificatePanel != null)
            certificatePanel.SetActive(false);

        CursorCameraController playerController = FindObjectOfType<CursorCameraController>();
        if (playerController != null)
            playerController.SetMovementEnabled(true);
    }
    public int GetCorrectAnswers()
{
    return correctAnswers;
}
}