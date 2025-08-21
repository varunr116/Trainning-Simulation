using UnityEngine;

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