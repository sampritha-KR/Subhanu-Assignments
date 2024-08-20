using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Text.Json;
using System.Linq;

namespace EmployeeTimeTracker
{
    public class Employee
    {
        public string Id { get; set; }
        public string EmployeeName { get; set; }
        public DateTime StarTimeUtc { get; set; }
        public DateTime EndTimeUtc { get; set; }
        public string EntryNotes { get; set; }
        public DateTime? DeletedOn { get; set; }
    }
    class Program
    {
        static async Task Main(string[] args)
        {
            var url = "https://rc-vault-fap-live-1.azurewebsites.net/api/gettimeentries?code=vO17RnE8vuzXzPJo5eaLLjXjmRW07law99QTD90zat9FfOQJKKUcgQ==";
            var httpClient = new HttpClient();
            var response = await httpClient.GetStringAsync(url);
            var employees = JsonSerializer.Deserialize<List<Employee>>(response);
            var employeeGroups = employees
                .GroupBy(e => e.EmployeeName)
                .Select(g => new
                {
                    Name = g.Key,
                    TotalTimeWorked = g.Sum(e => (e.EndTimeUtc - e.StarTimeUtc).TotalHours)
                })
                .OrderByDescending(e => e.TotalTimeWorked)
                .ToList();
            Console.WriteLine(GenerateHtmlTable(employeeGroups));
        }

        static string GenerateHtmlTable(IEnumerable<dynamic> employees)
        {
            string html = "<html><body><table border='1'>";
            html += "<tr><th>Name</th><th>Total Time Worked (hours)</th></tr>";

            foreach (var employee in employees)
            {
                string rowColor = employee.TotalTimeWorked < 100 ? "style='background-color:lightcoral'" : "";
                html += $"<tr {rowColor}><td>{employee.Name}</td><td>{employee.TotalTimeWorked:F2}</td></tr>";
            }
            html += "</table></body></html>";
            return html;
        }
    }
}