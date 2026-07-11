// UserDashboard.cshtml.cs
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace VotingSystem.Pages
{
    public class UserDashboardModel : PageModel
    {
        public List<Election> ElectionList { get; set; } = new();
        public List<Candidate> CandidateList { get; set; } = new();
        public List<string> Notifications { get; set; } = new();
        private readonly string connectionString = "server=localhost;uid=root;pwd=sqlkey;database=votingsystem";

        [BindProperty] public int ApplyElectionId { get; set; }
        [BindProperty] public string Slogan { get; set; }
        [BindProperty] public string Manifesto { get; set; }
        [BindProperty] public IFormFile Photo { get; set; }

        public List<int> VotedElections { get; set; } = new();

        public void OnGet()
        {
            LoadElections();
            LoadUserVotes();
            LoadNotifications();
        }

        // ---------- GET CANDIDATES ----------
        public ContentResult OnGetGetCandidates(int electionId)
        {
            // Load both elections and candidates here
            LoadElections();
            LoadCandidates(electionId);

            var activeCandidates = CandidateList
                .Where(c => !string.IsNullOrWhiteSpace(c.Status) &&
                            c.Status.Trim().Equals("Active", StringComparison.OrdinalIgnoreCase))
                .ToList();

            if (!activeCandidates.Any())
                return Content("<p>No active candidates available.</p>", "text/html");

            int? userId = HttpContext.Session.GetInt32("UserId");
            LoadUserVotes(); // ensure VotedElections is up to date
            bool hasVoted = userId != null && VotedElections.Contains(electionId);

            var election = ElectionList.FirstOrDefault(e => e.ElectionId == electionId);

            string html = "<div class='container'><div class='row'>";

            foreach (var c in activeCandidates)
            {
                html += $@"
        <div class='col-md-4 mb-4'>
            <div class='card shadow-sm text-center'>
                <img src='{c.Photo}' alt='Candidate Photo' class='card-img-top rounded-circle mx-auto mt-3'
                     style='width:120px;height:120px;object-fit:cover;' />
                <div class='card-body'>
                    <h5 class='card-title mt-2'>{c.Name}</h5>
                    <h6 class='card-subtitle mb-2 text-muted'><strong>Slogan:</strong> {c.Slogan}</h6>
                    <p class='card-text'>{c.Manifesto}</p>
                    <span class='badge bg-success'>{c.Status}</span>";

                // Show vote button only if election is ongoing and user hasn’t voted
                if (election != null && election.Status == "Ongoing" && !hasVoted)
                {
                    html += $@"
                    <button class='btn btn-primary btn-sm mt-2' 
                            onclick='vote({electionId}, {c.CandidateId})'>
                        Vote
                    </button>";
                }

                html += @"
                </div>
            </div>
        </div>";
            }
            html += "</div></div>";

            return Content(html, "text/html");
        }


        // ---------- APPLY AS CANDIDATE ----------
        public IActionResult OnPostApply()
        {
            int? userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null || userId == 0)
            {
                TempData["ErrorMessage"] = "User not logged in. Please sign in again.";
                return RedirectToPage("/Login");
            }

            string photoPath = null;
            if (Photo != null && Photo.Length > 0)
            {
                var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
                Directory.CreateDirectory(uploadsFolder);

                var fileName = $"{Guid.NewGuid()}_{Path.GetFileName(Photo.FileName)}";
                var filePath = Path.Combine(uploadsFolder, fileName);

                using var stream = new FileStream(filePath, FileMode.Create);
                Photo.CopyTo(stream);

                photoPath = "/uploads/" + fileName;
            }

            try
            {
                using var conn = new MySqlConnection(connectionString);
                conn.Open();

                string query = @"INSERT INTO Candidates 
                         (UserId, ElectionId, Status, Slogan, Manifesto, Photo) 
                         VALUES (@UserId, @ElectionId, 'Pending', @Slogan, @Manifesto, @Photo)";

                using var cmd = new MySqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@UserId", userId.Value);
                cmd.Parameters.AddWithValue("@ElectionId", ApplyElectionId);
                cmd.Parameters.AddWithValue("@Slogan", Slogan ?? "");
                cmd.Parameters.AddWithValue("@Manifesto", Manifesto ?? "");
                cmd.Parameters.AddWithValue("@Photo", photoPath ?? "");

                var rows = cmd.ExecuteNonQuery();
                TempData["SuccessMessage"] = rows > 0 ? "✅ Candidate application submitted!"
                                                     : "❌ Application failed to save.";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Database error: " + ex.Message;
            }

            return RedirectToPage("/UserDashboard");
        }

        // ---------- VOTE ----------
        public JsonResult OnPostVote(int electionId, int candidateId)
        {
            int? userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null || userId == 0)
                return new JsonResult(new { success = false, message = "User not logged in." });

            try
            {
                using var conn = new MySqlConnection(connectionString);
                conn.Open();

                string checkQuery = @"SELECT COUNT(*) FROM Votes 
                                      WHERE ElectionId=@ElectionId AND UserId=@UserId";
                using var checkCmd = new MySqlCommand(checkQuery, conn);
                checkCmd.Parameters.AddWithValue("@ElectionId", electionId);
                checkCmd.Parameters.AddWithValue("@UserId", userId.Value);
                var alreadyVoted = Convert.ToInt32(checkCmd.ExecuteScalar());

                if (alreadyVoted > 0)
                    return new JsonResult(new { success = false, message = "You have already voted in this election." });

                string voteQuery = @"INSERT INTO Votes (ElectionId, UserId, CandidateId, VoteTime) 
                                     VALUES (@ElectionId, @UserId, @CandidateId, NOW())";
                using var voteCmd = new MySqlCommand(voteQuery, conn);
                voteCmd.Parameters.AddWithValue("@ElectionId", electionId);
                voteCmd.Parameters.AddWithValue("@UserId", userId.Value);
                voteCmd.Parameters.AddWithValue("@CandidateId", candidateId);
                voteCmd.ExecuteNonQuery();

                VotedElections.Add(electionId);

                return new JsonResult(new { success = true, message = "Vote submitted successfully!" });
            }
            catch (Exception ex)
            {
                return new JsonResult(new { success = false, message = "Database error: " + ex.Message });
            }
        }

        // ---------- LOAD ELECTIONS ----------
        private void LoadElections()
        {
            ElectionList.Clear();
            using var conn = new MySqlConnection(connectionString);
            conn.Open();

            string query = "SELECT ElectionId, Title, Description, StartDate, EndDate, Status FROM Elections";
            using var cmd = new MySqlCommand(query, conn);
            using var reader = cmd.ExecuteReader();

            var elections = new List<Election>();
            while (reader.Read())
            {
                elections.Add(new Election
                {
                    ElectionId = Convert.ToInt32(reader["ElectionId"]),
                    Title = reader["Title"].ToString(),
                    Description = reader["Description"].ToString(),
                    StartDate = Convert.ToDateTime(reader["StartDate"]),
                    EndDate = Convert.ToDateTime(reader["EndDate"]),
                    Status = reader["Status"]?.ToString()
                });
            }
            reader.Close();

            foreach (var e in elections)
            {
                string newStatus;
                if (DateTime.Now < e.StartDate) newStatus = "Upcoming";
                else if (DateTime.Now >= e.StartDate && DateTime.Now <= e.EndDate) newStatus = "Ongoing";
                else newStatus = "Completed";

                if (e.Status != newStatus)
                {
                    using var updateCmd = new MySqlCommand(
                        "UPDATE Elections SET Status=@Status WHERE ElectionId=@Id", conn);
                    updateCmd.Parameters.AddWithValue("@Status", newStatus);
                    updateCmd.Parameters.AddWithValue("@Id", e.ElectionId);
                    updateCmd.ExecuteNonQuery();
                    e.Status = newStatus;
                }

                ElectionList.Add(e);
            }
        }

        // ---------- LOAD CANDIDATES ----------
        private void LoadCandidates(int electionId)
        {
            CandidateList.Clear();
            using var conn = new MySqlConnection(connectionString);
            conn.Open();

            string query = @"
                SELECT c.CandidateId, c.UserId, c.ElectionId, c.Slogan, c.Manifesto, c.Photo, c.Status,
                       u.Name
                FROM Candidates c
                JOIN Users u ON c.UserId = u.UserId
                WHERE c.ElectionId=@ElectionId";

            using var cmd = new MySqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@ElectionId", electionId);

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                CandidateList.Add(new Candidate
                {
                    CandidateId = Convert.ToInt32(reader["CandidateId"]),
                    ElectionId = Convert.ToInt32(reader["ElectionId"]),
                    UserId = Convert.ToInt32(reader["UserId"]),
                    Name = reader["Name"]?.ToString(),
                    Slogan = reader["Slogan"]?.ToString(),
                    Manifesto = reader["Manifesto"]?.ToString(),
                    Photo = reader["Photo"]?.ToString(),
                    Status = reader["Status"]?.ToString()
                });
            }
        }

        // ---------- LOAD USER VOTES ----------
        private void LoadUserVotes()
        {
            int? userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null || userId == 0) return;

            VotedElections.Clear();
            using var conn = new MySqlConnection(connectionString);
            conn.Open();

            string query = @"SELECT ElectionId FROM Votes WHERE UserId=@UserId";
            using var cmd = new MySqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@UserId", userId.Value);

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                VotedElections.Add(Convert.ToInt32(reader["ElectionId"]));
            }
        }

        // ---------- LOAD NOTIFICATIONS ----------
        private void LoadNotifications()
        {
            Notifications.Clear();
            int? userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null) return;

            using var conn = new MySqlConnection(connectionString);
            conn.Open();

            string query = @"
                SELECT e.Title AS ElectionTitle, c.Status 
                FROM Candidates c
                JOIN Elections e ON c.ElectionId = e.ElectionId
                WHERE c.UserId=@UserId
                ORDER BY c.CandidateId DESC";

            using var cmd = new MySqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@UserId", userId.Value);

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                string electionTitle = reader["ElectionTitle"].ToString();
                string status = reader["Status"].ToString()?.Trim();

                if (status.Equals("Active", StringComparison.OrdinalIgnoreCase))
                    Notifications.Add($"✅ Your application for '{electionTitle}' has been accepted!");
                else if (status.Equals("Rejected", StringComparison.OrdinalIgnoreCase))
                    Notifications.Add($"❌ Your application for '{electionTitle}' has been rejected.");
            }
        }

        // ---------- FEEDBACK HANDLER ----------
        public IActionResult OnPostSubmitFeedback(string Name, string Email, string Message)
        {
            int? userId = HttpContext.Session.GetInt32("UserId") ?? 0;

            try
            {
                using var conn = new MySqlConnection(connectionString);
                conn.Open();

                string query = @"INSERT INTO Feedbacks (UserId, Name, Email, Message)
                         VALUES (@UserId, @Name, @Email, @Message)";

                using var cmd = new MySqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@UserId", userId);
                cmd.Parameters.AddWithValue("@Name", Name ?? "");
                cmd.Parameters.AddWithValue("@Email", Email ?? "");
                cmd.Parameters.AddWithValue("@Message", Message ?? "");

                int rows = cmd.ExecuteNonQuery();

                TempData["SuccessMessage"] = rows > 0
                    ? "✅ Feedback submitted successfully!"
                    : "❌ Failed to submit feedback. Please try again.";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Database error: " + ex.Message;
            }

            return RedirectToPage("/UserDashboard");
        }

    }

    public class Election
    {
        public int ElectionId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Status { get; set; }
    }

    public class Candidate
    {
        public int CandidateId { get; set; }
        public int UserId { get; set; }
        public string Name { get; set; }
        public int ElectionId { get; set; }
        public string Slogan { get; set; }
        public string Manifesto { get; set; }
        public string Photo { get; set; }
        public string Status { get; set; }
    }
}
