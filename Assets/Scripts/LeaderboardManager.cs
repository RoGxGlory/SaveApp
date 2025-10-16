using System;
using UnityEngine;
using SaveAppCore;
using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;

public class LeaderboardManager : MonoBehaviour
{
    public List<LeaderboardEntry> Leaderboard { get; private set; } = new();
    public Transform leaderboardContent; // Assign in Inspector
    public GameObject leaderboardRowPrefab; // Assign in Inspector

    public async Task FetchAndDisplayLeaderboard()
    {
        Leaderboard = await ApiClient.GetLeaderboardAsync();
        Leaderboard = ApiClient.SortLeaderboard(Leaderboard);
        RefreshLeaderboard();
    }

    public void RefreshLeaderboard()
    {
        // Clear old rows
        foreach (Transform child in leaderboardContent)
            Destroy(child.gameObject);

        // Populate new rows
        foreach (var entry in Leaderboard)
        {
            var row = Instantiate(leaderboardRowPrefab, leaderboardContent);
            var fields = row.GetComponentsInChildren<TMP_Text>();
            fields[0].text = entry.Username;
            fields[1].text = entry.MonstersKilled.ToString() + " kills";
            fields[2].text = entry.DistanceTraveled.ToString() + "m";

            // Format ScoreDateUtc
            string dateStr = entry.ScoreDateUtc;
            if (DateTime.TryParse(entry.ScoreDateUtc, out var date))
                dateStr = date.ToString("yyyy-MM-dd HH:mm:ss");
            fields[3].text = dateStr;

            // Integrity
            fields[4].text = string.IsNullOrEmpty(entry.Integrity) ? "UNKNOWN" : entry.Integrity;
        }
    }
}