using UnityEngine;
using TMPro;
using Unity.MLAgents;

public class Scores : MonoBehaviour
{
    [Header("Agentes a monitorear")]
    public Agent crawlerAgent;
    public Agent wormAgent;

    [Header("UI")]
    public TextMeshProUGUI crawlerScoreText;
    public TextMeshProUGUI wormScoreText;

    void Update()
    {
        if (crawlerAgent != null)
            crawlerScoreText.text = "Crawler: " + crawlerAgent.GetCumulativeReward().ToString("F2");

        if (wormAgent != null)
            wormScoreText.text = "Worm: " + wormAgent.GetCumulativeReward().ToString("F2");
    }
}
