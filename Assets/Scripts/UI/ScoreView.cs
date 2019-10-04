using ECS.Systems;
using TMPro;
using Unity.Entities;
using UnityEngine;

namespace UI
{
    public class ScoreView : MonoBehaviour
    {
        [SerializeField]
        private TextMeshProUGUI _scoreText;
        
        private void Start()
        {
            var scoreSystem = World.Active.GetOrCreateSystem<ScoreDisplaySystem>();
            scoreSystem.ScoreChanged += x => _scoreText.text = x.ToString();
        }
    }
}
