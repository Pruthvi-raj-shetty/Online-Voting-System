using Microsoft.AspNetCore.Mvc.RazorPages;
using MySql.Data.MySqlClient;
using System.Collections.Generic;

namespace VotingSystem.Pages
{
    public class candidatedashboardModel : PageModel
    {
        public List<CandidateApplicationViewModel> Applications { get; set; } = new();
        public List<string> Notifications { get; set; } = new();

        public void OnGet()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                Response.Redirect("/Login");
                return;
            }

            string connStr = "server=localhost;user=root;password=sqlkey;database=VotingSystem;";
            using var connection = new MySqlConnection(connStr);
            connection.Open();

            string query = @"
               SELECT c.CandidateId, c.ElectionId, e.Title AS ElectionTitle, 
                      c.Slogan, c.Manifesto, c.Photo, c.Status 
               FROM candidates c
               JOIN elections e ON c.ElectionId = e.ElectionId
               WHERE c.UserId = @UserId";

            using var cmd = new MySqlCommand(query, connection);
            cmd.Parameters.AddWithValue("@UserId", userId);

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                var app = new CandidateApplicationViewModel
                {
                    CandidateId = reader.GetInt32("CandidateId"),
                    ElectionId = reader.GetInt32("ElectionId"),
                    ElectionTitle = reader.GetString("ElectionTitle"),
                    Slogan = reader.GetString("Slogan"),
                    Manifesto = reader.GetString("Manifesto"),
                    Photo = reader.GetString("Photo"),
                    Status = reader.GetString("Status")
                };

                Applications.Add(app);

                // Add notification if application is accepted or rejected
                if (app.Status == "Active")
                {
                    Notifications.Add($"Your application for '{app.ElectionTitle}' has been accepted ✅");
                }
                else if (app.Status == "Rejected")
                {
                    Notifications.Add($"Your application for '{app.ElectionTitle}' has been rejected ❌");
                }
            }
        }
    }

    public class CandidateApplicationViewModel
    {
        public int CandidateId { get; set; }
        public int ElectionId { get; set; }
        public string ElectionTitle { get; set; }
        public string Slogan { get; set; }
        public string Manifesto { get; set; }
        public string Photo { get; set; }
        public string Status { get; set; }
    }
}
