using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Text.Json;
using System.Linq;
using System.Drawing;
using System.Drawing.Imaging;
using System.Drawing.Text;

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

    public class EmployeeSummary
    {
        public string Name { get; set; }
        public double TotalTimeWorked { get; set; }
        public string Id { get; set; }
    }
    class Program
    {
        static async Task Main(string[] args)
        {
            var url = "https://rc-vault-fap-live-1.azurewebsites.net/api/gettimeentries?code=vO17RnE8vuzXzPJo5eaLLjXjmRW07law99QTD90zat9FfOQJKKUcgQ==";
            var httpClient = new HttpClient();
            var response = await httpClient.GetStringAsync(url);

            var employees = JsonSerializer.Deserialize<List<Employee>>(response);

            foreach (var employee in employees)
            {
                Console.WriteLine($"Employee Name: {employee.EmployeeName}, Employee ID: {employee.Id}");
            }

            var employeeGroups = employees
                .GroupBy(e => e.EmployeeName)
                .Select(g => new EmployeeSummary
                {
                    Name = g.Key,
                    TotalTimeWorked = g.Sum(e => (e.EndTimeUtc - e.StarTimeUtc).TotalHours),
                    Id = g.First().Id
                })
                .OrderByDescending(e => e.TotalTimeWorked)
                .ToList();

            GeneratePieChart(employeeGroups, "EmployeeTimePieChart.png");
        }

        static void GeneratePieChart(List<EmployeeSummary> employees, string fileName)
        {
            float totalHours = (float)employees.Sum(e => e.TotalTimeWorked);
            float startAngle = 0;
            int width = 600;
            int height = 600;

            using (Bitmap bitmap = new Bitmap(width, height))
            using (Graphics g = Graphics.FromImage(bitmap))
            {
                g.Clear(Color.White);

                g.TextRenderingHint = TextRenderingHint.AntiAlias;

                Random rand = new Random();
                foreach (var employee in employees)
                {
                    float sweepAngle = (float)(employee.TotalTimeWorked / totalHours * 360.0);
                    Color color = Color.FromArgb(rand.Next(256), rand.Next(256), rand.Next(256));
                    g.FillPie(new SolidBrush(color), 0, 0, width, height, startAngle, sweepAngle);

                    float midAngle = startAngle + sweepAngle / 2;
                    float x = (float)(width / 2 + (width / 2.5) * Math.Cos(midAngle * Math.PI / 180));
                    float y = (float)(height / 2 + (height / 2.5) * Math.Sin(midAngle * Math.PI / 180));

                    string text = $"{employee.Name}\n{employee.Id}";
                    g.DrawString(text, new Font("Arial", 10), Brushes.Black, new PointF(x, y), new StringFormat
                    {
                        Alignment = StringAlignment.Center,
                        LineAlignment = StringAlignment.Center
                    });

                    startAngle += sweepAngle;
                }

                bitmap.Save(fileName, ImageFormat.Png);
            }

            Console.WriteLine($"Pie chart saved as {fileName}");
        }
    }


}