using System;
using UnityEngine;
using SaveAppCore;
using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;

/// <summary>
/// Manages fetching, sorting, and displaying the leaderboard in the UI.
/// </summary>
public class LeaderboardManager : MonoBehaviour
{
    // List of leaderboard entries fetched from the API
    public List<LeaderboardEntry> Leaderboard { get; private set; } = new();
    // Parent transform for leaderboard rows (set in Inspector)
    public Transform leaderboardContent;
    // Prefab for a single leaderboard row (set in Inspector)
    public GameObject leaderboardRowPrefab;

    /// <summary>
    /// Fetches the leaderboard from the API, sorts it, and updates the UI.
    /// </summary>
    public async Task FetchAndDisplayLeaderboard()
    {
        Leaderboard = await ApiClient.GetLeaderboardAsync();
        Leaderboard = ApiClient.SortLeaderboard(Leaderboard);
        RefreshLeaderboard();
    }

    /// <summary>
    /// Clears and repopulates the leaderboard UI with the latest data.
    /// </summary>
    public void RefreshLeaderboard()
    {
        // Remove old rows
        foreach (Transform child in leaderboardContent)
            Destroy(child.gameObject);

        // Instantiate a new row for each leaderboard entry
        foreach (var entry in Leaderboard)
        {
            var row = Instantiate(leaderboardRowPrefab, leaderboardContent);
            var fields = row.GetComponentsInChildren<TMP_Text>();
            // Username
            fields[0].text = entry.Username;
            // Monsters killed
            fields[1].text = entry.MonstersKilled.ToString() + " kills";
            // Distance traveled
            fields[2].text = entry.DistanceTraveled.ToString() + "m";

            // Format and display the score date
            string dateStr = entry.ScoreDateUtc;
            if (DateTime.TryParse(entry.ScoreDateUtc, out var date))
                dateStr = date.ToString("yyyy-MM-dd HH:mm:ss");
            fields[3].text = dateStr;

            // Integrity value (or UNKNOWN if missing)
            fields[4].text = string.IsNullOrEmpty(entry.Integrity) ? "UNKNOWN" : entry.Integrity;
        }
    }
}