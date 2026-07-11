using Microsoft.AspNetCore.Mvc.RazorPages;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;

namespace VotingSystem.Pages
{
    public class FeedbackListModel : PageModel
    {
        private readonly string connectionString = "server=localhost;uid=root;pwd=sqlkey;database=votingsystem";

        public List<Feedback> Feedbacks { get; set; } = new();

        public void OnGet()
        {
            Feedbacks.Clear();
            using var conn = new MySqlConnection(connectionString);
            conn.Open();

            string query = "SELECT Name, Email, Message, SubmittedAt FROM Feedbacks ORDER BY SubmittedAt DESC";
            using var cmd = new MySqlCommand(query, conn);
            using var reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                Feedbacks.Add(new Feedback
                {
                    Name = reader["Name"].ToString(),
                    Email = reader["Email"].ToString(),
                    Message = reader["Message"].ToString(),
                    SubmittedAt = Convert.ToDateTime(reader["SubmittedAt"])
                });
            }
        }
    }

    public class Feedback
    {
        public string Name { get; set; }
        public string Email { get; set; }
        public string Message { get; set; }
        public DateTime SubmittedAt { get; set; }
    }
}
