using Microsoft.AspNetCore.Mvc.RazorPages;
using MySql.Data.MySqlClient;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;

namespace VotingSystem.Pages
{
    public class ResultsModel : PageModel
    {
        private readonly string connectionString = "server=localhost;uid=root;pwd=sqlkey;database=votingsystem";

        public List<ElectionResult> ElectionResults { get; set; } = new();

        // ✅ Make isAdmin a public property
        public bool IsAdmin { get; set; }

        public void OnGet()
        {
            LoadElectionResults();
        }

        private void LoadElectionResults()
        {
            ElectionResults.Clear();

            int? userId = HttpContext.Session.GetInt32("UserId");
            IsAdmin = false;

            using (var conn = new MySqlConnection(connectionString))
            {
                conn.Open();

                // Check if user is admin
                if (userId != null)
                {
                    string adminCheckQuery = "SELECT AdminId FROM Admins WHERE UserId=@UserId";
                    using var cmd = new MySqlCommand(adminCheckQuery, conn);
                    cmd.Parameters.AddWithValue("@UserId", userId.Value);
                    var result = cmd.ExecuteScalar();
                    IsAdmin = result != null;
                }

                // Load elections
                string electionQuery = IsAdmin
                    ? "SELECT ElectionId, Title, Status FROM Elections ORDER BY StartDate DESC"
                    : "SELECT ElectionId, Title, Status FROM Elections WHERE Status='Completed' ORDER BY StartDate DESC";

                using var electionCmd = new MySqlCommand(electionQuery, conn);
                using var reader = electionCmd.ExecuteReader();

                var elections = new List<ElectionResult>();

                while (reader.Read())
                {
                    elections.Add(new ElectionResult
                    {
                        ElectionId = Convert.ToInt32(reader["ElectionId"]),
                        Title = reader["Title"].ToString(),
                        Status = reader["Status"].ToString(),
                        Candidates = new List<CandidateResult>()
                    });
                }

                reader.Close();

                // Load candidates and votes for each election
                foreach (var election in elections)
                {
                    string candidateQuery = @"
                        SELECT c.CandidateId, u.Name AS CandidateName, c.Slogan, COUNT(v.VoteId) AS VoteCount
                        FROM Candidates c
                        JOIN Users u ON c.UserId = u.UserId
                        LEFT JOIN Votes v ON c.CandidateId = v.CandidateId
                        WHERE c.ElectionId=@ElectionId AND c.Status='Active'
                        GROUP BY c.CandidateId, u.Name, c.Slogan
                        ORDER BY VoteCount DESC";

                    using var candCmd = new MySqlCommand(candidateQuery, conn);
                    candCmd.Parameters.AddWithValue("@ElectionId", election.ElectionId);
                    using var candReader = candCmd.ExecuteReader();

                    while (candReader.Read())
                    {
                        election.Candidates.Add(new CandidateResult
                        {
                            CandidateId = Convert.ToInt32(candReader["CandidateId"]),
                            Name = candReader["CandidateName"].ToString(),
                            Slogan = candReader["Slogan"].ToString(),
                            Votes = Convert.ToInt32(candReader["VoteCount"])
                        });
                    }

                    candReader.Close();
                    ElectionResults.Add(election);
                }
            }
        }
    }

    public class ElectionResult
    {
        public int ElectionId { get; set; }
        public string Title { get; set; }
        public string Status { get; set; }
        public List<CandidateResult> Candidates { get; set; } = new();
    }

    public class CandidateResult
    {
        public int CandidateId { get; set; }
        public string Name { get; set; }
        public string Slogan { get; set; }
        public int Votes { get; set; }
    }
}
