using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MySql.Data.MySqlClient;
using System.Collections.Generic;

namespace VotingSystem.Pages
{
    public class CandidateRequestModel : PageModel
    {
        private readonly string connectionString = "server=localhost;uid=root;pwd=sqlkey;database=votingsystem;";
        public List<CandidateRequest> Requests { get; set; } = new();

        public void OnGet() => LoadRequests();

        public IActionResult OnPostAccept(int id)
        {
            UpdateCandidateStatus(id, "Active");
            TempData["SuccessMessage"] = "Candidate accepted successfully!";
            return RedirectToPage();
        }

        public IActionResult OnPostReject(int id)
        {
            UpdateCandidateStatus(id, "Rejected");
            TempData["SuccessMessage"] = "Candidate rejected.";
            return RedirectToPage();
        }

        private void UpdateCandidateStatus(int candidateId, string status)
        {
            using var conn = new MySqlConnection(connectionString);
            conn.Open();
            string sql = "UPDATE Candidates SET Status=@Status WHERE CandidateId=@CandidateId";
            using var cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@Status", status);
            cmd.Parameters.AddWithValue("@CandidateId", candidateId);
            cmd.ExecuteNonQuery();
        }

        private void LoadRequests()
        {
            Requests.Clear();
            using var conn = new MySqlConnection(connectionString);
            conn.Open();
            string sql = @"
                SELECT c.CandidateId, c.Slogan, c.Manifesto, c.Photo,
                       u.Name, e.Title AS ElectionTitle
                FROM Candidates c
                JOIN Users u ON c.UserId = u.UserId
                JOIN Elections e ON c.ElectionId = e.ElectionId
                WHERE c.Status = 'Pending'";
            using var cmd = new MySqlCommand(sql, conn);
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                Requests.Add(new CandidateRequest
                {
                    CandidateId = reader.GetInt32("CandidateId"),
                    UserName = reader["Name"].ToString(),
                    ElectionTitle = reader["ElectionTitle"].ToString(),
                    Slogan = reader["Slogan"].ToString(),
                    Manifesto = reader["Manifesto"].ToString(),
                    Photo = reader["Photo"].ToString()
                });
            }
        }
    }

    public class CandidateRequest
    {
        public int CandidateId { get; set; }
        public string UserName { get; set; }
        public string ElectionTitle { get; set; }
        public string Slogan { get; set; }
        public string Manifesto { get; set; }
        public string Photo { get; set; }
    }
}
